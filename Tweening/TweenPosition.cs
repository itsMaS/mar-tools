using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TweenPosition : TweenCore
{
    public Vector3 from;
    public Vector3 to;
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
}
