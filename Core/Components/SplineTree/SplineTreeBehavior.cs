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

            public Vector3 tangent1;
            public Vector3 tangent2;


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
                Gizmos.DrawSphere(node.position, 0.2f);

                foreach (var item in node.Connections)
                {

                    var targetNode = Nodes.Find(x => x.id == item);
                    //Debug.Log(node.id + "||" + targetNode.id);
                    
                    GizmosUtilities.DrawArrow(node.position, targetNode.position, 2);
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

        internal SplineTreeBehavior.SplineTreeNode GetNode(string guid)
        {
            return Nodes.Find(x => x.id == guid);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SplineTreeBehavior))]
    public class SplineTreeBehaviorEditor : Editor
    {
        private int selectedNodeIndex;
        private int connectionIndex = -1;

        private EditorPrefToggle showNetworkHash = new EditorPrefToggle("Show Network Hash", false);
        private EditorPrefToggle showConnections = new EditorPrefToggle("Show Connections", false);

        SplineTreeBehavior script;
        private void OnEnable()
        {
            script = (SplineTreeBehavior)target;
            Tools.hidden = true;
        }
        private void OnDisable()
        {
            Tools.hidden = false;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            showConnections.DrawToggle();
            showNetworkHash.DrawToggle();

            EditorGUILayout.Space(20);

            for (int i = 0; i < script.Nodes.Count; i++)
            {
                var node = script.Nodes[i];

                if (string.IsNullOrEmpty(node.id))
                {
                    node.GenerateNewId();
                }

                GUILayout.BeginHorizontal();

                if (i == selectedNodeIndex) GUI.color = Color.green;

                if (GUILayout.Button("[]", EditorUtilities.GetButtonStyle(Color.gray, Color.white, 10, 10)))
                {
                    selectedNodeIndex = i;
                }

                node.position = EditorGUILayout.Vector3Field("", node.position);
                if (GUILayout.Button("X", EditorUtilities.GetButtonStyle(Color.red, Color.white, 10, 10)))
                {
                    script.Nodes.Remove(node);

                    foreach (var item in script.Nodes)
                    {
                        item.Connections.Remove(node.id);
                    }

                }

                GUI.color = Color.white;
                GUILayout.EndHorizontal();

                if(showNetworkHash.value)
                {
                    GUILayout.Label(node.GetSplineHash(script).ToString());
                }

                GUIStyle style = new GUIStyle(GUI.skin.label);
                style.fontSize = 10;

                if(showConnections.value)
                {
                    EditorGUILayout.LabelField($"GUID: {node.id}", style);

                    EditorGUILayout.LabelField($"      Connections: {node.Connections.Count}", style);
                    foreach (var connection in node.Connections.ToArray())
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.Space(2);
                        if(GUILayout.Button("X", EditorUtilities.GetButtonStyle(Color.red, Color.white, 10, 10, 5)))
                        {
                            node.Connections.Remove(connection);
                        }
                        EditorGUILayout.LabelField($"{connection}", style);
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }

            if(GUILayout.Button("Add new node"))
            {
                if(script.Nodes.Count > 0)
                {
                    script.AddNode(script.Nodes.Last().position);
                }
                else
                {
                    script.AddNode(Vector3.zero);
                }
            }

            if(EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(script);
            }
        }

        private void OnSceneGUI()
        {
            if(Event.current.shift)
            {
                ConnectionMode();
            }
            else if (Event.current.control)
            {
                PlacementMode();
            }
            else
            {
                NormalMode();
            }
        }
        
        private void PlacementMode()
        {
            for (int i = 0; i < script.Nodes.Count; i++)
            {
                var node = script.Nodes[i];
                Handles.color = Color.white;

                float size = HandleUtility.GetHandleSize(node.position);
                Handles.color = Color.red;
                if (Handles.Button(node.position, Quaternion.identity, size * 0.15f, size * 0.2f, Handles.SphereHandleCap))
                {
                    script.RemoveNode(node);

                    return;
                }
            }

            if (Event.current.type == EventType.MouseDown)
            {
                EditorGUI.BeginChangeCheck();
                Undo.RecordObject(script, "");

                Vector3 insertPointWorldCoordinate = Vector3.zero;

                float floorLevel = 0;
                if (selectedNodeIndex > 0) floorLevel = script.Nodes[selectedNodeIndex].position.y;

                Plane plane = new Plane(Vector3.up, -floorLevel);
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                if (plane.Raycast(ray, out float distance))
                {
                    insertPointWorldCoordinate = ray.GetPoint(distance);
                }

                float closestDistance = float.MaxValue;
                Vector3 placementPoint = Vector3.zero;
                SplineTreeBehavior.SplineTreeNode nodeA = null;
                SplineTreeBehavior.SplineTreeNode nodeB = null;

                foreach (var connection in script.GetConnections())
                {
                    var prevNode = connection.nodeA;
                    var nextNode = connection.nodeB;

                    Vector3 closesPoint = Utilities.ClosestPointOnLineSegment(insertPointWorldCoordinate, prevNode.position, nextNode.position, out float progress);
                    float dist = Vector3.Distance(closesPoint, insertPointWorldCoordinate);

                    if (dist < closestDistance)
                    {
                        closestDistance = dist;
                        placementPoint = closesPoint;

                        nodeA = prevNode;
                        nodeB = nextNode;
                    }
                }

                if (closestDistance > 1)
                {
                    script.AddNode(insertPointWorldCoordinate);
                }
                else
                {
                    nodeA.Connections.Remove(nodeB.id);
                    var newNode = script.AddNode(placementPoint);

                    nodeA.Connections.Add(newNode.id);
                    newNode.Connections.Add(nodeB.id);
                }





                Event.current.Use();

                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(script);
                }
            }
        }

        private void NormalMode()
        {
            for (int i = 0; i < script.Nodes.Count; i++)
            {
                var node = script.Nodes[i];

                EditorGUI.BeginChangeCheck();
                Undo.RecordObject(script, "");


                node.position = Handles.PositionHandle(node.position, Quaternion.identity);

                if (EditorGUI.EndChangeCheck())
                {
                    selectedNodeIndex = i;
                    EditorUtility.SetDirty(script);
                }


                foreach (var connectionID in node.Connections)
                {
                    Handles.color = selectedNodeIndex == i ? Color.green : Color.white;


                    var targetNode = script.Nodes.Find(x => x.id == connectionID);
                    Handles.DrawLine(node.position, targetNode.position, 0.5f);
                }

                Handles.color = selectedNodeIndex == i ? Color.green : Color.white;

                float size = HandleUtility.GetHandleSize(node.position)*0.2f;
                Handles.SphereHandleCap(0, node.position, Quaternion.identity, size, EventType.Repaint);
            }
        }

        private void ConnectionMode()
        {
            EditorGUI.BeginChangeCheck();
            Undo.RecordObject(script, "");

            for (int i = 0; i < script.Nodes.Count; i++)
            {
                var node = script.Nodes[i];
                Handles.color = Color.white;

                float size = HandleUtility.GetHandleSize(node.position);
                if(connectionIndex >= 0)
                {
                    var targetNode = script.Nodes[connectionIndex];

                    if(connectionIndex == i)
                    {
                        Handles.color = Color.green;
                        if (Handles.Button(node.position, Quaternion.identity, size * 0.15f, size * 0.2f, Handles.SphereHandleCap))
                        {
                            connectionIndex = -1;
                        }
                    }
                    else
                    {
                        if (targetNode.Connections.Contains(node.id))
                        {
                            Handles.color = Color.red;
                            if (Handles.Button(node.position, Quaternion.identity, size * 0.15f, size * 0.2f, Handles.SphereHandleCap))
                            {
                                targetNode.Connections.Remove(node.id);
                                connectionIndex = -1;
                            }
                        }
                        else
                        {
                            Handles.color = Color.white;
                            if (Handles.Button(node.position, Quaternion.identity, size * 0.15f, size * 0.2f, Handles.SphereHandleCap))
                            {
                                targetNode.Connections.Add(node.id);
                                connectionIndex = -1;
                            }
                        }


                    }
                }
                else
                {
                    if (Handles.Button(node.position, Quaternion.identity, size*0.15f, size*0.2f, Handles.SphereHandleCap))
                    {
                        connectionIndex = i;
                        break;
                    }
                }
            }

            if(EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(script);
            }
        }
    }
#endif
}
