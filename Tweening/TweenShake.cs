namespace MarTools
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class TweenShake : TweenCore
    {
        public float frequency = 1;
        public float amplitude = 0.5f;

        public override void SetPose(float t)
        {
            float x = (Mathf.PerlinNoise1D((t*frequency)+478) - 0.5f) * 2;
            float y = (Mathf.PerlinNoise1D((t*frequency)+256)) - 0.5f * 2;
            float z = (Mathf.PerlinNoise1D((t*frequency)+4898)) - 0.5f * 2;

            transform.localPosition = new Vector3(x, y, z) * amplitude;

            if(t == 0 || t == 1)
            {
                transform.localPosition = Vector3.zero;
            }
        }
    }
}
