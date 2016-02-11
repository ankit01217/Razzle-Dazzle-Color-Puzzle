using UnityEngine;
using System.Collections.Generic;

public class ParticleManager : MonoBehaviour {

	private PlayerController player;
	public GameObject squareParticlesRoot;
	public GameObject circleParticlesRoot;
	public GameObject triangleParticlesRoot;
	public GameObject xParticlesRoot;
	private ParticleSystem[][] particlesByShape = new ParticleSystem[4][];
	public PlayerController.Shape currentBorderSystem;
	private bool isPlaying = false;

	// Use this for initialization
	void Start () {
		player = this.GetComponent<PlayerController> ();

		ParticleSystem[] squareParticles = squareParticlesRoot.GetComponentsInChildren<ParticleSystem> ();
		ParticleSystem[] circleParticles = circleParticlesRoot.GetComponentsInChildren<ParticleSystem> ();
		ParticleSystem[] triangleParticles = triangleParticlesRoot.GetComponentsInChildren<ParticleSystem> ();
		ParticleSystem[] xParticles = xParticlesRoot.GetComponentsInChildren<ParticleSystem> ();

		particlesByShape[(int)PlayerController.Shape.SQUARE] = squareParticles;
		particlesByShape[(int)PlayerController.Shape.CIRCLE] = circleParticles;
		particlesByShape[(int)PlayerController.Shape.TRIANGLE] = triangleParticles;
		particlesByShape[(int)PlayerController.Shape.CROSS] = xParticles;
	}

	public void ChangeBorderSystem(PlayerController.Shape shape, float r = 1, float g = 1, float b = 1, float a = 1) {
		if (isPlaying) {
			Stop();
		}
		foreach(ParticleSystem particleSystem in particlesByShape[(int)shape]) {
			particleSystem.startColor = new Color(r,g,b,a);
			this.currentBorderSystem = shape;
		}
		if (isPlaying) {
			Play();
		}
	}

	public void Play() {
		foreach(ParticleSystem particleSystem in particlesByShape[(int)currentBorderSystem]) {
			particleSystem.Play();
			isPlaying = true;
		}
	}

	public void Stop() {
		foreach(ParticleSystem particleSystem in particlesByShape[(int)currentBorderSystem]) {
			particleSystem.Stop();
			isPlaying = false;
		}
	}

	public bool IsPlaying() {
		return isPlaying;
	}

	public void Update() {

	}
}
