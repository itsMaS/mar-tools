namespace MarTools.AI
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;

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

            public UnityEvent<IDetectable> OnFirstDetected;

            public float viewRadius = 10;
            public float viewAngle = 45f;
            public LayerMask obstructionMask;

            public float durationToDetect = 0.2f;

            public Dictionary<IDetectable,float> ObjectsInView = new Dictionary<IDetectable,float>();
            public List<IDetectable> Detected = new List<IDetectable>();

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

                    Vector3 toTarget = detectable.transform.position + Vector3.up*0.01f - origin.position;
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
                
                    if(!ObjectsInView.Keys.Contains(detectable))
                    {
                        ObjectsInView.Add(detectable, 0);
                        OnDetectStart.Invoke(detectable);


                    }
                    Debug.DrawLine(origin.position, detectable.transform.position, Color.red, Time.fixedDeltaTime);
                }

                foreach (var detectable in ObjectsInView.Keys.Except(NewObjectsInView).ToList())
                {
                    ObjectsInView.Remove(detectable);
                    if(Detected.Contains(detectable))
                    {
                        Detected.Remove(detectable);
                    }

                    OnDetectEnd.Invoke(detectable);
                }


                foreach (var visible in ObjectsInView.Keys.ToArray())
                {
                    ObjectsInView[visible] += Time.fixedDeltaTime;

                    if (ObjectsInView[visible] > durationToDetect && !Detected.Contains(visible))
                    {
                        Detected.Add(visible);
                        OnDetectStart.Invoke(visible);

                        if (Detected.Count == 1)
                        {
                            OnFirstDetected.Invoke(visible);
                        }
                    }
                }
            }

            public bool TryGetClosestInView<T>(out T found) where T : MonoBehaviour, IDetectable
            {
                found = null;

                if (ObjectsInView.Count == 0) return false;
                var casted = ObjectsInView.Where(x => x is T);

                if (casted.Count() == 0) return false;

                found = casted.FindClosest(transform.position, x => x.Key.transform.position, out float _) as T;
                return true;
            }
        }

    #if UNITY_EDITOR
        [CustomEditor(typeof(DetectorBehavior))]
        public class DetectorBehaviorEditor : MarToolsEditor<DetectorBehavior>
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                EditorGUI.BeginChangeCheck();

                GUILayout.Label("Transform used for calculations");
                GUILayout.BeginHorizontal();

                if(GUILayout.Button(script.overrideOrigin ? "Custom" : "Self"))
                {
                    script.overrideOrigin = !script.overrideOrigin;
                }

                if(script.overrideOrigin)
                {
                    script.originOverride = (Transform)EditorGUILayout.ObjectField(script.originOverride, typeof(Transform), true);
                }

                GUILayout.EndHorizontal();

                if(EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(script);
                }


                if(Application.isPlaying)
                {
                    GUILayout.Label($"Objects In View [{script.ObjectsInView.Count}]:");
                    foreach (var item in script.ObjectsInView)
                    {
                        GUILayout.Label($"-{item.Key.transform.gameObject.name} ({item.Value:2}s) [{(item.Value >= script.durationToDetect ? "DETECTED" : "NOT DETECTED")}]");
                    }
                }
            }

            private void OnSceneGUI()
            {
                if(script.enabled)
                {
                    script.origin.DrawVisibility(script.viewAngle, script.viewRadius, Color.red * new Color(1, 1, 1, script.ObjectsInView.Count > 0 ? 0.3f : 0.1f));
                }
            }
        }
    #endif
    }
}
