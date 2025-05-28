using UnityEngine;
using UnityEngine.Events;

// Low execution order for a situation where rate of change is negative allowing the end of the frame to have
// a value of 1, because if rate of change calculation is last, no matter how much you increase it will always be 1-defaultRateOfChange
[DefaultExecutionOrder(-200)]
public class FloatAccumulator : MonoBehaviour
{
    public UnityEvent<float> OnValueUpdated;

    public float manipulationMultiplier = 1;
    public float defaultRateOfChange = 0;
    public float value { get; private set; }

    public void Increase(float amount)
    {
        Change(amount);
    }
    public void Decrease(float amount)
    {
        Change(-amount);
    }

    private void Change(float amount)
    {
        value += amount * manipulationMultiplier;
        value = Mathf.Clamp01(value);
        
        OnValueUpdated.Invoke(value);
    }

    private void Update()
    {
        if(Mathf.Abs(defaultRateOfChange) > 0)
        {
            Change(defaultRateOfChange*Time.deltaTime);
        }
    }
}
