namespace MarTools.Audio
{
    #if UNITY_EDITOR
    using UnityEditor;
    #endif
    using UnityEngine;

    [CreateAssetMenu(fileName = "New MarAudio clip", menuName = "MarTools/Audio Clip")]
    public class MarAudioClipSO : ScriptableObject
    {
        public void Play()
        {
            MarAudioController.Play(this, null);
        }

        public void Play(GameObject gameObject)
        {
            MarAudioController.Play(this, gameObject);
        }
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(MarAudioClipSO))]
    public class MarAudioClipSOEditor : Editor
    {
    }
    #endif
}
