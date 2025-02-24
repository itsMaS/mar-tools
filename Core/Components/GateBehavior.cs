using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.GlobalIllumination;

namespace MarTools
{
    [RequireComponent(typeof(Trigger))]
    public class GateBehavior : MonoBehaviour
    {
        public UnityEvent<GameObject> OnPassAB;
        public UnityEvent<GameObject> OnPassBA;

        public Dictionary<GameObject, Direction> DirectionTracking = new Dictionary<GameObject, Direction>();

        public enum Direction
        {
            A,
            B,
        }

        Trigger trigger;

        private void Awake()
        {
            trigger = GetComponent<Trigger>();

            trigger.OnEntered.AddListener(Entered);
            trigger.OnExited.AddListener(Exited);
        }

        private void Exited(GameObject arg0)
        {
            //Debug.Log($"{arg0.name} exited through {(GetDirection(arg0) == Direction.A ? "A" : "B")}");

            var exitDirection = GetDirection(arg0);
            if(exitDirection != DirectionTracking[arg0])
            {
                switch (exitDirection)
                {
                    case Direction.A:
                        //Debug.Log($"{arg0.name} Pass B->A");
                        OnPassBA.Invoke(arg0);
                        break;
                    case Direction.B:
                        //Debug.Log($"{arg0.name} Pass A->B");
                        OnPassAB.Invoke(arg0);
                        break;
                    default:
                        break;
                }
            }

            DirectionTracking.Remove(arg0);
        }

        private void Entered(GameObject arg0)
        {
            //Debug.Log($"{arg0.name} entered through {(GetDirection(arg0) == Direction.A ? "A" : "B")}");
            DirectionTracking[arg0] = GetDirection(arg0);
        }

        public Direction GetDirection(GameObject go)
        {
            Vector3 dist = go.transform.position - transform.position;
            float dot = Vector3.Dot(dist.normalized, transform.forward);
            return dot < 0 ? Direction.A : Direction.B;
        }


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            GizmosUtilities.DrawArrow(transform.position - transform.forward/2, transform.position + transform.forward/2);

#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position - transform.forward*0.6f, "A");
            UnityEditor.Handles.Label(transform.position + transform.forward*0.6f, "B");
#endif
        }
    }
}
