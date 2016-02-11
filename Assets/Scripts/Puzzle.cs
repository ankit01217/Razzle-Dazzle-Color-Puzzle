using UnityEngine;
using System.Collections;
using System;

public class Puzzle : MonoBehaviour {

	public float[] minPosX, maxPosX, minPosY, maxPosY, snapX, snapY;
	public float[] minScale, maxScale, snapScale;
	public float[] minRotation, maxRotation;
	public Quaternion[] snapRotation;

	public PlayerController.Shape[] shape;
	public bool[] isTargetMoving;
	public bool isBlueActive, isRedActive, isYellowActive = true;
	public GameObject[] players;
	public string[] movingTargetTag;
	private GameObject[] movingTarget;
	private const float scaleOffset = 0.18f;
	private const float rotOffset = 4.5f;
	private const float positionOffset = 2.5f;

	//Use this for initialization
	void Start() {
	}



	int getPlayerIndex(GameObject obj){
		if (obj.tag == "PlayerRed") {
			return 0;
		}
		else if (obj.tag == "PlayerBlue") {
			return 1;
		}
		else if (obj.tag == "PlayerYellow") {
			return 2;
		}


		return 0;
	}

	GameObject getChildObjectWithTag(GameObject parent , string tag){
		Transform[] allChildren = parent.GetComponentsInChildren<Transform>();
		foreach (Transform child in allChildren) {
			// do whatever with child transform here
			if (child.tag == tag)
			{
				return child.gameObject;
			}
		}
		
		return null;
	}

	// Update is called once per frame
	void Update () {
		if (players [0] != null && players [1] != null && players [2] != null) {
			for (int index = 0; index<players.Length; index++) {
				GameObject player = players [index];

				snapX[index] = player.transform.position.x;
				snapY[index] = player.transform.position.y;
				snapScale[index] = player.transform.localScale.x;
				snapRotation[index] = player.transform.rotation;

				minPosX [index] = player.transform.position.x - (player.GetComponent<BoxCollider2D> ().bounds.size.x / 2.0f);
				maxPosX [index] = player.transform.position.x + (player.GetComponent<BoxCollider2D> ().bounds.size.x / 2.0f);
				minPosY [index] = player.transform.position.y - (player.GetComponent<BoxCollider2D> ().bounds.size.y / 2.0f);
				maxPosY [index] = player.transform.position.y + (player.GetComponent<BoxCollider2D> ().bounds.size.y / 2.0f);
				minRotation [index] = getCurrentRotation(player);
				maxRotation [index] = getCurrentRotation(player);
				minScale [index] = player.transform.localScale.x - scaleOffset;
				maxScale [index] = player.transform.localScale.x + scaleOffset;
				shape [index] = player.GetComponent<PlayerController> ().curShape;
			}
		
		}

	}

	public bool isCorrect(GameObject target, int puzzleIndex){
		int index = getPlayerIndex(target);
		movingTarget = new GameObject[3];
		//print ("puzzle, player, pos,rot,scale :" + puzzleIndex + "," + (index+1) +","+ isCorrectPosition (target) + ","+ isCorrectRotation (target) + "," + isCorrectScale(target));
		return (isCorrectRotation(target) == true && isCorrectScale (target) == true && isCorrectPosition (target) == true && isCorrectShape (target) == true);
	}

	public int isAnyWrongSnapped(GameObject target, int puzzleIndex){
		int targetIndex = getPlayerIndex (target);
		for (int index = 0; index<players.Length; index++) {
			if(index != targetIndex) {
				if((isCorrectRotation(target,index) == true && isCorrectScale (target,index) == true && isCorrectPosition (target,index) == true && isCorrectShape (target,index) == true)){
					return index;
				}
			}

		}

		return -1;
	
	}

	public bool isCorrectPosition(GameObject target, int index = -1){

		int targetIndex = (index == -1) ? getPlayerIndex (target) : index;
		if (isTargetMoving[targetIndex] == true && movingTarget[targetIndex] != null) {
			movingTarget[targetIndex] = GameObject.FindGameObjectWithTag(movingTargetTag[targetIndex]);
			minPosX [targetIndex] = movingTarget[targetIndex].transform.position.x - (positionOffset / 2.0f);
			maxPosX [targetIndex] = movingTarget[targetIndex].transform.position.x + (positionOffset / 2.0f);
			minPosY [targetIndex] = movingTarget[targetIndex].transform.position.y - (positionOffset / 2.0f);
			maxPosY [targetIndex] = movingTarget[targetIndex].transform.position.y + (positionOffset / 2.0f);

		}

		float dis = Vector2.Distance(new Vector2(target.transform.position.x,target.transform.position.y), new Vector2(((minPosX[targetIndex] + maxPosX[targetIndex])/2.0f),((minPosY[targetIndex] + maxPosY[targetIndex])/2.0f)));
		//print("dis: "+ dis + ", positionOffset: "+ positionOffset);
		return dis <= positionOffset;


	}


