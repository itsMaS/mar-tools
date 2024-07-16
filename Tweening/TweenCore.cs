namespace MarTools
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.InputSystem.LowLevel;
    using UnityEngine.UIElements;
    using System;
    using UnityEngine.Events;
    #if UNITY_EDITOR
    using UnityEditor;
    #endif

    public abstract class TweenCore : MonoBehaviour
    {
        public UnityEvent OnComplete;
        public UnityEvent OnPlayForwards;
        public UnityEvent OnPlayBackwards;
        public UnityEvent OnStop;

        public bool looping = false;
        public bool yoyo = false;
        public bool playOnEnable = false;
        public bool relative = true;
        public Vector2 delayRange = Vector2.zero;

        public Utilities.Ease ease = Utilities.Ease.InOutQuad;

        public bool differentEaseBackwards = false;
        public Utilities.Ease backwardsEase = Utilities.Ease.InOutQuad;

        public bool local = true;
        public float duration = 1f;
        private Coroutine coroutine;
        private bool forward = false;

        private float lastInterpolator;

        private void Awake()
        {
            SetPose(0);
        }

        protected virtual void Reset()
        {

        }

        private void OnEnable()
        {
            if (playOnEnable)
            {
                this.DelayedAction(delayRange.PickRandom(), () => PlayForwards());

            }
        }

        public void PlayForwards()
        {
            PlayForward(false);
        }
        public void PlayBackwards()
        {
            PlayBackwards(false);
        }
        private void PlayForward(bool repeat = false)
        {
            float from = 0;
            float to = 1;

            if (relative) from = lastInterpolator;

            if (!repeat)
            {
                OnPlayForwards.Invoke();
            }
            forward = true;
            ResetCoroutine();
            coroutine = this.DelayedAction(duration, () =>
            {
                Complete();
                if (yoyo)
                {
                    PlayBackwards();
                }
                else if (looping)
                {
                    PlayForward(true);
                }
            }, t =>
            {
                lastInterpolator = Mathf.LerpUnclamped(from, to, t);
                SetPose(lastInterpolator);
            }, true, ease);
        }
        private void PlayBackwards(bool repeat = false)
        {
            float from = 1;
            float to = 0;

            if (relative) from = lastInterpolator;

            if (!repeat)
            {
                OnPlayBackwards.Invoke();
            }
            forward = false;
            ResetCoroutine();
            coroutine = this.DelayedAction(duration, () =>
            {
                if (looping && yoyo)
                {
                    PlayForwards();
                }
                else
                {
                    Complete();
                }

            }, t =>
            {
                lastInterpolator = Mathf.LerpUnclamped(from,to, t);
                SetPose(lastInterpolator);
            }, true, differentEaseBackwards ? backwardsEase : ease);
        }

        public void Stop()
        {
            OnStop.Invoke();
            ResetCoroutine();
        }

        public void Flip()
        {
            if (forward)
            {
                PlayBackwards();
            }
            else
            {
                PlayForwards();
            }
        }

        public void Toggle()
        {
            if (coroutine != null)
            {
                Stop();
            }
            else
            {
                PlayForwards();
            }
        }


        private void Complete()
        {
            coroutine = null;
            OnComplete.Invoke();
        }

        private void ResetCoroutine()
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }
        }

        public abstract void SetPose(float t);
    }
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(TweenCore), true)]
    public class TweenCoreEditor : Editor
    {
        float pose;
        bool playing = false;
    
        public override void OnInspectorGUI()
        {
            if(!Application.isPlaying)
            {
                float newPose = EditorGUILayout.Slider(pose, 0, 1);
                
    
                base.OnInspectorGUI();
    
                var script = (TweenCore)target;
    
                if(!playing)
                {
                    SetPose(script, pose);
                    pose = newPose;
                }
    
                if (!playing && GUILayout.Button("Play Tween"))
                {
                    playing = true;
                    pose = 0;
                }
    
                if (playing && GUILayout.Button("Stop"))
                {
                    playing = false;
                }
            }
        }
    
        private void EditorUpdate()
        {
            if(playing)
            {
                var script = (TweenCore)target;
                pose += Time.deltaTime / script.duration;
                SetPose(script, pose);
    
                if(pose >= 1)
                {
                    playing = false;
                }
    
                SceneView.RepaintAll();
            }
        }
    
        private void SetPose(TweenCore script, float t)
        {
            script.SetPose(Utilities.Eases[script.ease].Invoke(t));
        }
    
        private void OnEnable()
        {
            EditorApplication.update += EditorUpdate;
        }
        private void OnDisable()
        {
            EditorApplication.update -= EditorUpdate;
        }
    }
    #endif
    
}