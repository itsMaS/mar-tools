namespace MarTools
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UIElements;
    
    public class CompositeActivator : MonoBehaviour
    {
        public UnityEvent OnActivate;
        public UnityEvent OnDeactivate;
        public bool activateOnce = true;
    
    
        public int required = 0;
        private int amount = 0;
        private bool activated = false;
    
        private void Start()
        {
            Check();
        }
    
        public void Add()
        {
            amount += 1;
            Check();
        }
    
        public void Remove()
        {
            amount -= 1;
            Check();
        }
    
        private void Check()
        {
            if (activated && activateOnce) return;
    
            if (amount >= required)
            {
                if (!activated)
                {
                    activated = true;
                    OnActivate.Invoke();
                }
            }
            else
            {
                if(activated)
                {
                    activated = false;
                    OnDeactivate.Invoke();
                }
            }
        }
    }
    
}