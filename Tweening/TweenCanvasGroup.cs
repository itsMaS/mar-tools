namespace MarTools
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [RequireComponent(typeof(CanvasGroup))]
    public class TweenCanvasGroup : TweenCore
    {
        public float from = 0;
        public float to = 1;

        public override void SetPose(float t)
        {
            GetComponent<CanvasGroup>().alpha = Mathf.Lerp(from,to,t);
        }
    }
}
