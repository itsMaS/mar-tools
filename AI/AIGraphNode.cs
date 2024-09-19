namespace MarTools.AI 
{
    using UnityEditor.UIElements;
    using UnityEditor.Experimental.GraphView;
    using UnityEditor;
    using UnityEngine;

    public class AIGraphNode : UnityEditor.Experimental.GraphView.Node
    {
        public AIGraphNode(PropertyField propertyField)
        {
            mainContainer.Add(propertyField);
            this.MarkDirtyRepaint();
        }
    }
}