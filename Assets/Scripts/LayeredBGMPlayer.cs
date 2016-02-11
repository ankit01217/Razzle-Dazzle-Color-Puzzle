using UnityEngine;
using System.Collections;

public class LayeredBGMPlayer : MonoBehaviour {

    AudioSource[] layers = new AudioSource[15];

    int totalFadedIn = 0;

    public PuzzleTracker puzzleTracker;
    public float fadeInDuration = 1.0f;
    public float fadeOutDuration = 1.0f;

    private bool[] isFadingIn = new bool[15];
    private bool[] isFadingOut = new bool[15];
    private float[] fadeInEnd = new float[15];
    private float[] fadeOutStart = new float[15];
    private float[] fadeInElapsed = new float[15];
    private float[] fadeOutElapsed = new float[15];

    // Use this for initialization
    void Start() {
        AudioSource[] sources = GetComponents<AudioSource>();
        foreach (AudioSource source in sources) {
            layers[source.priority] = source;
        }
        for (int i = 0; i < 15; i++) {
            isFadingIn[i] = false;
            isFadingOut[i] = false;
            fadeInEnd[i] = 1.0f;
            fadeOutStart[i] = 1.0f;
            fadeInElapsed[i] = 0.0f;
            fadeOutElapsed[i] = 0.0f;
        }
        puzzleTracker.OnPuzzleCorrectEvent += PuzzleTracker_OnPuzzleCorrectEvent;
    }

    // Update is called once per frame
    void Update () {
        for (int i = 0; i < 15; i++) {
            if (isFadingIn[i]) {
                print("IS FADING IN");
                fadeInElapsed[i] = Mathf.Min(fadeInElapsed[i] + Time.deltaTime, fadeInDuration);
                layers[i].volume = (fadeInElapsed[i] / fadeInDuration) * fadeInEnd[i];
                if (fadeInElapsed[i] >= fadeInDuration) {
                    isFadingIn[i] = false;
                    fadeInElapsed[i] = 0.0f;
                }
            }
            if (isFadingOut[i]) {
                print("IS FADING OUT");
                fadeOutElapsed[i] = Mathf.Min(fadeOutElapsed[i] + Time.deltaTime, fadeOutDuration);
                layers[i].volume = ((fadeOutDuration - fadeOutElapsed[i]) / fadeOutDuration) * fadeOutStart[i];
                if (fadeOutElapsed[i] >= fadeOutDuration) {
                    isFadingOut[i] = false;
                    fadeOutElapsed[i] = 0.0f;
                }
            }
        }
	}

    void FadeInLayer(int index, float finalVolume) {
        print("FADING IN");
        if (isFadingIn[index] || isFadingOut[index]) {
            print("ATTEMPTED TO LAYER MULTIPLE TIMES - NOT SUPPORTED");
            return;
        }
        isFadingIn[index] = true;
        fadeInElapsed[index] = 0.0f;
        fadeInEnd[index] = finalVolume;
    }

    void FadeOutLayer(int index) {
        if (isFadingIn[index] || isFadingOut[index]) {
            print("ATTEMPTED TO LAYER MULTIPLE TIMES - NOT SUPPORTED");
            return;
        }
        isFadingOut[index] = true;
        fadeOutElapsed[index] = 0.0f;
        fadeOutStart[index] = layers[index].volume;
    }

    private void PuzzleTracker_OnPuzzleCorrectEvent(object sender, System.EventArgs e) {
        print("Fading in layer #" + puzzleTracker.currentPuzzleIndex);
        FadeInLayer(puzzleTracker.currentPuzzleIndex + 1, 0.3f);
        totalFadedIn++;
        if (puzzleTracker.currentPuzzleIndex == 7) {
            FadeOutLayer(1);
        }
    }

    public IEnumerator FinalMusic() {
        yield return new WaitForSeconds(7.0f);
        fadeOutDuration = 8.0f; // WARNING: Hardcoded change
        for (int i = 0; i < 15; i++) {
            if (i != 13 && i != 2 && i != 0 && i != 6 && i != 9 && i != 10 && i != 11)
                FadeOutLayer(i);
        }
    }

    public void FinalFadeout() {
        fadeOutDuration = 8.0f; // WARNING: Hardcoded change
        for (int i = 0; i < 15; i++) {
            FadeOutLayer(i);
        }
    }
}
