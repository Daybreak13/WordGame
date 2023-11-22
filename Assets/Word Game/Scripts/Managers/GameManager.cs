using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WordGame.Tools;
using System.Text;
using MoreMountains.Tools;
using UnityEngine.EventSystems;
using System.IO;
using System.Linq;
using UnityEditor;
using TMPro;
using UnityEngine.InputSystem.XR;
using System.ComponentModel;
using UnityEngine.UI;

namespace WordGame {

    public enum TileType { Wrong, Right, Neutral }
    public enum ErrorType { Invalid, AlreadyGuessed }

    /// <summary>
    /// Game manager
    /// </summary>
    public class GameManager : Singleton<GameManager> {
        /// <summary>
        /// The current word solution length
        /// </summary>
        [field: Tooltip("The current word solution length")]
        [field: SerializeField] public int CurrentWordLength { get; protected set; } = 4;
        /// <summary>
        /// The error message container
        /// </summary>
        [field: Tooltip("The error message container")]
        [field: SerializeField] public CanvasGroup ErrorMessageContainer { get; protected set; }
        /// <summary>
        /// The error message text
        /// </summary>
        [field: Tooltip("The error message text")]
        [field: SerializeField] public TextMeshProUGUI ErrorMessageText { get; protected set; }
        /// <summary>
        /// The game won container
        /// </summary>
        [field: Tooltip("The game won container")]
        [field: SerializeField] public GameObject GameWonContainer { get; protected set; }
        /// <summary>
        /// The game won text
        /// </summary>
        [field: Tooltip("The game won text")]
        [field: SerializeField] public TextMeshProUGUI GameWonText { get; protected set; }

        [field: Header("Canvas Groups")]

        /// <summary>
        /// Keyboard canvas group
        /// </summary>
        [field: Tooltip("Keyboard canvas group")]
        [field: SerializeField] public CanvasGroup KeyboardCanvasGroup { get; protected set; }

        [field: Header("Error Messages")]

        /// <summary>
        /// The error message fader duration
        /// </summary>
        [field: Tooltip("The error message fader duration")]
        [field: SerializeField] public float ErrorMessageDuration { get; protected set; } = 2f;
        /// <summary>
        /// The error message text
        /// </summary>
        [field: Tooltip("The error message text")]
        [field: TextArea]
        [field: SerializeField] public string InvalidWordMessage { get; protected set; }
        /// <summary>
        /// The error message text
        /// </summary>
        [field: Tooltip("The error message text")]
        [field: TextArea]
        [field: SerializeField] public string AlreadyGuessedMessage { get; protected set; }

        [field: Header("Score")]

        /// <summary>
        /// The number of guesses
        /// </summary>
        [field: Tooltip("The number of guesses")]
        [field: MMReadOnly]
        [field: SerializeField] public int CurrentGuesses { get; protected set; }
        /// <summary>
        /// The number of guesses
        /// </summary>
        [field: Tooltip("The number of guesses")]
        [field: SerializeField] public TextMeshProUGUI CurrentGuessesText { get; protected set; }

        [field: Header("Word Containers")]

        /// <summary>
        /// The prefab for the word container
        /// </summary>
        [field: Tooltip("The prefab for the word container")]
        [field: SerializeField] public WordContainer WordContainerPrefab { get; protected set; }
        /// <summary>
        /// The container for the current word
        /// </summary>
        [field: Tooltip("The container for the current word")]
        [field: SerializeField] public WordContainer CurrentWordContainer { get; protected set; }
        /// <summary>
        /// The container for all word containers
        /// </summary>
        [field: Tooltip("The container for all word containers")]
        [field: SerializeField] public GameObject WordContainerContent { get; protected set; }
        /// <summary>
        /// The word container scroller
        /// </summary>
        [field: Tooltip("The word container scroller")]
        [field: SerializeField] public ScrollRect Scroller { get; protected set; }
        /// <summary>
        /// List of previous word containers
        /// </summary>
        [field: Tooltip("List of previous word containers")]
        [field: SerializeField] public List<WordContainer> PreviousWordContainers { get; protected set; } = new();

