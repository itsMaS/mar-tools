namespace MarTools
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.InputSystem;

    [RequireComponent(typeof(RectTransform))]
    public class ScrollArea : MonoBehaviour
    {
        List<UIElement> Elements = new List<UIElement>();
        RectTransform rectTransform;
        UIManager manager;

        public RectTransform content;


        public InputActionReference scrollAction;


        Vector2 target;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            manager = GetComponentInParent<UIManager>();

            if(!content) content = transform.GetChild(0).GetComponent<RectTransform>();
            if (!content)
            {
                Debug.Log("Content rect tr not found");
                return;
            }
        }

        private void Start()
        {
            if(scrollAction != null)
            {
                manager.SubscribeInput<Vector2>(scrollAction, Scroll, InputActionPhase.Performed);
            }

            UpdateElements();
        }


        public void UpdateElements()
        {
            Elements = GetComponentsInChildren<UIElement>().ToList();
        }

        private void Scroll(Vector2 arg0)
        {
            if (!Elements.Exists(x => x.selected)) return;

            //Debug.Log($"{arg0}");
            target.x += arg0.y;
        }

        public void ScrollRight()
        {
            target.x += rectTransform.sizeDelta.x * 0.1f;
        }
        public void ScrollLeft()
        {
            target.x -= rectTransform.sizeDelta.x * 0.1f;
        }

        private void Update()
        {
            var selected = Elements.Find(x => x.selected);

            target = new Vector2(Mathf.Clamp(target.x, -content.sizeDelta.x + Mathf.Min(rectTransform.sizeDelta.x, content.sizeDelta.x), 0), target.y);
            
            if(selected != null)
            {
                var offsets = GetCornerOffsets(selected.rectTr, rectTransform);
                //Debug.Log($"Offset:{offsets[0].x} | {offsets[2].x} Width:{rectTransform.sizeDelta.x}");

                //Debug.Log($"G:{offsets[0]} O:{offsets[2]}");

                if (offsets[2].x > 0)
                {
                    target = content.anchoredPosition + new Vector2(-offsets[2].x, 0);
                    //Debug.Log("move right");
                }
                if (offsets[0].x < 0)
                {
                    target = content.anchoredPosition + new Vector2(-offsets[0].x, 0);
                    //Debug.Log("move left");
                }
            }

            content.anchoredPosition = Vector2.Lerp(content.anchoredPosition, target, Time.deltaTime * 20);
        }

        private List<Vector3> GetCornerOffsets(RectTransform inner, RectTransform outer)
        {
            // Get the world corners of the RectTransforms
            Vector3[] innerCorners = new Vector3[4];
            Vector3[] outerCorners = new Vector3[4];

            inner.GetWorldCorners(innerCorners);
            outer.GetWorldCorners(outerCorners);

            List<Vector3> result = new List<Vector3>(4);

            Color[] debugColor = new Color[4] { Color.green, Color.yellow, Color.Lerp(Color.yellow, Color.red, 0.5f), Color.red };
            for (int i = 0; i < 4; i++)
            {
                Debug.DrawLine(innerCorners[i], outerCorners[i], debugColor[i]);
                result.Add((innerCorners[i]-outerCorners[i]) / manager.transform.localScale.x);
            }

            return result;
        }
    }
}
