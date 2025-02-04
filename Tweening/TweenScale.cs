namespace MarTools
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    
    public class TweenScale : TweenCore
    {
        public bool use3D = false;

        public float startingScale = 1;
        public float endingScale = 1.1f;

        public Vector3 startingScale3D = Vector3.one;
        public Vector3 endingScale3D = Vector3.one;

        public override void SetPose(float t)
        {
            if(use3D)
            {
                transform.localScale = Vector3.Lerp(startingScale3D, endingScale3D, t);
            }
            else
            {
                transform.localScale = Vector3.one * Mathf.LerpUnclamped(startingScale, endingScale, t);
            }

        }
    }
    
}