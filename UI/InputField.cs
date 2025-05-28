using System;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.PlayerLoop;

namespace MarTools
{
    [RequireComponent(typeof(FixedTMPInputField))]
    public class InputField : UIElement // TODO(Tautvydas): Merge this with FixedTMPInputField
    {
        private bool _useKeyboardInput = true;
        public bool UseKeyboardInput
        {
            get
            {
                return _useKeyboardInput;
            }
            set
            {
                _useKeyboardInput = value;
                _inputField.enabled = value;
                if(value)
                {
                    _inputField.onValueChanged.RemoveListener(OnValueChanged_InternalInputField);
                    _inputField.onValueChanged.AddListener(OnValueChanged_InternalInputField);
                    _inputField.FixedOnSubmit.RemoveListener(OnSubmit_InternalInputField);
                    _inputField.FixedOnSubmit.AddListener(OnSubmit_InternalInputField);
                }
                else
                {
                    _inputField.onValueChanged.RemoveListener(OnValueChanged_InternalInputField);
                    _inputField.FixedOnSubmit.RemoveListener(OnSubmit_InternalInputField);
                }
            }
        }
         
        public UnityEvent<string> OnValueChanged;
        public UnityEvent<int> OnCaretPositionChanged;

        private void OnValueChanged_InternalInputField(string text)
        {
            if (!UseKeyboardInput)
                return;

            OnValueChanged?.Invoke(text);
        }

        private void OnSubmit_InternalInputField(string text)
        {
            if (!UseKeyboardInput)
                return;

            Debug.Log($"Calling OnSubmit_InternalInputField in {name}");
            OnDeselected?.Invoke();
        }

        [SerializeField] FixedTMPInputField _inputField;
        private bool _deactivated = false;
        private int _caretPosition = 0;
        public int CaretPosition
        {
            get
            {
                if (UseKeyboardInput)
                    _caretPosition = _inputField.caretPosition;

                return _caretPosition;
            }
            set
            {
                _inputField.caretPosition = value;
                _caretPosition = value;
            }
        }
        private string _text = "";
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _inputField.text = value;
                _text = value;
            }
        }

        private void Awake()
        {
            UseKeyboardInput = UseKeyboardInput;
            _text = _inputField.text;
            _inputField.caretPosition = _inputField.text.Length;
            _caretPosition = _inputField.caretPosition;
            _previousCaretPosition = CaretPosition;
        }

        private int _previousCaretPosition = 0;

        private void Update()
        {
            if (!UseKeyboardInput)
                return;

            if(_previousCaretPosition != CaretPosition)
            {
                _previousCaretPosition = CaretPosition;
                OnCaretPositionChanged?.Invoke(_previousCaretPosition);
            }
        }

        public void DeactivateInputField(bool clearSelection)
        {
            _deactivated = false;

            OnDeselected?.Invoke();

            if (clearSelection)
                EventSystem.current.SetSelectedGameObject(null);
        }

        public void ForceLabelUpdate()
        {
            _inputField.ForceLabelUpdate();
            _caretPosition = _inputField.caretPosition;
            _text = _inputField.text;
        }

        protected override void OnSubmitedInternal()
        {
            if (_useKeyboardInput)
                _inputField.Select();

            base.OnSubmitedInternal();
        }

        protected override void OnSelectedInternal()
        {
            base.OnSelectedInternal();
        }

        protected override void OnDeselectedInternal()
        {
            if (_useKeyboardInput)
                EventSystem.current.SetSelectedGameObject(null);

            base.OnDeselectedInternal();
        }
    }
}
