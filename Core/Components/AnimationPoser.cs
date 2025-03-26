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
        [SerializeField] public AnimationClip clip;
        [Range(0, 1)] public float progress;

        public float speed = 0;
    
        private void Start()
        {
            UpdateAnimation();
        }

        private void Update()
        {
            if(speed > 0)
            {
                progress += (speed / clip.length) * Time.deltaTime;
                progress %= 1;

                UpdateAnimation();
            }
        }

        private void OnValidate()
        {
            UpdateAnimation();
        }
    
        private void UpdateAnimation()
        {
            if (!clip) return;

            Quaternion rot = transform.rotation;
            Vector3 pos = transform.position;
            Vector3 scale = transform.localScale;

            clip.SampleAnimation(gameObject, progress * clip.length);
            gameObject.transform.rotation = rot;
            gameObject.transform.position = pos;
            gameObject.transform.localScale = scale;
        }

        public void SetPose(float progress)
        {
            this.progress = progress;
            UpdateAnimation();
        }
    }
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(AnimationPoser))]
    public class AnimationPoserEditor : Editor
    {

        AnimationPoser script;
        private void OnEnable()
        {
            script = (AnimationPoser)target;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if(script.clip)
            {
                GUILayout.Label($"Clip duration: {script.clip.length}");
            }
        }
    }
    #endif
    
}