        [field: Header("Lists")]

        /// <summary>
        /// List of keyboard buttons
        /// </summary>
        [field: Tooltip("List of keyboard buttons")]
        [field: SerializeField] public List<KeyboardButtonController> KeyboardList { get; protected set; } = new();
        /// <summary>
        /// List of previously guessed words
        /// </summary>
        [field: Tooltip("List of previously guessed words")]
        [field: SerializeField] public List<string> PreviousWordList { get; protected set; } = new();
        /// <summary>
        /// List of valid words
        /// </summary>
        [field: Tooltip("List of valid words")]
        [field: SerializeField] public List<string> ValidWordsList { get; protected set; } = new();
        /// <summary>
        /// List of all words
        /// </summary>
        [field: Tooltip("List of all words")]
        [field: SerializeField] public List<string> AllWordsList { get; protected set; } = new();

        [field: Header("Current Word")]

        /// <summary>
        /// The current word
        /// </summary>
        [field: Tooltip("The current word")]
        [field: SerializeField] public string CurrentWord { get; protected set; }
        /// <summary>
        /// The color to use for incorrect letters
        /// </summary>
        [field: Tooltip("The color to use for incorrect letters")]
        [field: SerializeField] public Color WrongColor { get; protected set; }
        /// <summary>
        /// The color to use for correct letters
        /// </summary>
        [field: Tooltip("The color to use for correct letters")]
        [field: SerializeField] public Color RightColor { get; protected set; }

        [field: Header("File Paths")]

        [field: SerializeField] public string ValidWordsFileName { get; protected set; }
        [field: SerializeField] public string AllWordsFileName { get; protected set; }

        [field: Header("Debug")]

        [field: SerializeField] public bool DebugMode { get; protected set; }

        public Dictionary<string, KeyboardButtonController> KeyboardDictionary { get; protected set; } = new();
        public bool GameRunning { get; protected set; } = true;

        protected Dictionary<string, string> PromptWordsDictionary = new();
        protected Dictionary<string, string> AllWordsDictionary = new();

        protected StringBuilder _sb = new();
        protected int _correctLetterCount;
        protected Stack<KeyboardButtonController> _letterStack = new();
        protected int _currentContainerIndex;
        protected Coroutine _errorCoroutine;

        protected string _validWordsFilePath = "Assets/Word Game/Resources/ValidWordsList.txt";
        protected string _allWordsFilePath = "Assets/Word Game/Resources/AllWordsList.txt";

        protected virtual void Start() {
            Initialization();
        }

        /// <summary>
        /// Initialize lists, dictionaries, containers
        /// </summary>
        protected virtual void Initialization() {
            EventSystem.current.sendNavigationEvents = true;

            //_validWordsFilePath = Application.dataPath + "/Word Game/Resources/ValidWordsList.txt";
            //_allWordsFilePath = Application.dataPath + "/Word Game/Resources/AllWordsList.txt";
            _validWordsFilePath = Application.dataPath + "/Word Game/Resources/" + ValidWordsFileName;
            _allWordsFilePath = Application.dataPath + "/Word Game/Resources/" + AllWordsFileName;

            GetWordLists();

            foreach (string word in AllWordsList) {
                AllWordsDictionary.Add(word, word);
            }

            foreach (string word in ValidWordsList) {
                PromptWordsDictionary.TryAdd(word, word);
                AllWordsDictionary.TryAdd(word, word);
            }

            if (ErrorMessageContainer == null) {
                ErrorMessageContainer = ErrorMessageText.transform.parent.gameObject.GetComponent<CanvasGroup>();
            }
            ErrorMessageContainer.gameObject.SetActive(false);

            if (PreviousWordContainers.Count == 0) {
                GetWordContainers();
            }
            foreach (WordContainer container in PreviousWordContainers) {
                container.gameObject.SetActive(false);
            }

            foreach (KeyboardButtonController controller in KeyboardList) {
                KeyboardDictionary.Add(controller.Key, controller);
            }

            if (!DebugMode) {
                SetNewWord();
            }
        }

