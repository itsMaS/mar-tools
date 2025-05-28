using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MarTools.Audio
{
    [Serializable]
    public class AudioEventData
    {
        public string eventName;
        public long frame;
    }

    [Serializable]
    public class AudioEventList
    {
        public List<AudioEventData> events = new List<AudioEventData>();

        public List<AudioEventData> ConsumeEvents(long frame)
        {
            var selected = events.FindAll(e => e.frame <= frame).ToList();
            events.RemoveAll(e => e.frame <= frame);

            return selected;
        }
    }


    public class AudioParameter
    {

    }

    public class LoopingAudioParameter : AudioParameter { }

    public static class AudioParameterUtility
    {
        public static bool TryGetParameter<T>(this AudioParameter[] parameters, out T parameter) where T : AudioParameter
        {
            parameter = parameters.OfType<T>().FirstOrDefault();
            return parameter != null;
        }
    }

    public class PositionAudioParameter : AudioParameter
    {
        public Vector3 position;
        public PositionAudioParameter(Vector3 position)
        {
            this.position = position;
        }
    }

    public class AudioManager
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        public static void ResetEvent()
        {
            OnAudioEvent = null;
        }

        public static Action<string, AudioParameter[]> OnAudioEvent;
        private static AudioEventContainer audioMappings => Resources.Load<AudioEventContainer>("AudioEvents");

        public static List<string> GetEventNames()
        {
            return audioMappings.mappings.Select(mapping => mapping.eventName)
                .ToList();
        }

        public static void PlayAudioEvent(string eventName, params AudioParameter[] parameters)
        {
            OnAudioEvent?.Invoke(eventName, parameters);
        }

        internal static void AddNewEvent(string stringValue)
        {
            if (audioMappings.mappings.Any(m => m.eventName == stringValue))
            {
                Debug.LogWarning($"Audio event '{stringValue}' already exists.");
                return;
            }

            audioMappings.mappings.Add(new AudioEventContainer.AudioMapping() { eventName = stringValue });

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(audioMappings);
#endif
        }
    }
}
