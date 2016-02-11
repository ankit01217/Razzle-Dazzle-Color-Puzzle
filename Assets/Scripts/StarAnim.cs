using UnityEngine;
using System.Collections;

public class StarAnim : MonoBehaviour {

	// Use this for initialization
	void Start () {
		LeanTween.alpha (gameObject, 0, 0.7f).setDelay(Random.Range(0,2)).setLoopPingPong (-1);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
