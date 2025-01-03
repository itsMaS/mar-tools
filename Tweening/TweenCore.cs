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

    public abstract class TweenCore : MonoBehaviour, IGameMechanic
    {
        public UnityEvent OnComplete;
        public UnityEvent OnPlayForwards;
        public UnityEvent OnPlayedForwards;
        public UnityEvent OnPlayBackwards;
        public UnityEvent OnPlayedBackwards;
        public UnityEvent OnStop;
        public UnityEvent<float> OnTick;

        public bool looping = false;
        public bool yoyo = false;
        public bool playOnEnable = false;
        public bool relative = true;
        public bool timeScaled = true;
        public Vector2 delayRange = Vector2.zero;

        public Utilities.Ease ease = Utilities.Ease.InOutQuad;

        public bool differentEaseBackwards = false;
        public Utilities.Ease backwardsEase = Utilities.Ease.InOutQuad;

        public bool local = true;
        public float duration = 1f;
        private Coroutine coroutine;
        private bool forward = false;

        private float lastInterpolator;

        public AnimationCurve curve;
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
                PlayForwards();
            }
        }

        public void PlayForwards()
        {
            if(delayRange.magnitude > 0)
            {
                this.DelayedAction(delayRange.PickRandom(), () => PlayForward(looping), null, timeScaled);
            }
            else
            {
                PlayForward(looping);
            }

        }
        public void PlayBackwards()
        {
            if(delayRange.magnitude > 0)
            {
                this.DelayedAction(delayRange.PickRandom(), () => PlayBackwards(looping), null, timeScaled);
            }
            else
            {
                PlayBackwards(looping);
            }

        }
        private void PlayForward(bool repeat = false)
        {
            float from = 0;
            float to = 1;

            if (relative) from = lastInterpolator;

            OnPlayForwards.Invoke();

            forward = true;
            ResetCoroutine();
            coroutine = this.DelayedAction(duration, () =>
            {
                OnPlayedForwards.Invoke();
                Complete();
                if (yoyo)
                {
                    PlayBackwards();
                }
                else if (looping)
                {
                    PlayForwards();
                }
            }, t =>
            {
                lastInterpolator = Mathf.LerpUnclamped(from, to, t);
                OnTick.Invoke(t);
                SetPose(lastInterpolator);
            }, timeScaled, ease, curve);
        }
        private void PlayBackwards(bool repeat = false)
        {
            float from = 1;
            float to = 0;

            if (relative) from = lastInterpolator;

            OnPlayBackwards.Invoke();

            forward = false;
            ResetCoroutine();
            coroutine = this.DelayedAction(duration, () =>
            {
                OnPlayedBackwards.Invoke();
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
                OnTick.Invoke(t);
                SetPose(lastInterpolator);
            }, timeScaled, differentEaseBackwards ? backwardsEase : ease, curve);
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

        public void Execute()
        {
            PlayForward();
        }
    }
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(TweenCore), true)]
    [CanEditMultipleObjects]
    public class TweenCoreEditor : Editor
    {
        float pose;
        bool playing = false;
    
        public override void OnInspectorGUI()
        {
            var script = (TweenCore)target;
            base.OnInspectorGUI();
            if(!Application.isPlaying)
            {
                float newPose = EditorGUILayout.Slider(pose, 0, 1);
    
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
            else
            {
                if(GUILayout.Button("Play Forwards"))
                {
                    script.PlayForwards();
                }
                if(GUILayout.Button("Play Backwards"))
                {
                    script.PlayBackwards();
                }
                if (GUILayout.Button("Toggle"))
                {
                    script.Toggle();
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
            if(script.ease == Utilities.Ease.Custom)
            {
                script.SetPose(script.curve.Evaluate(t));
            }
            else
            {
                script.SetPose(Utilities.Eases[script.ease].Invoke(t));
            }
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