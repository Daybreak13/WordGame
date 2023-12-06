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
        [field: Header("Game Parameters")]

        /// <summary>
        /// The current word
        /// </summary>
        [field: Tooltip("The current word")]
        [field: SerializeField] public string CurrentWord { get; protected set; }
        /// <summary>
        /// The number of guesses
        /// </summary>
        [field: Tooltip("The number of guesses")]
        [field: MMReadOnly]
        [field: SerializeField] public int CurrentGuesses { get; protected set; }
        /// <summary>
        /// The max number of guesses
        /// </summary>
        [field: Tooltip("The max number of guesses")]
        [field: SerializeField] public int MaxGuesses { get; protected set; } = 20;
        /// <summary>
        /// The current word solution length
        /// </summary>
        [field: Tooltip("The current word solution length")]
        [field: SerializeField] public int CurrentWordLength { get; protected set; } = 4;
        /// <summary>
        /// The font to use for all text
        /// </summary>
        [field: Tooltip("The font to use for all text")]
        [field: SerializeField] public TMP_FontAsset TextFont { get; protected set; }

        [field: Header("Color Coding")]

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

        [field: Header("Game Over")]

        /// <summary>
        /// The game over text
        /// </summary>
        [field: Tooltip("The game over text")]
        [field: SerializeField] public TextMeshProUGUI GameOverText { get; protected set; }
        /// <summary>
        /// The game solution text
        /// </summary>
        [field: Tooltip("The game solution text")]
        [field: SerializeField] public TextMeshProUGUI SolutionText { get; protected set; }
        /// <summary>
        /// The text displaying number of attempts this game
        /// </summary>
        [field: Tooltip("The text displaying number of attempts this game")]
        [field: SerializeField] public TextMeshProUGUI AttemptsText { get; protected set; }
        /// <summary>
        /// The game over message
        /// </summary>
        [field: Tooltip("The game over message")]
        [field: SerializeField] public string GameOverMessage { get; protected set; }
        /// <summary>
        /// The game won message
        /// </summary>
        [field: Tooltip("The game won message")]
        [field: SerializeField] public string GameWonMessage { get; protected set; }

        [field: Header("Canvases and Groups")]

        /// <summary>
        /// The system (menu) canvas
        /// </summary>
        [field: Tooltip("The system (menu) canvas")]
        [field: SerializeField] public GameObject SystemCanvas { get; protected set; }
        /// <summary>
        /// Keyboard canvas group
        /// </summary>
        [field: Tooltip("Keyboard canvas group")]
        [field: SerializeField] public CanvasGroup KeyboardCanvasGroup { get; protected set; }
        /// <summary>
        /// Game over canvas group
        /// </summary>
        [field: Tooltip("Game over canvas group")]
        [field: SerializeField] public CanvasGroup GameOverCanvasGroup { get; protected set; }
        /// <summary>
        /// The error message canvas group
        /// </summary>
        [field: Tooltip("The error message canvas group")]
        [field: SerializeField] public CanvasGroup ErrorMessageCanvasGroup { get; protected set; }

        [field: Header("Error Messages")]

        /// <summary>
        /// The error message text
        /// </summary>
        [field: Tooltip("The error message text")]
        [field: SerializeField] public TextMeshProUGUI ErrorMessageText { get; protected set; }
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

        [field: Header("File Paths")]

        [field: SerializeField] public string ValidWordsFileName { get; protected set; }
        [field: SerializeField] public string AllWordsFileName { get; protected set; }

        [field: Header("Debug")]

        /// If enabled, some info will be printed to console
        [field: Tooltip("If enabled, some info will be printed to console")]
        [field: SerializeField] public bool DebugMode { get; protected set; }

        public Dictionary<string, KeyboardButtonController> KeyboardDictionary { get; protected set; } = new();
        public bool GameRunning { get; protected set; } = true;

        protected Dictionary<string, string> PromptWordsDictionary = new();
        protected Dictionary<string, string> AllWordsDictionary = new();

        protected StringBuilder _sb = new();
        protected StringBuilder _guessesSb = new();
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

            if (ErrorMessageCanvasGroup == null) {
                ErrorMessageCanvasGroup = ErrorMessageText.transform.parent.gameObject.GetComponent<CanvasGroup>();
            }
            ErrorMessageCanvasGroup.gameObject.SetActive(false);

            if (PreviousWordContainers.Count == 0) {
                GetWordContainers();
            }
            foreach (WordContainer container in PreviousWordContainers) {
                container.gameObject.SetActive(false);
            }
            foreach (KeyboardButtonController controller in KeyboardList) {
                KeyboardDictionary.Add(controller.Key, controller);
            }

            GameOverCanvasGroup.gameObject.SetActive(false);

            CurrentGuessesText.text = BuildCurrentGuessesText(0);

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
            EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// Sets the font of every TextMeshProUGUI element in the scene
        /// </summary>
        public virtual void SetFont() {
            TextMeshProUGUI[] tmps = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (TextMeshProUGUI tmp in tmps) {
                tmp.font = TextFont;
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
            EditorUtility.SetDirty(this);
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
                CurrentGuessesText.text = BuildCurrentGuessesText(++CurrentGuesses);
                foreach (char c in word) {
                    LetterColorEvent.Trigger(c.ToString(), TileType.Right);
                }
                GameOver(true);
                return;
            }

            // Valid non-solution word entered
            ValidWordEntered(word);
        }

        /// <summary>
        /// Opens up the menu
        /// </summary>
        public virtual void OpenMenu() {
            SystemCanvas.SetActive(true);
        }

        public virtual void CloseMenu() {
            SystemCanvas.SetActive(false);

        }

        /// <summary>
        /// Player manually triggers a game over
        /// </summary>
        public virtual void GiveUp() {
            GameOver(false);
        }

        /// <summary>
        /// Initialize a new word container containing the valid word that was just entered
        /// </summary>
        protected virtual void InitializePreviousWordContainer() {
            PreviousWordContainers[_currentContainerIndex].gameObject.SetActive(true);
            PreviousWordContainers[_currentContainerIndex].SetScore(_correctLetterCount);

            StartCoroutine(InitializePreviousWordContainerCo(_currentContainerIndex));
        }

        /// <summary>
        /// Wait a frame to set up previous word container so that there are no conflicts with setting tile colors
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
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
            ErrorMessageCanvasGroup.alpha = 0f;
            ErrorMessageCanvasGroup.gameObject.SetActive(true);

            float elapsedTime = 0f;
            while (elapsedTime < 0.2f) {
                elapsedTime += Time.deltaTime;
                ErrorMessageCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / 0.2f);
                yield return null;
            }
            ErrorMessageCanvasGroup.alpha = 1f;

            yield return new WaitForSeconds(ErrorMessageDuration);

            elapsedTime = 0f;
            while (elapsedTime < 1f) {
                elapsedTime += Time.deltaTime;
                ErrorMessageCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / 1f);
                yield return null;
            }

            ErrorMessageCanvasGroup.gameObject.SetActive(false);
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

            CurrentGuessesText.text = BuildCurrentGuessesText(++CurrentGuesses);
            StartCoroutine(ForceScrollDown());

            // If we have exceeded max guesses, trigger game over
            if (CurrentGuesses == MaxGuesses) {
                GameOver(false);
            }

            _correctLetterCount = 0;
        }

        /// <summary>
        /// Forces the scroller to the bottom of the content
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// What happens when the continue button is pressed
        /// </summary>
        public virtual void ContinuePressed() {
            ContinueGame();
        }

        /// <summary>
        /// Continues the game
        /// </summary>
        protected virtual void ContinueGame() {
            GameRunning = true;
            KeyboardCanvasGroup.gameObject.SetActive(true);
            GameOverCanvasGroup.gameObject.SetActive(false);
            foreach (WordContainer container in PreviousWordContainers) {
                container.gameObject.SetActive(false);
                container.ResetContainer();
            }
            CurrentGuesses = 0;
            CurrentGuessesText.text = BuildCurrentGuessesText(0);

            _currentContainerIndex = 0;
            CurrentWordContainer.ResetContainer();
            SetNewWord();
            ResetKeyboard(true);
        }

        /// <summary>
        /// What to do when game ends
        /// If game was won, set the Game Won container active
        /// </summary>
        /// <param name="won">Was the game won?</param>
        protected virtual void GameOver(bool won) {
            EventSystem.current.SetSelectedGameObject(null);

            AttemptsText.text = AttemptsText.text.Remove(0, 1).Insert(0, CurrentGuesses.ToString());
            SolutionText.text = CurrentWord.ToUpper();
            GameOverCanvasGroup.gameObject.SetActive(true);
            GameOverCanvasGroup.gameObject.SetActive(true);
            GameRunning = false;
            PreviousWordList.Clear();

            // If we won
            if (won) {
                GameOverText.text = GameWonMessage;
            }
            // If we lost/gave up
            else {
                GameOverText.text = GameOverMessage;
            }
        }

        /// <summary>
        /// Gets and sets up a new word to guess
        /// </summary>
        protected virtual void SetNewWord() {
            int idx = Random.Range(0, ValidWordsList.Count);
            CurrentWord = ValidWordsList[idx];
        }

        /// <summary>
        /// Returns a string to put in current guesses text
        /// </summary>
        protected virtual string BuildCurrentGuessesText(int currentGuesses) {
            _guessesSb.Clear();
            _guessesSb.Append((currentGuesses).ToString());
            _guessesSb.Append("/");
            _guessesSb.Append(MaxGuesses.ToString());
            return _guessesSb.ToString();
        }

    }
}
