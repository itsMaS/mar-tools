namespace MarTools
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    
    public class TweenPosition : TweenCore
    {
        public Vector3 from;
        public Vector3 to;

        protected override void Reset()
        {
            base.Reset();

            from = transform.localPosition;
        }

        public override void SetPose(float t)
        {
            if(local)
            {
                transform.localPosition = Vector3.LerpUnclamped(from, to, t);
            }
            else
            {
                transform.position = Vector3.LerpUnclamped(from, to, t);
            }
        }

        public override float GetDistance(out string units)
        {
            units = "units/s";
            return Vector3.Distance(from, to);
        }

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if(transform.parent)
            {
                GizmosUtilities.DrawArrow(transform.parent.TransformPoint(from), transform.parent.TransformPoint(to));
            }
            else
            {
                GizmosUtilities.DrawArrow(from, to);
            }

        }
    }
    
}