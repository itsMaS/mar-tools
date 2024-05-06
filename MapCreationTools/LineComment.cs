using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LineComment : MonoBehaviour
{
    public LineBehavior line;
    public float speed = 4.8f;

    private void Reset()
    {
        line = GetComponentInParent<LineBehavior>();
    }

    private void OnDrawGizmos()
    {
        float distance = line.GetLength();
        float time = distance / speed;

        string text = $"{distance:.00}m\n{time:.00}s to pass at {speed:.00}m/s";
        Handles.Label(transform.position, text, new GUIStyle()
        {
            alignment = TextAnchor.MiddleCenter,
            normal = new GUIStyleState
            {
                textColor = Color.white*0.5f,
            }
        });
    }
}
