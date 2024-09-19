namespace MarTools
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

#if UNITY_EDITOR

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


        public static T DrawToggleGroup<T>(T current, params KeyValuePair<string, T>[] options)
        {
            GUILayout.BeginHorizontal(); // Begin a horizontal group

            int selectedIndex = options.ToList().FindIndex(x => x.Value.Equals(current));

            if (selectedIndex < 0) selectedIndex = 1;

            for (int i = 0; i < options.Length; i++)
            {
                var previousColor = GUI.color;
                if (i == selectedIndex) GUI.color = Color.green;


                // Render each button and check if it's clicked
                if (GUILayout.Button($"{options[i].Key}"))
                {
                    selectedIndex = i; // Update the selected index if this button is clicked
                }

                GUI.color = previousColor;
            }
            GUILayout.EndHorizontal(); // End the horizontal group

            return options[selectedIndex].Value;
        }

        public static List<Type> GetDerivedClasses(Type baseType)
        {
            // Get all types in the current assembly that inherit from the specified baseType
            return Assembly.GetAssembly(baseType)
                           .GetTypes()
                           .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(baseType))
                           .ToList();
        }

        public static List<Type> GetImplementationsOfInterface<TInterface>()
        {
            // Get the interface type
            var interfaceType = typeof(TInterface);

            // Get all types in the current assembly (or other assemblies, if needed)
            var types = Assembly.GetAssembly(interfaceType).GetTypes();

            // Filter types that are classes and implement the interface
            return types.Where(t => t.IsClass && !t.IsAbstract && interfaceType.IsAssignableFrom(t)).ToList();
        }
    }
#endif
}