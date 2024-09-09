#if UNITY_EDITOR
namespace MarTools
{
    using UnityEditor;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Codice.Client.BaseCommands;
    using UnityEngine.Events;

    public class GameVisualizerWindow : EditorWindow
    {
        public static bool displayTriggers
        {
            get
            {
                return EditorPrefs.GetBool("displayTriggers", true);
            }
            set
            {
                EditorPrefs.SetBool("displayTriggers", value);
            }
        }
        public static Color displayTriggersColor
        {
            get
            {
                return JsonUtility.FromJson<Color>(EditorPrefs.GetString("displayTriggerColor", JsonUtility.ToJson(Color.green)));
            }
            set
            {
                EditorPrefs.SetString("displayTriggerColor", JsonUtility.ToJson(value));
            }
        }

        [MenuItem("Window/Game Visualizer")]
        public static void OpenWindow()
        {
            // Create window instance
            GameVisualizerWindow window = GetWindow<GameVisualizerWindow>();
            window.titleContent = new GUIContent("Game Visualizer");
        }

        private void OnGUI()
        {
            displayTriggers = EditorGUILayout.Toggle("Display triggers", displayTriggers);
            displayTriggersColor = EditorGUILayout.ColorField("Diplay trigger color", displayTriggersColor);
        }

        void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        void OnSceneGUI(SceneView sceneView)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive)); // This line ensures your GUI handles are responsive

            Handles.color = displayTriggersColor;
            if(Selection.activeGameObject)
            {
                foreach (var item in Selection.activeGameObject.GetComponentsInChildren<Component>())
                {
                    if (item == null) continue;

                    foreach (var e in item.GetVariablesOfType<UnityEvent>())
                    {
                        if (e == null) continue;

                        for (int i = 0; i < e.GetPersistentEventCount(); i++)
                        {
                            var target = e.GetPersistentTarget(i);

                            Transform worldTransform;
                            if (target is Component)
                            {
                                Component targetComp = target as Component;
                                worldTransform = targetComp.transform;
                            }
                            else if(target is GameObject)
                            {
                                GameObject targetGameobject = target as GameObject;
                                worldTransform = targetGameobject.transform;
                            }
                            else
                            {
                                continue;
                            }
                            var methodName = e.GetPersistentMethodName(i);

                            GUIStyle style = new GUIStyle(GUI.skin.label);
                            style.alignment = TextAnchor.MiddleCenter;

                            Handles.DrawLine(item.transform.position, worldTransform.position, 5);
                            Handles.Label(worldTransform.position, $"{worldTransform.gameObject.name} ({target.GetType().Name}.{methodName})", style);
                        }

                    }
                }
            }



            if (!displayTriggers) return;

            Handles.color = displayTriggersColor;
            foreach (var item in GameObject.FindObjectsOfType<Trigger>())
            {





                if (item.gameObject == Selection.activeObject) continue;

                Collider col = item.GetComponent<Collider>();
                if(RenderCustomColliderGizmo(item, col, GizmoType.Selected))
                {
                    Selection.activeObject = item.gameObject;
                }
            }
        }
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        public static bool RenderCustomColliderGizmo(Trigger trigger, Collider collider, GizmoType gizmo)
        {
            if (collider == null)
                return false;

            bool isClicked = false;
            Color prevColor = Handles.color;
            Handles.color = trigger.completed ? trigger.debugDisplayColor : trigger.debugDisplayColor * new Color(1, 1, 1, 0.5f); // Green with transparency

            Event e = Event.current;
            Vector2 mousePosition = Event.current.mousePosition;

            if (collider is BoxCollider)
            {
                BoxCollider box = (BoxCollider)collider;
                isClicked = DrawBoxColliderGizmo(box, mousePosition, e);
            }
            else if (collider is SphereCollider)
            {
                SphereCollider sphere = (SphereCollider)collider;
                isClicked = DrawSphereColliderGizmo(sphere, mousePosition, e);
            }

            Handles.color = prevColor; // Restore the original color
            return isClicked;
        }

        private static bool DrawBoxColliderGizmo(BoxCollider box, Vector2 mousePosition, Event e)
        {
            Matrix4x4 oldMatrix = Handles.matrix;
            Handles.matrix = Matrix4x4.TRS(box.transform.TransformPoint(box.center), box.transform.rotation, Vector3.Scale(box.transform.lossyScale, box.size));
            bool isClicked = false;

            // Define box vertices
            Vector3[] verts = new Vector3[]
            {
            new Vector3(-0.5f, -0.5f, 0.5f),
            new Vector3(0.5f, -0.5f, 0.5f),
            new Vector3(0.5f, 0.5f, 0.5f),
            new Vector3(-0.5f, 0.5f, 0.5f),
            new Vector3(-0.5f, -0.5f, -0.5f),
            new Vector3(0.5f, -0.5f, -0.5f),
            new Vector3(0.5f, 0.5f, -0.5f),
            new Vector3(-0.5f, 0.5f, -0.5f)
            };

            if (Handles.Button(Vector3.zero, Quaternion.identity, 1, 1, Handles.CubeHandleCap))
            {
                Debug.Log("Box Collider Clicked");
                isClicked = true;
            }

            Handles.matrix = oldMatrix; // Restore the original matrix
            return isClicked;
        }

        private static bool DrawSphereColliderGizmo(SphereCollider sphere, Vector2 mousePosition, Event e)
        {
            bool isClicked = false;

            if (Handles.Button(sphere.transform.TransformPoint(sphere.center), Quaternion.identity, sphere.radius*2, sphere.radius*2, Handles.SphereHandleCap))
            {
                Debug.Log("Sphere Collider Clicked");
                isClicked = true;
            }

            return isClicked;
        }
    }
}
#endif
