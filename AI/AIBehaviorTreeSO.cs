namespace MarTools.AI
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(menuName = "MarTools/AIBehaviorTree", fileName = "New Behavior Tree")]
    public class AIBehaviorTreeSO : ScriptableObject
    {
        [SerializeReference] public BehaviorTreeNode rootNode;
    }
}
