using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public static class Utilities
{
    public static Coroutine DelayedAction(this MonoBehaviour behavior, float time, Action onComplete, Action<float> onUpdate = null, bool timeScaled = true, Ease ease = Ease.InOutQuad)
    {
        if (behavior.enabled && behavior.gameObject.activeInHierarchy)
            return behavior.StartCoroutine(DelayedCoroutine(time, onComplete, onUpdate, timeScaled, ease));
        else
            return null;
    }

    private static IEnumerator DelayedCoroutine(float duration, Action onComplete, Action<float> onUpdate, bool timeScaled = true, Ease ease = Ease.InOutSine)
    {
        float elapsed = 0;
        float t = 0;
        while (elapsed < duration) 
        {
            elapsed += timeScaled ? Time.deltaTime : Time.unscaledDeltaTime;
            t = elapsed / duration;
            onUpdate?.Invoke(Eases[ease].Invoke(t));
            yield return null;
        }
        onComplete?.Invoke();
    }

    public static T FindClosest<T>(this IEnumerable<T> collection, Vector3 target, Func<T, Vector2> PositionFunction, out float closestDistance)
    {
        closestDistance = float.MaxValue;
        T closestElement = collection.First();

        foreach (var item in collection)
        {
            float distance = Vector3.Distance(PositionFunction.Invoke(item), target);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestElement = item;
            }
        }

        return closestElement;
    }

    public static Vector2 FindClosest(this IEnumerable<Vector2> collection, Vector2 target, out float closestDistance)
    {
        return FindClosest<Vector2>(collection, target, item => item, out closestDistance);
    }

    public enum Ease
    {
        Linear,
        InOutSine,
        InOutQuad,
        OutBack,
        Pulse,
        OutBounce,
    }

    public static Dictionary<Ease, Func<float, float>> Eases = new Dictionary<Ease, Func<float, float>>
    {
        {Ease.Linear, t1 => Linear(t1)},
        {Ease.InOutSine, t1 => InOutSine(t1)},
        {Ease.InOutQuad, t1 => InOutQuad(t1)},
        {Ease.OutBack, t1 => OutBack(t1)},
        {Ease.Pulse, t1 => Pulse(t1)},
        {Ease.OutBounce, t1 => OutBounce(t1)},
    };

    public static float Linear(float t)
    {
        return t;
    }
    public static float InOutSine(float t)
    {
        return -0.5f * (Mathf.Cos(Mathf.PI * t) - 1);
    }
    public static float InOutQuad(float t)
    {
        t *= 2;
        if (t < 1) return 0.5f * t * t;
        t--;
        return -0.5f * (t * (t - 2) - 1);
    }
    public static float OutBack(float t)
    {
        float s = 1.70158f;  // Overshoot amount, can be adjusted
        return ((t -= 1) * t * ((s + 1) * t + s) + 1);
    }

    public static float Pulse(float t)
    {
        return Mathf.Sin(t * Mathf.PI);
    }

    public static float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    public static int LayerMaskToLayer(LayerMask layerMask)
    {
        int layerNumber = -1;
        int mask = layerMask.value;
        for (int i = 0; i < 32; i++)
        {
            if ((mask & (1 << i)) != 0)
            {
                layerNumber = i;
                break;
            }
        }
        return layerNumber;
    }
    public static float OutBounce(float t)
    {
        if (t < 1 / 2.75f)
        {
            return 7.5625f * t * t;
        }
        else if (t < 2 / 2.75f)
        {
            t -= 1.5f / 2.75f;
            return 7.5625f * t * t + 0.75f;
        }
        else if (t < 2.5 / 2.75f)
        {
            t -= 2.25f / 2.75f;
            return 7.5625f * t * t + 0.9375f;
        }
        else
        {
            t -= 2.625f / 2.75f;
            return 7.5625f * t * t + 0.984375f;
        }
    }

    public static Vector3 MaskY(this Vector3 vector, float yValue = 0)
    {
        vector.y = yValue;
        return vector;
    }
}
