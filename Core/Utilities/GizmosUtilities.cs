namespace MarTools
{
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System;

    public static class GizmosUtilities
    {
        public static void DrawArrow(Vector3 start, Vector3 end)
        {
            Vector3 direction = (end - start).normalized;
            float arrowHeadLength = 0.25f;
            float arrowHeadAngle = 20.0f;

            Gizmos.DrawLine(start, end);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);

            Gizmos.DrawLine(end, end + right * arrowHeadLength);
            Gizmos.DrawLine(end, end + left * arrowHeadLength);
        }

        internal static void DrawLine(List<Vector3> controlPoints, bool looping)
        {
            for (int i = 1; i < controlPoints.Count; i++)
            {
                Gizmos.DrawLine(controlPoints[i], controlPoints[i - 1]);
            }
            if(looping)
            {
                Gizmos.DrawLine(controlPoints[0], controlPoints[controlPoints.Count - 1]);
            }
        }
    }

    public static class DebugUtitlities
    {
        public static void DrawCircle(Vector3 center, Vector3 normal, float radius, int resolution = 10)
        {
            // Ensure the normal is normalized
            normal.Normalize();

            // Create a vector orthogonal to the normal (up) vector
            Vector3 right = Vector3.Cross(normal, Vector3.up);
            if (right == Vector3.zero)
            {
                right = Vector3.Cross(normal, Vector3.forward);
            }

            right.Normalize();
            Vector3 forward = Vector3.Cross(normal, right);

            int points = Mathf.CeilToInt(resolution * radius * radius);
            float angleStep = 360f / points;

            Vector3 prevPoint = center + right * radius;

            for (int i = 1; i <= points; i++)
            {
                float angle = i * angleStep;
                Vector3 nextPoint = center + (right * Mathf.Cos(angle * Mathf.Deg2Rad) + forward * Mathf.Sin(angle * Mathf.Deg2Rad)) * radius;
                Debug.DrawLine(prevPoint, nextPoint, Color.red, 0, false);
                prevPoint = nextPoint;
            }
        }
    }

#if UNITY_EDITOR
    public static class HandlesUtilities
    {
        public static void DrawArrow(Vector3 start, Vector3 end)
        {
            Vector3 direction = (end - start).normalized;
            float arrowHeadLength = 0.25f;
            float arrowHeadAngle = 20.0f;

            Handles.DrawLine(start, end);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);

            Handles.DrawLine(end, end + right * arrowHeadLength);
            Handles.DrawLine(end, end + left * arrowHeadLength);
        }

        public static void DrawVisibility(this Transform transform, float angle, float radius, Color color)
        {
            Color prev = Handles.color;
            Handles.color = color;

            Matrix4x4 rotationMatrix = Matrix4x4.Rotate(Quaternion.AngleAxis(-angle/2, Vector3.up));
            // Rotate the vector using the matrix
            Vector3 rotatedVector = rotationMatrix.MultiplyPoint3x4(Vector3.ProjectOnPlane(transform.forward, Vector3.up));
            Handles.DrawSolidArc(transform.position, Vector3.up, rotatedVector, angle, radius);
            Handles.color = prev;

        }
    }
#endif
}
