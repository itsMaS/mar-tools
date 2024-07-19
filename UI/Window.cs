namespace MarTools
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using JetBrains.Annotations;
    using System.Linq;
    #if UNITY_EDITOR
    using UnityEditor;
    #endif
    
    public class Window : MonoBehaviour
    {
        public UnityEvent OnInitialized;
        public UnityEvent OnOpened;
        public UnityEvent OnClosed;
    
        [HideInInspector] public UIElement firstSelectedButton;
    
        public WindowManager manager { get; private set; }
    
        /// <summary>
        /// DO NOT USE THIS
        /// </summary>
        /// <param name="manager"></param>
        public void Open(WindowManager manager)
        {
            gameObject.SetActive(true);
    
            if(firstSelectedButton != null)
            {
                manager.UIManager.SelectButton(firstSelectedButton);
            }
            OnOpened.Invoke();
        }
    
        /// <summary>
        /// DO NOT USE THIS
        /// </summary>
        /// <param name="manager"></param>
        public void Close(WindowManager manager)
        {
            gameObject.SetActive(false);
            OnClosed.Invoke();
        }
    
        public void Close()
        {
            manager.CloseWindow(this);
        }
    
        public void Open()
        {
            manager.OpenWindow(this);
        }
    
        internal void Initialize(WindowManager windowManager)
        {
            manager = windowManager;
    
            gameObject.SetActive(false);
    
            OnInitialized.Invoke();
        }
    }
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(Window))]
    public class WindowEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var script = (Window)target;
    
            var buttons = script.GetComponentsInChildren<UIElement>(true).ToList();
            
            if(!Application.isPlaying && buttons.Count > 0)
            {
                int currentIndex = buttons.IndexOf(script.firstSelectedButton);
                if(currentIndex < 0)
                {
                    currentIndex = 0;
                }
    
                currentIndex = EditorGUILayout.Popup("First selected button", currentIndex, buttons.ConvertAll(item => item.gameObject.name).ToArray());
                script.firstSelectedButton = buttons[currentIndex];
            }
        }
    }
    #endif
    
}