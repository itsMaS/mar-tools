namespace MarTools
{
    using UnityEngine;
    using UnityEditor;
    using System.IO;
    using System.Collections.Generic;
    
    public class AnimationUtilities : Editor
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
    
        [MenuItem("Assets/Set FBX Animations to Loop", false, 1000)]
        public static void SetFBXAnimationsToLoop()
        {
            // Get the selected objects in the Project window
            Object[] selectedObjects = Selection.objects;
    
            foreach (Object selectedObject in selectedObjects)
            {
                // Check if the selected object is an FBX file
                string assetPath = AssetDatabase.GetAssetPath(selectedObject);
                if (!assetPath.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
                {
                    Debug.LogWarning($"{selectedObject.name} is not an FBX file.");
                    continue;
                }
    
                // Load the model importer for this FBX file
                ModelImporter modelImporter = AssetImporter.GetAtPath(assetPath) as ModelImporter;
                if (modelImporter == null)
                {
                    Debug.LogError($"Unable to load ModelImporter for {selectedObject.name}");
                    continue;
                }
    
                // Retrieve the animation clips embedded in the FBX file
                ModelImporterClipAnimation[] clips = modelImporter.defaultClipAnimations;
    
                // Set each clip to loop
                foreach (ModelImporterClipAnimation clip in clips)
                {
                    clip.loopTime = true;
                    clip.name = selectedObject.name;
                    Debug.Log($"Setting clip {clip.name} to loop in {selectedObject.name}");
                }
    
                // Apply the changes back to the FBX file
                modelImporter.clipAnimations = clips;
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            }
        }
    
        [MenuItem("Assets/Set FBX Animations names to fbx names", false, 1000)]
        public static void SetFBXAnimationClipNames()
        {
            // Get the selected objects in the Project window
            Object[] selectedObjects = Selection.objects;
    
            foreach (Object selectedObject in selectedObjects)
            {
                // Check if the selected object is an FBX file
                string assetPath = AssetDatabase.GetAssetPath(selectedObject);
                if (!assetPath.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
                {
                    Debug.LogWarning($"{selectedObject.name} is not an FBX file.");
                    continue;
                }
    
                // Load the model importer for this FBX file
                ModelImporter modelImporter = AssetImporter.GetAtPath(assetPath) as ModelImporter;
                if (modelImporter == null)
                {
                    Debug.LogError($"Unable to load ModelImporter for {selectedObject.name}");
                    continue;
                }
    
                // Retrieve the animation clips embedded in the FBX file
                ModelImporterClipAnimation[] clips = modelImporter.defaultClipAnimations;
    
                // Set each clip to loop
                foreach (ModelImporterClipAnimation clip in clips)
                {
                    clip.name = selectedObject.name;
                }
    
                // Apply the changes back to the FBX file
                modelImporter.clipAnimations = clips;
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            }
        }
    }
    
}