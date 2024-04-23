using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
    public static void DelayedAction(this MonoBehaviour behavior, float time, Action onComplete, Action<float> onUpdate = null, bool timeScaled = true)
    {
        behavior.StartCoroutine(DelayedCoroutine(time, onComplete, onUpdate, timeScaled));
    }

    private static IEnumerator DelayedCoroutine(float duration, Action onComplete, Action<float> onUpdate, bool timeScaled = true)
    {
        float elapsed = 0;
        float t = 0;
        while (elapsed < duration) 
        {
            elapsed += timeScaled ? Time.deltaTime : Time.unscaledDeltaTime;
            t = elapsed / duration;
            onUpdate?.Invoke(t);
            yield return null;
        }
        onComplete?.Invoke();
    }
}
