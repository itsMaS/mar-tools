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
                topFill.fillAmount = newNormalizedValue;
                SetTarget(bottomFill, newNormalizedValue);
                OnSubtracted.Invoke();
            }
            // Additive
            else
            {
                bottomFill.fillAmount = newNormalizedValue;
                SetTarget(topFill, newNormalizedValue);
                OnAdded.Invoke();
            }
            currentValue = Mathf.Clamp01(newNormalizedValue) * totalValue;

            OnUpdated.Invoke();
        }

        private Coroutine SetTarget(Image image, float target)
        {
            float startValue = image.fillAmount;
            return this.DelayedAction(0.5f, () =>
            {

            }, t =>
            {
                image.fillAmount = Mathf.Lerp(startValue, target, t);
            }, false, Utilities.Ease.OutQuad);
        }
    }
}

