using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MarTools
{
    public class SplineTreeBehavior : MonoBehaviour
    {
        [System.Serializable]
        public class SplineTreeNode
        {
            public string id;
            public List<string> Connections = new List<string>();
            public Vector3 position;

            public Vector3 GetWorldPosition()
            {
                return anchorTransform ? anchorTransform.TransformPoint(position) : position;
            }
            public Transform anchorTransform;

            public void GenerateNewId()
            {
#if UNITY_EDITOR
                this.id = GUID.Generate().ToString();
#endif
            }

            public int GetSplineHash(SplineTreeBehavior tree)
            {
                return tree.GetIslandHash(this);
            }
        }

        public class Connection
        {
            public SplineTreeNode nodeA;
            public SplineTreeNode nodeB;
        }

        public List<SplineTreeNode> Nodes = new List<SplineTreeNode>();

        private void OnDrawGizmos()
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                var node = Nodes[i];
                Gizmos.DrawSphere(node.GetWorldPosition(), 0.2f);

                foreach (var item in node.Connections)
                {

                    var targetNode = Nodes.Find(x => x.id == item);
                    //Debug.Log(node.id + "||" + targetNode.id);
                    
                    GizmosUtilities.DrawArrow(node.GetWorldPosition(), targetNode.GetWorldPosition(), 2);
                }
            }
        }

        public List<Connection> GetConnections()
        {
            List<Connection> Connections = new List<Connection>();

            foreach (var node in Nodes)
            {
                foreach (var c in node.Connections)
                {
                    var targetNode = Nodes.Find(x => x.id == c);

                    Connections.Add(new Connection() { nodeA = node, nodeB = targetNode });
                }
            }

            return Connections;
        }

        public SplineTreeBehavior.SplineTreeNode AddNode(Vector3 position)
        {
            var newNode = new SplineTreeNode()
            {
                position = position
            };

            newNode.GenerateNewId();
            Nodes.Add(newNode);

            return newNode;
        }

        private List<SplineTreeNode> GetNeighborNodes(SplineTreeNode initial)
        {
            List<SplineTreeNode> Added = new List<SplineTreeNode>();

            List<SplineTreeNode> TargetsThisNode = Nodes.Where(x => x.Connections.Contains(initial.id)).ToList();
            List<SplineTreeNode> TargettedByThisNode = Nodes.Where(x => initial.Connections.Contains(x.id)).ToList();

            Added.AddRange(TargetsThisNode);
            Added.AddRange(TargettedByThisNode);

            Added = Added.Distinct().ToList();

            return Added;
        }

        public List<SplineTreeNode> GetOutgoingNodes(SplineTreeNode initial)
        {
            List<SplineTreeNode> TargettedByThisNode = Nodes.Where(x => initial.Connections.Contains(x.id)).ToList();
            return TargettedByThisNode;
        }

        private List<SplineTreeNode> GetNetworkNodes(SplineTreeNode initial)
        {
            // List to hold all nodes in the network.
            List<SplineTreeNode> networkNodes = new List<SplineTreeNode>();

            // Use a HashSet to track visited node IDs to avoid duplicates.
            HashSet<string> visitedIds = new HashSet<string>();

            // Queue for BFS traversal.
            Queue<SplineTreeNode> queue = new Queue<SplineTreeNode>();

            // Start with the initial node.
            queue.Enqueue(initial);
            visitedIds.Add(initial.id);
            networkNodes.Add(initial);

            // Process nodes until the queue is empty.
            while (queue.Count > 0)
            {
                SplineTreeNode current = queue.Dequeue();
                List<SplineTreeNode> neighbors = GetNeighborNodes(current);

                foreach (var neighbor in neighbors)
                {
                    // Check if the neighbor has already been visited.
                    if (!visitedIds.Contains(neighbor.id))
                    {
                        visitedIds.Add(neighbor.id);
                        networkNodes.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return networkNodes;
        }

        private int GetIslandHash(SplineTreeNode node)
        {
            var hashCodes = GetNetworkNodes(node).Select(x => x.GetHashCode()).ToArray();
            Array.Sort(hashCodes);

            int hash = 17;
            foreach (int code in hashCodes)
            {
                hash = hash * 31 + code;
            }
            return hash;
        }


        internal void RemoveNode(SplineTreeNode node)
        {
            Nodes.Remove(node);
            foreach (var item in Nodes)
            {
                item.Connections.Remove(node.id);
            }
        }

        public SplineTreeBehavior.SplineTreeNode GetNode(string guid)
        {
            return Nodes.Find(x => x.id == guid);
        }

        public Vector3 GetClosestPointOnNetwork(Vector3 point)
        {
            Vector3 checkedPoint = point;
            float minDist = float.MaxValue;
            Vector3 closestPointTotal = Vector3.zero;
            float minProgress = 0;

            foreach (var connection in GetConnections())
            {
                Vector3 closestPoint = Utilities.ClosestPointOnLineSegment(checkedPoint, connection.nodeA.GetWorldPosition(), connection.nodeB.GetWorldPosition(), out float progress);
                float sqrDistance = Vector3.SqrMagnitude(checkedPoint - closestPoint);
                if (sqrDistance < minDist)
                {
                    closestPointTotal = closestPoint;
                    minDist = sqrDistance;
                    minProgress = progress;
                }
            }

            return closestPointTotal;
        }

        public void Connect(SplineTreeNode nodeA, SplineTreeNode nodeB)
        {
            if(nodeA.Connections.Contains(nodeB.id) || nodeA == nodeB) return;
            nodeA.Connections.Add(nodeB.id);
        }

        internal void Disconnect(SplineTreeNode nodeA, SplineTreeNode nodeB)
        {
            if(!nodeA.Connections.Contains(nodeB.id)) return;
            nodeA.Connections.Remove(nodeB.id);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SplineTreeBehavior))]
    public class SplineTreeBehaviorEditor : Editor
    {
        SplineTreeBehavior script;
        Vector3 worldCursorPosition;
        List<SplineTreeBehavior.SplineTreeNode> SelectedNodes = new List<SplineTreeBehavior.SplineTreeNode>();

        private Transform selectedParent;

        EditorPrefToggle tutorialEnabled = new EditorPrefToggle("Enabled", false);
        private void OnEnable()
        {
            script = (SplineTreeBehavior)target;
            Tools.hidden = true;

            EditorApplication.update += UpdateSceneGUI;

        }
        private void OnDisable()
        {
            Tools.hidden = false;
            EditorApplication.update -= UpdateSceneGUI;
        }
        private void UpdateSceneGUI()
        {
            SceneView.RepaintAll();
        }

        private void UpdateWorldCursorPosition()
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if(Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, LayerMask.GetMask("Ground"), QueryTriggerInteraction.Ignore))
            {
                worldCursorPosition = hitInfo.point;
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            Undo.RecordObject(script, "SplineTreeBehavior");

            GUILayout.Label("Operations", EditorStyles.boldLabel);
            ///
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear points"))
            {
                script.Nodes.Clear();
            }
            if(GUILayout.Button("Select all nodes"))
            {
                SelectedNodes.Clear();
                SelectedNodes.AddRange(script.Nodes);
            }
            if (GUILayout.Button("Deselect all nodes"))
            {
                SelectedNodes.Clear();
            }
            GUILayout.EndHorizontal();
            ///

            if(SelectedNodes.Count > 0)
            {
                GUILayout.BeginHorizontal();

                var newTransform = EditorGUILayout.ObjectField(selectedParent, typeof(Transform)) as Transform;
                selectedParent = newTransform;

                if (GUILayout.Button("Assign parent to selected"))
                {
                    if(selectedParent)
                    {
                        foreach (var item in SelectedNodes)
                        {
                            Vector3 offset = selectedParent.InverseTransformPoint(item.position);

                            item.position = offset;
                            item.anchorTransform = selectedParent;
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(20);
            GUILayout.Label($"Selected nodes {SelectedNodes.Count}/{script.Nodes.Count}", EditorStyles.boldLabel);
            GUILayout.Space(5);


            foreach (var item in SelectedNodes)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{item.id}");
                var newTransform = EditorGUILayout.ObjectField(item.anchorTransform, typeof(Transform)) as Transform;

                if(newTransform != item.anchorTransform)
                {
                    Vector3 newPos = item.GetWorldPosition();

                    if(newTransform)
                    {
                        newPos = newTransform.InverseTransformPoint(newPos);
                    }

                    item.position = newPos;
                }

                item.anchorTransform = newTransform;

                GUILayout.EndHorizontal();
            }

            if(EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(script);
            }
        }

        private void OnSceneGUI()
        {
            EditorGUI.BeginChangeCheck();
            Undo.RecordObject(script, "SplineTreeBehavior");
            Undo.RecordObject(this, "Editor");

            SelectedNodes.RemoveAll(x => !script.Nodes.Contains(x));

            UpdateWorldCursorPosition();

            
            if(!Event.current.control)
            {
                foreach (var item in script.Nodes)
                {
                    bool selected = SelectedNodes.Contains(item);

                    Handles.color = selected ? Color.green : Color.white;

                    if (selected && (Tools.current == Tool.Move && !Event.current.shift)) continue;

                    Vector3 buttonPosition = item.GetWorldPosition(); 
                    float buttonSize = HandleUtility.GetHandleSize(buttonPosition) * 0.5f;
                    buttonSize = Mathf.Min(buttonSize, 1.5f);


                    if (Handles.Button(buttonPosition, Quaternion.identity, selected ? buttonSize*1.1f : buttonSize, buttonSize, Handles.SphereHandleCap))
                    {
                        if(Event.current.shift)
                        {
                            if(SelectedNodes.Contains(item))
                            {
                                SelectedNodes.Remove(item);
                            }
                            else
                            {
                                SelectedNodes.Add(item);
                            }
                        }
                        else
                        {
                            SelectedNodes.Clear();
                            SelectedNodes.Add(item);
                        }

                        Repaint();
                    }
                }
            }

            if (Tools.current == Tool.Move && !Event.current.shift && !Event.current.control)
            {
                foreach (var item in SelectedNodes)
                {
                    Vector3 newPos = Handles.PositionHandle(item.GetWorldPosition(), Quaternion.identity);
                    if (newPos != item.GetWorldPosition())
                    {
                        item.position = item.anchorTransform ? item.anchorTransform.InverseTransformPoint(newPos) : newPos;
                    }
                }

            }

            var closestNodeToCursor = script.Nodes.Count == 0 ? null : script.Nodes.FindClosest(worldCursorPosition, x => x.GetWorldPosition(), out float dist);

            if (Event.current.alt)
            {
                Handles.zTest = UnityEngine.Rendering.CompareFunction.Less;
                Handles.DrawWireDisc(worldCursorPosition, Vector3.up, 0.5f, 2);

                var connections = script.GetConnections();

                SplineTreeBehavior.Connection closestConnection = null;
                if (connections.Count > 0)
                {
                    closestConnection = connections.FindClosest(worldCursorPosition, x =>
                    {
                        return Utilities.ClosestPointOnLineSegment(worldCursorPosition, x.nodeA.GetWorldPosition(), x.nodeB.GetWorldPosition(), out float progress);
                    }, out float distance);
                    if(distance < 2)
                    {
                        Handles.DrawLine(worldCursorPosition, closestConnection.nodeB.GetWorldPosition());
                        Handles.DrawLine(worldCursorPosition, closestConnection.nodeA.GetWorldPosition());
                    }
                    else
                    {
                        closestConnection = null;
                    }
                }

                if (closestNodeToCursor != null)
                {
                    Handles.color = Color.red;
                    Handles.DrawWireDisc(closestNodeToCursor.GetWorldPosition(), Vector3.up, 0.5f, 2);
                }


                if (Event.current.type == EventType.MouseDown)
                {
                    if(Event.current.button == 0)
                    {
                        var node = script.AddNode(worldCursorPosition);
                        
                        if(closestConnection != null)
                        {
                            closestConnection.nodeA.Connections.Add(node.id);
                            node.Connections.Add(closestConnection.nodeB.id);

                            closestConnection.nodeA.Connections.RemoveAll(x => x == closestConnection.nodeB.id);
                            closestConnection.nodeB.Connections.RemoveAll(x => x == closestConnection.nodeA.id);
                        }
                        else
                        {
                            if(SelectedNodes.Count > 0)
                            {
                                script.Connect(SelectedNodes.First(), node);
                            }
                        }
                        
                        SelectedNodes.Clear();
                        SelectedNodes.Add(node);
                    }
                    else if(Event.current.button == 1)
                    {
                        if (closestNodeToCursor != null)
                        {
                            if (SelectedNodes.Contains(closestNodeToCursor))
                            {
                                SelectedNodes.Remove(closestNodeToCursor);
                            }

                            script.RemoveNode(closestNodeToCursor);
                        }
                    }

                    Event.current.Use();
                }
            }
            else if(Event.current.control)
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    if (Event.current.button == 0)
                    {
                        foreach (var item in SelectedNodes)
                        {
                            script.Connect(item, closestNodeToCursor);
                        }
                        SelectedNodes.Clear();
                        SelectedNodes.Add(closestNodeToCursor);
                    }
                    else if (Event.current.button == 1)
                    {
                        foreach (var item in SelectedNodes)
                        {
                            script.Disconnect(item, closestNodeToCursor);
                        }
                    }

                    Event.current.Use();
                }


                foreach (var item in SelectedNodes)
                {
                    Handles.DrawDottedLine(item.GetWorldPosition(), worldCursorPosition, 5);
                }
            }

            Handles.BeginGUI();

            Rect rect = new Rect(10, 10, 250, tutorialEnabled.value ? 220 : 25);

            GUILayout.BeginArea(rect, GUI.skin.box);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Tutorial", EditorStyles.boldLabel);
            tutorialEnabled.DrawToggle();

            GUILayout.EndHorizontal();


            GUI.color = Event.current.alt ? Color.white : Color.white.SetAlpha(0.5f);
            GUILayout.Label("-Node placement Mode [Alt]");
            GUILayout.Label("   Place [LMB]");
            GUILayout.Label("   Remove [RMB]");

            GUI.color = Event.current.control ? Color.white : Color.white.SetAlpha(0.5f);
            GUILayout.Label("-Connection Mode (Ctrl)");
            GUILayout.Label("   Connect from selected [LMB]");
            GUILayout.Label("   Disconnect from selected [RMB]");

            GUI.color = Event.current.shift ? Color.white : Color.white.SetAlpha(0.5f);
            GUILayout.Label("-Selection");
            GUILayout.Label("   Hold [Shift] to select/deselect multiple");


            GUILayout.EndArea();
            Handles.EndGUI();

            if(Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                SelectedNodes.Clear();
                Event.current.Use();
            }


            if(EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(script);
            }
        }
    }
#endif
}
