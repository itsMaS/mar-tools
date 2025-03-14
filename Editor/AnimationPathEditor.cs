using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class AnimationPathEditor : EditorWindow
{
    private AnimationClip selectedClip;
    private Dictionary<string, string> pathMapping = new Dictionary<string, string>();
    private Vector2 scrollPosition;

    // Regex Fields
    private string regexPattern = "";
    private string replacementPattern = "";

    // Toggle for creating a copy or replacing the original
    private bool createCopy = false;

    [MenuItem("Window/Animation/Path Editor")]
    public static void Open()
    {
        GetWindow<AnimationPathEditor>("Animation Path Editor");
    }

    private void OnGUI()
    {
        if (Selection.activeObject is AnimationClip clip)
        {
            if (selectedClip != clip)
            {
                selectedClip = clip;
                LoadAnimationPaths();
            }

            GUILayout.Label("Editing Animation Path Names", EditorStyles.boldLabel);
            EditorGUILayout.ObjectField("Selected Clip:", selectedClip, typeof(AnimationClip), false);

            GUILayout.Space(5);

            // Regex Input Fields
            GUILayout.Label("Batch Rename Paths (Regex)", EditorStyles.boldLabel);
            regexPattern = EditorGUILayout.TextField("Find (Regex):", regexPattern);
            replacementPattern = EditorGUILayout.TextField("Replace With:", replacementPattern);

            if (GUILayout.Button("Apply Regex to All Paths"))
            {
                ApplyRegexToPaths();
            }

            GUILayout.Space(10);

            if (pathMapping.Count == 0)
            {
                GUILayout.Label("No paths found in this animation clip.");
                return;
            }

            // Scrollable List of Paths
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));

            List<string> keys = new List<string>(pathMapping.Keys);
            foreach (var originalPath in keys)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(originalPath, GUILayout.Width(200));
                pathMapping[originalPath] = EditorGUILayout.TextField(pathMapping[originalPath]);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView(); // End Scrollable View

            GUILayout.Space(10);

            // Toggle for replacing vs. creating a copy
            createCopy = EditorGUILayout.Toggle("Create a Copy Instead", createCopy);

            if (GUILayout.Button("Apply Path Changes"))
            {
                ApplyPathChanges();
            }
        }
        else
        {
            GUILayout.Label("Select an AnimationClip to edit.");
        }
    }

    private void LoadAnimationPaths()
    {
        pathMapping.Clear();

        if (selectedClip == null) return;

        EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(selectedClip);
        foreach (var binding in bindings)
        {
            if (!pathMapping.ContainsKey(binding.path))
            {
                pathMapping[binding.path] = binding.path;
            }
        }
    }

    private void ApplyRegexToPaths()
    {
        if (string.IsNullOrEmpty(regexPattern))
        {
            Debug.LogWarning("Regex pattern is empty!");
            return;
        }

        try
        {
            List<string> keys = new List<string>(pathMapping.Keys);
            foreach (var originalPath in keys)
            {
                pathMapping[originalPath] = Regex.Replace(originalPath, regexPattern, replacementPattern);
            }

            Debug.Log("Regex applied successfully!");
        }
        catch (Exception e)
        {
            Debug.LogError("Invalid Regex: " + e.Message);
        }
    }

    private void ApplyPathChanges()
    {
        if (selectedClip == null) return;

        AnimationClip targetClip = createCopy ? new AnimationClip() : selectedClip;
        EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(selectedClip);

        foreach (var binding in bindings)
        {
            string newPath = pathMapping[binding.path];

            if (binding.path == newPath) continue; // Skip if unchanged

            AnimationCurve curve = AnimationUtility.GetEditorCurve(selectedClip, binding);
            EditorCurveBinding newBinding = binding;
            newBinding.path = newPath;

            AnimationUtility.SetEditorCurve(targetClip, newBinding, curve);
        }

        if (createCopy)
        {
            string newClipPath = AssetDatabase.GetAssetPath(selectedClip);
            targetClip.name = selectedClip.name + "_Modified";
            AssetDatabase.CreateAsset(targetClip, newClipPath.Replace(".anim", "_modified.anim"));
            Debug.Log("Animation paths updated and saved as a new clip.");
        }
        else
        {
            Debug.Log("Animation paths updated in the original clip.");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void OnEnable()
    {
        Selection.selectionChanged += Repaint;
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= Repaint;
    }
}
