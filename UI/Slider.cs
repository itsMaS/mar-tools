namespace MarTools
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;

    [RequireComponent(typeof(Button))]
    public class Slider : MonoBehaviour
    {
        public enum Type
        {
            Horizontal,
            Vertical,
            Both,
        }

        public UnityEvent<Vector2> OnUpdateValue2D;
        public UnityEvent<float> OnUpdateValue;

        public Type type;
        public RectTransform knobTransform;

        public Vector2 value { get; private set; }
        Button button;
        private void OnEnable()
        {
            button = GetComponent<Button>();

            button.OnUpdateCursorPositionNormalized.AddListener(UpdateValue);
        }

        public void UpdateValue(Vector2 arg0)
        {
            if (!button.manager.holdingSubmit) return;

            value = arg0;

            OnUpdateValue2D.Invoke(value);
            OnUpdateValue.Invoke(value.x);

            var parent = knobTransform.parent.GetComponent<RectTransform>();

            Vector2 size = parent.sizeDelta;
            Vector2 anchorPosition = new Vector2(size.x * (arg0.x-0.5f), size.y * (arg0.y-0.5f));

            switch (type)
            {
                case Type.Horizontal:
                    anchorPosition.y = 0;
                    break;
                case Type.Vertical:
                    anchorPosition.x = 0;
                    break;
            }

            knobTransform.anchoredPosition = anchorPosition;
        }

        public void UpdateValue(float value)
        {
            UpdateValue(Vector2.one * value);
        }

        private void OnDisable()
        {
            button.OnUpdateCursorPositionNormalized.RemoveListener(UpdateValue);
        }
    }
}
