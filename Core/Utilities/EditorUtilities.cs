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

    public class EditorPrefToggle
    {
        public string name;
        public bool defaultValue;

        public bool value
        {
            get
            {
                return EditorPrefs.GetBool(name, defaultValue);
            }
            set
            {
                EditorPrefs.SetBool(name, value);
            }
        }


        public EditorPrefToggle(string name, bool defaultValue = false)
        {
            this.name = name;
            this.defaultValue = defaultValue;
        }

        public void DrawToggle()
        {
            value = EditorGUILayout.Toggle(name, value);
        }
    }

    public class MarToolsEditor<T> : Editor where T : UnityEngine.Object
    {
        protected T script;
        protected List<T> scripts => targets.Cast<T>().ToList();

        protected virtual void OnEnable()
        {
            script = (T)target;
            // Force the editor to update every frame
            EditorApplication.update += UpdateInspector;
        }

        protected virtual void OnDisable()
        {
            EditorApplication.update -= UpdateInspector;
        }

        private void UpdateInspector()
        {
            Repaint();
        }
    }
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
            var interfaceType = typeof(TInterface);
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t != null); }
                })
                .Where(t => t != null && t.IsClass && !t.IsAbstract && interfaceType.IsAssignableFrom(t))
                .ToList();
        }

        public static GUIStyle GetButtonStyle(Color bgColor, Color textColor, float width, float height, int textSize = 10)
        {
            GUIStyle redButtonStyle = new GUIStyle(GUI.skin.button);
            redButtonStyle.normal.textColor = Color.white;
            redButtonStyle.fontSize = textSize; // Make text smaller
            redButtonStyle.fixedHeight = 20; // Smaller height
            redButtonStyle.fixedWidth = 20; // Smaller width
            redButtonStyle.normal.background = MakeTex(2, 2, bgColor);
            redButtonStyle.hover.background = MakeTex(2, 2, bgColor * 0.5f);

            return redButtonStyle;
        }
        public static Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        public static void DrawProgressBar(float value, Color backgroundColor, Color fillColor, Color textColor, string label = "", float height = 20)
        {
            // Reserve space
            Rect rectBG = GUILayoutUtility.GetRect(0, height, GUILayout.ExpandWidth(true));

            // Draw background
            EditorGUI.DrawRect(rectBG, Color.gray);

            // Draw fill

            Rect rectFill = new Rect(rectBG);
            rectFill.width *= Mathf.Clamp01(value);
            EditorGUI.DrawRect(rectFill, Color.green);

            // Draw text
            GUIStyle style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            style.normal.textColor = textColor;
            EditorGUI.LabelField(rectBG, label + $" ({(value * 100):0}%)", style);
        }

    }
#endif
}