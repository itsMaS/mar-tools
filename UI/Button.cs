namespace MarTools
{
    using UnityEngine;
    using UnityEngine.Events;
    using System.Linq;
    
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.Events;
    using System;
#endif

    public class Button : UIElement
    {
        public UnityEvent OnClick;
        public UnityEvent<Vector2> OnUpdateCursorPositionNormalized;
        public UnityEvent<Vector2> OnUpdateCursorPositionPixel;
    
        [HideInInspector] public Window navigatesTo;

        protected override void OnSubmitedInternal()
        {
            base.OnSubmitedInternal();
            OnClick.Invoke();
            if (!gameObject.activeInHierarchy) return;


            if(navigatesTo != null)
            {
                GetComponentInParent<WindowManager>().OpenWindow(navigatesTo);
            }

            OnClick.Invoke();
        }

        internal void SetCursorPosition(Vector2 normalized, Vector3 pixel)
        {
            OnUpdateCursorPositionNormalized.Invoke(normalized);
            OnUpdateCursorPositionPixel.Invoke(pixel);
        }
    }
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(Button))]
    public class ButtonEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
    
            var script = (Button)target;
    

            var parentWindow = script.GetComponentInParent<Window>(true);

            if (parentWindow)
            {
                var windows = script.GetComponentInParent<WindowManager>(true).GetComponentsInChildren<Window>(true).Where(item => item != parentWindow).ToList();
                windows.Insert(0, null);
                if(!Application.isPlaying)
                {
                    int currentIndex = windows.IndexOf(script.navigatesTo);
                    if(currentIndex < 0)
                    {
                        currentIndex = 0;
                    }
    
                    currentIndex = EditorGUILayout.Popup("Navigates to", currentIndex, windows.ConvertAll(item => item != null ? item.gameObject.name : "None").ToArray());
                    script.navigatesTo = windows[currentIndex];
                }
            }
            else
            {
                script.navigatesTo = null;
            }
        }
    }
    #endif
}