using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using JetBrains.Annotations;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace MarTools
{
    [CreateAssetMenu]
    public class InputPromptMapping : ScriptableObject
    {
        [System.Serializable]
        public class Mapping
        {
            public string actionPath;
            public SerializedDictionary<string, Sprite> PromptSprites = new SerializedDictionary<string, Sprite>();
        }

        public InputActionAsset inputActions;

        [HideInInspector] public List<Mapping> Mappings;

        public Sprite GetPromptSprite(string actionPath, string controlScheme)
        {
            return Mappings.Find(x => x.actionPath == actionPath).PromptSprites[controlScheme];
        }

        public Sprite GetPromptSprite(InputAction action, PlayerInput input)
        {
            return GetPromptSprite($"{action.actionMap}/{action.name}", input.currentControlScheme);
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(InputPromptMapping))]
    public class InputPromptMappingEditor : MarToolsEditor<InputPromptMapping>
    {
        public string currentControlScheme;

        protected override void OnEnable()
        {
            base.OnEnable();
            currentControlScheme = script.inputActions.controlSchemes.First().name;
        }



        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUI.BeginChangeCheck();

            GUILayout.BeginHorizontal();
            foreach (var item in script.inputActions.controlSchemes)
            {
                GUI.color = item.name == currentControlScheme ? Color.green : Color.white;

                if(GUILayout.Button($"{item.name}"))
                {
                    currentControlScheme = item.name;
                }
            }

            GUI.color = Color.white;
            GUILayout.EndHorizontal();



            foreach (var actionMap in script.inputActions.actionMaps)
            {
                foreach (var action in actionMap.actions)
                {
                    string path = $"{actionMap.name}/{action.name}";

                    if(!script.Mappings.Exists(x => x.actionPath == path))
                    {
                        script.Mappings.Add(new InputPromptMapping.Mapping() { actionPath = path });
                    }
                }
            }

            foreach (var item in script.Mappings)
            {
                GUILayout.BeginHorizontal();

                GUILayout.Label(item.actionPath);

                if(!item.PromptSprites.ContainsKey(currentControlScheme))
                {
                    item.PromptSprites.Add(currentControlScheme, null);
                }

                item.PromptSprites[currentControlScheme] = EditorGUILayout.ObjectField(item.PromptSprites[currentControlScheme], typeof(Sprite)) as Sprite;


                GUILayout.EndHorizontal();
            }

            if(EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(script);
            }
        }
    }

    #endif


}