        /// <summary>
        /// Get the list of valid words from a text file, as well as all words dictionary
        /// </summary>
        public virtual void GetWordLists() {
            if (File.Exists(_validWordsFilePath)) {
                string[] lines = File.ReadAllLines(_validWordsFilePath);
                foreach (string line in lines) {
                    ValidWordsList.Add(line);
                }
            }
            if (File.Exists(_allWordsFilePath)) {
                string[] lines = File.ReadAllLines(_allWordsFilePath);
                foreach (string line in lines) {
                    AllWordsList.Add(line);
                }
            }
        }

        /// <summary>
        /// Set keyboard button settings
        /// Called via inspector button
        /// </summary>
        public virtual void SetKeyboardButtons() {
            foreach (KeyboardButtonController controller in KeyboardList) {
                controller.SetKey();
                EditorUtility.SetDirty(controller);
                EditorUtility.SetDirty(controller.Text);
            }
        }

        /// <summary>
        /// Get word containers
        /// </summary>
        public virtual void GetWordContainers() {
            PreviousWordContainers.Clear();
            foreach (Transform child in WordContainerContent.transform) {
                PreviousWordContainers.Add(child.GetComponent<WordContainer>());
            }
        }

        /// <summary>
        /// Populates list of keyboard buttons
        /// </summary>
        public virtual void PopulateKeyboardList() {
            KeyboardList.Clear();
            GameObject[] arr = GameObject.FindGameObjectsWithTag("KeyboardButton");
            foreach (GameObject item in arr) {
                KeyboardButtonController controller = item.GetComponent<KeyboardButtonController>();
                KeyboardList.Add(controller);
                //KeyboardDictionary.Add(controller.Key, controller);
            }
        }

        /// <summary>
        /// Called when a keyboard button is pressed
        /// </summary>
        /// <param name="key"></param>
        public virtual void PressKey(string key, KeyboardButtonController controller) {
            if (!GameRunning) {
                return;
            }
            if (key.Equals("ENT")) {
                EnterPressed();
            }
            else if (key.Equals("DEL")) {
                DeletePressed();
            }
            else {
                if (_sb.Length < CurrentWordLength) {
                    if (controller == null) {
                        controller = KeyboardDictionary[key];
                    }
                    if (!KeyboardDictionary[key].Enabled) {
                        return;
                    }
                    controller.DisableKey();
                    _sb.Append(key.ToLower());
                    _letterStack.Push(controller);
                    CurrentWordContainer.AddLetter(key);
                }
            }
        }

        /// <summary>
        /// ENT (enter) key pressed
        /// Guess the currently entered word
        /// </summary>
        protected virtual void EnterPressed() {
            string word = _sb.ToString();
            // If we do not have enough letters, return
            if ((_sb.Length != CurrentWordLength)) {
                return;
            }

            // If we have already guessed this word, return
            if (PreviousWordList.Contains(word)) {
                ThrowErrorMessage(ErrorType.AlreadyGuessed);
                return;
            }

            // If it is not a valid word, return
            if (!AllWordsDictionary.ContainsKey(word)) {
                ThrowErrorMessage(ErrorType.Invalid);
                return;
            }

            // If it is the solution, win the game and return
            if (CurrentWord.Equals(word, System.StringComparison.OrdinalIgnoreCase)) {
                CurrentGuessesText.text = (++CurrentGuesses).ToString();
                foreach (char c in word) {
                    LetterColorEvent.Trigger(c.ToString(), TileType.Right);
                }
                GameWon();
                return;
            }

            // Valid non-solution word entered
            ValidWordEntered(word);
        }

