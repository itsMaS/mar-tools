namespace MarTools
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    
    public class TweenRotation : TweenCore
    {
        public Vector3 from;
        public Vector3 to;
    
        public override void SetPose(float t)
        {
            if(local)
            {
                transform.localRotation = Quaternion.Euler(Vector3.LerpUnclamped(from, to, t));
            }
            else
            {
                transform.rotation = Quaternion.Euler(Vector3.LerpUnclamped(from, to, t));
            }
        }
    }
    
}