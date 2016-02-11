using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    public GameObject bvwImage, lettersImage, bgLayer2, fireworks, fader;
    public LayeredBGMPlayer layerPlayer;
    public GameObject puzzlesContainer;
	public int curState = 1;
	public const int maxState = 14;
	private PuzzleTracker puzzleTracker;
	private Animator cameraAnim;
	private Animator pathAnim;
	private float cameraIdleSpeed = 0;
	private float cameraNormalSpeed = 0.5f;
	private float cameraZoomSpeed = 0.9f;

	// Use this for initialization


	void Start () {

		lettersImage.SetActive (false);
		cameraAnim = GetComponent<Animator>();
		puzzleTracker = GameObject.FindObjectOfType<PuzzleTracker> ();
		pathAnim = bvwImage.GetComponent<Animator> ();

		cameraAnim.speed = cameraIdleSpeed;


	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			Application.LoadLevel("Title");
		}

	}

	public void enablePath(){
		pathAnim.SetTrigger("PathFadeIn");
	}

	public void disablePath(){
		pathAnim.SetTrigger("PathFadeOut");

	}

	public void pauseCamera(){
		cameraAnim.speed = cameraIdleSpeed;
		disablePath();
	}

	public void resuemCamera(){
		print("resuem camera");
		cameraAnim.speed = cameraNormalSpeed;
		curState++;
		//enablePath();

	}


	public void onEnterState(int state){
		print ("onEnterState: " + state);
		curState = state;
		pauseCamera();

		if (puzzleTracker != null) {
			puzzleTracker.ActivateControls();

		}



	}

	public void onExitState(int state){
		print ("onExitState: " + state);
		if (state == maxState) {
			cameraAnim.speed = cameraZoomSpeed;
		}
	}


	public void onZoomOutComplete(){
		print("onZoomOutComplete");
		pauseCamera();
		cameraAnim.StopPlayback ();
		bgLayer2.SetActive (true);
		lettersImage.SetActive (true);
		fireworks.SetActive(true);
		lettersImage.GetComponent<Animator> ().SetTrigger("LettersFadeIn");
		GameObject.FindGameObjectWithTag ("FinishedPuzzles").SetActive(false);
		AudioManager.Main.PlayNewSound ("Fireworks");
        StartCoroutine(layerPlayer.FinalMusic());
        StartCoroutine(FinalFadeout());
    }

    public IEnumerator FinalFadeout() {
        yield return new WaitForSeconds(15.0f);
        layerPlayer.FinalFadeout();
        fader.GetComponent<Animator>().SetTrigger("EndingFade");
    }
}
