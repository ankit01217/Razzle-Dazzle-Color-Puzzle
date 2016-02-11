using UnityEngine;
using System.Collections;

public class DestroyParticles : MonoBehaviour {

	// Use this for initialization
	void Start () {

		LeanTween.delayedCall (3f,()=>{
			Destroy(gameObject);
		});
	}

	// Update is called once per frame
	void Update () {
	
	}
}
