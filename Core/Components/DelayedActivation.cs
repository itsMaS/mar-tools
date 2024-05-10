namespace MarTools
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    
    public class DelayedActivation : MonoBehaviour
    {
        public UnityEvent OnActivated;
        public float delay = 1;
        public bool activateOnEnable = false;

        private void OnEnable()
        {
            if (activateOnEnable) ActivateDelayed();
        }

        public void Activate()
        {
            OnActivated.Invoke();
        }
    
        public void ActivateDelayed()
        {
            this.DelayedAction(delay, Activate);
        }
    }
    
}