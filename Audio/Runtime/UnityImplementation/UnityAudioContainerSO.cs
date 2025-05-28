using MarTools.Audio;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu]
public class UnityAudioContainerSO : ScriptableObject
{
    [System.Serializable]
    public class AudioBinding
    {
        public string eventName;
        public AudioResource audioResource;
        public float volume = 1;
    }

    [HideInInspector] public List<AudioBinding> Bindings = new List<AudioBinding>();
}

#if UNITY_EDITOR
[CustomEditor(typeof(UnityAudioContainerSO))]
public class UnityAudioContainerSOEditor : Editor
{
    UnityAudioContainerSO script;
    Vector2 scrollPosition;
    string searchText
    {
        get
        {
            return EditorPrefs.GetString("UnityAudioContainerSOEditor_SearchText", "");
        }
        set
        {
            EditorPrefs.SetString("UnityAudioContainerSOEditor_SearchText", value);
        }
    }

    private void OnEnable()
    {
        script = (UnityAudioContainerSO)target;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var events = AudioManager.GetEventNames();
        var bindings = script.Bindings;

        GUILayout.Space(20);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Search:", GUILayout.Width(50));
        searchText = GUILayout.TextField(searchText, GUILayout.Width(200));
        GUILayout.EndHorizontal();


        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300), GUILayout.ExpandHeight(true));

        for (int i = 0; i < events.Count; i++)
        {
            var eventName = events[i];

            if (searchText.Length > 0 && !eventName.ToLower().Contains(searchText.ToLower())) continue;

            GUILayout.BeginHorizontal();
            GUILayout.Label($"{eventName}", GUILayout.Width(200));

            var obj = bindings.Find(b => b.eventName == eventName);
            if (obj == null)
            {
                obj = new UnityAudioContainerSO.AudioBinding { eventName = eventName };
                bindings.Add(obj);

                EditorUtility.SetDirty(script);
            }

            var newObj = (AudioResource)EditorGUILayout.ObjectField(obj.audioResource, typeof(AudioResource), false);

            obj.volume = EditorGUILayout.Slider(obj.volume, 0f, 1f, GUILayout.Width(150));

            GUILayout.EndHorizontal();

            if (obj.audioResource != newObj)
            {
                obj.audioResource = newObj;
                EditorUtility.SetDirty(script);
            }
        }
        GUILayout.EndScrollView();
    }
}
#endif
