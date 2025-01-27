using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MarTools
{
    public class GateBehavior : MonoBehaviour
    {
        public UnityEvent<GameObject> OnPassAB;
        public UnityEvent<GameObject> OnPassBA;

        [SerializeField, HideInInspector] BoxCollider gateA;
        [SerializeField, HideInInspector] BoxCollider gateB;

        public enum State 
        {
            Outside,
            EnteredThroughA,
            EnteredThroughB,
        }

        public Dictionary<GameObject, State> States = new Dictionary<GameObject, State>();

        public void Enter(Collider col, GameObject owner)
        {
            if(!States.TryGetValue(owner, out State state))
            {
                States.Add(owner, col == gateA ? State.EnteredThroughA : State.EnteredThroughB);
            }
        }

        public void Exit(Collider col, GameObject owner)
        {

            if (States.TryGetValue(owner, out State state))
            {
                if(col == gateA)
                {

                    if (state == State.EnteredThroughB)
                    {
                        OnPassBA?.Invoke(owner);
                        States.Remove(owner);
                    }
                }
                else if (col == gateB) 
                {

                    if (state == State.EnteredThroughA)
                    {
                        OnPassAB?.Invoke(owner);
                        States.Remove(owner);
                    }
                }
            }
        }

        private void Reset()
        {
            gateA = gameObject.AddComponent<BoxCollider>();
            gateA.isTrigger = true;

            gateA.center = new Vector3(0, 0, 2);

            gateB = gameObject.AddComponent<BoxCollider>();
            gateB.isTrigger = true;

            gateB.center = new Vector3(0, 0, -2);
        }

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.TransformPoint(gateA.center), "Gate A");
            UnityEditor.Handles.Label(transform.TransformPoint(gateB.center), "Gate B");
#endif
        }
    }
}
