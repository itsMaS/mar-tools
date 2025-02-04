using DG.Tweening.Core;
using UnityEngine;
using UnityEngine.Events;

namespace MarTools
{
    public class TweenFloat : TweenCore
    {
        public UnityEvent<float> OnTweenFloat;

        public override void SetPose(float t)
        {
            OnTweenFloat.Invoke(t);
        }
    }
}
