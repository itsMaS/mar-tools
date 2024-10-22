namespace MarTools.AI 
{
    using System;
    using UnityEditor;
    using UnityEditor.Experimental.GraphView;
    using UnityEditor.UIElements;
    using UnityEditorInternal;
    using UnityEngine;
    using UnityEngine.UIElements;
    //using Node = UnityEditor.Experimental.GraphView.Node;

    [System.Serializable]
    public class AIGraphEditorNode : Node
    {
        public SerializedProperty displayedProperty { get; private set; }
        public BehaviorTreeNode value { get; private set; }

        public AIGraphEditorNode(SerializedProperty property)
        {
            title = property.displayName;
            displayedProperty = property;

            PropertyField displayField = new PropertyField(property);
            displayField.Bind(property.serializedObject);
            mainContainer.Add(displayField);

            value = property.managedReferenceValue as BehaviorTreeNode;
            if(value != null ) 
            {
                SetPosition(new Rect(value.nodePosition.x, value.nodePosition.y, 0, 0));
                
                var inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(float));
                inputContainer.Add(inputPort);




                if(value is Composite || value is Decorator)
                {
                    var outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, value is Composite ? Port.Capacity.Multi : Port.Capacity.Single, typeof(float));
                    outputContainer.Add(outputPort);
                }

                ValueChanged(null);
            }

            displayField.RegisterValueChangeCallback(ValueChanged);


            RefreshPorts();
            RefreshExpandedState();
        }

        private void ValueChanged(SerializedPropertyChangeEvent evt)
        {
            title = value.GetType().Name;
            //Debug.Log("Value changed");
        }

        internal void UpdatePosition()
        {
            if(value != null)
            {
                value.nodePosition = GetPosition().position;
            }
        }
    }
}