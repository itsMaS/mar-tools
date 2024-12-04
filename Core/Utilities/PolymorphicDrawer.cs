using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace MarTools
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NameAttribute : Attribute
    {
        public string Name { get; }
        public NameAttribute(string name)
        {
            Name = name;
        }
    }

#if UNITY_EDITOR
    public class PolymorphicDrawer<T> : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Check if the property has a value
            if (property.managedReferenceValue != null)
            {
                // Draw the foldout for expanding/collapsing the property
                property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), property.isExpanded, label);

                if (property.isExpanded)
                {
                    // Adjust the position for child fields
                    Rect fieldPosition = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, position.height);

                    // Draw the property fields
                    DrawProperty(fieldPosition, property);
                }
            }
            else
            {
                // Create a Rect for the button when no class is selected
                Rect buttonRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

                GUI.Label(buttonRect, new GUIContent(property.displayName));
                if (GUI.Button(buttonRect, $"{typeof(T).Name}"))
                {
                    DrawDropdown(property);
                }
            }

            EditorGUI.EndProperty();
        }

        protected virtual void DrawDropdown(SerializedProperty property)
        {
            GenericMenu menu = new GenericMenu();

            // Loop over all implementations of the interface and add them to the menu
            foreach (var item in GetDropDownOptions(property))
            {
                menu.AddItem(new GUIContent(item.Item1), false, () => item.Item2.Invoke());
            }

            // Add an option to remove the current value
            menu.AddItem(new GUIContent("None"), false, () => RemoveValue(property));

            menu.ShowAsContext();
        }

        protected virtual List<(string, System.Action)> GetDropDownOptions(SerializedProperty property)
        {
            List<(string, Action)> Options = new List<(string, Action)>();

            // Loop over all implementations of the interface and add them to the menu
            foreach (var item in EditorUtilities.GetImplementationsOfInterface<T>())
            {
                var attributes = item.GetCustomAttributes(typeof(NameAttribute), false);
                string name = attributes.Length > 0 ? ((NameAttribute)attributes[0]).Name : item.Name;

                Action a = () => CreateAndAssignValueOfType(property, item);
                Options.Add((name, a));
            }

            return Options;
        }

        protected virtual void DrawProperty(Rect position, SerializedProperty property)
        {
            // Create a Rect for the button below the property fields
            Rect buttonRect = new Rect(position.x+position.width*0.5f, position.y- EditorGUIUtility.singleLineHeight, position.width*0.5f, EditorGUIUtility.singleLineHeight);
            // Draw the button with GUI.Button instead of GUILayout
            if (GUI.Button(buttonRect, $"{property.managedReferenceValue.GetType().Name}"))
            {
                DrawDropdown(property);
            }

            // Adjust the position for property fields (leave space for label)
            position = EditorGUI.IndentedRect(position);

            // Iterate through all visible children of the property
            SerializedProperty prop = property.Copy();
            SerializedProperty endProp = property.GetEndProperty();



            prop.NextVisible(true); // Move to the first child property

            // Iterate over each property field within the object
            while (!SerializedProperty.EqualContents(prop, endProp))
            {
                position.height = EditorGUI.GetPropertyHeight(prop, true);
                EditorGUI.PropertyField(position, prop, true);
                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;

                if (!prop.NextVisible(false)) break; // Move to the next visible property, or break if there are none
            }
            position.y -= EditorGUIUtility.singleLineHeight;
        }

        private void CreateAndAssignValueOfType(SerializedProperty property, Type type)
        {
            property.serializedObject.Update();
            property.managedReferenceValue = System.Activator.CreateInstance(type);
            property.serializedObject.ApplyModifiedProperties();
        }

        protected void AssignValue(SerializedProperty property, object obj)
        {
            property.serializedObject.Update();
            property.managedReferenceValue = obj;
            property.serializedObject.ApplyModifiedProperties();
        }

        private void RemoveValue(SerializedProperty property)
        {
            property.serializedObject.Update();
            property.managedReferenceValue = null;
            property.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Start with label height
            float totalHeight = EditorGUIUtility.singleLineHeight;

            // If the property is expanded and has a value, calculate the height of its fields
            if (property.isExpanded && property.managedReferenceValue != null)
            {
                SerializedProperty prop = property.Copy();
                SerializedProperty endProp = property.GetEndProperty();

                prop.NextVisible(true); // Move to the first child property

                while (!SerializedProperty.EqualContents(prop, endProp))
                {
                    totalHeight += EditorGUI.GetPropertyHeight(prop, true) + EditorGUIUtility.standardVerticalSpacing;

                    if (!prop.NextVisible(false)) break; // Move to the next visible property, or break if there are none
                }

                // Add space for the "Change type" button
                totalHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            else
            {
                // Add space for the "Select class" button if no value is assigned
                totalHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            return totalHeight;
        }
    }
#endif
}
