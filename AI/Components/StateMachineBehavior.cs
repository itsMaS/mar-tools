using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.VisualScripting.YamlDotNet.Core;
using UnityEngine.Events;





#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MarTools.AI
{
    public abstract class StateMachineBehavior : MonoBehaviour
    {
        public List<IState> AllStates = new List<IState>();
        public IState activeState { get; private set; }

        protected virtual void Awake()
        {
            AllStates = this.GetVariablesOfType<IState>(true);

            Debug.Log(AllStates.Count);
        }

        protected virtual void Start()
        {
            Spawn();
        }

        private void Update()
        {
            if(activeState != null)
            {
                activeState.Update(this, Time.deltaTime);
            }
        }

        public void ChangeState<T>(State<T> newState) where T : StateMachineBehavior
        {
            if(activeState != newState)
            {
                IState prev = activeState;

                activeState = newState;
                
                prev?.Exit(this, newState);
                activeState?.Enter(this, prev);
            }
        }

        public abstract void Spawn();

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if(Application.isPlaying)
            {
                GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
                style.alignment = TextAnchor.MiddleCenter;

                Handles.Label(transform.position + Vector3.up, $"{activeState.Name}", style);
            }
#endif
        }
    }

    [System.Serializable]
    public abstract class State<T> : IState where T : StateMachineBehavior
    {
        [System.Serializable]
        public class Events
        {
            public UnityEvent OnEnter;
            public UnityEvent OnUpdate;
            public UnityEvent OnExit;
        }

        public Events events;
        public float timeSinceEntered { get; private set; } = 0;
        string IState.Name => GetType().Name;
        protected T owner { get; private set; }

        protected virtual void Initialize(T b)
        {
        }

        protected virtual void OnEnterState(T b, IState previousState)
        {
        }

        protected virtual void OnUpdateState(T b, float deltaTime)
        {
        }

        protected virtual void OnExitState(T b, IState targetState)
        {
        }

        Type IState.GetAgentType()
        {
            return typeof (T);
        }

        void IState.Enter(StateMachineBehavior b, IState previousState)
        {
            owner = b as T;
            timeSinceEntered = 0;
            OnEnterState(b as T, previousState);
            events.OnEnter.Invoke();
        }

        void IState.Exit(StateMachineBehavior b, IState targetState)
        {
            OnExitState(b as T, targetState);
            events.OnExit.Invoke();
        }

        void IState.Update(StateMachineBehavior b, float deltaTime)
        {
            timeSinceEntered += deltaTime;
            OnUpdateState(b as T, deltaTime);
            events.OnUpdate.Invoke();
        }
    }

    public interface IState 
    {
        string Name { get; }
        Type GetAgentType();
        internal void Enter(StateMachineBehavior agent, IState previousState);
        internal void Exit(StateMachineBehavior agent, IState targetState);
        internal void Update(StateMachineBehavior agent, float deltaTime);
    }



#if UNITY_EDITOR
    [CustomEditor(typeof(StateMachineBehavior), true)]
    public class StateMachineBehaviorEditor : MarToolsEditor<StateMachineBehavior>
    {
        public override void OnInspectorGUI()
        {
            if (Application.isPlaying)
            {
                foreach (var item in script.AllStates)
                {
                    GUILayout.Label(item.Name);
                }
            }
            else
            {
                base.OnInspectorGUI();
            }
        }
    }
#endif
}

