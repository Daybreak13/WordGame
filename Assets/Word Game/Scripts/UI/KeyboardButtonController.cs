using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WordGame {

    public class KeyboardButtonController : LetterTile {
        /// The button component for this keyboard button
        [field: Tooltip("The button component for this keyboard button")]
        [field: SerializeField] public Button ButtonComponent { get; protected set; }
        /// The text component for this keyboard button
        //[field: Tooltip("The text component for this keyboard button")]
        //[field: SerializeField] public TextMeshProUGUI Text { get; protected set; }

        /// The key that this button activates
        [field: Tooltip("The key that this button activates")]
        [field: SerializeField] public string Key { get; protected set; }

        public bool Enabled { get; protected set; } = true;

        protected bool _initialTextState;

        /// <summary>
        /// Get references
        /// </summary>
        protected override void Awake() {
            base.Awake();
            if (ButtonComponent == null) {
                ButtonComponent = GetComponentInChildren<Button>();
            }
            if (Text == null) {
                Text = GetComponentInChildren<TextMeshProUGUI>();
            }
            if (Key == null && Text != null) {
                Key = Text.text;
            }
            _initialTextState = Text.gameObject.activeSelf;
        }

        /// <summary>
        /// Used by editor button to set key names
        /// </summary>
        public virtual void SetKey() {
            Text.text = gameObject.name;
            Key = Text.text;
        }

        /// <summary>
        /// Resets the tile type and color
        /// </summary>
        public virtual void ResetKey() {
            Background.color = _originalColor;
            CurrentTileType = TileType.Neutral;
            EnableKey();
        }

        /// <summary>
        /// OnClick, press key
        /// </summary>
        public virtual void PressKey() {
            GameManager.Instance.PressKey(Key, this);
        }

        public virtual void EnableKey() {
            ButtonComponent.interactable = true;
            Text.gameObject.SetActive(_initialTextState);
            Enabled = true;
        }

        public virtual void DisableKey() {
            ButtonComponent.interactable = false;
            Text.gameObject.SetActive(false);
            Enabled = false;
        }
    }
}
