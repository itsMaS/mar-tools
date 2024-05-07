using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class AnimatorOverrideControllerClipReplacer : Editor
{
    public const int START_TRIM_AMOUNT = 3;

    [MenuItem("Assets/Select Replacement Folder", false, 1000)]
    private static void SelectReplacementFolder()
    {
        string selectedPath = EditorUtility.OpenFolderPanel("Select Animation Folder", "", "");

        if (string.IsNullOrEmpty(selectedPath))
        {
            Debug.LogWarning("No folder selected.");
            return;
        }

        var selectedGuids = Selection.assetGUIDs;
        if (selectedGuids.Length == 0)
        {
            Debug.LogWarning("No Animator Override Controller selected.");
            return;
        }

        foreach (string guid in selectedGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var overrideController = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(path);

            if (overrideController != null)
            {
                ReplaceAnimationClips(overrideController, selectedPath);
            }
            else
            {
                Debug.LogWarning($"The selected asset is not an Animator Override Controller: {path}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void ReplaceAnimationClips(AnimatorOverrideController overrideController, string folderPath)
    {

        // Create a dictionary of animation clip replacements based on file names
        Dictionary<string, AnimationClip> replacementClips = new Dictionary<string, AnimationClip>();

        string[] files = Directory.GetFiles(folderPath, "*.fbx");
        foreach (string file in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(GetAssetPath(file));
            if (clip != null)
            {
                string name = fileName.Substring(START_TRIM_AMOUNT);
                replacementClips[name] = clip;
            }
        }

        // Override clips in the Animator Override Controller
        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        overrideController.GetOverrides(overrides);

        for (int i = 0; i < overrides.Count; i++)
        {
            AnimationClip originalClip = overrides[i].Key;
            string originalClipName = originalClip != null ? originalClip.name.Substring(START_TRIM_AMOUNT) : string.Empty;

            if (replacementClips.TryGetValue(originalClipName, out AnimationClip newClip))
            {
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(originalClip, newClip);
            }
        }

        overrideController.ApplyOverrides(overrides);
        Debug.Log($"Clips in '{overrideController.name}' replaced successfully.");
    }

    private static string GetAssetPath(string fullPath)
    {
        return "Assets" + fullPath.Substring(Application.dataPath.Length).Replace('\\', '/');
    }
}
