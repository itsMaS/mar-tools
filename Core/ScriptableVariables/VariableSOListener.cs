using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MarTools
{
    public abstract class VariableSOListener<T> : MonoBehaviour
    {
        public UnityEvent<T> OnValueChanged;
        public VariableSO<T> targetVariable;
        public bool fireOnValueChangedOnEnabled = true;

        private void OnEnable()
        {
            targetVariable.OnValueChanged.AddListener(ValueChanged);
            if(fireOnValueChangedOnEnabled)
            {
                ValueChanged(targetVariable.Value);
            }
        }
        private void OnDisable() 
        {
            targetVariable.OnValueChanged.RemoveListener(ValueChanged);
        }
        private void ValueChanged(T arg0)
        {
            OnValueChanged.Invoke(arg0);
        }
    }
}
