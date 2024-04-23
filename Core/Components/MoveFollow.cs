using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarTools
{
    public class MoveFollow : MonoBehaviour
    {
        public float stopDistance = 1;
        public float speed = 5;
        public Transform target;

        private void Update()
        {
            if (target)
            {
                Vector3 toTarget = target.position - transform.position;

                if(toTarget.magnitude > stopDistance)
                {
                    float moveAmount = Mathf.Min(toTarget.magnitude/Time.deltaTime, speed);
                    transform.position += toTarget.normalized * moveAmount * Time.deltaTime;
                }
            }
        }

        public void SetTarget(Transform target) 
        {
            this.target = target;
        }
    }
}
