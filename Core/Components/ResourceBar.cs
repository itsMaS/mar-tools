namespace MarTools
{
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;

    public class ResourceBar : MonoBehaviour
    {
        public UnityEvent OnUpdated;
        public UnityEvent OnAdded;
        public UnityEvent OnSubtracted;


        public float totalValue = 1;
        public Vector2 mapping = new Vector2(0, 1);
        public float currentValue { get; private set; } = -1;
        public float currentNormalizedValue => currentValue / totalValue;

        [SerializeField] Image bottomFill;
        [SerializeField] Image topFill;

        public void Add(float amount)
        {
            SetValue(currentValue + amount);
        }

        public void Subtract(float amount)
        {
            SetValue(currentValue - amount);
        }

        public void SetValue(float amount)
        {
            SetValueNormalized(amount / totalValue);
        }

        public void UpdateWithoutAnimation()
        {
            StopAllCoroutines();

            bottomFill.fillAmount = currentNormalizedValue;
            topFill.fillAmount = currentNormalizedValue;
        }

        public void UpdateNormalizedWithoutAnimation(float value)
        {
            currentValue = value / totalValue;
            UpdateWithoutAnimation();
        }

        public void SetValueNormalized(float newNormalizedValue)
        {
            // Return if nothing changes
            if (newNormalizedValue == currentNormalizedValue) return;


            StopAllCoroutines();

            // Subtract
            if(currentNormalizedValue > newNormalizedValue)
            {
                topFill.fillAmount = newNormalizedValue.Remap(new Vector2(0, 1), mapping);
                SetTarget(bottomFill, newNormalizedValue);
                OnSubtracted.Invoke();
            }
            // Additive
            else
            {
                bottomFill.fillAmount = newNormalizedValue.Remap(new Vector2(0, 1), mapping);
                SetTarget(topFill, newNormalizedValue);
                OnAdded.Invoke();
            }
            currentValue = Mathf.Clamp01(newNormalizedValue) * totalValue;

            OnUpdated.Invoke();
        }

        private void SetTarget(Image image, float target)
        {
            float startValue = image.fillAmount.Remap(mapping, new Vector2(0,1));
            float delta = Mathf.Abs(target - startValue);

            if (delta > 0.1f)
            {
                this.DelayedAction(0.5f, () =>
                {

                }, t =>
                {
                    image.fillAmount = Mathf.Lerp(startValue, target, t).Remap(new Vector2(0, 1), mapping);
                }, false, Utilities.Ease.OutQuad);
            }
            else
            {
                image.fillAmount = target;
            }
        }
    }
}

