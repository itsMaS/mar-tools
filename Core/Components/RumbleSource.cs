#if UNITY_CINEMACHINE
using Unity.Cinemachine;
using UnityEngine;

namespace MarTools
{ 
    public class RumbleSource : MonoBehaviour
    {
        [Header("Impulse Settings")]
        public CinemachineImpulseSource impulseSource;

        [Header("Rumble Settings")]
        [Tooltip("The interval (in seconds) between each rumble.")]
        public float rumbleInterval = 0.5f;

        [Tooltip("The duration of each rumble in seconds.")]
        public float rumbleDuration = 0.2f;

        [Tooltip("The strength of the rumble.")]
        public float rumbleIntensity = 1f;

        public bool isRumbling;
        private float rumbleTimer;

        void Start()
        {
            if (impulseSource == null)
            {
                impulseSource = GetComponent<CinemachineImpulseSource>();
                if (impulseSource == null)
                {
                    Debug.LogError("CinemachineImpulseSource is missing. Please attach one to this GameObject.");
                }
            }
        }

        void Update()
        {
            if (isRumbling)
            {
                rumbleTimer -= Time.deltaTime;

                if (rumbleTimer <= 0)
                {
                    TriggerRumble();
                    rumbleTimer = rumbleInterval;
                }
            }
        }

        public void StartRumble()
        {
            isRumbling = true;
            rumbleTimer = 0f; // Start immediately
        }

        public void StopRumble()
        {
            isRumbling = false;
        }

        private void TriggerRumble()
        {
            if (impulseSource != null)
            {
                impulseSource.ImpulseDefinition.AmplitudeGain = rumbleIntensity;
                impulseSource.GenerateImpulse();
            }
        }
    }
}
#endif