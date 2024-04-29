using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UIElements;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class TweenCore : MonoBehaviour
{
    public Utilities.Ease ease = Utilities.Ease.InOutQuad;
    public bool local = true;
    public bool relative = true;
    public float duration = 1f;

    protected Vector3 originPosition;
    protected Quaternion originRotation;
    protected Vector3 originScale;

    private Coroutine coroutine;

    private void Awake()
    {
        SetOrigin();
    }

    public void PlayForward()
    {
        ResetCoroutine();
        this.DelayedAction(duration, null, t => SetPose(t), true, ease);
    }
    public void PlayBackwards()
    {
        ResetCoroutine();
        this.DelayedAction(duration, null, t => SetPose(1-t), true, ease);
    }

    private void ResetCoroutine()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
    }

    public abstract void SetPose(float t);
    public virtual void SetOrigin()
    {
        originPosition = transform.localPosition;
        originRotation = transform.localRotation;
        originScale = transform.localScale;
    }
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
                if(newPose != pose)
                {
                    SetPose(script, pose);
                    pose = newPose;
                }
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

            if(!playing && GUILayout.Button("Set Origin"))
            {
                script.SetOrigin();
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
