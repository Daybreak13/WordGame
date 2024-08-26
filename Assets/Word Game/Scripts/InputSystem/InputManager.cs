using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace WordGame {

    /// <summary>
    /// Word game input manager
    /// </summary>
    public class InputManager : MonoBehaviour {
        [field: SerializeField] public bool UseKeyboardInputs { get; protected set; }

        public WordGameInputActions InputActions;

        protected IDisposable m_EventListener;
        protected bool _initialized;

        protected virtual void Awake() {
            InputActions = new();
        }

        protected virtual void Initialization() {
            _initialized = true;
        }

        /// <summary>
        /// Read any key press
        /// </summary>
        /// <param name="ctrl"></param>
        protected virtual void KeyPressed(InputControl ctrl) {
            //Debug.Log(ctrl.name);

            if (!UseKeyboardInputs) {
                return;
            }

            if (ctrl.name.Equals("backspace", StringComparison.OrdinalIgnoreCase) || ctrl.name.Equals("delete", StringComparison.OrdinalIgnoreCase)) {
                GameManager.Instance.PressKey("DEL", null);
            }

            if (ctrl.name.Equals("enter", StringComparison.OrdinalIgnoreCase) || ctrl.name.Equals("submit", StringComparison.OrdinalIgnoreCase)) {
                GameManager.Instance.PressKey("ENT", null);
            }

            // If the key pressed was a letter
            if (ctrl.name.Length == 1) {
                char[] charArray = ctrl.name.ToCharArray();
                if (char.IsLetter(charArray[0])) {
                    GameManager.Instance.PressKey(charArray[0].ToString().ToUpper(), null);
                }
            }
        }

        protected virtual void CancelPressed(InputAction.CallbackContext ctx) {
            
        }

        protected virtual void OnEnable() {
            InputActions.Enable();
            InputActions.UI.Cancel.performed += CancelPressed;
            m_EventListener = InputSystem.onAnyButtonPress.Call(KeyPressed);
        }

        protected virtual void OnDisable() {
            InputActions.Disable();
            InputActions.UI.Cancel.performed -= CancelPressed;
            m_EventListener.Dispose();
        }
    }
}
