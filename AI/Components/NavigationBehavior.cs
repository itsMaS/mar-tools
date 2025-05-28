using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MarTools.AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavigationBehavior : MonoBehaviour
    {
        public Transform followTarget;
        public Vector3 targetPosition { get; private set; } = Vector3.zero;

        public float startingMovementSpeed = 5;

        public string movementSpeedAnimatorParameter;
        public NavMeshAgent agent
        {
            get
            {
                if (!_agent) _agent = GetComponent<NavMeshAgent>();
                return _agent;
            }
        }
        private NavMeshAgent _agent;

        Animator an;

        private void Awake()
        {
            an = GetComponentInChildren<Animator>();
            agent.speed = startingMovementSpeed;
        }

        public void Patrol(List<Vector3> Points)
        {

        }

        public void Follow(Transform target)
        {
            followTarget = target;
        }

        public void MoveTo(Vector3 position)
        {
            targetPosition = position;
            agent.SetDestination(position);
        }

        private Vector3 patrolStart;

        public void PatrolRange(float range)
        {

        }

        private void Update()
        {
            if(an && agent.enabled && movementSpeedAnimatorParameter.Length > 0)
            {
                an.SetFloat(movementSpeedAnimatorParameter, agent.velocity.MaskY().magnitude);
            }

            if(followTarget && agent.enabled)
            {
                agent.SetDestination(followTarget.position);
                targetPosition = followTarget.position;
            }
        }

        public void Stop()
        {
            agent.enabled = false;
            an.SetFloat(movementSpeedAnimatorParameter, 0);
        }
        public void Resume()
        {
            agent.enabled = true;
        }

        public float movementSpeed
        {
            get
            {
                return agent.speed;
            }
            set
            {
                agent.speed = value;
            } 
        }

        public float GetDistanceToTarget()
        {
            if(agent.enabled)
                return agent.remainingDistance;
            else return 0;
        }

        public bool agentEnabled => agent.enabled;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(NavigationBehavior))]
    public class NavigationBehaviorEditor : MarToolsEditor<NavigationBehavior>
    {
        Vector3 worldCursorPosition;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }

        private void OnSceneGUI()
        {
            if(Application.isPlaying)
            {
                UpdateWorldCursorPosition();

                Handles.DrawSolidDisc(worldCursorPosition, Vector3.up, 0.5f);

                var e = Event.current;

                if(e.type == EventType.MouseDown && e.button == 0)
                {
                    script.MoveTo(worldCursorPosition);


                    e.Use();
                }
            }
        }

        private void UpdateWorldCursorPosition()
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, LayerMask.GetMask("Ground"), QueryTriggerInteraction.Ignore))
            {
                worldCursorPosition = hitInfo.point;
            }
        }
    }
#endif
}
