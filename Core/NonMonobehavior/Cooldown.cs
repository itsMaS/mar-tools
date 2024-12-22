namespace MarTools
{
    using System;
    using UnityEngine;

    [System.Serializable]
    public class Cooldown
    {
        public float duration = 0.5f;
        public bool timeScaled = true;

        private float performTimestamp = -99;
        public float timeSincePerform => (timeScaled ? Time.time : Time.unscaledTime) - performTimestamp;
        public float cooldownProgress => Mathf.Clamp01(timeSincePerform / duration);

        public void Refresh()
        {
            performTimestamp = -duration;
        }

        public bool TryPerform()
        {
            if(cooldownProgress >= 1)
            {
                performTimestamp = timeScaled ? Time.time : Time.unscaledTime;
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}
