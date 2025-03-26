namespace MarTools.AI
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Events;

    namespace HauntedPaws
    {
        public interface IDetectable
        {
            public Transform transform { get; }
        }

        public class DetectorBehavior : MonoBehaviour
        {
            public Transform origin => (overrideOrigin && originOverride != null) ? originOverride : transform;
            [HideInInspector] public bool overrideOrigin = false;
            [HideInInspector] public Transform originOverride = null;

            public UnityEvent<IDetectable> OnDetectStart;
            public UnityEvent<IDetectable> OnDetectEnd;

            public float viewRadius = 10;
            public float viewAngle = 45f;
            public LayerMask obstructionMask;

            public List<IDetectable> ObjectsInView = new List<IDetectable>();
            private void FixedUpdate()
            {
                if(!enabled)
                {
                    ObjectsInView.Clear();
                    return;
                }

                List<IDetectable> NewObjectsInView = new List<IDetectable>();

                foreach (var item in Physics.OverlapCapsule(origin.position - Vector3.up * 5, origin.position + Vector3.up * 5, viewRadius))
                {
                    IDetectable detectable = null;
                    if((item.attachedRigidbody && item.attachedRigidbody.TryGetComponent<IDetectable>(out detectable)) || item.TryGetComponent<IDetectable>(out detectable))
                    {

                    }

                    if (detectable == null) continue;

                    Vector3 toTarget = detectable.transform.position - origin.position;
                    Vector3 viewProjection = Vector3.ProjectOnPlane(origin.forward, Vector3.up);
                    Vector3 toTargetProjection = Vector3.ProjectOnPlane(toTarget, Vector3.up);
                    float angle = Vector3.Angle(viewProjection, toTargetProjection);

                    if (angle > viewAngle / 2)
                    {
                        Debug.DrawLine(origin.position, detectable.transform.position, Color.white * new Color(1, 1, 1, 0.05f), Time.fixedDeltaTime);
                        // Failure due to angle check
                        continue;
                    }

                    if (Physics.Raycast(origin.position, toTarget, out RaycastHit hit, toTarget.magnitude, obstructionMask))
                    {
                        Debug.DrawLine(origin.position, hit.point, Color.red * new Color(1, 1, 1, 0.5f), Time.fixedDeltaTime);
                        Debug.DrawLine(hit.point, detectable.transform.position, Color.white * new Color(1, 1, 1, 0.5f), Time.fixedDeltaTime);
                        // Failure due to obstruction
                        continue;
                    }

                    // Object is in view

                    NewObjectsInView.Add(detectable);
                
                    if(!ObjectsInView.Contains(detectable))
                    {
                        ObjectsInView.Add(detectable);
                        OnDetectStart.Invoke(detectable);
                    }
                    Debug.DrawLine(origin.position, detectable.transform.position, Color.red, Time.fixedDeltaTime);
                }

                foreach (var detectable in ObjectsInView.Except(NewObjectsInView).ToList())
                {
                    ObjectsInView.Remove(detectable);
                    OnDetectEnd.Invoke(detectable);
                }
            }

            public bool TryGetClosestInView<T>(out T found) where T : MonoBehaviour, IDetectable
            {
                found = null;

                if (ObjectsInView.Count == 0) return false;
                var casted = ObjectsInView.Where(x => x is T);

                if (casted.Count() == 0) return false;

                found = casted.FindClosest(transform.position, x => x.transform.position, out float _) as T;
                return true;
            }
        }

    #if UNITY_EDITOR
        [CustomEditor(typeof(DetectorBehavior))]
        public class DetectorBehaviorEditor : Editor
        {
            DetectorBehavior behavior;

            private void OnEnable()
            {
                behavior = (DetectorBehavior)target;
            }

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                EditorGUI.BeginChangeCheck();

                GUILayout.Label("Transform used for calculations");
                GUILayout.BeginHorizontal();

                if(GUILayout.Button(behavior.overrideOrigin ? "Custom" : "Self"))
                {
                    behavior.overrideOrigin = !behavior.overrideOrigin;
                }

                if(behavior.overrideOrigin)
                {
                    behavior.originOverride = (Transform)EditorGUILayout.ObjectField(behavior.originOverride, typeof(Transform), true);
                }

                GUILayout.EndHorizontal();

                if(EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(behavior);
                }
            }

            private void OnSceneGUI()
            {
                if(behavior.enabled)
                {
                    behavior.origin.DrawVisibility(behavior.viewAngle, behavior.viewRadius, Color.red * new Color(1, 1, 1, behavior.ObjectsInView.Count > 0 ? 0.3f : 0.1f));
                }
            }
        }
    #endif
    }
}
