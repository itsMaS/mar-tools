namespace MarTools
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
#endif

    public static class TagsUtilities
    {
        public static bool ContainsTag(this GameObject go, string tag)
        {
            if(go.TryGetComponent<Tags>(out Tags tagComponent))
            {
                return tagComponent.ContainsTag(tag);
            }
            else
            {
                return false;
            }
        }

        public static void AddTag(this GameObject go, TagSO tag)
        {
            Tags tagComponent = null;
            if (!go.TryGetComponent<Tags>(out tagComponent))
            {
                tagComponent = go.AddComponent<Tags>();
            }
            tagComponent._Tags.Add(tag);
        }
    }

    public class Tags : MonoBehaviour
    {
        public List<TagSO> _Tags = new List<TagSO>();

        public bool ContainsTag(string tag)
        {
            return _Tags.Exists(item => item.name == tag);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Tags))]
    public class TagsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var tagSOs = EditorUtilities.FindAssets<TagSO>();
        }
    }
#endif
}

