using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WordGame {

    /// <summary>
    /// Component to make toggling canvases faster
    /// </summary>
    public class UICanvasManager : MonoBehaviour {
        /// Main canvas
        [field: Tooltip("Main canvas")]
        [field: SerializeField] public GameObject MainCanvas { get; protected set; }
        /// System canvas
        [field: Tooltip("System canvas")]
        [field: SerializeField] public GameObject SystemCanvas { get; protected set; }
    }
}
