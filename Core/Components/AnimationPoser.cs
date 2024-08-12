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

        public float speed = 0;
    
        private void Start()
        {
            UpdateAnimation();
        }

        private void Update()
        {
            progress += (speed / clip.length) * Time.deltaTime;
            progress %= 1;

            UpdateAnimation();
        }

        private void OnValidate()
        {
            UpdateAnimation();
        }
    
        private void UpdateAnimation()
        {
            Quaternion rot = transform.rotation;
            Vector3 pos = transform.position;
            Vector3 scale = transform.localScale;

            clip.SampleAnimation(gameObject, progress * clip.length);
            gameObject.transform.rotation = rot;
            gameObject.transform.position = pos;
            gameObject.transform.localScale = scale;
        }
    }
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(AnimationPoser))]
    public class AnimationPoserEditor : Editor
    {
    
    }
    #endif
    
}