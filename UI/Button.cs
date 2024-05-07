namespace MarTools
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using System.Linq;
    
    #if UNITY_EDITOR
    using UnityEditor;
    #endif
    
    public class Button : MonoBehaviour
    {
        public UnityEvent OnSelected;
        public UnityEvent OnDeselected;
        public UnityEvent OnClick;
    
        [HideInInspector] public Window navigatesTo;
    
        public void Select()
        {
            transform.localScale = Vector3.one * 1.1f;
    
            OnSelected.Invoke();
        }
    
        public void Deselect()
        {
            OnDeselected.Invoke();
            transform.localScale = Vector3.one;
        }
    
        public void Click()
        {
            if(navigatesTo != null)
            {
                GetComponentInParent<WindowManager>().OpenWindow(navigatesTo);
            }
            OnClick.Invoke();
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
    }
    #endif
}