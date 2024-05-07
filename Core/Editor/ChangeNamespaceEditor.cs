namespace MarTools
{
    using UnityEditor;
    using UnityEngine;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;
    
    public class ChangeNamespaceEditor : EditorWindow
    {
        private string newNamespace;
    
        [MenuItem("Assets/Change Namespace", false, 1000)]
        private static void Init()
        {
            ChangeNamespaceEditor window = (ChangeNamespaceEditor)GetWindow(typeof(ChangeNamespaceEditor), true, "Change Namespace");
            window.Show();
        }
    
        private void OnGUI()
        {
            GUILayout.Label("New Namespace", EditorStyles.boldLabel);
            newNamespace = EditorGUILayout.TextField("Namespace:", newNamespace);
    
            if (GUILayout.Button("Apply"))
            {
                ApplyNamespaceChange();
                Close();
            }
        }
    
        private static void ApplyNamespaceChange()
        {
            string[] selectedGuids = Selection.assetGUIDs;
            List<string> scriptPaths = new List<string>();
    
            foreach (string guid in selectedGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith(".cs"))
                {
                    scriptPaths.Add(path);
                }
            }
    
            if (scriptPaths.Count == 0)
            {
                Debug.LogWarning("No C# scripts selected.");
                return;
            }
    
            ChangeNamespaceEditor window = (ChangeNamespaceEditor)GetWindow(typeof(ChangeNamespaceEditor));
            string newNamespace = window.newNamespace;
    
            foreach (string scriptPath in scriptPaths)
            {
                string scriptContent = File.ReadAllText(scriptPath);
    
                // Remove any existing namespace declaration
                scriptContent = Regex.Replace(scriptContent, @"namespace\s+\w+\s*{", string.Empty);
                scriptContent = scriptContent.Replace("\r\n", "\n"); // Normalize newlines
    
                // Add the new namespace
                scriptContent = $"namespace {newNamespace}\n{{\n" +
                                IndentCode(scriptContent) +
                                "\n}";
    
                // Write the modified content back to the file
                File.WriteAllText(scriptPath, scriptContent);
            }
    
            AssetDatabase.Refresh();
            Debug.Log($"Namespace changed to '{newNamespace}' for selected scripts.");
        }
    
        private static string IndentCode(string code)
        {
            string[] lines = code.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = "    " + lines[i];
            }
            return string.Join("\n", lines);
        }
    }
}