using UnityEngine;

namespace MarTools
{
    public class TweenShakeRotation : TweenCore
    {
        public float amplitude = 10;
        public float frequency = 10;

        public override void SetPose(float t)
        {
            float x = Mathf.PerlinNoise1D(t*frequency).Remap(0, 1, -1, 1);
            float y = Mathf.PerlinNoise1D(t*frequency+84984).Remap(0, 1, -1, 1);
            float z = Mathf.PerlinNoise1D(t*frequency+100598).Remap(0,1,-1,1);

            Vector3 deviation = new Vector3(x, y, z);

            transform.localRotation = Quaternion.Euler(deviation * amplitude * (1-t));
        }
    }
}