        /// <summary>
        /// Initialize a new word container containing the valid word that was just entered
        /// </summary>
        protected virtual void InitializePreviousWordContainer() {
            PreviousWordContainers[_currentContainerIndex].gameObject.SetActive(true);
            PreviousWordContainers[_currentContainerIndex].SetScore(_correctLetterCount);

            //for (int i = 0; i < CurrentWordContainer.Tiles.Count; i++) {
            //    string txt = CurrentWordContainer.Tiles[i].Text.text;   // Current letter
            //    PreviousWordContainers[_currentContainerIndex].Tiles[i].Text.text = txt;
            //    // Color the tile of the new word (if applicable)
            //    PreviousWordContainers[_currentContainerIndex].AddTile(txt, KeyboardDictionary[txt].CurrentTileType);
            //    if (KeyboardDictionary[txt].CurrentTileType == TileType.Wrong) {
            //        PreviousWordContainers[_currentContainerIndex].IncrementWrongCount();
            //    }
            //    if (KeyboardDictionary[txt].CurrentTileType == TileType.Right) {
            //        PreviousWordContainers[_currentContainerIndex].IncrementMarkedCount();
            //    }
            //}

            //_currentContainerIndex++;

            StartCoroutine(InitializePreviousWordContainerCo(_currentContainerIndex));
        }

        protected virtual IEnumerator InitializePreviousWordContainerCo(int idx) {
            StringBuilder sb = new();
            string str;
            foreach(LetterTile tile in CurrentWordContainer.Tiles) {
                sb.Append(tile.Text.text);
            }
            str = sb.ToString();

            yield return new WaitForEndOfFrame();

            int wrongCount = 0;
            int markedCount = 0;
            for (int i = 0; i < CurrentWordLength; i++) {
                string txt = str[i].ToString();   // Current letter
                PreviousWordContainers[_currentContainerIndex].Tiles[i].Text.text = txt;
                // Color the tile of the new word (if applicable)
                PreviousWordContainers[_currentContainerIndex].AddTile(txt, KeyboardDictionary[txt].CurrentTileType);
                if (KeyboardDictionary[txt].CurrentTileType == TileType.Wrong) {
                    //PreviousWordContainers[_currentContainerIndex].IncrementWrongCount();
                    wrongCount++;
                }
                if (KeyboardDictionary[txt].CurrentTileType == TileType.Right) {
                    //PreviousWordContainers[_currentContainerIndex].IncrementMarkedCount();
                    markedCount++;
                }
            }

            PreviousWordContainers[_currentContainerIndex].IncrementWrongCount(wrongCount);
            PreviousWordContainers[_currentContainerIndex].IncrementMarkedCount(markedCount);

            _currentContainerIndex++;
        }

        /// <summary>
        /// DEL (delete) key pressed
        /// Delete the last entered letter
        /// </summary>
        protected virtual void DeletePressed() {
            if (_sb.Length == 0) {
                return;
            }
            _sb.Remove(_sb.Length - 1, 1);
            KeyboardButtonController controller = _letterStack.Pop();
            controller.EnableKey();
            CurrentWordContainer.RemoveLetter();
        }

        /// <summary>
        /// Throw an error message
        /// Start fader coroutine
        /// </summary>
        /// <param name="errorType"></param>
        protected virtual void ThrowErrorMessage(ErrorType errorType) {
            if (_errorCoroutine != null) {
                StopCoroutine(_errorCoroutine);
            }
            switch (errorType) {
                case ErrorType.Invalid:
                    ErrorMessageText.text = InvalidWordMessage;
                    break;
                case ErrorType.AlreadyGuessed:
                    ErrorMessageText.text = AlreadyGuessedMessage;
                    break;
            }
            _errorCoroutine = StartCoroutine(ErrorMessageFader());
        }

