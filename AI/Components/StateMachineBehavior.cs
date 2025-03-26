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


        [HideInInspector] public int startingStateIndex;

        protected virtual void Awake()
        {
            Initialize();

            foreach (var item in this.GetFieldsOfType<IState>(true))
            {
                IState state = item.GetValue(this) as IState;

                AllStates.Add(item.Name, state);

                AllStatesIndexed.Add(state);
            }
        }

        protected virtual void Initialize()
        {

        }

        private void Start()
        {
            ChangeState(AllStatesIndexed[startingStateIndex]);
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
            Debug.Log(stateName);

            if(AllStates.TryGetValue(stateName, out IState newState))
            {
                ChangeState(newState);
            }
            else
            {
                Debug.LogWarning($"There is not state with the name of {stateName}");
            }
        }

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
                    GUI.color = item.Value == script.activeState ? Color.green : Color.white;
                    GUILayout.Label(item.Key);
                }
            }
            else
            {
                var states = script.GetFieldsOfType<IState>(true);

                EditorGUI.BeginChangeCheck();

                GUILayout.Label("Starting state:");
                script.startingStateIndex = EditorGUILayout.Popup(script.startingStateIndex, states.ConvertAll<string>(x => x.Name).ToArray());

                if (EditorGUI.EndChangeCheck()) 
                {
                    EditorUtility.SetDirty(script);
                }
                
                base.OnInspectorGUI();
            }
        }
    }
#endif
}

