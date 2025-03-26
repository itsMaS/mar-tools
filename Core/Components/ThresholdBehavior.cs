using UnityEngine;
using UnityEngine.Events;

namespace MarTools 
{
    public class ThresholdBehavior : MonoBehaviour
    {
        public UnityEvent<bool> OnValueChanged;
        public UnityEvent OnValueOn;
        public UnityEvent OnValueOff;

        public float threshold = 0.5f;

        public void SetValue(float value)
        {
            if(value > threshold)
            {
                OnValueChanged.Invoke(true);
                OnValueOn.Invoke();
            }
            else
            {
                OnValueChanged.Invoke(false);
                OnValueOff.Invoke();
            }
        }
    }
}
