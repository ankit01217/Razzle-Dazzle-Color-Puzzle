using UnityEngine;
using System.Collections;
using System;

public class PlayerController : MonoBehaviour {

	public event EventHandler OnStartCorrectEvent;
	public event EventHandler OnStartIncorrectEvent;
	public event EventHandler OnMoveTriggerEvent;

	public event EventHandler LeftSideStartEvent;

	public Sprite[] squareSprites, triangleSprites,crossSprties, circleSprites;
	public enum Shape{
		CIRCLE = 0,
		SQUARE = 1,
		TRIANGLE = 2,
		CROSS = 3,
	}

	public enum SnapState{
		UnsnappedAndSnappable = 0,
		UnsnappedAndUnsnappable = 1,
		SnappedAndUnsnappable = 1,

	}
	public SnapState snapState = SnapState.UnsnappedAndSnappable;
	public PuzzleTracker puzzleTracker;
	public PSMoveExample psMoveWraper;
	public int index;
	public Shape curShape = Shape.CIRCLE;
	public const float m_clampMinX = -90f;
	public const float m_clampMaxX = 90f;
	public const float m_clampMinY = -48f;
	public const float m_clampMaxY = 48f;
	private int isRaisingScale = 0;
	private int isRaisingYPosition = 0;
	private int isRaisingXPosition = 0;
	private float minScale = 1f;
	private float maxScale = 40f;
	private float moveSpeed;
	private float scaleSpeed;
	private float rotateSpeed = 30f; 
	private float prevZPosition = 0f;
	private float prevXPosition = 0f;
	private float prevYPosition = 0f;
	private Shape prevShape;
	private bool playedTriggerSound = false;
	public bool isCorrect = false;
	private MoveController controller;
	private MoveData moveData;
	private Vector3 prevControllerPos;
	private bool isCalibrationEnabled = true;
	private Vector2 moveRange = new Vector2(3f,2f);
	private Vector2 moveCenter;
	private TutorialController tutorialController;
	private GameObject finishedPuzzlesContainer;

	// Use this for initialization
	void Start () {

		finishedPuzzlesContainer = GameObject.FindGameObjectWithTag ("FinishedPuzzles");
		tutorialController = GameObject.FindObjectOfType<TutorialController> ();
		rotateSpeed = (isWindows() == true) ? 30f : 50f;
		moveSpeed = (isWindows() == true) ? 10f : 15f;
		scaleSpeed = (isWindows() == true) ? 4f : 1f;
		if (gameObject.tag == "PlayerRed") {
			index = 0;
		}
		else if (gameObject.tag == "PlayerBlue") {
			index = 1;
		}
		else if (gameObject.tag == "PlayerYellow") {
			index = 2;
		}





	}
	
