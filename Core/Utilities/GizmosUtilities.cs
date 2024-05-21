namespace MarTools
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

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
    }
#endif
}
