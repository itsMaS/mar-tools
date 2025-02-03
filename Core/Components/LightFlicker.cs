using UnityEngine;

namespace MarTools
{
    [RequireComponent(typeof(Light))]
    public class LightFlicker : MonoBehaviour
    {
        private Light lightSource;
        private float baseIntensity;
        private float timeSinceLastFlicker;

        [Header("Flicker Settings")]
        [Tooltip("The minimum intensity the light can flicker to.")]
        [Range(0f, 8f)]
        public float minIntensity = 0.5f;

        [Tooltip("The maximum intensity the light can flicker to.")]
        [Range(0f, 8f)]
        public float maxIntensity = 2.0f;

        [Tooltip("The speed of the flickering effect.")]
        public float flickerSpeed = 0.1f;

        [Tooltip("Smooth flickering using Perlin noise.")]
        public bool useSmoothFlicker = true;

        private float noiseSeed;

        void Awake()
        {
            lightSource = GetComponent<Light>();
            baseIntensity = lightSource.intensity;
            noiseSeed = Random.Range(0f, 100f);
        }

        void Update()
        {
            if (useSmoothFlicker)
            {
                // Smooth flickering using Perlin noise
                float noise = Mathf.PerlinNoise(noiseSeed, Time.time * flickerSpeed);
                lightSource.intensity = baseIntensity * Mathf.Lerp(minIntensity, maxIntensity, noise);
            }
            else
            {
                // Random flickering
                timeSinceLastFlicker += Time.deltaTime;
                if (timeSinceLastFlicker >= flickerSpeed)
                {
                    lightSource.intensity = Random.Range(minIntensity, maxIntensity);
                    timeSinceLastFlicker = 0f;
                }
            }
        }
    }
}