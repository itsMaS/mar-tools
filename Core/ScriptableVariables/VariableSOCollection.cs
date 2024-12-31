namespace MarTools
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;

    public class VariableSOCollection<T> : ScriptableObject
    {
        public UnityEvent<T> OnAdded;
        public UnityEvent<T> OnRemoved;

        public List<T> Objects = new List<T>();

        public void Add(T obj)
        {
            OnAdded.Invoke(obj);
        }

        public void Remove(T obj)
        {
            OnRemoved.Invoke(obj);
        }
    }
}

