using System.Collections.Generic;
using UnityEngine;

namespace MarTools.Audio
{
    public class AudioPlayer : MonoBehaviour
    {
        [AudioDropdown]
        public string eventName;

        public bool playOnAwake = false;
        public bool looping = false;

        public void Play()
        {
            List<AudioParameter> parameters = new List<AudioParameter>();

            if(looping)
            {
                parameters.Add(new LoopingAudioParameter());
            }

            AudioManager.PlayAudioEvent(eventName, parameters.ToArray());
        }

        private void Awake()
        {
            if(playOnAwake)
            {
                Play();
            }
        }
    }
}
