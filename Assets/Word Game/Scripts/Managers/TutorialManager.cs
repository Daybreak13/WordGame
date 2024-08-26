using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WordGame {

    /// <summary>
    /// Manages the tutorial page
    /// </summary>
    public class TutorialManager : MonoBehaviour {
        [field: Header("Tutorial")]

        /// <summary>
        /// Tutorial canvas group
        /// </summary>
        [field: Tooltip("Tutorial canvas group")]
        [field: SerializeField] public CanvasGroup TutorialCanvasGroup { get; protected set; }
        /// <summary>
        /// List of tutorial pages (ordered)
        /// </summary>
        [field: Tooltip("List of tutorial pages (ordered)")]
        [field: SerializeField] public List<GameObject> TutorialPages { get; protected set; }

        protected int _currentIndex;

        protected virtual void Awake() {
            // Deactivate all tutorial pages
            foreach (GameObject obj in TutorialPages) {
                obj.SetActive(false);
            }
        }

        /// <summary>
        /// Go back a page of tutorial
        /// </summary>
        public virtual void BackPage() {
            if (_currentIndex <= 0) {
                return;
            }

            TutorialPages[_currentIndex].SetActive(false);
            TutorialPages[--_currentIndex].SetActive(true);
        }

        /// <summary>
        /// Advance to next page of tutorial
        /// </summary>
        public virtual void NextPage() {
            if (_currentIndex >= TutorialPages.Count) {
                return;
            }

            TutorialPages[_currentIndex].SetActive(false);
            TutorialPages[++_currentIndex].SetActive(true);
        }
    }
}
