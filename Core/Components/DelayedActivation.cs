using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DelayedActivation : MonoBehaviour
{
    public UnityEvent OnActivated;
    public float delay = 1;

    public void Activate()
    {
        OnActivated.Invoke();
    }

    public void ActivateDelayed()
    {
        this.DelayedAction(delay, Activate);
    }
}
