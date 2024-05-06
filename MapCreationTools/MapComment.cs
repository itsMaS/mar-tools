using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MapComment : MonoBehaviour
{
    public enum Type
    {
        Environment,
    }

#if UNITY_EDITOR
    [SerializeField, TextArea] string description;
    [SerializeField] float size = 10;

    private void OnDrawGizmos()
    {
        DrawText($"<b>{gameObject.name.ToUpper()}</b>\n{description}", transform.position);
    }

    private void DrawText(string text, Vector3 position)
    {
        GUIStyle style = new GUIStyle();
        float scale = HandleUtility.GetHandleSize(position);
        style.fontSize = Mathf.RoundToInt(size / scale);
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = Color.white;

        Handles.Label(position, text, style);
    }
#endif
}