	// Update is called once per frame
	void Update () {

		if (PSMoveInput.IsConnected == true) {
			controller = PSMoveInput.MoveControllers[index];
			moveData = controller.Data;

			if (controller.Connected == true && moveData != null) {
				string buttonsSt = ""+moveData.Buttons;

				bool isTriggerDown = moveData.Buttons == MoveButton.T || (psMoveWraper.gyroEnabled && psMoveWraper.gyroAnalogScale && moveData.ValueT >= psMoveWraper.gyroAnalogMin);

				if(moveData.Buttons == MoveButton.Start && isCalibrationEnabled == true){
					isCalibrationEnabled = false;
					LeanTween.delayedCall(1f,()=>{
						isCalibrationEnabled = true;
					});
					psMoveWraper.CalibratePlayer(index);

				}
				else if ((buttonsSt == "Square, T" || buttonsSt == "T, Square" || moveData.Buttons == MoveButton.Square) && curShape != Shape.SQUARE && snapState == SnapState.UnsnappedAndSnappable) {
					changeShape(Shape.SQUARE);
				}
				else if ((buttonsSt == "Triangle, T" || buttonsSt == "T, Triangle" || moveData.Buttons == MoveButton.Triangle) && curShape != Shape.TRIANGLE && snapState == SnapState.UnsnappedAndSnappable) {
					changeShape(Shape.TRIANGLE);

					
				}
				else if ((buttonsSt == "Circle, T" || buttonsSt == "T, Circle" || moveData.Buttons == MoveButton.Circle) && curShape != Shape.CIRCLE && snapState == SnapState.UnsnappedAndSnappable) {
					changeShape(Shape.CIRCLE);

				}
				else if ((buttonsSt == "Cross, T" || buttonsSt == "T, Cross" || moveData.Buttons == MoveButton.Cross) && curShape != Shape.CROSS && snapState == SnapState.UnsnappedAndSnappable) {
					changeShape(Shape.CROSS);

				}
				else if (isTriggerDown) {
					if(!playedTriggerSound) {
						AudioManager.Main.PlayNewSound("Puzzle_lock");
						playedTriggerSound = true;
					}

					// Update border particles
					if (!GetComponent<ParticleManager> ().IsPlaying () && this.snapState != SnapState.SnappedAndUnsnappable) {
						GetComponent<ParticleManager> ().Play ();
					} else if (GetComponent<ParticleManager> ().IsPlaying () && this.snapState == SnapState.SnappedAndUnsnappable) {
						GetComponent<ParticleManager> ().Stop ();
					}

					if (!psMoveWraper.gyroEnabled) {
						
						Vector3 curScale = gameObject.transform.localScale;
						if(prevZPosition == 0f){
							prevZPosition = (float) Math.Round((double)moveData.Position.z, 1) ;
							isRaisingScale = 0;
						}
						
						float curZPosition = (float) Math.Round((double)moveData.Position.z, 1) ;
						
						if(curZPosition > prevZPosition){
							isRaisingScale = 1;
						}
						else if(curZPosition < prevZPosition){
							isRaisingScale = -1;
						}
						else{
							isRaisingScale = 0;
						}
						
						prevZPosition = (float) Math.Round((double) moveData.Position.z, 1);
						
						if(isRaisingScale == 1){
							gameObject.transform.localScale = new Vector3(minScale,minScale,minScale) * Mathf.Clamp(curScale.x + (Time.deltaTime*scaleSpeed),minScale,maxScale);
							
						}
						else if(isRaisingScale == -1){
							gameObject.transform.localScale = new Vector3(minScale,minScale,minScale) * Mathf.Clamp(curScale.x - (Time.deltaTime*scaleSpeed),minScale,maxScale);
							
						}
						else{
							isRaisingScale = 0;
						}
					}
				}
				else{
					isRaisingScale = 0;
					playedTriggerSound = false;
					if(snapState == SnapState.SnappedAndUnsnappable)
					{
						if(moveData.ValueT == 0 && psMoveWraper.isHandleActive[index] == false){
							snapState = SnapState.UnsnappedAndUnsnappable;
							enablePlayer();

						}
					}

				}

				// Update border particles
				if (!isTriggerDown && GetComponent<ParticleManager>().IsPlaying())
					GetComponent<ParticleManager>().Stop();

			}
		}


		if( ((Input.GetKey(KeyCode.Keypad1) == true || Input.GetKey(KeyCode.Alpha1) == true) && index == 0) || ((Input.GetKey(KeyCode.Keypad2) == true || Input.GetKey(KeyCode.Alpha2) == true) && index == 1) || ((Input.GetKey(KeyCode.Keypad3) == true || Input.GetKey(KeyCode.Alpha3) == true) && index == 2)){
			checkKeyboardControls();

		}





	}

	public void disablePlayer(){
		psMoveWraper.isHandleActive [index] = false;

	}

	public void enablePlayer(){
		psMoveWraper.isHandleActive [index] = true;

	}

	public void UpdateCurrentStatus(bool status){


		if (isCorrect == false && status == true) {
			print("OnStart CorrectEvent");
			AudioManager.Main.PlayNewSound("Snap_sound");

			puzzleTracker.SnapPlayer(index);

			if (this.OnStartCorrectEvent != null) {
				this.OnStartCorrectEvent(this, EventArgs.Empty);
			}	


		} else if (isCorrect == true && status == false) {
			print("OnStart IncorrectEvent");
			AudioManager.Main.PlayNewSound("Unsnap_sound");
			ActivateSnapping();

			if (this.OnStartIncorrectEvent != null) {
				this.OnStartIncorrectEvent(this, EventArgs.Empty);
			}
		}

		isCorrect = status;


	}

	public void ActivateSnapping(){
		if(snapState == SnapState.UnsnappedAndUnsnappable){
			snapState = SnapState.UnsnappedAndSnappable;
			
		}
	}

	public void changeShape(Shape shape){

		if (puzzleTracker.currentPuzzleIndex > 0)
		{
			if (curShape != shape) {
				GetComponent<ParticleManager> ().ChangeBorderSystem (shape);
				AudioManager.Main.PlayNewSound ("Shape_change_sound");
				
			}
			
			curShape = shape;
			
			if (curShape == Shape.CIRCLE) {
				GetComponent<SpriteRenderer> ().sprite = circleSprites [index];
				
			} else if (curShape == Shape.TRIANGLE) {
				GetComponent<SpriteRenderer> ().sprite = triangleSprites [index];
				
			} else if (curShape == Shape.CROSS) {
				GetComponent<SpriteRenderer> ().sprite = crossSprties [index];
				
			} else if (curShape == Shape.SQUARE) {
				GetComponent<SpriteRenderer> ().sprite = squareSprites [index];
				
			}
		}


	}

