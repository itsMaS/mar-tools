using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class MaterialParameterLerp : MonoBehaviour
{
    public bool activateOnEnable;

    public float duration = 0.5f;
    public string materialID;
    public int materialIndex = 0;

    public float from = 0;
    public float to = 1;

    public bool relative = true;

    private void OnEnable()
    {
        if(activateOnEnable)
        {
            PlayForwards();
        }
    }

    public void PlayForwards()
    {
        Material mat = GetComponent<Renderer>().materials[materialIndex];

        float origin = relative ? mat.GetFloat(materialID) : from;
        this.DelayedAction(duration, null, value => mat.SetFloat(materialID, Mathf.Lerp(origin, to, value)));
    }
    public void PlayBackwards()
    {
        Material mat = GetComponent<Renderer>().materials[materialIndex];
        float origin = relative ? mat.GetFloat(materialID) : to;
        this.DelayedAction(duration, null, value => mat.SetFloat(materialID, Mathf.Lerp(origin, from, value)));
    }
}
