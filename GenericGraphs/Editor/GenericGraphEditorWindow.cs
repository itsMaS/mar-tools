using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class GenericGraphEditorWindow : EditorWindow
{
    [MenuItem("Window/Generic Graph")]
    public static void Open()
    {
        var window = GetWindow<GenericGraphEditorWindow>("Generic Graph");
    }

    GenericGraphView graphView;

    private void OnEnable()
    {
        SelectionChanged();

        BuildToolbar();
        Selection.selectionChanged += SelectionChanged;
    }
    private void OnDisable()
    {
        Selection.selectionChanged -= SelectionChanged;
    }

    private void SelectionChanged()
    {
        if(Selection.activeGameObject)
        {
            CreateGraph();
        }
        else if(graphView != null)
        {
            rootVisualElement.Remove(graphView);
            graphView = null;
        }
    }

    private void CreateGraph()
    {
        graphView = new GenericGraphView();
        graphView.StretchToParentSize();
        rootVisualElement.Insert(0, graphView);
    }

    protected virtual void BuildToolbar()
    {
        Toolbar toolbar = new Toolbar();

        toolbar.Add(new Button(() => Debug.Log("Test"))
        {
            text = "Refresh",
        });

        rootVisualElement.Add(toolbar);
    }
}
