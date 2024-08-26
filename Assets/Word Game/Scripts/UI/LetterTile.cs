using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using MoreMountains.Tools;

namespace WordGame {

    /// <summary>
    /// Letter tile prefab to instantiate
    /// </summary>
    public class LetterTile : MonoBehaviour, MMEventListener<LetterColorEvent> {
        /// <summary>
        /// The tile's background image
        /// </summary>
        [field: Tooltip("The tile's background image")]
        [field: SerializeField] public Image Background { get; protected set; }
        /// <summary>
        /// The tile's text component
        /// </summary>
        [field: Tooltip("The tile's text component")]
        [field: SerializeField] public TextMeshProUGUI Text { get; protected set; }

        public WordContainer ParentContainer { get; protected set; }
        public TileType CurrentTileType { get; protected set; } = TileType.Neutral;

        protected Color _originalColor;
        protected Color _originalFontColor;

        protected virtual void Awake() {
            _originalColor = Background.color;
            _originalFontColor = Text.color;
        }

        public virtual void SetParent(WordContainer container) {
            ParentContainer = container;
        }

        /// <summary>
        /// Resets the tile's string and color. Do not reset string if is a keyboard tile.
        /// </summary>
        public virtual void ResetTile() {
            if (CompareTag("KeyboardButton")) {
                return;
            }
            Text.text = string.Empty;
            Text.color = _originalFontColor;
            Background.color = _originalColor;
            CurrentTileType = TileType.Neutral;
        }

        /// <summary>
        /// Set indirectly by GameManager when container is activated for the first time
        /// </summary>
        /// <param name="letter"></param>
        /// <param name="tileType"></param>
        public virtual void SetTile(string letter, TileType tileType) {
            Text.text = letter;
            switch (tileType) {
                case TileType.Wrong:
                    Background.color = GameManager.Instance.WrongColor;
                    if (GameManager.Instance.UseWhiteFont) {
                        Text.color = GameManager.Instance.WhiteFontColor;
                    }
                    CurrentTileType = TileType.Wrong;
                    break;
                case TileType.Right:
                    Background.color = GameManager.Instance.RightColor;
                    CurrentTileType = TileType.Right;
                    break;
                case TileType.Neutral:
                    Background.color = _originalColor;
                    CurrentTileType = TileType.Neutral;
                    break;
            }
        }

        /// <summary>
        /// Receive letter color event
        /// If applicable, color this tile and increment count in parent container
        /// </summary>
        /// <param name="letterColorEvent"></param>
        public void OnMMEvent(LetterColorEvent letterColorEvent) {
            if ((ParentContainer != null) && ParentContainer.CompareTag("CurrentWordContainer")) {
                return;
            }
            if (letterColorEvent.key.Equals(Text.text, System.StringComparison.OrdinalIgnoreCase)) {
                // If the tile is already colored, do nothing
                if (letterColorEvent.tileType == CurrentTileType) {
                    return;
                }

                if (letterColorEvent.tileType == TileType.Wrong) {
                    Background.color = GameManager.Instance.WrongColor;
                    if (GameManager.Instance.UseWhiteFont) {
                        Text.color = GameManager.Instance.WhiteFontColor;
                    }
                    CurrentTileType = TileType.Wrong;
                    if (ParentContainer != null) {
                        ParentContainer.IncrementWrongCount();
                    }
                }
                else {
                    Background.color = GameManager.Instance.RightColor;
                    CurrentTileType = TileType.Right;
                    if (ParentContainer != null) {
                        ParentContainer.IncrementMarkedCorrectCount();
                    }
                }
            }
        }

        protected virtual void OnEnable() {
            this.MMEventStartListening<LetterColorEvent>();
        }

        protected virtual void OnDisable() {
            this.MMEventStopListening<LetterColorEvent>();
        }
    }
}
