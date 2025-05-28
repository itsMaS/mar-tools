namespace MarTools
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [RequireComponent(typeof(SkinnedMeshRenderer))]
    public class TweenShapeKey : TweenCore
    {
        public int index;
        public float from;
        public float to;

        private SkinnedMeshRenderer mr;

        protected override void Reset()
        {
            base.Reset();
        }

        public override void SetPose(float t)
        {
            if (!mr) mr = GetComponent<SkinnedMeshRenderer>();

            mr.SetBlendShapeWeight(index, Mathf.Lerp(from, to, t));
        }

        protected override void OnDrawGizmos()
        {
        }
    }
    
}