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
#if UNITY_EDITOR
        [MenuItem("GameObject/Gameplay/Trigger", false, 1)]
        public static void CreateTrigger()
        {
            GameObject triggerGO = new GameObject("New Trigger");
            Trigger trigger = triggerGO.AddComponent<Trigger>();
            BoxCollider collider = triggerGO.AddComponent<BoxCollider>();
            collider.isTrigger = true;

            if (Selection.activeGameObject)
            {
                triggerGO.transform.parent = Selection.activeGameObject.transform;
            }


            Selection.activeGameObject = triggerGO;
            EditorGUIUtility.editingTextField = true;

            trigger.checkFunction = new CompareTag() { tag = "Player" };
        }
#endif



        [SerializeReference]
        public IGameObjectConditional checkFunction;

        public UnityEvent<GameObject> OnEntered;
        public UnityEvent<GameObject> OnEnteredFirst;
        public UnityEvent<GameObject> OnExited;
        public UnityEvent<GameObject> OnExitedAll;

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

            OnEntered.Invoke(other.gameObject);

            if(EnteredGameobjects.Count == 1)
            {
                OnEnteredFirst.Invoke(other.gameObject);
            }
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

            OnExited.Invoke(other.gameObject);

            if(EnteredGameobjects.Count == 0)
            {
                OnExitedAll.Invoke(other.gameObject);
            }
        }

        private bool CheckObject(Collider col)
        {
            return checkFunction != null ? checkFunction.IsTrue(col.gameObject) : true;
        }

        public bool CheckCompletion()
        {
            return EnteredGameobjects.Count > 0;
        }

        private void OnDrawGizmos()
        {
            Color col = Color.gray;
            if(Application.isPlaying)
            {
                col = completed ? Color.green : Color.gray;
            }

            col.a = 0.05f;
            Gizmos.color = col;

            if(TryGetComponent<BoxCollider>(out var box))
            {
                // Set the Gizmos color with transparency (RGBA)

                // Get the box collider's size and center relative to the GameObject's transform
                Vector3 boxSize = box.size;
                Vector3 boxCenter = box.center;

                // Apply the GameObject's local transform for correct positioning in the world space
                Gizmos.matrix = transform.localToWorldMatrix;

                // Draw a transparent wireframe box
                Gizmos.DrawWireCube(boxCenter, boxSize);

                // Draw a transparent solid box
                Gizmos.DrawCube(boxCenter, boxSize);
            }
            if (TryGetComponent<SphereCollider>(out var sphere))
            {
                // Set the Gizmos color with transparency (RGBA)
                // Get the sphere collider's radius and center relative to the GameObject's transform
                float sphereRadius = sphere.radius;
                Vector3 sphereCenter = sphere.center;

                // Apply the GameObject's local transform for correct positioning in the world space
                Gizmos.matrix = transform.localToWorldMatrix;

                // Draw a transparent wireframe sphere
                Gizmos.DrawWireSphere(sphereCenter, sphereRadius);

                // Draw a transparent solid sphere
                Gizmos.DrawSphere(sphereCenter, sphereRadius);
            }
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
