using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class GenericGraphView : GraphView
{
    public GenericGraphView()
    {
        Debug.Log("Test create");

        Initialize();
    }

    private void Initialize()
    {
        styleSheets.Add(Resources.Load<StyleSheet>("GraphStyle"));
        GridBackground background = new GridBackground() { name = "Grid" };

        Insert(0, background);

        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        this.AddManipulator(CreateContextualMenu());
    }

    private IManipulator CreateContextualMenu()
    {
        ContextualMenuManipulator manipulator = new ContextualMenuManipulator(e => e.menu.AppendAction("Add node", a =>
        {
            AddNode(a.eventInfo.localMousePosition);
        }));

        return manipulator;
    }

    public void AddNode(Vector2 position)
    {
        Debug.Log("Node added");

        GenericGraphEditorNode node = new GenericGraphEditorNode()
        {
            title = "Test",
        };

        var oPort = node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
        node.outputContainer.Add(oPort);

        var iPort = node.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
        node.outputContainer.Add(iPort);


        node.SetPosition(new Rect(position.x, position.y, 500, 500));
        node.RefreshExpandedState();
        node.RefreshPorts();
        
        AddElement(node);
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.ToList();
    }
}
