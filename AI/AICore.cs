namespace MarTools.AI
{
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.AI;
    using UnityEngine.Events;
    using System;
    using System.Reflection;
    using Unity.VisualScripting.YamlDotNet.Core;
    using UnityEngine.InputSystem.XR;

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(BehaviorTreeNode))]
    public class BehaviorTreeNodeDrawer : PolymorphicDrawer<BehaviorTreeNode>
    {
        protected override List<(string, Action)> GetDropDownOptions(SerializedProperty property)
        {
            var options = base.GetDropDownOptions(property);

            var target = property.serializedObject.targetObject as Component;

            foreach (var item in target.GetComponents<MonoBehaviour>())
            {
                Type type = item.GetType();

                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (method.ReturnType == typeof(Status))
                    {
                        options.Add(($"{type.Name}/{method.Name}", () =>
                        {
                            AssignValue(property, new ComponentBehaviorTreeNode()
                            {
                                component = item,
                                methodName = method.Name,
                            });
                        }
                        ));
                    }
                }
            }


            return options;
        }
    }
#endif

    [Name("Generic/Component Tree Node")]
    public class ComponentBehaviorTreeNode : BehaviorTreeNode
    {
        public Component component;
        public string methodName;

        private MethodInfo method = null;
        protected override Status TickInternal(AIController controller)
        {
            if(method == null) method = component.GetType().GetMethod(methodName);

            return (Status)method.Invoke(component, null);
        }
    }

    public enum Status
    {
        Running = 0,
        Success = 1,
        Failure = 2,
    }

    [System.Serializable]
    public abstract class BehaviorTreeNode
    {
        private bool active = false;

        public Status Tick(AIController controller)
        {
            if(!active)
            {
                active = true;
                Reset();
            }

            Status status = TickInternal(controller);

            if(status == Status.Success)
            {
                active = false;
                Success();
            }
            else if(status == Status.Failure)
            {
                active = false;
                Failure();
            }

            controller.NodeUsed(this, status);
            return status;
        }

        private void Failure()
        {
        }

        private void Success()
        {
        }

        protected virtual void Reset()
        {
        }

        protected abstract Status TickInternal(AIController controller);

        public BehaviorTreeNode Clone()
        {
            return (BehaviorTreeNode)this.MemberwiseClone();
        }
    }

    [Name("Generic/Preset")]
    public class ScriptableObjectBehavior : BehaviorTreeNode
    {
        public AIBehaviorTreeSO preset;
        private BehaviorTreeNode instance;
        
        protected override Status TickInternal(AIController controller)
        {
            if(instance == null) 
            {
                instance = preset.rootNode.Clone();
            }

            return instance.Tick(controller);
        }
    }

    [System.Serializable]
    public abstract class Composite : BehaviorTreeNode
    {
        [SerializeReference] public List<BehaviorTreeNode> Children = new List<BehaviorTreeNode>();
    }

    [Name("Generic/Fire Event")]
    public class FireEvent : BehaviorTreeNode
    {
        public UnityEvent OnFired;

        protected override Status TickInternal(AIController controller)
        {
            OnFired.Invoke();
            return Status.Success;
        }
    }

    [Name("Generic/Sequence")]
    [System.Serializable]
    public class Sequence : Composite
    {
        [Tooltip("Whether to always check all nodes before the execution")]
        public bool alwaysCheckAllNodes = false;

        private int currentChildIndex = 0;
        protected override Status TickInternal(AIController controller)
        {
            if (alwaysCheckAllNodes) currentChildIndex = 0;

            for (int i = currentChildIndex; i < Children.Count; i++)
            {
                var child = Children[i];
                var childStatus = child.Tick(controller);

                switch(childStatus)
                {
                    case Status.Success:

                        currentChildIndex++;
                        if(currentChildIndex >= Children.Count)
                        {
                            return Status.Success;
                        }
                        continue;
                    case Status.Failure:
                        return Status.Failure;
                    case Status.Running:
                        return Status.Running;
                }
            }

            return Status.Success;
        }
        protected override void Reset()
        {
            base.Reset();
            currentChildIndex = 0;
        }
    }

    [Name("Generic/Selector")]
    [System.Serializable]
    public class Selector : Composite
    {
        //private int currentChildIndex = 0;

        protected override Status TickInternal(AIController controller)
        {

            foreach (var item in Children)
            {
                var childStatus = item.Tick(controller);

                switch (childStatus)
                {
                    case Status.Success:
                        return Status.Success;
                    case Status.Failure:
                        continue;
                    case Status.Running:
                        return Status.Running;
                }
            }

            return Status.Failure;


            //if (currentChildIndex >= Children.Count)
            //{
            //    currentChildIndex = 0;
            //    return Status.Failure;
            //}

            //var childStatus = Children[currentChildIndex].Tick(controller);

            //if (childStatus == Status.Success)
            //{
            //    currentChildIndex = 0;
            //    return Status.Success;
            //}
            //else if (childStatus == Status.Failure)
            //{
            //    currentChildIndex++;
            //    return Status.Running;
            //}
            //else
            //{
            //    return Status.Running;
            //}
        }
    }

    [Name("Movement/NavMeshAgent/Move To Position")]
    [System.Serializable]
    public class NavMeshAgentMoveToPosition : BehaviorTreeNode
    {
        private NavMeshAgent agent;

        public Vector3 targetPosition = Vector3.zero;
        protected override Status TickInternal(AIController controller)
        {
            if(!agent) agent = controller.GetComponent<NavMeshAgent>();

            agent.SetDestination(targetPosition);

            if(Vector3.Distance(agent.transform.position, targetPosition) < 0.5f)
            {
                return Status.Success;
            }
            else
            {
                return Status.Running;
            }
        }
    }
    [Name("Movement/NavMeshAgent/Follow Transform")]
    [System.Serializable]
    public class NavMeshAgentMoveToTransform : BehaviorTreeNode
    {
        private NavMeshAgent agent;

        public Transform targetTransform;
        protected override Status TickInternal(AIController controller)
        {
            if (!agent) agent = controller.GetComponent<NavMeshAgent>();

            if (!targetTransform) return Status.Failure;

            agent.SetDestination(targetTransform.position);

            if (Vector3.Distance(agent.transform.position, targetTransform.position) < 0.5f)
            {
                return Status.Success;
            }
            else
            {
                return Status.Running;
            }
        }
    }

    public abstract class Decorator : BehaviorTreeNode
    {
        [SerializeReference] public BehaviorTreeNode child;
    }

    [Name("Generic/Repeater")]
    [System.Serializable]
    public class Repeater : Decorator
    {
        protected override Status TickInternal(AIController controller)
        {
            var childState = child.Tick(controller);

            if(childState == Status.Success)
            {
                return Status.Success;
            }
            else
            {
                return Status.Running;
            }

        }
    }

    [Name("Generic/Inverter")]
    public class Inverter : Decorator
    {
        protected override Status TickInternal(AIController controller)
        {
            var childState = child.Tick(controller);
            switch (childState)
            {
                case Status.Running:
                    return Status.Running;
                case Status.Success:
                    return Status.Failure;
                case Status.Failure:
                    return Status.Success;
                default:
                    break;
            }
            return Status.Failure;
        }
    }

    [Name("Generic/Wait")]
    public class Wait : BehaviorTreeNode
    {
        public float duration = 1f;

        private float startTimestamp;

        protected override void Reset()
        {
            base.Reset();

            startTimestamp = Time.time;
        }

        protected override Status TickInternal(AIController controller)
        {
            return Time.time - startTimestamp > duration ? Status.Success : Status.Running;
        }
    }

    [Name("Generic/Successor")]
    public class Succesor : BehaviorTreeNode
    {
        protected override Status TickInternal(AIController controller)
        {
            return Status.Success;
        }
    }
}
