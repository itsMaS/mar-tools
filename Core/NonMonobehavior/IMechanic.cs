namespace MarTools
{
    #if UNITY_EDITOR
    using UnityEditor;
    #endif
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public interface IMechanic
    {
        public void Activate(GameObject gameobject);
    }

    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(IMechanic), true)]
    public class IMechanicDrawer : PolymorphicDrawer<IMechanic> { }
#endif

    [System.Serializable]
    public class PlaySoundUnity : IMechanic
    {
        public AudioClip clip;
        public float volume;
        public float pitch;

        public void Activate(GameObject gameobject)
        {
            GameObject go = new GameObject($"Sound {clip.name}");
            AudioSource source = go.AddComponent<AudioSource>();

            source.clip = clip;
            source.volume = volume;
            source.pitch = pitch;

            source.Play();

            GameObject.Destroy(go, clip.length);
        }
    }
}
