using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;





#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MarTools
{
    public class InputPromptDisplay : MonoBehaviour
    {
        public InputPromptMapping mapping;
        public PlayerInput playerInput;
        [HideInInspector] public string displayedAction;


        private Image target;
        private Action cleanupAction;


        private void Awake()
        {
            target = GetComponent<Image>();

            if(playerInput)
            {
                SetPlayer(playerInput);
            }
        }

        private void ControlsChanged(PlayerInput obj)
        {
            UpdatePrompt();
        }

        private void UpdatePrompt()
        {
            string scheme = playerInput.currentControlScheme;

            if (scheme != null)
            {
                if (!target) target = GetComponent<Image>();

                Sprite sprite = mapping.GetPromptSprite(displayedAction, playerInput.currentControlScheme);
                target.sprite = sprite;
            }
        }

        public void SetPlayer(PlayerInput playerInput)
        {
            this.playerInput = playerInput;

            cleanupAction?.Invoke();
            cleanupAction = () => playerInput.onControlsChanged -= ControlsChanged;
            playerInput.onControlsChanged += ControlsChanged;

            UpdatePrompt();
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(InputPromptDisplay))]
    public class InputPromptDisplayEditor : MarToolsEditor<InputPromptDisplay>
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var mappings = script.mapping.Mappings;

            int currentIndex = mappings.FindIndex(x => x.actionPath == script.displayedAction);
            if (currentIndex < 0) currentIndex = 0;

            List<string> Options = mappings.ConvertAll(x => x.actionPath);
            int newIndex = EditorGUILayout.Popup(currentIndex, Options.ToArray());

            script.displayedAction = mappings[newIndex].actionPath;

            //foreach (var obj in script.mapping.Mappings)
            //{
            //    foreach (var item in obj.PromptSprites)
            //    {
            //        GUILayout.Label($"{item.Key}/{item.Value}");
            //    }
            //}

            if(Application.isPlaying)
            {
                GUILayout.Label($"Current layout: {script.playerInput.currentControlScheme}");
            }
        }
    }
#endif
}
