using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WordGame.Editors {

    [CustomEditor(typeof(GameManager))]
    public class GameManagerEditor : Editor {
        private GameManager gameManager {
            get { return (GameManager)target; }
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            DrawDefaultInspector();

            GUILayout.Space(10);

            if (GUILayout.Button("Get Keyboard Buttons")) {
                gameManager.PopulateKeyboardList();
                EditorUtility.SetDirty(gameManager);
            }

            if (GUILayout.Button("Set Keyboard Buttons")) {
                gameManager.SetKeyboardButtons();
                foreach (KeyboardButtonController controller in gameManager.KeyboardList) {
                    EditorUtility.SetDirty(controller);
                }
                EditorUtility.SetDirty(gameManager);
            }

            if (GUILayout.Button("Set Keyboard Button Names")) {
                gameManager.SetKeyboardButtonName();
                foreach (KeyboardButtonController controller in gameManager.KeyboardList) {
                    EditorUtility.SetDirty(controller);
                }
                EditorUtility.SetDirty(gameManager);
            }

            if (GUILayout.Button("Get Word Containers")) {
                gameManager.GetWordContainers();
                EditorUtility.SetDirty(gameManager);
            }

            if (GUILayout.Button("Set Font")) {
                TextMeshProUGUI[] tmps = gameManager.SetFont();
                foreach (TextMeshProUGUI tmp in tmps) {
                    EditorUtility.SetDirty(tmp);
                }
            }

            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
                SceneView.RepaintAll();
            }
        }
    }
}
