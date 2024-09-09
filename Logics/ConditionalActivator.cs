using MarTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ConditionalActivator : MonoBehaviour
{
    public UnityEvent OnActivate;
    [SerializeReference] IConditional condition;
    public void Activate()
    {
        if(condition != null && condition.IsTrue())
        {
            OnActivate.Invoke();
        }
    }
}
