using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;

namespace WordGame {

    /// <summary>
    /// The object containing the word's letter tiles
    /// </summary>
    public class WordContainer : MonoBehaviour, MMEventListener<LetterColorEvent> {
        /// <summary>
        /// List of letter tiles for this word
        /// </summary>
        [field: Tooltip("List of letter tiles for this word")]
        [field: SerializeField] public Image ScoreImage { get; protected set; }
        /// <summary>
        /// How many correct letters are there
        /// </summary>
        [field: Tooltip("How many correct letters are there")]
        [field: SerializeField] public TextMeshProUGUI ScoreText { get; protected set; }
        /// <summary>
        /// List of letter tiles for this word
        /// </summary>
        [field: Tooltip("List of letter tiles for this word")]
        [field: SerializeField] public List<LetterTile> Tiles { get; protected set; } = new();

        public string StoredWord { get; protected set; }
        public int CorrectLetterCount { get; protected set; }

        protected StringBuilder _sb = new();
        protected int _currentIndex;
        protected int _wrongLetterCount;
        protected int _markedCorrectCount;
        protected Color _neutralColor;

        protected virtual void Awake() {
            Initialize();
        }

        /// <summary>
        /// Get tiles and set their parent
        /// </summary>
        public virtual void Initialize() {
            Tiles = GetComponentsInChildren<LetterTile>().ToList();
            foreach (LetterTile tile in Tiles) {
                tile.SetParent(this);
            }
            _neutralColor = ScoreImage.color;
        }

        /// <summary>
        /// Resets every tile in the container (clear the text, reset color)
        /// </summary>
        public virtual void ResetContainer() {
            foreach (LetterTile tile in Tiles) {
                tile.ResetTile();
            }
            _sb.Clear();
            StoredWord = string.Empty;
            ScoreText.text = string.Empty;
            ScoreImage.color = _neutralColor;
            _currentIndex = 0;
            CorrectLetterCount = 0;
            _wrongLetterCount = 0;
            _markedCorrectCount = 0;
        }

        /// <summary>
        /// Set the score (used when activating container)
        /// Change score image color (if applicable)
        /// </summary>
        /// <param name="score"></param>
        public virtual void SetScore(int score) {
            ScoreText.text = score.ToString();
            CorrectLetterCount = score;

            if (!GameManager.Instance.ChangeScoreColor) {
                return;
            }
            if (score == GameManager.Instance.CurrentWordLength) {
                ScoreImage.color = GameManager.Instance.RightColor;
            }
            if (score == 0) {
                ScoreImage.color = GameManager.Instance.WrongColor;
            }
        }

        /// <summary>
        /// Used for current word container (not previous words)
        /// </summary>
        /// <param name="letter"></param>
        public virtual void AddLetter(string letter) {
            Tiles[_currentIndex].Text.text = letter;
            _currentIndex++;
        }
        
        /// <summary>
        /// Used by previous word containers when GameManager sets them for the first time
        /// Sets the tile text and tile type
        /// </summary>
        /// <param name="letter"></param>
        /// <param name="tileType"></param>
        public virtual void AddTile(string letter, TileType tileType) {
            Tiles[_currentIndex].Text.text = letter;
            Tiles[_currentIndex].SetTile(letter, tileType);
            _currentIndex++;
            if (!CompareTag("CurrentWordContainer")) {
                _sb.Append(letter);
                if (_currentIndex == GameManager.Instance.CurrentWordLength) {
                    StoredWord = _sb.ToString();
                }
            }
        }

        /// <summary>
        /// Deletes the letter at the end of the string. Used in current word container.
        /// </summary>
        public virtual void RemoveLetter() {
            _currentIndex--;
            Tiles[_currentIndex].Text.text = string.Empty;
        }

        /// <summary>
        /// Called by child letter tile if the letter tile is marked as wrong
        /// If wrong letter count and correct letter count add up to total word length, mark the unknown correct tiles as correct
        /// </summary>
        public virtual void IncrementWrongCount(int increment = 1) {
            _wrongLetterCount += increment;
            if (_wrongLetterCount + CorrectLetterCount == GameManager.Instance.CurrentWordLength) {
                foreach (LetterTile tile in Tiles) {
                    if (tile.CurrentTileType == TileType.Neutral) {
                        LetterColorEvent.Trigger(tile.Text.text, TileType.Right);
                    }
                }

                if (_markedCorrectCount == CorrectLetterCount) {
                    foreach (LetterTile tile in Tiles) {
                        if (tile.CurrentTileType == TileType.Neutral) {
                            LetterColorEvent.Trigger(tile.Text.text, TileType.Wrong);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Increment number of marked correct tiles
        /// If marked correct count = the actual correct count, we know the rest of the tiles are wrong
        /// So mark the rest as wrong
        /// </summary>
        public virtual void IncrementMarkedCorrectCount(int increment = 1) {
            _markedCorrectCount += increment;
            if (_markedCorrectCount == CorrectLetterCount) {
                foreach (LetterTile tile in Tiles) {
                    if (tile.CurrentTileType == TileType.Neutral) {
                        LetterColorEvent.Trigger(tile.Text.text, TileType.Wrong);
                    }
                }
            }
        }

        /// <summary>
        /// After initialization, tile colors are changed only via event received by the LetterTile itself
        /// </summary>
        /// <param name="letterColorEvent"></param>
        public void OnMMEvent(LetterColorEvent letterColorEvent) {
            //if (gameObject.CompareTag("CurrentWordContainer")) {
            //    return;
            //}
            //if (letterColorEvent.tileType == TileType.Wrong) {
            //    StartCoroutine(LetterPressCo(letterColorEvent.key));
            //}
        }

        protected virtual void OnEnable() {
            this.MMEventStartListening<LetterColorEvent>();
        }

        protected virtual void OnDisable() { 
            this.MMEventStopListening<LetterColorEvent>();
        }
    }
}
