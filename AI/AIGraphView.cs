namespace MarTools.AI
{
    using Codice.CM.Client.Differences;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using UnityEditor;
    using UnityEditor.Experimental.GraphView;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class AIGraphView : GraphView
    {
        public Dictionary<SerializedProperty, AIGraphNode> Nodes = new Dictionary<SerializedProperty, AIGraphNode>();
        public List<Edge> Edges = new List<Edge>();

        public AIController controller
        {
            get
            {
                if(Selection.activeGameObject != null &&  Selection.activeGameObject.TryGetComponent<AIController>(out var c))
                {
                    return c;
                }
                return null;
            }
        }

        public AIGraphView()
        {
            StyleSheet sheet = Resources.Load<StyleSheet>("AIGraph");
            styleSheets.Add(sheet);
            GridBackground background = new GridBackground();
            Add(background);


            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            // Add manipulators (for dragging, selecting, etc.)
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.StopPropagation(); // Stop further propagation of the event
        }

        public override EventPropagation DeleteSelection()
        {
            foreach (var selectable in selection)
            {
                if(selectable is AIGraphNode)
                {
                    Debug.Log(selectable);

                    var node = selectable as AIGraphNode;

                    List<Port> Ports = new List<Port>();
                    Ports.AddRange(node.inputContainer.Children().OfType<Port>());
                    Ports.AddRange(node.outputContainer.Children().OfType<Port>());

                    foreach (var port in Ports)
                    {
                        DeleteElements(edges.Where(x => x.input == port || x.output == port));

                        //foreach (var edge in port.connections.ToList())
                        //{
                        //    edge.input.Disconnect(edge);
                        //    edge.output.Disconnect(edge);
                        //    RemoveElement(edge);
                        //    Edges.Remove(edge);
                        //}
                    }
                }
            }

            return base.DeleteSelection();
        }

        public void ClearGraph()
        {
            foreach (var item in Nodes)
            {
                List<Port> ports = new List<Port>();
                RemoveElement(item.Value);
            }
            foreach (var item in Edges)
            {
                RemoveElement(item);
            }
            Edges.Clear();
            Nodes.Clear();
        }

        public void Refresh()
        {
            ClearGraph();

            var grid = new GridBackground();
            grid.StretchToParentSize();

            LoadNodes(controller);

            ConnectNodes();
            MarkDirtyRepaint();
        }

        private void ConnectNodes()
        {
            foreach (var item in Nodes)
            {
                BehaviorTreeNode value = item.Key.managedReferenceValue as BehaviorTreeNode;

                if(value is Composite)
                {
                    foreach (var child in (value as Composite).Children)
                    {
                        ConnectBehaviorNodes(value, child);
                    }
                }
                else if(value is Decorator)
                {
                    ConnectBehaviorNodes(value, (value as Decorator).child);
                }
            }
        }

        private void ConnectBehaviorNodes(BehaviorTreeNode output, BehaviorTreeNode input)
        {
            var inputKey = Nodes.Keys.ToList().Find(x => x.managedReferenceValue == input);
            var outputKey = Nodes.Keys.ToList().Find(x => x.managedReferenceValue == output);

            var inputNode = Nodes[inputKey];
            var outputNode = Nodes[outputKey];

            var inputPort = inputNode.inputContainer[0]as Port;
            var outputPort = outputNode.outputContainer[0] as Port;

            Edge edge = new Edge()
            {
                input = inputPort,
                output = outputPort,
            };

            inputPort.ConnectTo(outputPort);
            outputPort.ConnectTo(inputPort);

            Edges.Add(edge);
            AddElement(edge);
        }


        public void LoadNodes(AIController controller)
        {
            SerializedObject obj = new SerializedObject(controller);

            // Start from the rootNode
            SerializedProperty root = obj.FindProperty("rootNode");

            if (root != null)
            {
                var rootNode = CreateNode(root, obj);
                Nodes.Add(root, rootNode);

                // Call a recursive method to iterate through all fields
                var offsets = new Dictionary<int, float>();

                LoadNodesRecursive(root, obj, 1, ref offsets);
            }
        }

        private float LoadNodesRecursive(SerializedProperty property, SerializedObject serializedObject, int recursionLevel, ref Dictionary<int, float> ElementOffsetsPerLevel)
        {
            if(ElementOffsetsPerLevel.TryAdd(recursionLevel, 0))
            {

            }

            float horizontalSpacing = 600f; // Increased horizontal spacing for deeper levels

            // Find the Children property, assuming it is a List or Array
            var childrenContainer = property.FindPropertyRelative("Children");
            var child = property.FindPropertyRelative("child");


            float insideOffset = 0;

            // Check if the Children property exists and is an array or list
            if (childrenContainer != null && childrenContainer.isArray && childrenContainer.arraySize > 0)
            {
                // Loop through each child in the Children array
                for (int i = 0; i < childrenContainer.arraySize; i++)
                {
                    SerializedProperty childProperty = childrenContainer.GetArrayElementAtIndex(i);

                    // Recursively check if the child has its own Children property
                    float offset = LoadNodesRecursive(childProperty, serializedObject, recursionLevel + 1, ref ElementOffsetsPerLevel);
                    insideOffset += offset;


                    // Create the node and position it
                    var node = CreateNode(childProperty, serializedObject);
                    Nodes.Add(childProperty, node);

                    ElementOffsetsPerLevel[recursionLevel] += 200;
                    // Adjust node position based on recursion level and vertical index
                    float xPos = recursionLevel * horizontalSpacing; // Increased horizontal offset based on recursion level
                    float yPos = ElementOffsetsPerLevel[recursionLevel] + ElementOffsetsPerLevel[recursionLevel+1]/2;


                    node.transform.position = new Vector2(xPos, yPos);

                    // Add the height of the subtree to the total offset
                }
            }
            else if(child != null)
            {
                // Create the node and position it
                var node = CreateNode(child, serializedObject);
                Nodes.Add(child, node);

                // Adjust node position based on recursion level and vertical index
                float xPos = recursionLevel * horizontalSpacing; // Increased horizontal offset based on recursion level
                ElementOffsetsPerLevel[recursionLevel] += 200;
                float yPos = ElementOffsetsPerLevel[recursionLevel];

                node.transform.position = new Vector2(xPos, yPos);

                float offset = LoadNodesRecursive(child, serializedObject, recursionLevel + 1, ref ElementOffsetsPerLevel);
            }
            else
            {
                // If no children, increase vertical offset
                //totalOffset += verticalSpacing;
                insideOffset += 200;
            }

            return insideOffset;
        }


        public AIGraphNode CreateNode(SerializedProperty property, SerializedObject obj)
        {
            PropertyField fieldToDisplay = new PropertyField(property);
            fieldToDisplay.Bind(obj);

            AIGraphNode node = new AIGraphNode(fieldToDisplay)
            {
                title = property.managedReferenceValue.GetType().Name,
            };
            node.SetPosition(new Rect(100, 100, 500, 200));
            if(property.managedReferenceValue is Composite || property.managedReferenceValue is Decorator)
            {
                var p = node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
                node.Add(p);
                node.outputContainer.Add(p);
            }

            
            var port = node.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(float));
            node.Add(port);
            node.inputContainer.Add(port);

            node.RefreshPorts();
            node.RefreshExpandedState();

            AddElement(node);
            return node;
        }

        internal void Update()
        {
            if (!controller) return;

            float timeSinceLastTick = Time.time - controller.lastTickTimestamp;
            float progressToNextTick = timeSinceLastTick / controller.tickInterval;

            float opacity = progressToNextTick.Remap(0f, 1f, 0.5f, 0.1f);

            foreach (var item in Nodes)
            {
                var node = item.Value;

                if(Application.isPlaying)
                {
                    if (controller.NodesUsedThisTick.TryGetValue(item.Key.managedReferenceValue as BehaviorTreeNode, out Status status))
                    {
                        Color col = status switch
                        {
                            Status.Success => Color.green,
                            Status.Running => Color.yellow,
                            Status.Failure => Color.red,
                            _ => Color.white  // Optionally handle other cases
                        };

                        col.a = opacity;
                        node.style.backgroundColor = col;
                    }
                    else
                    {
                        node.style.backgroundColor = Color.clear;
                    }
                }
                else
                {
                    node.style.backgroundColor = Color.clear;
                }
            }
        }
    }
}
