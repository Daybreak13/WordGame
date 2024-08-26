using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WordGame {

    /// <summary>
    /// Manages main menu functions
    /// </summary>
    public class MainMenuManager : MonoBehaviour {
        /// <summary>
        /// Main canvas group
        /// </summary>
        [field: Tooltip("Main canvas group")]
        [field: SerializeField] public CanvasGroup MainCanvasGroup { get; protected set; }
        /// <summary>
        /// Tutorial canvas group
        /// </summary>
        [field: Tooltip("Tutorial canvas group")]
        [field: SerializeField] public CanvasGroup TutorialCanvasGroup { get; protected set; }

        protected virtual void Awake() {
            MainCanvasGroup.gameObject.SetActive(true);
            TutorialCanvasGroup.gameObject.SetActive(false);
        }

        public virtual void ToggleTutorial() {
            MainCanvasGroup.gameObject.SetActive(TutorialCanvasGroup.gameObject.activeSelf);
            TutorialCanvasGroup.gameObject.SetActive(!TutorialCanvasGroup.gameObject.activeSelf);
        }
    }
}
