namespace MarTools
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    
    #if UNITY_EDITOR
    using UnityEditor;
    #endif
    
    public class AnimationPoser : MonoBehaviour
    {
        [SerializeField] AnimationClip clip;
        [Range(0, 1)] public float progress;
    
        private void Start()
        {
            UpdateAnimation();
        }
    
        private void OnValidate()
        {
            UpdateAnimation();
        }
    
        private void UpdateAnimation()
        {
            Quaternion rot = transform.rotation;
            clip.SampleAnimation(gameObject, progress);
            gameObject.transform.rotation = rot;
        }
    }
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(AnimationPoser))]
    public class AnimationPoserEditor : Editor
    {
    
    }
    #endif
    
}