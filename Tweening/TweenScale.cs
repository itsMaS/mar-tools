namespace MarTools
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    
    public class TweenScale : TweenCore
    {
        public float startingScale = 1;
        public float endingScale = 1.1f;
    
        public override void SetPose(float t)
        {
            transform.localScale = Vector3.one * Mathf.LerpUnclamped(startingScale, endingScale, t);
        }
    }
    
}