	void checkKeyboardControls(){
		Vector3 curPos = gameObject.transform.position;
		Vector3 curScale = gameObject.transform.localScale;
		Vector3 curRotation = gameObject.transform.localRotation.eulerAngles;
		Vector3 cameraPos = Camera.main.transform.position;

		if (Input.GetKey (KeyCode.LeftArrow)) {
			curPos.x = Mathf.Clamp(curPos.x - Time.deltaTime*moveSpeed,m_clampMinX + cameraPos.x,m_clampMaxX + cameraPos.x);
			gameObject.transform.position = curPos;

		}

		else if (Input.GetKey (KeyCode.RightArrow)) {
			curPos.x = Mathf.Clamp(curPos.x + Time.deltaTime*moveSpeed,m_clampMinX + cameraPos.x,m_clampMaxX + cameraPos.x);
			gameObject.transform.position = curPos;

		}
		else if (Input.GetKey (KeyCode.UpArrow)) {
			curPos.y = Mathf.Clamp(curPos.y + Time.deltaTime*moveSpeed,m_clampMinY + cameraPos.y,m_clampMaxY + cameraPos.y);
			gameObject.transform.position = curPos;

		}
		else if (Input.GetKey (KeyCode.DownArrow)) {
			curPos.y = Mathf.Clamp(curPos.y - Time.deltaTime*moveSpeed,m_clampMinY + cameraPos.y,m_clampMaxY + cameraPos.y);
			gameObject.transform.position = curPos;

		}
		else if (Input.GetKey (KeyCode.Return)) {
			gameObject.transform.localScale = new Vector3(minScale,minScale,minScale) * Mathf.Clamp(curScale.x + (Time.deltaTime*scaleSpeed),minScale,maxScale);
		
		}
		else if (Input.GetKey (KeyCode.RightShift)) {
			gameObject.transform.localScale = new Vector3(minScale,minScale,minScale) * Mathf.Clamp(curScale.x - (Time.deltaTime*scaleSpeed),minScale,maxScale);
		
		}
		else if (Input.GetKey (KeyCode.RightCommand) || Input.GetKey (KeyCode.LeftBracket)) {
			LeanTween.rotateZ(gameObject,Mathf.Clamp(curRotation.z +  Time.deltaTime*rotateSpeed, -360f,360f),Time.deltaTime);

		}
		else if (Input.GetKey (KeyCode.RightAlt) || Input.GetKey (KeyCode.RightBracket)) {
			LeanTween.rotateZ(gameObject,Mathf.Clamp(curRotation.z -  Time.deltaTime*rotateSpeed, -360f,360f),Time.deltaTime);

		}


		//shape controlls
		if (Input.GetKey(KeyCode.A) == true) {
			changeShape(Shape.SQUARE);
			
		}
		else if (Input.GetKey (KeyCode.S) == true) {
			changeShape(Shape.TRIANGLE);
			
		}
		else if (Input.GetKey(KeyCode.D) == true ) {
			changeShape(Shape.CIRCLE);
			
		}
		else if (Input.GetKey (KeyCode.F) == true) {
			changeShape(Shape.CROSS);
			
		}

	}

	public void reset(){


		Vector3 cameraPos = Camera.main.transform.position;
		Vector3 curPos = transform.position;

		curPos.y = cameraPos.y - 25f;
		if (index == 0) {
			curPos.x = cameraPos.x-23;
		}
		else if (index == 1) {
			curPos.x = cameraPos.x;
		}
		else if (index == 2) {
			curPos.x = cameraPos.x+23;
		}

		transform.position = curPos;


		transform.localScale = new Vector3(1.2f,1.2f,1.2f);
		transform.rotation = Quaternion.identity;


		psMoveWraper.isHandleActive [index] = true;
		changeShape (Shape.CIRCLE);
	}


	public void clone(){
		Vector3 curPos = transform.position;
		curPos.z = -25;

		GameObject cloneObj = (GameObject)Instantiate(gameObject,gameObject.transform.position, gameObject.transform.rotation);
		cloneObj.GetComponent<PlayerController> ().enabled = false;
		cloneObj.transform.position = curPos;
		cloneObj.transform.localScale = transform.localScale;
		cloneObj.layer = LayerMask.NameToLayer("Default");
		cloneObj.transform.parent = finishedPuzzlesContainer.transform;
	}

	public bool isWindows(){
		return (Application.platform != RuntimePlatform.OSXEditor && Application.platform != RuntimePlatform.OSXPlayer);
	}


}
