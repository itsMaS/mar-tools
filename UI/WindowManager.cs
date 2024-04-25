using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JetBrains.Annotations;
using System.Linq;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif




[RequireComponent(typeof(UIManager))]
public class WindowManager : MonoBehaviour
{
    [HideInInspector] public Window startingWindow;

    List<Window> Windows;

    List<Window> OpenedWindows = new List<Window>();

    public UIManager UIManager { get; private set; }

    private void Awake()
    {
        UIManager = GetComponent<UIManager>();
        Windows = GetComponentsInChildren<Window>(true).ToList();

        foreach (Window w in Windows)
        {
            w.Initialize(this);
        }
    }

    private void Start()
    {
        if(startingWindow != null)
        {
            OpenWindow(startingWindow);
        }
    }

    public void OpenWindow(Window window)
    {
        CloseAllWindows();
        
        OpenedWindows.Add(window);
        window.Open(this);
    }

    public void CloseAllWindows()
    {
        foreach (var item in OpenedWindows)
        {
            CloseWindow(item);
        }
        OpenedWindows.Clear();
    }

    internal void CloseWindow(Window window)
    {
        window.Close(this);
    }
}

#if UNITY_EDITOR
[InitializeOnLoad]
public static class SceneVisibilityController
{
    static SceneVisibilityController()
    {
        Selection.selectionChanged += OnSelectionChanged;
    }

    private static void OnSelectionChanged()
    {
        var windowManager = GameObject.FindObjectOfType<WindowManager>(true);

        if (Selection.activeGameObject != null)
        {
            var obj = Selection.activeGameObject;
            
            if(obj.GetComponentInParent<WindowManager>())
            {
                var window = obj.GetComponentInParent<Window>();
                if(window != null)
                {
                    SceneVisibilityManager.instance.Hide(windowManager.gameObject, true);
                    SceneVisibilityManager.instance.Show(window.gameObject, true);
                }
                else
                {
                    SceneVisibilityManager.instance.ShowAll();
                }
            }
            else
            {
                SceneVisibilityManager.instance.ShowAll();
                SceneVisibilityManager.instance.Hide(windowManager.gameObject, true);
            }
        }
        else
        {
            SceneVisibilityManager.instance.ShowAll();
            SceneVisibilityManager.instance.Hide(windowManager.gameObject, true);
        }
    }
}

[CustomEditor(typeof(WindowManager))]
public class WindowManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var script = (WindowManager)target;
        var windows = script.GetComponentsInChildren<Window>(true);

        if(Application.isPlaying)
        {
            GUILayout.Label("Open window...");
            foreach( var win in windows )
            {
                if (GUILayout.Button(win.name))
                {
                    script.OpenWindow(win);
                }
            }
        }
        else
        {
            int currentIndex = windows.ToList().IndexOf(script.startingWindow);
            if(currentIndex < 0)
            {
                currentIndex = 0;
            }

            int newIndex = EditorGUILayout.Popup("Starting window", currentIndex, windows.ToList().ConvertAll(item => item.gameObject.name).ToArray());
            script.startingWindow = windows[newIndex];

            GUILayout.Label("Select window...");
            foreach (var win in windows)
            {
                if (GUILayout.Button(win.name))
                {
                    Selection.activeGameObject = win.gameObject;
                }
            }
        }
    }
}
#endif
