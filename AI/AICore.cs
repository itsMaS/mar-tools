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
        [HideInInspector]
        public Vector2 nodePosition;

        private bool active = false;

        public Status Tick(AIController controller)
        {
            if(!active)
            {
                active = true;
                Reset(controller);
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

        protected virtual void Reset(AIController controller)
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
        //[HideInInspector]
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
        private int currentChildIndex = 0;
        protected override Status TickInternal(AIController controller)
        {
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
        protected override void Reset(AIController controller)
        {
            base.Reset(controller);
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

    public abstract class NavMeshBehaviorTreeNode : BehaviorTreeNode
    {
        protected NavMeshAgent agent;
        public float stopDistance = 0.1f;

        protected override void Reset(AIController controller)
        {
            base.Reset(controller);
            if (!agent) agent = controller.GetComponent<NavMeshAgent>();
        }
    }

    [Name("Movement/NavMeshAgent/Move To Position")]
    [System.Serializable]
    public class NavMeshAgentMoveToPosition : NavMeshBehaviorTreeNode
    {
        public Vector3 targetPosition = Vector3.zero;
        protected override Status TickInternal(AIController controller)
        {

            agent.SetDestination(targetPosition);

            if(Vector3.Distance(agent.transform.position, targetPosition) < stopDistance)
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
    public class NavMeshAgentMoveToTransform : NavMeshBehaviorTreeNode
    {
        public Transform targetTransform;
        protected override Status TickInternal(AIController controller)
        {
            if (!targetTransform) return Status.Failure;

            agent.SetDestination(targetTransform.position);

            if (Vector3.Distance(agent.transform.position, targetTransform.position) < stopDistance)
            {
                return Status.Success;
            }
            else
            {
                return Status.Running;
            }
        }
    }

    [Name("Movement/NavMeshAgent/Patrol path")]
    public class NavMeshAgentPatrol : NavMeshBehaviorTreeNode
    {
        public LineBehavior path;

        private int nextPointIndex = 0;
        private Vector3 nextPoint => path.worldPoints[nextPointIndex];

        protected override void Reset(AIController controller)
        {
            base.Reset(controller);
            nextPointIndex = 0;
        }

        protected override Status TickInternal(AIController controller)
        {
            if(!path) return Status.Failure;

            float distanceToNextPoint = Vector3.Distance(controller.transform.position, nextPoint);
            if (distanceToNextPoint < stopDistance)
            {
                nextPointIndex = nextPointIndex + 1;
                if(nextPointIndex >= path.worldPoints.Count)
                {
                    return Status.Success;
                }
                agent.SetDestination(nextPoint);
                return Status.Running;
            }
            else
            {
                agent.SetDestination(nextPoint);
                return Status.Running;
            }
        }
    }

    public abstract class Decorator : BehaviorTreeNode
    {
        [SerializeReference] public BehaviorTreeNode child;
    }

    public class ConditionalGate : Decorator
    {
        [SerializeReference] public BehaviorTreeNode condition;

        protected override Status TickInternal(AIController controller)
        {
            if (condition.Tick(controller) == Status.Failure) return Status.Failure;

            return child.Tick(controller);
        }
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

        protected override void Reset(AIController controller)
        {
            base.Reset(controller);
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

    #region Animator

    public abstract class SetAnimatorParameterBase<T> : BehaviorTreeNode
    {
        public string parameterName;
        public T parameter;

        private Animator an;

        protected override void Reset(AIController controller)
        {
            base.Reset(controller);
            if(!an) an = controller.GetComponent<Animator>();
        }

        protected abstract void SetParameter(Animator an, string name, T param);

        protected override Status TickInternal(AIController controller)
        {
            SetParameter(an, parameterName, parameter);
            return Status.Success;
        }
    }


    [Name("Animator/Set float")]
    public class SetAnimatorParameterFloat : SetAnimatorParameterBase<float>
    {
        protected override void SetParameter(Animator an, string name, float param)
        {
            an.SetFloat(name, param);
        }
    }


    [Name("Animator/Set bool")]
    public class SetAnimatorParameterBool : SetAnimatorParameterBase<bool>
    {
        protected override void SetParameter(Animator an, string name, bool param)
        {
            an.SetBool(name, param);
        }
    }

    [Name("Animator/Set int")]
    public class SetAnimatorParameterInt : SetAnimatorParameterBase<int>
    {
        protected override void SetParameter(Animator an, string name, int param)
        {
            an.SetInteger(name, param);
        }
    }

    [Name("Animator/Set trigger")]
    public class SetAnimatorTrigger : SetAnimatorParameterBase<object>
    {
        protected override void SetParameter(Animator an, string name, object param)
        {
            an.SetTrigger(name);
        }
    }
    #endregion
}
