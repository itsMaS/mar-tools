using UnityEngine;
using UnityEngine.Events;

public class FloatStorageBehavior : MonoBehaviour
{
    public UnityEvent OnFilled;
    public UnityEvent<float> OnAdded;

    public float progress { get; private set; }

    public void AddFraction(int division)
    {
        progress += 1f / division;
        OnAdded.Invoke(progress);

        if(progress >= 1)
        {
            OnFilled.Invoke();
        }
    }
}
