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
        bool thresholdMet = false;

        public void SetValue(float value)
        {
            bool newValue = value > threshold;

            if(newValue != thresholdMet)
            {
                thresholdMet = newValue;
                if (thresholdMet)
                {
                    OnValueOn.Invoke();
                }
                else
                {
                    OnValueOff.Invoke();
                }
                OnValueChanged.Invoke(true);
            }
        }
    }
}
