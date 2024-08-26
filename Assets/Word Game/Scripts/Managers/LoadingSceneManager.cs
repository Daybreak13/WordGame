using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WordGame {

    /// <summary>
    /// Handles the loading screen
    /// Pass in SceneToLoad to load a specific scene
    /// </summary>
    public class LoadingSceneManager : MonoBehaviour {
        public static string SceneToLoad;

        protected virtual void Start() {
            SceneManager.LoadSceneAsync(SceneToLoad);
        }
    }
}
