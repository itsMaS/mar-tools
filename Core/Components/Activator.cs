namespace MarTools
{
    using UnityEngine;
    using UnityEngine.Events;

    public class Activator : MonoBehaviour
    {
        public UnityEvent OnActivated;
        public UnityEvent OnDeactivated;
        
        public void Activate()
        {
            OnActivated.Invoke();
        }
        public void Deactivate()
        {
            OnDeactivated.Invoke();
        }

        /// <summary>
        /// Activate or deactivate with a single method
        /// </summary>
        /// <param name="value"></param>
        public void Activate(bool value)
        {
            if(value)
            {
                Activate();
            }
            else
            {
                Deactivate();
            }
        }
    }
}
