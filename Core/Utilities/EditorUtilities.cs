namespace MarTools
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    
    public static class EditorUtilities
    {
        // Example function to find all assets of a specified type
        public static List<T> FindAssets<T>() where T : UnityEngine.Object
        {
            // Find all asset GUIDs of the specified type using AssetDatabase
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            List<T> list = new List<T>();

            // Iterate through each GUID to get its asset path
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
    
                list.Add(asset );
            }

            return list;
        }
    }
    
}