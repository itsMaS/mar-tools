namespace MarTools
{
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.PlayerLoop;

    public class Slider : UIElement
    {
        public enum Type
        {
            Horizontal = 0,
            Vertical = 1,
        }

        public UnityEvent<int, int> OnValueChanged;



        public Type type = Type.Horizontal;
        public int valuePositions = 10;
        public int currentValue { get; private set; }
        public float normalizedValue => (float)currentValue / (valuePositions-1);

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
                int newValue = currentValue;
                if(type == Type.Horizontal)
                {
                    newValue = direction.x > 0 ? currentValue + 1 : currentValue - 1;
                }
                else
                {
                    newValue = direction.y > 0 ? currentValue + 1 : currentValue - 1;
                }
                newValue = Mathf.Clamp(newValue, 0, valuePositions-1);
                SetValue(newValue);
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
                int newValue = Mathf.RoundToInt(cursorPositionNormalizedClamped.x * (valuePositions-1));
                SetValue(newValue);
            }
        }

        public void SetValue(int value)
        {
            if(value != currentValue)
            {
                int oldValue = currentValue;
                currentValue = value;
                OnValueChanged.Invoke(oldValue, currentValue);
            }
 
            UpdateKnobPosition();
        }
    }
}
