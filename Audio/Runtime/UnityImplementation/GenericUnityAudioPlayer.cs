using UnityEngine;
using System.Collections.Generic;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MarTools.Audio
{
    [DefaultExecutionOrder(-1000)]
    public class GenericUnityAudioPlayer : MonoBehaviour
    {
        public UnityAudioContainerSO audioList;

        List<AudioSource> instances = new List<AudioSource>();

        private void Awake()
        {
            AudioManager.OnAudioEvent += HandleAudioEvent;
        }

        private void HandleAudioEvent(string eventName, AudioParameter[] parameters)
        {
            var obj = audioList.Bindings.Find(b => b.eventName == eventName);

            if (obj != null)
            {
                GameObject go = new GameObject($"{eventName}_Audio");
                var source = go.AddComponent<AudioSource>();

                if (parameters.TryGetParameter<PositionAudioParameter>(out var positionParameter))
                {
                    source.transform.position = positionParameter.position;
                }

                if(parameters.TryGetParameter<LoopingAudioParameter>(out var loopingParameter))
                {
                    source.loop = true;
                }


                source.volume = obj.volume;

                source.resource = obj.audioResource;
                source.Play();

                instances.Add(source);
            }
            else
            {
                Debug.LogWarning($"Missing audio for event: {eventName}");
            }
        }

        int currentInstanceLookup;
        private void FixedUpdate()
        {
            if(instances.Count == 0)
            {
                return;
            }

            currentInstanceLookup++;
            currentInstanceLookup %= instances.Count;

            //Debug.Log($"{instances[currentInstanceLookup].gameObject.name} is playing: {instances[currentInstanceLookup].isPlaying}");


            if (!instances[currentInstanceLookup].isPlaying)
            {
                Destroy(instances[currentInstanceLookup].gameObject);
                instances.RemoveAt(currentInstanceLookup);
            }
        }
    }
}


