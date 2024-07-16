namespace MarTools 
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;

    public class TabNode : MonoBehaviour
    {
        public UnityEvent OnSelected;
        public UnityEvent OnDeselected;

        TabGroup group;

        public void Initialize(TabGroup group)
        {
            this.group = group;

            if(TryGetComponent<Button>(out var button))
            {
                button.OnClick.AddListener(SelectInternal);
            }
        }

        private void SelectInternal()
        {
            group.Select(this);
        }

        public void Select()
        {
            OnSelected.Invoke();
        }

        public void Deselect()
        {
            OnDeselected.Invoke();
        }
    }
}
