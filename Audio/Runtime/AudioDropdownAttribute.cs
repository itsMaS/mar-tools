using System;
using UnityEngine;
using MarTools.Audio;
using System.Linq;



#if UNITY_EDITOR
using UnityEditor;
#endif

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class AudioDropdownAttribute : PropertyAttribute
{
    public string providerMethodName;

    public AudioDropdownAttribute(string providerMethodName = "")
    {
        this.providerMethodName = providerMethodName;
    }
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(AudioDropdownAttribute))]
public class StringDropdownDrawer : PropertyDrawer
{
    bool dropdown = true;
    Vector2 scrollView= Vector2.zero;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.LabelField(position, label.text, "Use StringDropdown with string.");
            return;
        }

        string[] options = AudioManager.GetEventNames().ToArray();
        
        if(dropdown && property.stringValue.Length == 0)
        {
            property.stringValue = options[0];
        }
        
        int index = Array.IndexOf(options, property.stringValue);

        bool found = index >= 0;

        if (!found) dropdown = false;

        if(dropdown)
        {
            GUILayout.BeginHorizontal();

            // Placeholder options – fill dynamically later
            int newIndex = EditorGUILayout.Popup(label.text, index, options);

            if (newIndex < options.Length)
            {
                property.stringValue = options[newIndex];
            }

            if (GUILayout.Button("Search", GUILayout.Width(100)))
            {
                dropdown = false;
            }
            GUILayout.EndHorizontal();
        }
        else
        {
            var simillarList = AudioManager.GetEventNames().Where(x => property.stringValue.Length > 0 && x.ToLower().Contains(property.stringValue.ToLower())).ToList();

            GUILayout.BeginHorizontal();

            GUILayout.Label($"{label.text}");
            if(simillarList.Count > 0 && !found)
            {
                GUILayout.Label($"Simillar {simillarList.Count}");
            }

            property.stringValue = GUILayout.TextField(property.stringValue);
 
            if(found)
            {
                if (GUILayout.Button("Dropdown", GUILayout.Width(100)))
                {
                    dropdown = true;
                }
            }
            else
            {
                if (GUILayout.Button("Add", GUILayout.Width(100)))
                {
                    if(string.IsNullOrWhiteSpace(property.stringValue))
                    {
                        EditorUtility.DisplayDialog("Error", "Event name cannot be empty.", "OK");
                        return;
                    }
                    AudioManager.AddNewEvent(property.stringValue);
                    dropdown = true;
                }

                if(GUILayout.Button("Dropdown"))
                {
                    property.stringValue = options[0];
                }
            }

            GUILayout.EndHorizontal();


            if (simillarList.Count > 0 && !found)
            {
                GUILayout.BeginVertical();
                scrollView = GUILayout.BeginScrollView(scrollView, GUILayout.Height(50));
                GUIStyle style = new GUIStyle(GUI.skin.label);
                style.fontSize = 10;
                foreach (var item in simillarList)
                {
                    if(GUILayout.Button(item, style))
                    {
                        property.stringValue = item;
                        dropdown = true;
                    }
                }
                GUILayout.EndScrollView();
                GUILayout.EndVertical();
            }

        }

    }
}
#endif

