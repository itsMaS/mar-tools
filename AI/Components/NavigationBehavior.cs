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
        public string movementSpeedAnimatorParameter;
        NavMeshAgent agent;

        Animator an;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            an = GetComponentInChildren<Animator>();
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
            if(an)
            {
                an.SetFloat(movementSpeedAnimatorParameter, agent.velocity.MaskY().magnitude);
            }
        }

        public void Stop()
        {
            agent.enabled = false;
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
