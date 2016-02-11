using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class PuzzleTracker : MonoBehaviour {

	public event EventHandler OnPuzzleCorrectEvent;
	public GameObject[] filledPuzzles;
	public int currentPuzzleIndex = -1;
	public GameObject[] players;
	public Puzzle[] puzzles;
	private GameObject playerContainer;
	private bool[] movingPuzzleFlags;
	private float[] movingPuzzleTimers;
	private float movingPuzzleTrackDuration = 8f;
	private TutorialController tutorialController;
	private int prevPuzzleSolved;
	private CameraController cameraController;
	private bool[]playerSnapFlag = new bool[3];

	// Use this for initialization
	void Start () {
		movingPuzzleFlags = new bool[puzzles.Length];
		movingPuzzleTimers = new float[puzzles.Length];
		cameraController = GameObject.FindObjectOfType<CameraController> ();
		playerContainer = GameObject.FindGameObjectWithTag ("PlayerContainer");
	
		tutorialController = GameObject.FindObjectOfType<TutorialController> ();
	}


	
	// Update is called once per frame
	void Update () {
		bool isAnyPuzzleSolved = false;

		//for (int i = 0; i<puzzles.Length; i++) {
			//Puzzle curPuzzle = puzzles[i];
			//int puzzleIndex = i + 1;
		if (currentPuzzleIndex < puzzles.Length && currentPuzzleIndex >= 0 && currentPuzzleIndex < puzzles.Length) {
			Puzzle curPuzzle = puzzles[currentPuzzleIndex];
            int puzzleIndex = currentPuzzleIndex + 1;
			int i = currentPuzzleIndex;
			
			if(curPuzzle.isTargetMoving[0] == true || curPuzzle.isTargetMoving[1] == true || curPuzzle.isTargetMoving[2] == true){
				//check moving target puzzles
				if ((curPuzzle.isRedActive == false || curPuzzle.isCorrect(players[0],puzzleIndex)) && (curPuzzle.isYellowActive == false || curPuzzle.isCorrect(players[2],puzzleIndex)) && (curPuzzle.isBlueActive == false ||  curPuzzle.isCorrect(players[1],puzzleIndex)) && prevPuzzleSolved != puzzleIndex) {
					if(movingPuzzleFlags[i] == false){
						//start puzzle timer
						movingPuzzleTimers[i] = 0;
					}
					movingPuzzleFlags[i] = true;
					movingPuzzleTimers[i] += Time.deltaTime;
					if(movingPuzzleTimers[i] >= movingPuzzleTrackDuration){
						//print("Bingo!!!! Puzzle "+ (puzzleIndex) + " solved");
						prevPuzzleSolved = puzzleIndex;
						//puzzleStatus.text = "Bingo!!!! Puzzle "+ (puzzleIndex) + " solved";
						isAnyPuzzleSolved = true;
						OnPuzzleSolved(puzzleIndex);
					}

				}
				else{
					movingPuzzleFlags[i] = false;
					movingPuzzleTimers[i] = 0;
					
				}
			}
			else{

				//for all static puzzles
				if ((curPuzzle.isRedActive == false || curPuzzle.isCorrect(players[0],puzzleIndex)) && (curPuzzle.isYellowActive == false || curPuzzle.isCorrect(players[2],puzzleIndex)) && (curPuzzle.isBlueActive == false ||  curPuzzle.isCorrect(players[1],puzzleIndex)) && prevPuzzleSolved != puzzleIndex) {
					//print("Bingo!!!! Puzzle "+ (puzzleIndex) + " solved");
					prevPuzzleSolved = puzzleIndex;
					//puzzleStatus.text = "Bingo!!!! Puzzle "+ (puzzleIndex) + " solved";
					isAnyPuzzleSolved = true;
					
					OnPuzzleSolved(puzzleIndex);
				}

				//check for wrong snapping of shapes 
				for(int j=0;j<players.Length;j++){
					int worngSnapIndex = curPuzzle.isAnyWrongSnapped(players[j],puzzleIndex);
					if(worngSnapIndex != -1){

						if(playerSnapFlag[j] == false){
							AudioManager.Main.PlayNewSound("PlayerInCorrect");

							playerSnapFlag[j] = true;
							curPuzzle.Snap(players[j],worngSnapIndex);
						}

					}
					else if(playerSnapFlag[j] == true){
						playerSnapFlag[j] = false;
						players[j].GetComponent<PlayerController>().ActivateSnapping();
						AudioManager.Main.PlayNewSound("Unsnap_sound");

					}
					 

				}




			}

			
			//update current puzzle status to player controller
			players[0].GetComponent<PlayerController>().UpdateCurrentStatus(curPuzzle.isCorrect(players[0],puzzleIndex));
			players[1].GetComponent<PlayerController>().UpdateCurrentStatus(curPuzzle.isCorrect(players[1],puzzleIndex));
			players[2].GetComponent<PlayerController>().UpdateCurrentStatus(curPuzzle.isCorrect(players[2],puzzleIndex));

			

		}

		//}


		if(Input.GetKeyDown(KeyCode.Space)){
			OnPuzzleSolved(currentPuzzleIndex);
		}
	}

	public void SnapPlayer(int index){
		Puzzle curPuzzle = puzzles[currentPuzzleIndex];
		curPuzzle.Snap (players [index]);
	}

	void OnPuzzleSolved(int puzzleIndex){

		cameraController.enablePath();
		filledPuzzles [currentPuzzleIndex].SetActive (true);

		LeanTween.delayedCall (0.8F,()=>{
			cameraController.resuemCamera();
			playerContainer.SetActive (false);

		});

		AudioManager.Main.PlayNewSound("Puzzle_success", false, 0.5f);
		if (OnPuzzleCorrectEvent != null) {
			this.OnPuzzleCorrectEvent(this, EventArgs.Empty);
		}

		tutorialController.UpdateTutorial (TutorialController.TutorialState.None);


	}

	public void ActivateControls(){
		for(int i=0;i<players.Length;i++){
			GameObject handle = players[i];
			handle.GetComponent<PlayerController>().reset();
		}
		
		currentPuzzleIndex++;
		print("curIndex: "+currentPuzzleIndex);
		if(currentPuzzleIndex < CameraController.maxState){
			playerContainer.SetActive (true);
			
		}

		if (currentPuzzleIndex == 0) {
			tutorialController.UpdateTutorial (TutorialController.TutorialState.Move);
		} else if (currentPuzzleIndex == 1) {
			tutorialController.UpdateTutorial (TutorialController.TutorialState.Shape);
		} else if (currentPuzzleIndex == 2) {
			tutorialController.UpdateTutorial (TutorialController.TutorialState.Scale);
		}else if (currentPuzzleIndex == 3) {
			tutorialController.UpdateTutorial (TutorialController.TutorialState.Rotate);
		} 
		else{
			tutorialController.UpdateTutorial (TutorialController.TutorialState.None);
		} 
	}




}