	public float getCurrentRotation(GameObject target){
		int targetIndex = getPlayerIndex (target);
		float curRot = target.transform.rotation.eulerAngles.z;
		curRot = getRotation360 (curRot);

		return curRot;
	}

	float getRotation360(float rot){
		if (rot < 180) {
			rot = 360 - rot;
		}

		return rot;
	}



	public bool isCorrectRotation(GameObject target,int index = -1){

		int targetIndex = (index == -1) ? getPlayerIndex (target) : index;
		if (isTargetMoving[targetIndex] == true) {
			movingTarget[targetIndex] = GameObject.FindGameObjectWithTag(movingTargetTag[targetIndex]);
			minRotation[targetIndex] = getCurrentRotation(movingTarget[targetIndex]);
		}


		if (target.GetComponent<PlayerController> ().curShape == PlayerController.Shape.CIRCLE) {
			return true;

		} else if (target.GetComponent<PlayerController> ().curShape == PlayerController.Shape.SQUARE) {

			float curRot = getCurrentRotation (target);
			float[] rotArr = { getRotation360(minRotation[targetIndex] % 360), getRotation360((minRotation[targetIndex] + 90)%360), getRotation360((minRotation[targetIndex] + 180)%360), getRotation360((minRotation[targetIndex] + 270)%360)};
			for(int i=0;i<rotArr.Length;i++){
				float diff = Mathf.Abs(curRot - rotArr[i]);
				if(diff <= rotOffset){
					return true;
				}

			}

			return false;
		} 
		else if (target.GetComponent<PlayerController> ().curShape == PlayerController.Shape.TRIANGLE) {
			float curRot = getCurrentRotation (target);
			float[] rotArr = { getRotation360(minRotation[targetIndex]), getRotation360((minRotation[targetIndex] + 120)%360),getRotation360((minRotation[targetIndex] + 240)%360)};
			for(int i=0;i<rotArr.Length;i++){
				float diff = Mathf.Abs(curRot - rotArr[i]);
				if(diff <= rotOffset){
					return true;
				}
				
			}
			
			return false;
			
		} 
		else if (target.GetComponent<PlayerController> ().curShape == PlayerController.Shape.CROSS) {

			float curRot = getCurrentRotation (target);
			float[] rotArr = { getRotation360(minRotation[targetIndex]), getRotation360((minRotation[targetIndex] + 90)%360), getRotation360((minRotation[targetIndex] + 180)%360), getRotation360((minRotation[targetIndex] + 270)%360), getRotation360((minRotation[targetIndex] + 360)%360)};
			for(int i=0;i<rotArr.Length;i++){
				float diff = Mathf.Abs(curRot - rotArr[i]);
				if(diff <= rotOffset){
					return true;
				}
				
			}

			return false;
		}
		else {
			float curRot = getCurrentRotation (target);
			float diff = Mathf.Abs(curRot - minRotation [targetIndex]);
			return (diff <= rotOffset);
		}





	}



	public bool isCorrectScale(GameObject target,int index = -1){

		int targetIndex = (index == -1) ? getPlayerIndex (target) : index;
		if (isTargetMoving[targetIndex] == true) {
			movingTarget[targetIndex] = GameObject.FindGameObjectWithTag(movingTargetTag[targetIndex]);
			minScale [targetIndex] = movingTarget[targetIndex].transform.localScale.x - scaleOffset;
			maxScale [targetIndex] = movingTarget[targetIndex].transform.localScale.x + scaleOffset;

		}


		return (target.transform.localScale.x >= minScale[targetIndex] && target.transform.localScale.x <= maxScale[targetIndex]);

	}



	public bool isCorrectShape(GameObject target,int index = -1){

		int targetIndex = (index == -1) ? getPlayerIndex (target) : index;
		return (target.GetComponent<PlayerController> ().curShape == shape[targetIndex]);
	
	}



	public void Snap(GameObject target, int index = -1){
		PlayerController playerController = target.GetComponent<PlayerController> ();
		if (playerController.snapState == PlayerController.SnapState.UnsnappedAndSnappable) {
			playerController.snapState = PlayerController.SnapState.SnappedAndUnsnappable;
			playerController.disablePlayer ();

			int targetIndex = (index == -1) ? getPlayerIndex (target) : index;
			Vector3 newPos = target.transform.position;
			newPos.x = snapX[targetIndex];
			newPos.y = snapY[targetIndex];
			target.transform.position = newPos;
			
			Vector3 newScale = target.transform.localScale;
			newScale.x = snapScale[targetIndex];
			newScale.y = snapScale[targetIndex];
			target.transform.localScale = newScale;
			
			Quaternion curRot = target.transform.rotation;
			curRot = snapRotation[targetIndex];
			target.transform.rotation = curRot;
		}

		 

	}
}
