namespace MarTools.Audio
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class MarAudioController : MonoBehaviour
    {
        private static MarAudioController Instance;

        Dictionary<Transform, Transform> PositionMappings = new Dictionary<Transform, Transform>();

        public static void Play(MarAudioClipSO marAudioClipSO, GameObject origin)
        {
            Initialize();

            // TEMP CODE

            GameObject go = new GameObject($"{marAudioClipSO.name}");
            go.transform.parent = Instance.gameObject.transform;
            var newSource = go.AddComponent<AudioSource>();

            if(origin)
            {
                Instance.PositionMappings.Add(go.transform, origin.transform);
                newSource.spatialBlend = 1;
            }
            else
            {
                newSource.spatialBlend = 0;
            }
        }

        private static void Initialize()
        {
            if(!Instance || !Instance.gameObject)
            {
                GameObject instanceGO = new GameObject("[MarAudioController]");
                DontDestroyOnLoad(instanceGO);

                Instance = instanceGO.AddComponent<MarAudioController>();
            }
        }

        private void Update()
        {
            foreach (var item in PositionMappings.ToArray())
            {
                if(!item.Value)
                {
                    Destroy(item.Key.gameObject);
                    PositionMappings.Remove(item.Key);
                }
                else
                {
                    item.Key.position = item.Value.position;
                }
            }
        }
    }
}
