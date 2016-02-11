using UnityEngine;
using System.Collections;

public class LoadMain : MonoBehaviour {

    IEnumerator Start() {
        AsyncOperation async = Application.LoadLevelAsync("Main");
        async.allowSceneActivation = false;
        yield return async;
    }

}
