namespace MarTools
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Mono.CSharp;
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
        public bool fixedSize = false;
        public int horizontalElementCount = 4;
        public int verticalElementCount = 3;
        public int leftSideIndex = 0;
        public float shiftAmount = 100;
        private float startX = 0;

        public int elementCount = 0;

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

            startX = content.anchoredPosition.x;
            target.y = content.anchoredPosition.y;
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

            elementCount = Elements.Count;
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
            if (fixedSize)
                FixedMovement();
            else
                DynamicMovement();
        }

        private void DynamicMovement()
        {
            var selected = Elements.Find(x => x.selected);

            target = new Vector2(Mathf.Clamp(target.x, -content.sizeDelta.x + Mathf.Min(rectTransform.sizeDelta.x, content.sizeDelta.x), 0), target.y);

            if (selected != null)
            {
                var offsets = GetCornerOffsets(selected.rectTr, rectTransform);
                //Debug.Log($"Offset:{offsets[0].x} | {offsets[2].x} Width:{rectTransform.sizeDelta.x}");

                //Debug.Log($"G:{offsets[0]} O:{offsets[2]}");

                if (offsets[2].x > 0)
                {
                    target = content.anchoredPosition + new Vector2(-offsets[2].x * 1.1f, 0);
                    //Debug.Log("move right");
                }
                else if (offsets[0].x < 0)
                {
                    target = content.anchoredPosition + new Vector2(-offsets[0].x * 1.1f, 0);
                    //Debug.Log("move left");
                }
            }

            content.anchoredPosition = Vector2.Lerp(content.anchoredPosition, target, Time.deltaTime * 20);
        }

        private void FixedMovement()
        {
            var selected = Elements.Find(x => x.selected);

            if(selected != null)
            {
                var index = Elements.IndexOf(selected);

                int maxRow = Math.Max((int)(Mathf.Ceil(Elements.Count / verticalElementCount)), 1);
                int row = index % maxRow;

                if(row >= leftSideIndex + horizontalElementCount)
                {
                    leftSideIndex = Math.Clamp(leftSideIndex + 1, 0, maxRow);
                }else if(row < leftSideIndex)
                {
                    leftSideIndex = Math.Clamp(leftSideIndex - 1, 0, maxRow);
                }
            }

            target = new Vector2(startX -leftSideIndex * shiftAmount, target.y);

            content.anchoredPosition = Vector2.Lerp(content.anchoredPosition, target, Mathf.Clamp(Time.deltaTime * 20, 0, 1));
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
