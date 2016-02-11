using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TitleScreenController : MonoBehaviour {

	public Text titleText;
	public GameObject fader;
	private bool isTriggered = false;
	// Use this for initialization
	void Start () {
		Cursor.visible = false;
		AudioManager.Main.PlayNewSound ("TitleMusic", true);
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Return)){
			StartMainScene();

		}


		int totTriggers = 0;
		for (int i=0;i<3; i++) {
			MoveController moveController = PSMoveInput.MoveControllers [i];
			if(PSMoveInput.IsConnected && moveController.Connected) {
				MoveData moveData = PSMoveInput.MoveControllers[i].Data;
				if(moveData.ValueT > 0){
					totTriggers++;
				}
			}
		}

		if (totTriggers >= 2 && isTriggered == false) {
			isTriggered = true;
			StartMainScene();

		}
	}

	void StartMainScene(){
		titleText.enabled = false;
		fader.GetComponent<Animator> ().SetTrigger ("FadeIn");
		LeanTween.delayedCall(1f,()=>{
			Application.LoadLevel("Main");
		});

	}
}
