namespace MarTools
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class TweenMoveAlongPath : TweenCore
    {
        public LineBehavior lineBehavior;
        [Range(0,1)] public float offset = 0;

        public bool rotateAlongNormal = true;

        Quaternion target;

        public override void SetPose(float t)
        {
            if (!lineBehavior) return;

            var p = lineBehavior.GetPositionAndNormalAt(t + offset);
            transform.position = p.Item1;

            if(rotateAlongNormal)
            {
                target = Quaternion.LookRotation(p.Item2.Item1, Vector3.up);
            }
        }

        private void Update()
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, target, Time.deltaTime * 20);
        }
    }
}
