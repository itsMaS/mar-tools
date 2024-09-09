namespace MarTools 
{
    #if UNITY_EDITOR
    using UnityEditor;
    #endif
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using System;

    public class Trigger : MonoBehaviour
    {
        [SerializeReference]
        public IGameObjectConditional checkFunction = new CompareTag();

        public UnityEvent OnEntered;
        public UnityEvent OnExited;

        public UnityEvent OnComplete;
        public UnityEvent OnIncomplete;

        public int countRequiredToComplete = 1;
        [Tooltip("When enabled, objects do not trigger any logic associated with exiting after exiting the collider")]
        public bool dontTrackObjectExit = false;
        [Tooltip("When enabled, no more events are triggered after the initial completion")]
        public bool dontTrackAfterCompletion = false;

        public Color debugDisplayColor = new Color(0.0f, 0.8f, 0f, 0.6f);

        public List<GameObject> EnteredGameobjects { get; private set; } = new List<GameObject>();
        public bool completed { get; private set; } = false;

        private void OnTriggerEnter(Collider other)
        {
            if (completed && dontTrackAfterCompletion) return;

            if (!CheckObject(other)) return;

            if (!EnteredGameobjects.Contains(other.gameObject))
            {
                EnteredGameobjects.Add(other.gameObject);

                if(!completed && countRequiredToComplete >= EnteredGameobjects.Count)
                {
                    Complete();
                }
            }

            OnEntered.Invoke();
        }

        private void Complete()
        {
            completed = true;
            OnComplete.Invoke();
        }

        private void Incomplete()
        {
            completed = false;
            OnIncomplete.Invoke();
        }

        private void OnTriggerExit(Collider other)
        {
            if (completed && dontTrackAfterCompletion) return;

            if (dontTrackObjectExit) return;

            if (!CheckObject(other)) return;
        
            if(EnteredGameobjects.Contains(other.gameObject))
            {
                EnteredGameobjects.Remove(other.gameObject);

                if(completed && EnteredGameobjects.Count < countRequiredToComplete)
                {
                    Incomplete();
                }
            }

            OnExited.Invoke();
        }

        private bool CheckObject(Collider col)
        {
            return checkFunction != null ? checkFunction.IsTrue(col.gameObject) : true;
        }

        public bool CheckCompletion()
        {
            return EnteredGameobjects.Count > 0;
        }
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(Trigger))]
    [CanEditMultipleObjects]
    public class TriggerEditor : Editor
    {
        Trigger script;
        private void OnEnable()
        {
            script = (Trigger)target;
        }

        public override void OnInspectorGUI()
        {
            GUIStyle style = new GUIStyle(GUI.skin.button);

            Color col = GUI.color;

            style.fontSize = 30;
            GUI.color = script.completed ? Color.green : Color.red;
            GUILayout.Button(script.completed ? "Complete" : "Incomplete", style);

            GUI.color = col;
            if(Application.isPlaying)
            {
                GUI.enabled = false;
                EditorGUILayout.HelpBox("Entered objects:", MessageType.Info);
                foreach (var item in script.EnteredGameobjects)
                {
                    EditorGUILayout.ObjectField("go", item, typeof(GameObject));
                }
                GUI.enabled = true;
            }
            base.OnInspectorGUI();
        }
    }
    #endif
}
