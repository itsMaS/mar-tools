namespace MarTools
{
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

        [SerializeField] bool scaleWithDistance = true;
    
        public Vector2 fontSizeBounds = new Vector2(1, 30);
    
        private void OnDrawGizmos()
        {
            DrawText($"<b>{gameObject.name.ToUpper()}</b>\n{description}", transform.position);
        }
    
        private void DrawText(string text, Vector3 position)
        {
            GUIStyle style = new GUIStyle();
    
            float scale = HandleUtility.GetHandleSize(position);
            int fontSize = scaleWithDistance ? Mathf.RoundToInt(size / scale) : Mathf.RoundToInt(size);
    
            float distanceToEdge = Mathf.Min(Mathf.Abs(fontSizeBounds.x-fontSize), Mathf.Abs(fontSizeBounds.y-fontSize));
            Color color = Color.white;
            color.a = Mathf.InverseLerp(20, 10, distanceToEdge);

            Handles.color = color;

            //if(fontSize > fontSizeBounds.x && fontSize < fontSizeBounds.y)
            //{
                style.fontSize = fontSize;
                style.alignment = TextAnchor.MiddleCenter;
                style.normal.textColor = color;
    
                Handles.Label(position, text, style);
            //}
        }
    #endif
    }
    
}