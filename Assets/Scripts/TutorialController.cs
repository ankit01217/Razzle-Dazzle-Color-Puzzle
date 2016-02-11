using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TutorialController : MonoBehaviour {

	public Sprite[] tutorialImages;
	public enum TutorialState{
		Move,
		Shape,
		Scale,
		Rotate,
		None

	}

	public TutorialState curState = TutorialState.Shape;
	private SpriteRenderer tutorialSP;
	private float moveTime = 1f;
	private bool isRaising = false;

	// Use this for initialization
	void Start () {

		tutorialSP = GetComponent<SpriteRenderer> ();
		UpdateTutorial (curState);
	
	}

	public void UpdateTutorial(TutorialState state){
		curState = state;
		if (curState == TutorialState.Shape) {
			tutorialSP.sprite = tutorialImages[1];
			tutorialSP.enabled = true;

		}	
		else if (curState == TutorialState.Move) {
			tutorialSP.sprite = tutorialImages[0];
			tutorialSP.enabled = true;

		}	
		else if (curState == TutorialState.Rotate) {
			tutorialSP.sprite = tutorialImages[3];
			tutorialSP.enabled = true;

		}	
		else if (curState == TutorialState.Scale) {
			tutorialSP.sprite = tutorialImages[2];
			tutorialSP.enabled = true;

		}	
		else {
			tutorialSP.enabled = false;

		}	

	}
	
	// Update is called once per frame
	void Update () {

		moveTime += Time.deltaTime;
		Vector3 pos = transform.position;
		if (moveTime >= 0.5f) {
			moveTime = 0;
			isRaising = !isRaising;
		}

		if(isRaising == false){
			pos.y -= 1 * Time.deltaTime;
		}
		else{
			pos.y += 1 * Time.deltaTime;
			
		}

		transform.position = pos;


	}
}
