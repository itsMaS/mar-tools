using System.Runtime.InteropServices;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace MarTools
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleSystemFollowCurve : MonoBehaviour
    {
        public LineBehavior line;
        public ParticleSystem particleSystem;
        private ParticleSystem.Particle[] particles;
        public Vector3 rotationOffset;

        Vector3 offset;

        private bool scattering = false;


        private void Awake()
        {
            particleSystem = GetComponent<ParticleSystem>();
        }

        private void Start()
        {
        }

        void LateUpdate()
        {
            if (particles == null || particles.Length < particleSystem.main.maxParticles)
                particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];

            int count = particleSystem.GetParticles(particles);

            if(scattering)
            {
                for (int i = 0; i < count; i++)
                {
                    float lifetimePercent = 1 - (particles[i].remainingLifetime / particles[i].startLifetime);

                    var state = line.GetPositionAndNormalAt(lifetimePercent);
                    Vector3 targetPosition = state.Item1;

                    Quaternion initial = Quaternion.Euler(rotationOffset);
                    Quaternion combined = Quaternion.LookRotation(state.Item2.Item2, Vector3.up) * initial;

                    Vector3 noiseOffset = particleSystem.noise.strength.Evaluate(lifetimePercent) * new Vector3(1, 0, 1);

                    float seed01 = particles[i].randomSeed / (float)uint.MaxValue;

                    particles[i].rotation3D = combined.eulerAngles;
                    particles[i].position += Time.deltaTime * state.Item2.Item2.normalized * 5 * (seed01 > 0.5f ? 1 : -1);
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    float lifetimePercent = 1 - (particles[i].remainingLifetime / particles[i].startLifetime);

                    var state = line.GetPositionAndNormalAt(lifetimePercent);
                    Vector3 targetPosition = state.Item1;

                    Quaternion initial = Quaternion.Euler(rotationOffset);
                    Quaternion combined = Quaternion.LookRotation(state.Item2.Item1, Vector3.up) * initial;

                    Vector3 noiseOffset = particleSystem.noise.strength.Evaluate(lifetimePercent) * new Vector3(1, 0, 1);

                    float seed01 = particles[i].randomSeed / (float)uint.MaxValue;

                    particles[i].rotation3D = combined.eulerAngles;
                    particles[i].position = targetPosition + offset + noiseOffset * seed01.Remap01(-1,1);
                }
            }

            particleSystem.SetParticles(particles, count);
        }

        public void Scatter()
        {
            if (scattering) return;

            scattering = true;
            particleSystem.emissionRate = 0;

            Destroy(gameObject, 5);
        }
    }
}
