namespace MarTools.AI
{
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System;

    public class AIController : MonoBehaviour
    {
        [SerializeReference] public Sequence rootNode;
        public Dictionary<BehaviorTreeNode, Status> NodesUsedThisTick = new Dictionary<BehaviorTreeNode, Status>();


        public float tickInterval = 0.1f;
        public float lastTickTimestamp { get; private set; }

        IEnumerator Start()
        {
            WaitForSeconds wfs = new WaitForSeconds(tickInterval);

            while(true)
            {
                yield return wfs;
                NodesUsedThisTick.Clear();
                var result = rootNode.Tick(this);
                
                lastTickTimestamp = Time.time;
            }

        }

        public List<(BehaviorTreeNode, BehaviorTreeNode)> FetchAllNodes(BehaviorTreeNode root)
        {
            var result = new List<(BehaviorTreeNode,BehaviorTreeNode)>();
            if(root is Composite)
            {
                foreach (var item in ((Composite)root).Children)
                {
                    result.Add((item, root));
                    result.AddRange(FetchAllNodes(item));
                }
            }
            return result;
        }

        internal void NodeUsed(BehaviorTreeNode selector, Status status)
        {
            if (NodesUsedThisTick.TryAdd(selector, status))
            {

            }
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(AIController))]
    public class AIControllerEditor : Editor
    {
        AIController script;
        private void OnEnable()
        {
            script = (AIController)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if(GUILayout.Button("Edit behavior"))
            {
                OpenGraphView();
            }
        }

        private void OpenGraphView()
        {
            AIGraphEditorWindow.ShowWindow((AIController)target);
        }
    }
#endif
}
