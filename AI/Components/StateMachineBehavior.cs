using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MarTools.AI
{
    public abstract class StateMachineBehavior : MonoBehaviour
    {
        public IState activeState { get; private set; }

        public Dictionary<string, IState> AllStates = new Dictionary<string, IState>();
        public List<IState> AllStatesIndexed = new List<IState>();

        private bool initialized = false;

        protected virtual void Awake()
        {
            if(!initialized) Initialize();
        }

        protected virtual void Initialize()
        {
            if (initialized) return;

            foreach (var item in this.GetFieldsOfType<IState>(true))
            {
                IState state = item.GetValue(this) as IState;

                AllStates.Add(item.Name, state);
                AllStatesIndexed.Add(state);
            }

            initialized = true;
        }

        private void Update()
        {
            if(activeState != null)
            {
                activeState.Update(this, Time.deltaTime);
            }
        }

        public void ChangeState(IState newState)
        {
            if (!initialized) Initialize();

            if (activeState != newState)
            {
                IState prev = activeState;

                activeState = newState;

                prev?.Exit(this, newState);
                activeState?.Enter(this, prev);
            }
        }

        public void ChangeState(string stateName)
        {
            if (!initialized) Initialize();

            if(AllStates.TryGetValue(stateName, out IState newState))
            {
                ChangeState(newState);
            }
            else
            {
                Debug.LogWarning($"There is not state with the name of {stateName}");
            }
        }

        public void ClearState()
        {
            ChangeState((null) as IState);
        }

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if(Application.isPlaying && activeState != null)
            {
                GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
                style.fontSize = 10;
                style.alignment = TextAnchor.MiddleCenter;

                Handles.Label(transform.position + Vector3.up, $"{activeState.Name}", style);

                style.fontSize = 8;
                Handles.Label(transform.position + Vector3.up*.8f, $"{activeState.debugInfo}", style);
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
        public string debugInfo { get; set; }

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
        public string debugInfo { get; set; }
    }



#if UNITY_EDITOR
    [CustomEditor(typeof(StateMachineBehavior), true)]
    [CanEditMultipleObjects]
    public class StateMachineBehaviorEditor : MarToolsEditor<StateMachineBehavior>
    {
        public override void OnInspectorGUI()
        {
            if (Application.isPlaying)
            {
                foreach (var item in script.AllStates)
                {
                    GUI.color = item.Value == script.activeState ? Color.green : Color.white;
                    GUILayout.Label(item.Key);
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

