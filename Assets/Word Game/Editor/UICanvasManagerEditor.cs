using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WordGame.Editors {

    /// <summary>
    /// Custom editor adding inspector buttons for UICanvasManager
    /// </summary>
    [CustomEditor(typeof(UICanvasManager))]
    public class UICanvasManagerEditor : Editor {
        private UICanvasManager canvasManager {
            get { return (UICanvasManager)target; }
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            DrawDefaultInspector();

            GUILayout.Space(10);

            if (GUILayout.Button("Toggle Canvases")) {
                canvasManager.MainCanvas.SetActive(!canvasManager.MainCanvas.activeSelf);
                canvasManager.SystemCanvas.SetActive(!canvasManager.MainCanvas.activeSelf);
            }

            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
                SceneView.RepaintAll();
            }
        }
    }
}
