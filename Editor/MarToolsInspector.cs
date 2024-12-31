using UnityEditor;
using UnityEngine;

namespace MarTools
{
    //[CustomEditor(typeof(MonoBehaviour), true)]
    public class MarToolsInspector : Editor
    {
        private int selectedTab = 0;
        private string[] tabs = { "Properties", "Unity Events", "Scene References", "Asset References" };

        public override void OnInspectorGUI()
        {
            // Tab Selection
            selectedTab = GUILayout.Toolbar(selectedTab, tabs);

            // Fetch the serialized object
            SerializedObject serializedObject = new SerializedObject(target);
            serializedObject.Update();

            // Iterate through all the serialized properties
            SerializedProperty property = serializedObject.GetIterator();
            if (property.NextVisible(true))
            {
                do
                {
                    // Skip "m_Script" property to avoid editing the script field
                    if (property.name == "m_Script")
                    {
                        if (selectedTab == 0) // Show only in "Uncategorized"
                        {
                            continue;
                        }
                        else
                        {
                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUILayout.PropertyField(property, true);
                            EditorGUI.EndDisabledGroup();
                            continue;
                        }
                    }

                    bool isUnityEvent = property.propertyType == SerializedPropertyType.Generic && property.type.Contains("UnityEvent");
                    bool isSceneReference = property.propertyType == SerializedPropertyType.ObjectReference && IsSceneObject(property.objectReferenceValue);
                    bool isAssetReference = property.propertyType == SerializedPropertyType.ObjectReference && !IsSceneObject(property.objectReferenceValue);

                    switch (selectedTab)
                    {
                        case 0: // Uncategorized
                            if (!isUnityEvent && !isSceneReference && !isAssetReference)
                            {
                                EditorGUILayout.PropertyField(property, true);
                            }
                            break;

                        case 1: // Unity Events
                            if (isUnityEvent)
                            {
                                EditorGUILayout.PropertyField(property, true);
                            }
                            break;

                        case 2: // Scene References
                            if (isSceneReference)
                            {
                                EditorGUILayout.PropertyField(property, true);
                            }
                            break;

                        case 3: // Asset References
                            if (isAssetReference)
                            {
                                EditorGUILayout.PropertyField(property, true);
                            }
                            break;
                    }

                } while (property.NextVisible(false));
            }

            // Apply modified properties
            serializedObject.ApplyModifiedProperties();
        }

        private bool IsSceneObject(Object obj)
        {
            return obj != null && !EditorUtility.IsPersistent(obj) && (obj is Object || obj is GameObject);
        }
    }
}
