using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WordGame {

    public class SceneSelector : MonoBehaviour {
        /// The name/path of the scene to load
        [field: Tooltip("The name/path of the scene to load")]
        [field: SerializeField] public string SceneName { get; protected set; }
        /// Whether or not to use a loading screen
        [field: Tooltip("Whether or not to use a loading screen")]
        [field: SerializeField] public bool UseLoadingScreen { get; protected set; }

        public virtual void LoadScene() {
            if (UseLoadingScreen) {
                LoadingSceneManager.SceneToLoad = SceneName;
                SceneManager.LoadScene("LoadingScreen");
            }
            else {
                SceneManager.LoadScene(SceneName);
            }
        }
    }
}
