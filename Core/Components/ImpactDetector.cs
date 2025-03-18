using UnityEngine;
using UnityEngine.Events;

namespace MarTools
{
    [RequireComponent(typeof(Rigidbody))]
    public class ImpactDetector : MonoBehaviour
    {
        public UnityEvent<Collision> OnTriggerImpact;
        public float impactThreshold = 1;

        Rigidbody rb;

        public bool isBreakable = false;

        private bool broken = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (isBreakable && broken) return;

            if ((collision.relativeVelocity.magnitude > impactThreshold))
            {
                broken = true;
                OnTriggerImpact.Invoke(collision);
            }
        }
    }
}
