using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MarTools.Audio
{
    [CreateAssetMenu]
    public class AudioEventContainer : ScriptableObject
    {
        [System.Serializable]
        public class AudioMapping
        {
            public string eventName;

            public string DisplayName => GetNameOnly();
            public string Category => GetCategory();

            private string GetCategory()
            {
                if (string.IsNullOrEmpty(eventName)) return "";
                int slashIndex = eventName.IndexOf('/');
                return slashIndex >= 0 ? eventName.Substring(0, slashIndex) : "";
            }

            private string GetNameOnly()
            {
                if (string.IsNullOrEmpty(eventName)) return "";
                int slashIndex = eventName.IndexOf('/');
                return slashIndex >= 0 ? eventName.Substring(slashIndex + 1) : eventName;
            }
        }


        public List<AudioMapping> mappings = new List<AudioMapping>();

        public static void Play(string evenName) 
        {
            AudioManager.PlayAudioEvent(evenName);
        }
    }

#if UNITY_EDITOR
[CustomEditor(typeof(AudioEventContainer))]
    public class AudioEventContainerEditor : Editor
    {
        private SerializedProperty mappingsProp;
        private AudioEventContainer scriptableObject;

        private string newEventName = "";
        private List<string> Categories = new List<string>();
        private int categoryIndex = 0;

        public string searchString = "";

        void OnEnable()
        {
            mappingsProp = serializedObject.FindProperty("mappings");
            scriptableObject = (AudioEventContainer)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawCategories();
            GUILayout.Space(20);
            DrawSearch();
            GUILayout.Space(5);
            DrawList();
            GUILayout.Space(5);
            DrawAdd();
            
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawCategories()
        {
            // Build categories
            Categories.Clear();
            Categories.Add("All"); // Default category
            Categories.Add("Uncategorized"); // Default category

            List<string> eventNames = new List<string>();

            for (int i = 0; i < mappingsProp.arraySize; i++)
            {
                var eventName = mappingsProp.GetArrayElementAtIndex(i).FindPropertyRelative("eventName").stringValue;
                eventNames.Add(eventName);

                string category = (eventName.Contains("/") ? eventName.Split('/')[0] : "Uncategorized");

                if (!Categories.Contains(category))
                {
                    Categories.Add(category);
                }
            }

            GUILayout.BeginHorizontal();

            for (int i = 0; i < Categories.Count; i++)
            {
                GUI.color = i == categoryIndex ? Color.Lerp(Color.green, Color.white, 0.5f) : Color.white;
                if (GUILayout.Button(Categories[i], GUILayout.MinWidth(10), GUILayout.MaxWidth(500)))
                {
                    categoryIndex = i;
                }
                GUI.color = Color.white;
            }
            GUILayout.EndHorizontal();

            if(Categories.Count <= categoryIndex)
            {
                categoryIndex = 0;
            }
        }

        private void DrawSearch()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Search", GUILayout.Width(50));
            searchString = GUILayout.TextField(searchString, GUILayout.Width(300));
            GUILayout.EndHorizontal();
        }

        private void DrawList()
        {
            GUILayout.BeginVertical();

            int itemsDrawn = 0;
            for (int i = 0; i < mappingsProp.arraySize; i++)
            {
                var item = mappingsProp.GetArrayElementAtIndex(i).FindPropertyRelative("eventName").stringValue;
                string category = item.Contains("/") ? item.Split('/')[0] : "";

                if (searchString.Length > 0 && !item.ToLower().Contains(searchString.ToLower()))
                {
                    continue; // Skip if search string does not match
                }

                if (category == Categories[categoryIndex] || (category.Length == 0 && Categories[categoryIndex] == "Uncategorized") || Categories[categoryIndex] == "All")
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("X", GUILayout.Width(20f)))
                    {
                        mappingsProp.DeleteArrayElementAtIndex(i);
                        break;
                    }

                    if(newEventName == item)
                    {
                        GUI.color = Color.red;
                    }
                    GUILayout.Label(item);
                    GUI.color = Color.white;


                    itemsDrawn++;
                    GUILayout.EndHorizontal();
                }
            }

            if(itemsDrawn == 0)
            {
                GUILayout.Label("No events found", EditorStyles.boldLabel);
            }
            GUILayout.EndVertical();
        }

        private void DrawAdd()
        {
            GUILayout.BeginHorizontal();

            GUI.SetNextControlName("NewEventInput");
            newEventName = GUILayout.TextField(newEventName, GUILayout.Width(200));

            bool addCategory = Categories[categoryIndex] != "All" && Categories[categoryIndex] != "Uncategorized";
            string finalString = addCategory ? $"{Categories[categoryIndex]}/{newEventName}" : newEventName;
            bool duplicateFound = scriptableObject.mappings.Exists(x => x.eventName == finalString);

            if (GUILayout.Button("Add", GUILayout.Width(80)))
            {
                TryAddEvent(finalString, duplicateFound);
            }

            GUILayout.EndHorizontal();

            if (duplicateFound)
                EditorGUILayout.HelpBox($"Duplicate event name", MessageType.Warning);
        }

        private void TryAddEvent(string name, bool duplicate)
        {
            if (duplicate || string.IsNullOrWhiteSpace(newEventName))
                return;

            mappingsProp.InsertArrayElementAtIndex(mappingsProp.arraySize);
            var newElement = mappingsProp.GetArrayElementAtIndex(mappingsProp.arraySize - 1);
            newElement.FindPropertyRelative("eventName").stringValue = name;
            newEventName = "";
            GUI.FocusControl(null); // Unfocus after add
        }
    }


#endif
}
