using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TweenPosition : TweenCore
{
    public Vector3 from;
    public Vector3 to;

    public bool tweenX = true;
    public bool tweenY = true;
    public bool tweenZ = true;

    public override void SetPose(float t)
    {
        if(local)
        {
            Vector3 position = relative ? originPosition : Vector3.zero;
            transform.localPosition = position + Vector3.LerpUnclamped(from, to, t);
        }
        else
        {
            transform.position = Vector3.LerpUnclamped(from, to, t);
        }
    }
}
