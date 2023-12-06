using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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
            }

            if (GUILayout.Button("Set Keyboard Buttons")) {
                gameManager.SetKeyboardButtons();
            }

            if (GUILayout.Button("Get Word Containers")) {
                gameManager.GetWordContainers();
            }

            if (GUILayout.Button("Set Font")) {
                gameManager.SetFont();
            }

            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
                SceneView.RepaintAll();
            }
        }
    }
}
