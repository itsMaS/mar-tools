namespace MarTools
{
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System;
    using UnityEngine.UIElements;

    public static class GizmosUtilities
    {
        public static void DrawArrow(Vector3 start, Vector3 end, float size = 0.25f, float angle = 20)
        {
            Vector3 direction = (end - start);

            if (direction.magnitude < 0.01f) return;


            direction.Normalize();


            Gizmos.DrawLine(start, end);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + angle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - angle, 0) * new Vector3(0, 0, 1);

            Gizmos.DrawLine(end, end + right * size);
            Gizmos.DrawLine(end, end + left * size);
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

        internal static void DrawRotation(Vector3 position, Quaternion rotation, float scale = 1)
        {
            // Calculate directions from the rotation
            Vector3 forward = rotation * Vector3.forward;
            Vector3 right = rotation * Vector3.right;
            Vector3 up = rotation * Vector3.up;

            // Draw the forward vector in blue
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(position, position + forward * scale);

            // Draw the right vector in red
            Gizmos.color = Color.red;
            Gizmos.DrawLine(position, position + right * scale);

            // Draw the up vector in green
            Gizmos.color = Color.green;
            Gizmos.DrawLine(position, position + up * scale);
        }

    }

    public static class DebugUtitlities
    {
        public static void DrawArrow(float duration, Color color, Vector3 start, Vector3 end, float size = 0.25f, float angle = 20)
        {
            Vector3 direction = (end - start);

            if (direction.magnitude < 0.01f) return;


            direction.Normalize();
            Debug.DrawLine(start, end, color, duration);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + angle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - angle, 0) * new Vector3(0, 0, 1);

            Debug.DrawLine(end, end + right * size, color, duration);
            Debug.DrawLine(end, end + left * size, color, duration);
        }

        public static void DrawArrow(Vector3 start, Vector3 end, Color color, float size = 0.25f, float angle = 20)
        {
            DrawArrow(Time.deltaTime, color, start, end, size, angle);
        }

        public static void DrawCircle(Vector3 center, Vector3 normal, float radius, Color color, float duration = 0, int resolution = 10)
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
                Debug.DrawLine(prevPoint, nextPoint, color, duration, false);
                prevPoint = nextPoint;
            }
        }

        public static void DrawWireBox(Vector3 position, Vector3 half, Quaternion rotation, Color color, float duration)
        {
            // Local corners
            Vector3[] corners = new Vector3[8]
            {
                new Vector3(-half.x, -half.y, -half.z),
                new Vector3( half.x, -half.y, -half.z),
                new Vector3( half.x, -half.y,  half.z),
                new Vector3(-half.x, -half.y,  half.z),
                new Vector3(-half.x,  half.y, -half.z),
                new Vector3( half.x,  half.y, -half.z),
                new Vector3( half.x,  half.y,  half.z),
                new Vector3(-half.x,  half.y,  half.z)
            };

            for (int i = 0; i < corners.Length; i++)
                corners[i] = position + rotation * corners[i];

            void Line(int i0, int i1) => Debug.DrawLine(corners[i0], corners[i1], color, duration);

            // Bottom
            Line(0, 1); Line(1, 2); Line(2, 3); Line(3, 0);
            // Top
            Line(4, 5); Line(5, 6); Line(6, 7); Line(7, 4);
            // Sides
            Line(0, 4); Line(1, 5); Line(2, 6); Line(3, 7);
        }

        public static void DrawWireSphere(Vector3 position, float radius, Color color, float duration)
        {
            int segments = 20;
            float angleStep = 360f / segments;
            Vector3 prevPoint = position + new Vector3(radius, 0, 0);
            for (int i = 1; i <= segments; i++)
            {
                float angle = i * angleStep;
                Vector3 nextPoint = position + new Vector3(radius * Mathf.Cos(angle * Mathf.Deg2Rad), radius * Mathf.Sin(angle * Mathf.Deg2Rad), 0);
                Debug.DrawLine(prevPoint, nextPoint, color, duration);
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
