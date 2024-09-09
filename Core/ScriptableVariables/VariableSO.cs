using UnityEngine;
using UnityEngine.Events;

namespace MarTools
{
    public abstract class VariableSO<T> : ScriptableObject
    {
        public UnityEvent<T> OnValueSet;
        public UnityEvent<T> OnValueChanged;

        public T Value 
        { 
            get
            {
                return _value;
            }
            set
            {
                var newValue = value;
                _value = value;

                if(!newValue.Equals(_value))
                {
                    OnValueChanged.Invoke(value);
                }

                OnValueSet?.Invoke(value);
            }
        }
        private T _value;
    }
}
