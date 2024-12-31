using UnityEngine;
using UnityEngine.Events;

public class OnGameObjectDestroyed : MonoBehaviour
{
    public UnityEvent OnDestroyed;

    private void OnDestroy()
    {
        OnDestroyed.Invoke();
    }
}
