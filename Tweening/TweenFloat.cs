using UnityEngine;
using UnityEngine.Events;

namespace MarTools
{
    public class TweenFloat : TweenCore
    {
        public UnityEvent<float> OnTweenFloat;

        public float from = 0;
        public float to = 1;

        public override void SetPose(float t)
        {
            OnTweenFloat.Invoke(Mathf.LerpUnclamped(from, to, t));
        }
    }
}
