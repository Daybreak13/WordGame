using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WordGame.Tools;
using System.Text;
using MoreMountains.Tools;
using UnityEngine.EventSystems;
using System.IO;
using TMPro;
using UnityEngine.InputSystem.XR;
using System.ComponentModel;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WordGame {

    public enum TileType { Wrong, Right, Neutral }
    public enum ErrorType { Invalid, AlreadyGuessed }

    /// <summary>
    /// Game manager
    /// </summary>
    public class GameManager : Singleton<GameManager>, MMEventListener<LetterColorEvent> {
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
        /// <summary>
        /// If true, use white font for red tiles
        /// </summary>
        [field: Tooltip("If true, use white font for red tiles")]
        [field: SerializeField] public bool UseWhiteFont { get; protected set; }
        /// <summary>
        /// The color font for red tiles
        /// </summary>
        [field: Tooltip("The color font for red tiles")]
        [field: SerializeField] public Color WhiteFontColor { get; protected set; }

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
        /// <summary>
        /// Tutorial canvas group
        /// </summary>
        [field: Tooltip("Tutorial canvas group")]
        [field: SerializeField] public CanvasGroup TutorialCanvasGroup { get; protected set; }

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
        /// If true, change the word container score color if all letters are wrong or right
        /// </summary>
        [field: Tooltip("If true, change the word container score color if all letters are wrong or right")]
        [field: SerializeField] public bool ChangeScoreColor { get; protected set; }
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
        [field: Tooltip("Set to true to use proper pathing")]
        [field: SerializeField] public bool UseAPKPaths { get; protected set; }
        /// If enabled, do not set a new word at start. Also, some info will be printed to console.
        [field: Tooltip("If enabled, do not set a new word at start. Also, some info will be printed to console.")]
        [field: SerializeField] public bool DebugMode { get; protected set; }

        /// Dictionary of all Keyboard buttons (KBC)
        public Dictionary<string, KeyboardButtonController> KeyboardDictionary { get; protected set; } = new();
        public bool GameRunning { get; protected set; } = true;

        protected Dictionary<string, string> PromptWordsDictionary = new();
        protected Dictionary<string, string> AllWordsDictionary = new();

        protected StringBuilder _sb = new();
        protected StringBuilder _guessesSb = new();
        protected List<string> _correctLetters = new();  // List of correctly guessed letters

        protected int _correctLetterCount;  // Correct letters in current guess
        protected Stack<KeyboardButtonController> _letterStack = new();
        protected int _currentContainerIndex;
        protected Coroutine _errorCoroutine;
        protected bool _gameWon;

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

            _validWordsFilePath = ValidWordsFileName;
            _allWordsFilePath = AllWordsFileName;
            //_validWordsFilePath = Application.dataPath + "/Word Game/Resources/" + ValidWordsFileName;
            //_allWordsFilePath = Application.dataPath + "/Word Game/Resources/" + AllWordsFileName;

            GetWordLists();

            foreach (string word in AllWordsList) {
                string newWord = word.ToUpper();
                AllWordsDictionary.Add(newWord, newWord);
            }

            foreach (string word in ValidWordsList) {
                string newWord = word.ToUpper();
                PromptWordsDictionary.TryAdd(newWord, newWord);
                AllWordsDictionary.TryAdd(newWord, newWord);
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

            //CurrentWordContainer.SetScore(0);
            CurrentWordContainer.ScoreText.text = string.Empty;
            foreach (LetterTile tile in CurrentWordContainer.Tiles) {
                tile.Text.text = string.Empty;
            }

            SystemCanvas.SetActive(false);
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
            TextAsset validWords = Resources.Load<TextAsset>(_validWordsFilePath);
            if (validWords != null) {
                string[] lines = validWords.text.Split("\n");
                foreach (string line in lines) {
                    ValidWordsList.Add(line.Trim().ToUpper());
                }
            }
            else {
                Debug.Log("Failed to load valid words");
            }
            TextAsset allWords = Resources.Load<TextAsset>(_allWordsFilePath);
            if (allWords != null) {
                string[] lines = allWords.text.Split("\n");
                foreach (string line in lines) {
                    AllWordsList.Add(line.Trim().ToUpper());
                }
            }
            else {
                Debug.Log("Failed to load all words");
            }
        }

#if UNITY_EDITOR

        /// <summary>
        /// Set keyboard button settings
        /// Called via inspector button
        /// </summary>
        public virtual void SetKeyboardButtons() {
            foreach (KeyboardButtonController controller in KeyboardList) {
                controller.SetKey();
                //EditorUtility.SetDirty(controller);
                //EditorUtility.SetDirty(controller.Text);
            }
        }

        /// <summary>
        /// Set names of keyboard button gameobjects
        /// Called via inspector button
        /// </summary>
        public virtual void SetKeyboardButtonName() {
            foreach (KeyboardButtonController controller in KeyboardList) {
                controller.gameObject.name = controller.Key;
                //EditorUtility.SetDirty(controller);
            }
        }

#endif

        /// <summary>
        /// Get word containers
        /// </summary>
        public virtual void GetWordContainers() {
            PreviousWordContainers.Clear();
            foreach (Transform child in WordContainerContent.transform) {
                PreviousWordContainers.Add(child.GetComponent<WordContainer>());
            }
            //EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// Sets the font of every TextMeshProUGUI element in the scene
        /// </summary>
        public virtual TextMeshProUGUI[] SetFont() {
            TextMeshProUGUI[] tmps = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (TextMeshProUGUI tmp in tmps) {
                tmp.font = TextFont;
            }
            return tmps;
        }

        /// <summary>
        /// Populates list of keyboard buttons
        /// Used in Editor via inspector button
        /// </summary>
        public virtual void PopulateKeyboardList() {
            KeyboardList.Clear();
            GameObject[] arr = GameObject.FindGameObjectsWithTag("KeyboardButton");
            foreach (GameObject item in arr) {
                KeyboardButtonController controller = item.GetComponent<KeyboardButtonController>();
                KeyboardList.Add(controller);
            }
            //EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// Called when a keyboard button is pressed
        /// The KBC component of a button will call this method when the button is pressed with a click
        /// Otherwise, InputManager reads a keyboard press and passes it to the GameManager
        /// </summary>
        /// <param name="key"></param>
        public virtual void PressKey(string key, KeyboardButtonController controller) {
            if (GameOverCanvasGroup.isActiveAndEnabled) {
                if (key.Equals("ENT")) {
                    ContinuePressed();
                }
                return;
            }
            if (key.Equals("ENT") || key.Equals("SUBMIT")) {
                EnterPressed();
            }
            else if (key.Equals("DEL")) {
                DeletePressed();
            }
            else if (key.Equals("MENU")) {
                OpenMenu();
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
                    _sb.Append(key);
                    _letterStack.Push(controller);
                    CurrentWordContainer.AddLetter(key);
                }
            }
        }

        /// <summary>
        /// Opens up the menu
        /// </summary>
        public virtual void OpenMenu() {
            GameRunning = false;
            SystemCanvas.SetActive(true);
        }

        public virtual void CloseMenu() {
            GameRunning = true;
            SystemCanvas.SetActive(false);

        }

        /// <summary>
        /// Toggles the tutorial page
        /// </summary>
        public virtual void ToggleTutorial() {
            TutorialCanvasGroup.gameObject.SetActive(!TutorialCanvasGroup.gameObject.activeSelf);
        }

        /// <summary>
        /// Player manually triggers a game over
        /// </summary>
        public virtual void GiveUp() {
            SystemCanvas.SetActive(false);
            GameOver(false);
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
                CurrentGuessesText.text = BuildCurrentGuessesText(CurrentGuesses);
                foreach (char c in word) {
                    LetterColorEvent.Trigger(c.ToString(), TileType.Right);
                }
                _gameWon = true;
                //GameOver(true);
                //return;
            }

            // Valid non-solution word entered
            ValidWordEntered(word);

            if (_gameWon) GameOver(true);
        }

        /// <summary>
        /// Initialize a new word container containing the valid word that was just entered
        /// </summary>
        protected virtual void InitializePreviousWordContainer() {
            PreviousWordContainers[_currentContainerIndex].gameObject.SetActive(true);
            PreviousWordContainers[_currentContainerIndex].SetScore(_correctLetterCount);

            //StartCoroutine(InitializePreviousWordContainerCo(_currentContainerIndex));

            StringBuilder sb = new();
            string str;
            foreach (LetterTile tile in CurrentWordContainer.Tiles) {
                sb.Append(tile.Text.text);
            }
            str = sb.ToString();

            int wrongCount = 0;
            int markedCount = 0;
            for (int i = 0; i < CurrentWordLength; i++) {
                string txt = str[i].ToString();   // Current letter
                PreviousWordContainers[_currentContainerIndex].Tiles[i].Text.text = txt;

                // Color the tiles of the new word container (if applicable)
                PreviousWordContainers[_currentContainerIndex].AddTile(txt, KeyboardDictionary[txt].CurrentTileType);
                if (KeyboardDictionary[txt].CurrentTileType == TileType.Wrong) {
                    wrongCount++;
                }
                if (KeyboardDictionary[txt].CurrentTileType == TileType.Right) {
                    markedCount++;
                }
            }

            PreviousWordContainers[_currentContainerIndex].IncrementWrongCount(wrongCount);
            PreviousWordContainers[_currentContainerIndex].IncrementMarkedCorrectCount(markedCount);

            _currentContainerIndex++;
        }

        /// <summary>
        /// Wait a frame to set up previous word container so that there are no conflicts with setting tile colors
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        protected virtual IEnumerator InitializePreviousWordContainerCo(int idx) {
            StringBuilder sb = new();
            string str;
            foreach (LetterTile tile in CurrentWordContainer.Tiles) {
                sb.Append(tile.Text.text);
            }
            str = sb.ToString();

            yield return new WaitForEndOfFrame();

            int wrongCount = 0;
            int markedCount = 0;
            for (int i = 0; i < CurrentWordLength; i++) {
                string txt = str[i].ToString();   // Current letter
                PreviousWordContainers[_currentContainerIndex].Tiles[i].Text.text = txt;

                // Color the tiles of the new word container (if applicable)
                PreviousWordContainers[_currentContainerIndex].AddTile(txt, KeyboardDictionary[txt].CurrentTileType);
                if (KeyboardDictionary[txt].CurrentTileType == TileType.Wrong) {
                    wrongCount++;
                }
                if (KeyboardDictionary[txt].CurrentTileType == TileType.Right) {
                    markedCount++;
                }
            }

            PreviousWordContainers[_currentContainerIndex].IncrementWrongCount(wrongCount);
            PreviousWordContainers[_currentContainerIndex].IncrementMarkedCorrectCount(markedCount);

            _currentContainerIndex++;
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
                // string s = c.ToString().ToUpper();
                if (CurrentWord.Contains(c)) {      //  || CurrentWord.Contains(s)
                    _correctLetterCount++;
                }
            }

            InitializePreviousWordContainer();

            // If all characters are wrong, color the tiles as the wrong color
            if (_correctLetterCount == 0) {
                foreach (char c in word) {
                    LetterColorEvent.Trigger(c.ToString(), TileType.Wrong);
                }
            }
            // If all characters are correct, color the tiles as right color
            if (_correctLetterCount == 4) {
                foreach (char c in word) {
                    LetterColorEvent.Trigger(c.ToString(), TileType.Right);
                }
            }

            CalculatePrevious(word);

            CurrentWordContainer.ResetContainer();

            ResetKeyboard(false);

            CurrentGuessesText.text = BuildCurrentGuessesText(++CurrentGuesses);
            StartCoroutine(ForceScrollDown());

            // If we have exceeded max guesses, trigger game over
            if (CurrentGuesses == MaxGuesses && !_gameWon) {
                GameOver(false);
            }

            _correctLetterCount = 0;
        }

        /// <summary>
        /// Go back to each previous word to determine if the newly inputted word allows us to mark any tiles
        /// </summary>
        protected virtual void CalculatePrevious(string word) {
            foreach (WordContainer container in PreviousWordContainers) {
                // Once we've hit the first inactive container, we're done
                if (!container.gameObject.activeSelf) {
                    break;
                }
                int diff = _correctLetterCount - container.CorrectLetterCount;
                HashSet<char> set1 = new(word);
                HashSet<char> set2 = new(container.StoredWord);
                set1.SymmetricExceptWith(set2);

                TileType t = (diff > 0) ? TileType.Right : TileType.Wrong;
                TileType t2 = (t == TileType.Right) ? TileType.Wrong : TileType.Right;
                // If the number of letters swapped out equals the change in score
                if (set1.Count / 2 == Mathf.Abs(diff)) {
                    foreach (char c in set1) {
                        if (word.Contains(c)) {
                            LetterColorEvent.Trigger(c.ToString(), t);
                        }
                        else {
                            LetterColorEvent.Trigger(c.ToString(), t2);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Forces the scroller to the bottom of the content
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator ForceScrollDown() {
            yield return new WaitForEndOfFrame();
            Scroller.verticalNormalizedPosition = 0f;
            Canvas.ForceUpdateCanvases();
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
        /// Re-enables stacked keys in the keyboard
        /// </summary>
        /// <param name="resetAll">If true, reset all keys (active state, color, tile type)</param>
        protected virtual void ResetKeyboard(bool resetAll) {
            _sb.Clear();

            // If the correct letter count is max, it means we are enabling only correct letters
            // Otherwise, we can reset the keyboard
            if (_correctLetters.Count == CurrentWordLength) {
                return;
            }
            
            if (resetAll) {
                foreach (KeyboardButtonController controller in KeyboardList) {
                    controller.ResetKey();
                }
                _letterStack.Clear();
            }
            else {
                while (_letterStack.Count > 0) {
                    KeyboardButtonController controller = _letterStack.Pop();
                    controller.EnableKey();
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
            _gameWon = false;
            CurrentWordContainer.ResetContainer();
            _correctLetters.Clear();
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

            _sb.Clear();
            _sb.Append(CurrentGuesses.ToString());
            if (CurrentGuesses == 1) {
                _sb.Append(" attempt");
            }
            else {
                _sb.Append(" attempts");
            }
            AttemptsText.text = _sb.ToString();
            //AttemptsText.text = AttemptsText.text.Remove(0, 1).Insert(0, CurrentGuesses.ToString());
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
            string newWord = CurrentWord;
            while (newWord.Equals(CurrentWord, System.StringComparison.OrdinalIgnoreCase)) {
                int idx = Random.Range(0, ValidWordsList.Count);
                newWord = ValidWordsList[idx];
            }
            CurrentWord = newWord;
        }

        /// <summary>
        /// Returns a string to put in current guesses text
        /// </summary>
        protected virtual string BuildCurrentGuessesText(int currentGuesses) {
            _guessesSb.Clear();
            _guessesSb.Append((currentGuesses).ToString());
            _guessesSb.Append("/");
            _guessesSb.Append(MaxGuesses.ToString());
            _guessesSb.Append(" ");
            _guessesSb.Append("ATTEMPTS");
            return _guessesSb.ToString();
        }

        /// <summary>
        /// Disable all keys in the keyboard except for the ones that have correct letters
        /// </summary>
        protected virtual void SetOnlyCorrectKeyboard() {
            foreach (KeyboardButtonController kbc in KeyboardList) {
                if (kbc.Key.Length == 1 && !_correctLetters.Contains(kbc.Key)) {
                    kbc.DisableKey();
                }
                else if (kbc.Key.Length == 1) {
                    kbc.EnableKey();
                }
            }
        }

        /// <summary>
        /// On letter color event, append to our correct letter list (if applicable)
        /// </summary>
        /// <param name="letterColorEvent"></param>
        public void OnMMEvent(LetterColorEvent letterColorEvent) {
            if ((letterColorEvent.tileType == TileType.Right) && !_correctLetters.Contains(letterColorEvent.key)) {
                _correctLetters.Add(letterColorEvent.key);
            }
            // If we've marked every correct letter, enable only the correct keys
            if (_correctLetters.Count == CurrentWordLength) {
                SetOnlyCorrectKeyboard();
            }
        }

        protected virtual void OnEnable() {
            this.MMEventStartListening<LetterColorEvent>();
        }

        protected virtual void OnDisable() {
            this.MMEventStopListening<LetterColorEvent>();
            StopAllCoroutines();
        }

        protected virtual void OnDestroy() {
            StopAllCoroutines();
        }

    }
}
