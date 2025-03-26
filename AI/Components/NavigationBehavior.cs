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
        public float startingMovementSpeed = 5;

        public string movementSpeedAnimatorParameter;
        NavMeshAgent agent;

        Animator an;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            an = GetComponentInChildren<Animator>();

            agent.speed = startingMovementSpeed;
        }

        public void Patrol(List<Vector3> Points)
        {

        }

        public void Follow(Transform target)
        {

        }

        public void MoveTo(Vector3 position)
        {
            agent.SetDestination(position);
        }

        private Vector3 patrolStart;

        public void PatrolRange(float range)
        {

        }

        private void Update()
        {
            if(an && agent.enabled)
            {
                an.SetFloat(movementSpeedAnimatorParameter, agent.velocity.MaskY().magnitude);
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
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(NavigationBehavior))]
    public class NavigationBehaviorEditor : MarToolsEditor<NavigationBehavior>
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
#endif
}
