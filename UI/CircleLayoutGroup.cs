namespace MarTools
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    [ExecuteAlways]
    public class CircleLayoutGroup : MonoBehaviour
    {
        public float radius = 10f;

        List<RectTransform> Children = new List<RectTransform>();
        private void FetchChildren()
        {
            Children.Clear();
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);

                // Skip layout element if an element ignores it
                if (child.TryGetComponent<LayoutElement>(out var le) && le.ignoreLayout) continue;

                Children.Add(child.GetComponent<RectTransform>());
            }
        }

        private void UpdateLayout()
        {
        }

        private void Update()
        {
        
        }
    }
}

