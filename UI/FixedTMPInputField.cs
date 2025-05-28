using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace MarTools
{
    public class FixedTMPInputField : TMP_InputField
    {
        bool _deselecting = false;
        public UnityEvent<string> FixedOnSubmit; // Note(Tautvydas): Do not use TMP_InputField.onSubmit externally

        protected override void OnEnable()
        {
            onSubmit.RemoveAllListeners();
            onSubmit.AddListener(FixedOnSubmit_Internal);
        }

        protected override void OnDisable()
        {
            onSubmit.RemoveListener(FixedOnSubmit_Internal);

        }

        protected override void OnDestroy()
        {
            onSubmit.RemoveListener(FixedOnSubmit_Internal);
        }

        public override void OnSelect(BaseEventData eventData)
        {
            if (!enabled)
                return;

            Debug.Log($"OnSelect called in {name}");

            _deselecting = false;
            base.OnSelect(eventData);
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            Debug.Log($"OnDeselect called in {name} with _deselecting at: " + (_deselecting ? "true" : "fase"));

            if (_deselecting)
            {
                base.OnDeselect(eventData);
            }
            else
                this.OnSelect(eventData);
        }

        public override void OnSubmit(BaseEventData eventData)
        {
            return;

            Debug.Log("OnSubmit called with _deselecting at: " + (_deselecting ? "true" : "fase"));

            foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKey(KeyCode.Return))
                {
                    _deselecting = true;
                }
            }
        }

        private void FixedOnSubmit_Internal(string text)
        {
            if (!enabled)
                return;

            Debug.Log($"OnSubmit called in {name} with _deselecting at: " + (_deselecting ? "true" : "fase"));

            if (Input.GetKey(KeyCode.Return))
            {
                _deselecting = true;
                FixedOnSubmit.Invoke(text);
                DeactivateInputField(true);
                SendOnFocusLost();
            }
        }
    }
}
