namespace MarTools
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.Events;

    public class TabGroup : MonoBehaviour
    {
        public UnityEvent OnNextTab;
        public UnityEvent OnPreviousTab;
        public UnityEvent<TabNode> OnSelected;

        private List<TabNode> TabNodes = new List<TabNode>();

        public int selectedIndex { get; private set; } = 0;

        private void Awake()
        {
            TabNodes = GetComponentsInChildren<TabNode>().ToList();
            TabNodes.ForEach(x => x.Initialize(this));
            if (TabNodes.Count == 0) Debug.LogError("No Tab Nodes found inside the tab group");
        }

        private void Start()
        {
            Select(0);
        }

        internal void NextTab()
        {
            Select((selectedIndex + 1) % TabNodes.Count);
            OnNextTab.Invoke();
        }

        internal void PreviousTab()
        {
            Select((selectedIndex - 1 + TabNodes.Count) % TabNodes.Count);
            OnPreviousTab.Invoke();
        }

        public void Select(int index)
        {
            selectedIndex = index;
            TabNode selected = null;

            for (int i = 0; i < TabNodes.Count; i++)
            {
                TabNode node = TabNodes[i];
                if(i == selectedIndex)
                {
                    selected = node;
                    node.Select();
                }
                else
                {
                    node.Deselect();
                }
            }

            OnSelected.Invoke(selected);
        }

        public void Select(TabNode node)
        {
            Select(TabNodes.IndexOf(node));
        }
    }
}
