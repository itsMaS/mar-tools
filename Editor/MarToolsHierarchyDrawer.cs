using UnityEditor;
using UnityEngine;
using MarTools;
using System;

[InitializeOnLoad]
public static class MarToolsHierarchyDrawer
{
    static MarToolsHierarchyDrawer()
    {
        // Subscribe to the Hierarchy GUI callback
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
        EditorApplication.update += EditorUpdate;
    }

    private static void EditorUpdate()
    {
        if(Application.isPlaying)
        {
            EditorApplication.RepaintHierarchyWindow();
        }
    }

    private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
    {
        // Get the GameObject from the instance ID
        GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (obj == null) return;

        // Try getting a progress value (using a component with a "progress" field)
        float progress = GetProgressFromObject(obj);

        // Draw a background color based on progress
        if (progress >= 0)
        {
            EditorGUI.DrawRect(selectionRect, Color.white.SetAlpha(0.05f));
            Rect rect = new Rect(selectionRect);
            rect.width *= progress;

            EditorGUI.DrawRect(rect, Color.green.SetAlpha(0.2f));
        }
    }

    private static float GetProgressFromObject(GameObject obj)
    {
        // Example: Look for a script with a float 'progress' field

        var component = obj.GetComponent<IProgressProvider>();
        if (component != null)
        {
            return Mathf.Clamp01(component.Progress); // Ensure value is between 0-1
        }

        return -1;
    }
}
