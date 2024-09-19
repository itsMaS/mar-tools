#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Linq;

namespace HauntedPaws
{
    public class PuzzleElement : MonoBehaviour
    {
        [Serializable]
        public enum CompositeActivationType
        {
            AND = 0,
            OR = 1,
        }

        public bool activated { get; private set; } = false;

        public UnityEvent OnActivated;
        public UnityEvent OnDeactivated;

        [Tooltip("If all elements of this list is activated, this element is activated as well")]
        public List<PuzzleElement> CompositeActivation = new List<PuzzleElement>();
        public CompositeActivationType compositeActivationType = CompositeActivationType.AND;

        private void Awake()
        {
            if(CompositeActivation.Count > 0)
            {
                foreach (var item in CompositeActivation)
                {
                    item.OnActivated.AddListener(CheckSum);
                    item.OnDeactivated.AddListener(CheckSum);
                }
            }
        }

        private void CheckSum()
        {
            if ((compositeActivationType == CompositeActivationType.AND && CompositeActivation.TrueForAll(x => x.activated)) ||
                (compositeActivationType == CompositeActivationType.OR && CompositeActivation.Any(x => x.activated)))
            {
                if(!activated)
                    Activate();
            }
            else
            {
                if(activated)
                    Deactivate();
            }
        }

        public void Activate()
        {
            activated = true;
            OnActivated.Invoke();
        }

        public void Deactivate()
        {
            activated = false;
            OnDeactivated.Invoke();
        }

        public void Toggle()
        {
            if(activated)
            {
                Deactivate();
            }
            else
            {
                Activate();
            }
        }

        private void OnDrawGizmos()
        {
            foreach (var item in CompositeActivation)
            {
                Gizmos.color = item.activated ? Color.green : Color.red;
                Gizmos.DrawLine(transform.position, item.transform.position);
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(PuzzleElement))]
    public class PuzzleElementEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var script = (PuzzleElement)target;
            //GUI.color = script.activated ? Color.green : Color.red;

            Color prev = GUI.color;

            GUIStyle style = new GUIStyle(GUI.skin.textField);
            style.fontSize = 20;
            style.alignment = TextAnchor.MiddleCenter;
            GUI.color = script.activated ? Color.green : Color.red;
            GUILayout.Label($"{(script.activated ? "ACTIVATED" : "DEACTIVATED")}", style);

            GUI.color = prev;

            base.OnInspectorGUI();

            if(GUILayout.Button("Activate"))
            {
                script.Activate();
            }
            if(GUILayout.Button("Deactivate"))
            {
                script.Deactivate();
            }
        }
    }
#endif
}
