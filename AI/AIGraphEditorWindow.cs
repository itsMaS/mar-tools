namespace MarTools.AI
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class AIGraphEditorWindow : EditorWindow
    {
        private AIGraphView graphView;
        private AIController controller;

        [MenuItem("MarTools/AI Editor")]
        public static void ShowWindow(AIController controller)
        {
            var window = GetWindow<AIGraphEditorWindow>("AI Editor");
            window.titleContent = new GUIContent(text: "Dialogue Graph");
            window.controller = controller;
        }

        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();

            var btn = new Button(() => graphView.Refresh()) { text = "Refresh" };
            toolbar.Add(btn);

            rootVisualElement.Add(toolbar);
        }

        private void OnEnable()
        {
            graphView = new AIGraphView()
            {
                name = "AI Graph",
            };

            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);

            GenerateToolbar();

            EditorApplication.update += UpdateEditor;
        }
        private void OnDisable()
        {
            rootVisualElement.Remove(graphView);
            EditorApplication.update -= UpdateEditor;
        }
        private void UpdateEditor()
        {
            graphView.Update();
        }
    }
}
