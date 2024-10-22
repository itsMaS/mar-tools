namespace MarTools.AI
{
    using Codice.CM.Client.Differences;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Unity.VisualScripting.YamlDotNet.Core;
    using UnityEditor;
    using UnityEditor.Experimental.GraphView;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class AIGraphView : GraphView
    {
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
            background.name = "Grid";
            Insert(0, background);

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            // Add manipulators (for dragging, selecting, etc.)
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            this.AddManipulator(CreateContextualMenu());

            graphViewChanged += Changed;
        }

        private BehaviorTreeNode Delete(BehaviorTreeNode node)
        {
            return RecursiveDelete(controller.rootNode, node);
        }

        private BehaviorTreeNode RecursiveDelete(BehaviorTreeNode branch, BehaviorTreeNode searched)
        {
            if(branch is Composite)
            {

                var comp = (Composite)branch;
                if(comp.Children.Contains(searched))
                {
                    comp.Children.Remove(searched);
                    return comp;
                }
                else
                {
                    foreach (var node in comp.Children)
                    {
                        var target = RecursiveDelete(node, searched);
                        if (target != null) return target;
                    }
                }
            }

            return null;
        }

        private GraphViewChange Changed(GraphViewChange graphViewChange)
        {
            if(graphViewChange.movedElements != null) 
            {
                foreach (var item in graphViewChange.movedElements)
                {
                    if(item is AIGraphEditorNode)
                    {
                        var node = item as AIGraphEditorNode;
                        node.UpdatePosition();
                    }
                }
            }

            if(graphViewChange.elementsToRemove != null)
            {
                foreach (var item in graphViewChange.elementsToRemove)
                {
                    if (item is AIGraphEditorNode)
                    {
                        var node = item as AIGraphEditorNode;
                        var data = node.value;

                        Delete(data);
                        DrawGraph();
                    }
                }
            }

            if(graphViewChange.edgesToCreate != null)
            {
                foreach (var item in graphViewChange.edgesToCreate)
                {
                    AIGraphEditorNode input = item.input.node as AIGraphEditorNode;
                    AIGraphEditorNode output = item.output.node as AIGraphEditorNode;

                    Debug.Log($"Connected {output.value.GetType()} -> {input.value.GetType()}");

                    BehaviorTreeNode inputNode = input.value;
                    Composite outputNode = output.value as Composite;

                    Delete(inputNode);

                    outputNode.Children.Add(inputNode);
                }
            }

            return graphViewChange;
        }

        private IManipulator CreateContextualMenu()
        {
            ContextualMenuManipulator manipulator = new ContextualMenuManipulator(e => e.menu.AppendAction("Add node", a =>
            {
                // Get the position in world space from the event
                Vector2 mouseWorldPosition = a.eventInfo.mousePosition;

                // Convert world position to local graph coordinates
                Vector2 localPosition = contentViewContainer.WorldToLocal(mouseWorldPosition);

                AddNewNode(localPosition); // Pass the converted position
                DrawGraph();
            }));

            return manipulator;
        }

        public void Load()
        {
            DrawGraph();
        }

        public void ClearGraph()
        {
            foreach (var node in this.nodes.ToList())
            {
                RemoveElement(node);
            }
            foreach (var edge in this.edges.ToList())
            {
                RemoveElement(edge);
            }
        }

        public void DrawGraph()
        {
            ClearGraph();
            var controller = this.controller;
            SerializedObject obj = new SerializedObject(controller);
            SerializedProperty rootNode = obj.FindProperty("rootNode");
            SerializedProperty rootChildren = rootNode.FindPropertyRelative("Children");
            DrawGraph(rootChildren, rootNode);


            Node node = new Node()
            {
                title = "ROOT",
            };
            Port p = node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
            node.outputContainer.Add(p);

            var outputPort = p;
            foreach (var item in nodes.ToList().ConvertAll<AIGraphEditorNode>(x => x as AIGraphEditorNode).FindAll(x => controller.rootNode.Children.Contains(x.value)))
            {
                var inputPort = item.inputContainer[0] as Port;

                Edge edge = new Edge()
                {
                    input = inputPort,
                    output= outputPort,
                };
                inputPort.Connect(edge);
                outputPort.Connect(edge);

                AddElement(edge);
            }

            node.RefreshExpandedState();
            node.RefreshPorts();

            AddElement(node);
        }

        public void DrawGraph(SerializedProperty rootProperty, SerializedProperty parent)
        {




            if (rootProperty != null && rootProperty.isArray && rootProperty.arraySize > 0)
            {
                for (int i = 0; i < rootProperty.arraySize; i++)
                {
                    SerializedProperty childProperty = rootProperty.GetArrayElementAtIndex(i);

                    var parentNode = nodes.ToList().ConvertAll<AIGraphEditorNode>(x => x as  AIGraphEditorNode).Find(x =>
                    {
                        return x.displayedProperty == parent;
                    });
                    LoadNode(childProperty, parentNode);

                    var nestedValues = childProperty.FindPropertyRelative("Children");
                    if (nestedValues != null)
                    {
                        DrawGraph(nestedValues, childProperty);
                    }
                }
            }
        }

        public AIGraphEditorNode LoadNode(SerializedProperty propertyToDisplay, AIGraphEditorNode parentNode)
        {
            var node = new AIGraphEditorNode(propertyToDisplay);

            if (parentNode != null)
            {
                var outputPort = parentNode.outputContainer[0] as Port;
                var inputPort = node.inputContainer[0] as Port;

                Edge edge = new Edge()
                {
                    input = inputPort,
                    output = outputPort,
                };

                inputPort.Connect(edge);
                outputPort.Connect(edge);

                AddElement(edge);
            }

            AddElement(node);

            return node;
        }

        public void AddNewNode(Vector2 position)
        {
            controller.rootNode.Children.Add(new Sequence() { nodePosition = position });
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList();
        }

        //private void ConnectNodes()
        //{
        //    foreach (var item in Nodes)
        //    {
        //        BehaviorTreeNode value = item.Key.managedReferenceValue as BehaviorTreeNode;

        //        if(value is Composite)
        //        {
        //            foreach (var child in (value as Composite).Children)
        //            {
        //                ConnectBehaviorNodes(value, child);
        //            }
        //        }
        //        else if(value is Decorator)
        //        {
        //            ConnectBehaviorNodes(value, (value as Decorator).child);
        //        }
        //    }
        //}

        //private void ConnectBehaviorNodes(BehaviorTreeNode output, BehaviorTreeNode input)
        //{
        //    var inputKey = Nodes.Keys.ToList().Find(x => x.managedReferenceValue == input);
        //    var outputKey = Nodes.Keys.ToList().Find(x => x.managedReferenceValue == output);

        //    var inputNode = Nodes[inputKey];
        //    var outputNode = Nodes[outputKey];

        //    var inputPort = inputNode.inputContainer[0]as Port;
        //    var outputPort = outputNode.outputContainer[0] as Port;

        //    Edge edge = new Edge()
        //    {
        //        input = inputPort,
        //        output = outputPort,
        //    };

        //    inputPort.ConnectTo(outputPort);
        //    outputPort.ConnectTo(inputPort);

        //    Edges.Add(edge);
        //    AddElement(edge);
        //}


        //public void LoadNodes(AIController controller)
        //{
        //    SerializedObject obj = new SerializedObject(controller);

        //    // Start from the rootNode
        //    SerializedProperty root = obj.FindProperty("rootNode");

        //    if (root != null)
        //    {
        //        var rootNode = CreateNode(root, obj);
        //        Nodes.Add(root, rootNode);

        //        // Call a recursive method to iterate through all fields
        //        var offsets = new Dictionary<int, float>();

        //        LoadNodesRecursive(root, obj, 1, ref offsets);
        //    }
        //}

        //private float LoadNodesRecursive(SerializedProperty property, SerializedObject serializedObject, int recursionLevel, ref Dictionary<int, float> ElementOffsetsPerLevel)
        //{
        //    if(ElementOffsetsPerLevel.TryAdd(recursionLevel, 0))
        //    {

        //    }

        //    float horizontalSpacing = 600f; // Increased horizontal spacing for deeper levels

        //    // Find the Children property, assuming it is a List or Array
        //    var childrenContainer = property.FindPropertyRelative("Children");
        //    var child = property.FindPropertyRelative("child");


        //    float insideOffset = 0;

        //    // Check if the Children property exists and is an array or list
        //    if (childrenContainer != null && childrenContainer.isArray && childrenContainer.arraySize > 0)
        //    {
        //        // Loop through each child in the Children array
        //        for (int i = 0; i < childrenContainer.arraySize; i++)
        //        {
        //            SerializedProperty childProperty = childrenContainer.GetArrayElementAtIndex(i);

        //            // Recursively check if the child has its own Children property
        //            float offset = LoadNodesRecursive(childProperty, serializedObject, recursionLevel + 1, ref ElementOffsetsPerLevel);
        //            insideOffset += offset;


        //            // Create the node and position it
        //            var node = CreateNode(childProperty, serializedObject);
        //            Nodes.Add(childProperty, node);

        //            ElementOffsetsPerLevel[recursionLevel] += 200;
        //            // Adjust node position based on recursion level and vertical index
        //            float xPos = recursionLevel * horizontalSpacing; // Increased horizontal offset based on recursion level
        //            float yPos = ElementOffsetsPerLevel[recursionLevel] + ElementOffsetsPerLevel[recursionLevel+1]/2;


        //            node.transform.position = new Vector2(xPos, yPos);

        //            // Add the height of the subtree to the total offset
        //        }
        //    }
        //    else if(child != null)
        //    {
        //        // Create the node and position it
        //        var node = CreateNode(child, serializedObject);
        //        Nodes.Add(child, node);

        //        // Adjust node position based on recursion level and vertical index
        //        float xPos = recursionLevel * horizontalSpacing; // Increased horizontal offset based on recursion level
        //        ElementOffsetsPerLevel[recursionLevel] += 200;
        //        float yPos = ElementOffsetsPerLevel[recursionLevel];

        //        node.transform.position = new Vector2(xPos, yPos);

        //        float offset = LoadNodesRecursive(child, serializedObject, recursionLevel + 1, ref ElementOffsetsPerLevel);
        //    }
        //    else
        //    {
        //        // If no children, increase vertical offset
        //        //totalOffset += verticalSpacing;
        //        insideOffset += 200;
        //    }

        //    return insideOffset;
        //}


        //public AIGraphEditorNode CreateNode(SerializedProperty property, SerializedObject obj)
        //{
        //    PropertyField fieldToDisplay = new PropertyField(property);
        //    fieldToDisplay.Bind(obj);

        //    AIGraphEditorNode node = new AIGraphEditorNode()
        //    {
        //        title = property.managedReferenceValue.GetType().Name,
        //    };
        //    node.SetPosition(new Rect(100, 100, 500, 200));
        //    if(property.managedReferenceValue is Composite || property.managedReferenceValue is Decorator)
        //    {
        //        var p = node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
        //        node.Add(p);
        //        node.outputContainer.Add(p);
        //    }


        //    var port = node.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(float));
        //    node.Add(port);
        //    node.inputContainer.Add(port);

        //    node.RefreshPorts();
        //    node.RefreshExpandedState();

        //    AddElement(node);
        //    return node;
        //}

        internal void Update()
        {
            if (!controller) return;

            float timeSinceLastTick = Time.time - controller.lastTickTimestamp;
            float progressToNextTick = timeSinceLastTick / controller.tickInterval;

            float opacity = progressToNextTick.Remap(0f, 1f, 0.5f, 0.1f);

            foreach (var item in nodes)
            {
                if (item is not AIGraphEditorNode) continue;

                var node = item as AIGraphEditorNode;

                if (Application.isPlaying)
                {
                    if (controller.NodesUsedThisTick.TryGetValue(node.value, out Status status))
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
