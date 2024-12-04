namespace MarTools.Audio
{
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using UnityEngine;

    public class MarAudioComponent : MonoBehaviour
    {
        public MarAudioClipSO clipPlayedOnStart;

        private void Start()
        {
            if(clipPlayedOnStart)
            {
                Play(clipPlayedOnStart);
            }
        }

        public void Play(MarAudioClipSO clip)
        {
            clip.Play(gameObject);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(MarAudioComponent))]
    public class MarAudioComponentEditor : Editor
    {
        public static string AudioClipPath
        {
            get
            {
                return EditorPrefs.GetString("MarAudio/ClipPath", "Assets/Audio/Clips");
            }
            set
            {
                EditorPrefs.SetString("MarAudio/ClipPath", value);
            }
        }

        //public override void OnInspectorGUI()
        //{
        //    base.OnInspectorGUI();

        //    GUILayout.BeginHorizontal();
        //    GUILayout.Label($"{AudioClipPath}");
        //    if(GUILayout.Button("Select Clip Path"))
        //    {
        //        AudioClipPath = MakePathRelativeToProject(EditorUtility.OpenFolderPanel("Select clip folder", "Assets", "Assets"));
        //    }
        //    GUILayout.EndHorizontal();

        //    if(GUILayout.Button("Create clip"))
        //    {
        //        CreateAsset<MarAudioClipSO>(AudioClipPath);
        //    }
        //}

        //private string MakePathRelativeToProject(string absolutePath)
        //{
        //    string projectPath = Application.dataPath;
        //    // Remove "Assets" from the path to get the project root
        //    projectPath = projectPath.Substring(0, projectPath.Length - "Assets".Length);

        //    if (absolutePath.StartsWith(projectPath))
        //    {
        //        // Make path relative to project root
        //        return absolutePath.Substring(projectPath.Length);
        //    }
        //    return null; // Folder is not inside the project
        //}

        //private void CreateAsset<T>(string assetPath) where T : ScriptableObject
        //{
        //    // Ensure the directory exists
        //    string directory = System.IO.Path.GetDirectoryName(assetPath);
        //    if (!AssetDatabase.IsValidFolder(directory))
        //    {
        //        System.IO.Directory.CreateDirectory(directory);
        //        AssetDatabase.Refresh();
        //    }

        //    // Create the ScriptableObject
        //    T asset = ScriptableObject.CreateInstance<T>();
        //    AssetDatabase.CreateAsset(asset, assetPath+$"/Test");

        //    // Save and refresh
        //    AssetDatabase.SaveAssets();
        //    AssetDatabase.Refresh();

        //    Debug.Log($"ScriptableObject created at: {assetPath}");
        //    Selection.activeObject = asset;
        //}
    }
    #endif
}

