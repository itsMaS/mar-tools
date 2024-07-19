namespace MarTools
{
    using UnityEngine;
    using UnityEngine.PlayerLoop;

    public class Slider : UIElement
    {
        public enum Type
        {
            Horizontal = 0,
            Vertical = 1,
        }

        public Type type = Type.Horizontal;
        public int valuePositions = 10;
        public int currentValue { get; private set; }
        public float normalizedValue => (float)currentValue / valuePositions;

        [SerializeField] RectTransform knobTransform;

        private void Awake()
        {
            UpdateKnobPosition();
        }

        protected override void OnNavigateInternal(Vector2 direction)
        {
            if (type == Type.Horizontal && Mathf.Abs(direction.y) > Mathf.Abs(direction.x)) base.OnNavigateInternal(direction);
            else if (type == Type.Vertical && Mathf.Abs(direction.x) > Mathf.Abs(direction.y)) base.OnNavigateInternal(direction);
            else
            {
                if(type == Type.Horizontal)
                {
                    currentValue = direction.x > 0 ? currentValue + 1 : currentValue - 1;
                }
                else
                {
                    currentValue = direction.y > 0 ? currentValue + 1 : currentValue - 1;
                }
                currentValue = Mathf.Clamp(currentValue, 0, valuePositions);
                UpdateKnobPosition();
            }
        }

        private void UpdateKnobPosition()
        {
            if(knobTransform != null)
            {
                Vector2 size = rectTr.sizeDelta;

                if(type == Type.Horizontal)
                {
                    knobTransform.anchoredPosition = new Vector2(Mathf.Lerp(-size.x/2, size.x/2, normalizedValue), knobTransform.anchoredPosition.y);
                }
                else
                {
                    knobTransform.anchoredPosition = new Vector2(knobTransform.anchoredPosition.x, Mathf.Lerp(-size.y / 2, size.y / 2, normalizedValue));
                }

            }
        }

        private void Update()
        {
            if (selected && manager.currentNavigationType == UIManager.NavigationType.Pointer && manager.holdingSubmit)
            {
                currentValue = Mathf.RoundToInt(cursorPositionNormalizedClamped.x * valuePositions);
                UpdateKnobPosition();
            }
        }
    }
}
