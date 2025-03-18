using UnityEngine;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MarTools
{
    [ExecuteAlways]
    public class SplineTreeCameraSettings : MonoBehaviour
    {
        [System.Serializable]
        public class CameraSettings
        {
            [HideInInspector] public string guid;
            public Vector3 positionOffset;
            public Vector3 rotationOffset;
            public float addedPositionDamping = 0;
            public float addedRotationDamping = 0;
            public float splitScreenThresholdOffset = 0;

            [Range(0,1)] public float playerFollowFactor = 0;

            public Vector3 position { get; set; }
            public Quaternion rotation { get; set; }

            public Vector3 samplePoint { get; set; }

            public static CameraSettings GetInterpolated(CameraSettings A, CameraSettings B, float t)
            {
                return new CameraSettings() 
                { 
                    positionOffset = Vector3.Lerp(A.positionOffset, B.positionOffset, t), 
                    rotationOffset = Quaternion.Slerp(Quaternion.Euler(A.rotationOffset), Quaternion.Euler(B.rotationOffset), t).eulerAngles,
                    addedPositionDamping = Mathf.Lerp(A.addedPositionDamping, B.addedPositionDamping, t),
                    addedRotationDamping = Mathf.Lerp(A.addedRotationDamping, B.addedRotationDamping, t),
                    playerFollowFactor = Mathf.Lerp(A.playerFollowFactor, B.playerFollowFactor, t),
                    splitScreenThresholdOffset = Mathf.Lerp(A.splitScreenThresholdOffset, B.splitScreenThresholdOffset, t),
                };
            }
            public List<SplineTreeBehavior.SplineTreeNode> Nodes { get; set; }
        }

        public SplineTreeBehavior splineTree;
        public List<CameraSettings> NodeSettings = new List<CameraSettings>();

        public Vector3 globalPositionOffset;
        public Vector3 globalRotationOffset;
        public bool allowSplitscreen;

        private void Update()
        {
            if (!splineTree) return;
            Refresh();
        }

        private void Refresh()
        {
            if (!splineTree) return;

            List<CameraSettings> RemainingSettings = new List<CameraSettings>();
            foreach (var item in splineTree.Nodes)
            {
                var node = NodeSettings.Find(x => x.guid == item.id);
                if (node != null)
                {
                    RemainingSettings.Add(node);
                }
                else
                {
                    RemainingSettings.Add(new CameraSettings() { guid =  item.id });
                }
            }

            NodeSettings = RemainingSettings;
        }

        public CameraSettings GetSettingsAtPoint(Vector3 point)
        {
            Vector3 checkedPoint = point;
            float minDist = float.MaxValue;
            Vector3 closestPointTotal = Vector3.zero;
            float minProgress = 0;

            int indexA = 0;
            int indexB = 0;

            foreach (var connection in splineTree.GetConnections())
            {
                Vector3 closestPoint = Utilities.ClosestPointOnLineSegment(checkedPoint, connection.nodeA.position, connection.nodeB.position, out float progress);
                float sqrDistance = Vector3.SqrMagnitude(checkedPoint - closestPoint);
                if (sqrDistance < minDist)
                {
                    closestPointTotal = closestPoint;
                    minDist = sqrDistance;
                    minProgress = progress;

                    indexA = splineTree.Nodes.IndexOf(connection.nodeA);
                    indexB = splineTree.Nodes.IndexOf(connection.nodeB);
                }
            }

            var interpolated = CameraSettings.GetInterpolated(NodeSettings[indexA], NodeSettings[indexB], minProgress);
            Vector3 basePosition = Vector3.Lerp(splineTree.Nodes[indexA].position, splineTree.Nodes[indexB].position, minProgress);

            interpolated.position = basePosition + interpolated.positionOffset + globalPositionOffset;
            interpolated.rotation = (Quaternion.Euler(interpolated.rotationOffset) * Quaternion.Euler(globalRotationOffset));

            interpolated.samplePoint = closestPointTotal;

            interpolated.Nodes = new List<SplineTreeBehavior.SplineTreeNode>() { splineTree.Nodes[indexA], splineTree.Nodes[indexB] };

            return interpolated;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SplineTreeCameraSettings))]
    public class SplineTreeCameraSamplerEditor : Editor
    {
        public enum Tool
        {
            Position,
            Rotation,
            Settings,
        }

        Tool currentTool = Tool.Position;
        SplineTreeCameraSettings script;

        int selectedIndex = -1;

        private void OnEnable()
        {
            script = (SplineTreeCameraSettings)target;

            Tools.hidden = true;
        }

        private void OnDisable()
        {
            Tools.hidden = false;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            if(EditorGUI.EndChangeCheck())
            {
                if(selectedIndex >= 0)
                {
                    OnSceneGUI();
                }
            }

            if(GUILayout.Button("Reset overrides"))
            {
                script.NodeSettings.Clear();
            }


            EditorGUI.BeginChangeCheck();
            Undo.RecordObject(script, "");


            if(selectedIndex >= 0)
            {
                GUILayout.Space(20);
                GUILayout.Label($"Selected node index [{selectedIndex}]");

                script.NodeSettings[selectedIndex].addedPositionDamping = EditorGUILayout.FloatField("Position damping", script.NodeSettings[selectedIndex].addedPositionDamping);
                script.NodeSettings[selectedIndex].addedRotationDamping = EditorGUILayout.FloatField("Rotation damping", script.NodeSettings[selectedIndex].addedRotationDamping);
                script.NodeSettings[selectedIndex].splitScreenThresholdOffset = EditorGUILayout.FloatField("Splitscreen Threshold", script.NodeSettings[selectedIndex].splitScreenThresholdOffset);


                script.NodeSettings[selectedIndex].positionOffset = EditorGUILayout.Vector3Field("Position", script.NodeSettings[selectedIndex].positionOffset);
                script.NodeSettings[selectedIndex].rotationOffset = EditorGUILayout.Vector3Field("Rotation", script.NodeSettings[selectedIndex].rotationOffset);


                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Player Influence");
                script.NodeSettings[selectedIndex].playerFollowFactor = EditorGUILayout.Slider(script.NodeSettings[selectedIndex].playerFollowFactor, 0, 1);
                GUILayout.EndHorizontal();
            }

            if(EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(script);
            }   
        }

        private void OnSceneGUI()
        {
            if(Event.current.type == EventType.KeyDown)
            {
                if(Event.current.keyCode == KeyCode.W)
                {
                    currentTool = Tool.Position;
                    //Event.current.Use();
                }
                else if(Event.current.keyCode == KeyCode.E)
                {
                    currentTool = Tool.Rotation;
                    //Event.current.Use();
                }
                else if(Event.current.keyCode == KeyCode.R)
                {
                    currentTool = Tool.Settings;
                }
            }


            for (int i = 0; i < script.NodeSettings.Count; i++)
            {
                var camSettingsNode = script.NodeSettings[i];
                var splineNode = script.splineTree.Nodes[i];


                EditorGUI.BeginChangeCheck();
                Undo.RecordObject(script, "Move");
                Vector3 camPos = splineNode.position + camSettingsNode.positionOffset + script.globalPositionOffset;

                // Check if Unity is using Local or Global space
                bool isLocal = Tools.pivotRotation == PivotRotation.Local;

                switch (currentTool)
                {
                    case Tool.Position:
                        // Define the handle rotation space
                        Quaternion handleRotation = isLocal ? Quaternion.Euler(camSettingsNode.rotationOffset) : Quaternion.identity;
                        Vector3 newPos = Handles.PositionHandle(splineNode.position + camSettingsNode.positionOffset + script.globalPositionOffset, handleRotation);
                        camSettingsNode.positionOffset = newPos - splineNode.position - script.globalPositionOffset;
                        break;
                    case Tool.Rotation:
                        Quaternion newRot = Handles.RotationHandle(
                            Quaternion.Euler(script.globalRotationOffset) * Quaternion.Euler(camSettingsNode.rotationOffset),
                            camPos
                        );

                        // Correct order: Inverse(global) * newRot gives the local rotation.
                        camSettingsNode.rotationOffset =
                            (Quaternion.Inverse(Quaternion.Euler(script.globalRotationOffset)) * newRot).eulerAngles;

                        break;
                        case Tool.Settings:
                        SettingsTool();
                        break;
                    default:
                        break;
                }

                if(EditorGUI.EndChangeCheck())
                {
                    selectedIndex = i;
                    EditorUtility.SetDirty(script);
                }

                Handles.DrawDottedLine(script.splineTree.Nodes[i].position, camPos, 5f);
            }

            if(selectedIndex >= 0 && !Application.isPlaying)
            {
                UpdateFakeCamera(
                    script.splineTree.Nodes[selectedIndex].position + script.NodeSettings[selectedIndex].positionOffset + script.globalPositionOffset,
                    Quaternion.Euler(script.globalRotationOffset) * Quaternion.Euler(script.NodeSettings[selectedIndex].rotationOffset));
            }
            DrawCameraBounds(Camera.main);
        }

        private void SettingsTool()
        {
            for (int i = 0; i < script.NodeSettings.Count; i++)
            {
                var setting = script.NodeSettings[i];
                var node = script.splineTree.GetNode(setting.guid);

                Vector3 pos = node.position + setting.positionOffset + script.globalPositionOffset;

                float size = HandleUtility.GetHandleSize(pos) *0.2f;

                Handles.color = i == selectedIndex ? Color.green : Color.white;

                if(Handles.Button(pos, Quaternion.identity, size, size * 1.2f, Handles.SphereHandleCap))
                {
                    selectedIndex = i;
                }

                Handles.color = Color.white;
            }
        }

        private void UpdateFakeCamera(Vector3 position, Quaternion rotation)
        {
            Camera.main.transform.position = position;
            Camera.main.transform.rotation = rotation;
        }


        private void DrawCameraBounds(Camera cam)
        {
            Transform camTransform = cam.transform;

            // Get the four corners of the camera frustum
            Vector3[] nearCorners = new Vector3[4];
            Vector3[] farCorners = new Vector3[4];

            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);

            // Calculate frustum corners

            Handles.color = new Color(1, 1, 1, 0.4f);
            GetFrustumCorners(cam, nearCorners, farCorners);

            // Draw near plane
            Handles.DrawLine(nearCorners[0], nearCorners[1]);
            Handles.DrawLine(nearCorners[1], nearCorners[3]);
            Handles.DrawLine(nearCorners[3], nearCorners[2]);
            Handles.DrawLine(nearCorners[2], nearCorners[0]);

            // Draw far plane
            Handles.DrawLine(farCorners[0], farCorners[1]);
            Handles.DrawLine(farCorners[1], farCorners[3]);
            Handles.DrawLine(farCorners[3], farCorners[2]);
            Handles.DrawLine(farCorners[2], farCorners[0]);

            // Connect near and far planes
            for (int i = 0; i < 4; i++)
            {
                Handles.DrawLine(nearCorners[i], farCorners[i]);
            }
        }

        private void GetFrustumCorners(Camera cam, Vector3[] nearCorners, Vector3[] farCorners)
        {
            Transform camTransform = cam.transform;

            float nearDist = cam.nearClipPlane;
            float farDist = cam.farClipPlane;

            float aspect = cam.aspect;
            float fov = cam.fieldOfView * 0.5f;

            float nearHeight = 2f * Mathf.Tan(fov * Mathf.Deg2Rad) * nearDist;
            float nearWidth = nearHeight * aspect;

            float farHeight = 2f * Mathf.Tan(fov * Mathf.Deg2Rad) * farDist;
            float farWidth = farHeight * aspect;

            Vector3 nearCenter = camTransform.position + camTransform.forward * nearDist;
            Vector3 farCenter = camTransform.position + camTransform.forward * farDist;

            nearCorners[0] = nearCenter + (camTransform.up * nearHeight / 2f) - (camTransform.right * nearWidth / 2f);
            nearCorners[1] = nearCenter + (camTransform.up * nearHeight / 2f) + (camTransform.right * nearWidth / 2f);
            nearCorners[2] = nearCenter - (camTransform.up * nearHeight / 2f) - (camTransform.right * nearWidth / 2f);
            nearCorners[3] = nearCenter - (camTransform.up * nearHeight / 2f) + (camTransform.right * nearWidth / 2f);

            farCorners[0] = farCenter + (camTransform.up * farHeight / 2f) - (camTransform.right * farWidth / 2f);
            farCorners[1] = farCenter + (camTransform.up * farHeight / 2f) + (camTransform.right * farWidth / 2f);
            farCorners[2] = farCenter - (camTransform.up * farHeight / 2f) - (camTransform.right * farWidth / 2f);
            farCorners[3] = farCenter - (camTransform.up * farHeight / 2f) + (camTransform.right * farWidth / 2f);
        }
    }
#endif
}