        /// <summary>
        /// Fades the error message canvas group in, then out
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator ErrorMessageFader() {
            ErrorMessageContainer.alpha = 0f;
            ErrorMessageContainer.gameObject.SetActive(true);

            float elapsedTime = 0f;
            while (elapsedTime < 0.2f) {
                elapsedTime += Time.deltaTime;
                ErrorMessageContainer.alpha = Mathf.Lerp(0f, 1f, elapsedTime / 0.2f);
                yield return null;
            }
            ErrorMessageContainer.alpha = 1f;

            yield return new WaitForSeconds(ErrorMessageDuration);

            elapsedTime = 0f;
            while (elapsedTime < 1f) {
                elapsedTime += Time.deltaTime;
                ErrorMessageContainer.alpha = Mathf.Lerp(1f, 0f, elapsedTime / 1f);
                yield return null;
            }

            ErrorMessageContainer.gameObject.SetActive(false);
        }

        /// <summary>
        /// When game is won, set the Game Won container active
        /// </summary>
        protected virtual void GameWon() {
            KeyboardCanvasGroup.gameObject.SetActive(false);
            GameWonContainer.SetActive(true);
            GameRunning = false;
            PreviousWordList.Clear();
        }

        /// <summary>
        /// When a valid word is entered
        /// </summary>
        /// <param name="word"></param>
        protected virtual void ValidWordEntered(string word) {
            PreviousWordList.Add(word);

            // Valid non-solution word was entered
            // Check the word for same characters as the solution
            foreach (char c in word) {
                string s = c.ToString().ToUpper();
                if (CurrentWord.Contains(c) || CurrentWord.Contains(s)) {
                    _correctLetterCount++;
                    //LetterPressEvent.Trigger(c.ToString(), TileType.Right);
                }
                else {
                    //LetterPressEvent.Trigger(c.ToString(), TileType.Wrong);
                }
            }

            InitializePreviousWordContainer();

            // If all characters are wrong, color the tiles as the wrong color
            if (_correctLetterCount == 0) {
                foreach (char c in word) {
                    LetterColorEvent.Trigger(c.ToString(), TileType.Wrong);
                }
            }
            if (_correctLetterCount == 4) {
                foreach (char c in word) {
                    LetterColorEvent.Trigger(c.ToString(), TileType.Right);
                }
            }

            //CurrentWordContainer.ScoreText.text = _correctLetterCount.ToString();
            CurrentWordContainer.ResetContainer();

            ResetKeyboard(false);

            CurrentGuessesText.text = (++CurrentGuesses).ToString();
            StartCoroutine(ForceScrollDown());

            _correctLetterCount = 0;
        }

        protected virtual IEnumerator ForceScrollDown() {
            yield return new WaitForEndOfFrame();
            Canvas.ForceUpdateCanvases();
            Scroller.verticalNormalizedPosition = 0f;
            Canvas.ForceUpdateCanvases();
        }

        /// <summary>
        /// Re-enables all keys in the keyboard
        /// If true, also reset the keys (color, etc)
        /// </summary>
        protected virtual void ResetKeyboard(bool resetKeys) {
            _sb.Clear();
            while (_letterStack.Count > 0) {
                KeyboardButtonController controller = _letterStack.Pop();
                controller.EnableKey();
            }
            if (resetKeys) {
                foreach (KeyboardButtonController controller in KeyboardList) {
                    controller.ResetKey();
                }
            }
        }

        public virtual void ContinuePressed() {
            ContinueGame();
        }

        protected virtual void ContinueGame() {
            GameRunning = true;
            KeyboardCanvasGroup.gameObject.SetActive(true);
            GameWonContainer.SetActive(false);
            foreach (WordContainer container in PreviousWordContainers) {
                container.gameObject.SetActive(false);
                container.ResetContainer();
            }
            CurrentGuesses = 0;
            CurrentGuessesText.text = CurrentGuesses.ToString();

            _currentContainerIndex = 0;
            CurrentWordContainer.ResetContainer();
            SetNewWord();
            ResetKeyboard(true);
        }

        protected virtual void SetNewWord() {
            int idx = Random.Range(0, ValidWordsList.Count);
            CurrentWord = ValidWordsList[idx];
        }

    }
}
