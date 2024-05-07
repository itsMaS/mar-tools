namespace MarTools
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using JetBrains.Annotations;
    using System.Linq;
    using System;
    using UnityEngine.InputSystem;
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
        public InputActionReference pauseButton;
        public Window pauseWindow;
    
        private void OnEnable()
        {
            pauseButton.action.performed += TogglePause;
        }
    
    
        private void OnDisable()
        {
            pauseButton.action.performed -= TogglePause;    
        }
        private void TogglePause(InputAction.CallbackContext obj)
        {
            if(OpenedWindows.Count == 0)
            {
                OpenWindow(pauseWindow);
            }
            else if(OpenedWindows.Exists(item => item == pauseWindow))
            {
                CloseAllWindows();
            }
        }
    
        private void Awake()
        {
            pauseButton.action.Enable();
    
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
    
        public void QuitGame()
        {
            Application.Quit();
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
    
            if (!windowManager) return;
    
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
    
}