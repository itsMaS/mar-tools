#if UNITY_CINEMACHINE
using UnityEngine;
using System.Net;
using Unity.Cinemachine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MarTools
{
    [ExecuteAlways]
    public class DollyCartLineBehavior : MonoBehaviour
    {
        public Vector3 positionOffset;
        public Vector3 rotationOffset;

        public Vector3 samplingOffset;
        public Vector3 offsetMultiplier;

        public float positionDamping;
        public float rotationDamping;

        public LineBehavior2 trackingLine;
        public LineBehavior2 cameraLine;


        public Transform target;

        [HideInInspector] public bool preview = false;

        Vector3 positionVelocity;
        Quaternion rotationVelocity;


        public void Update()
        {
            if (!target || !cameraLine || !trackingLine) return;

            if (!preview || Application.isPlaying)
            {
                var t = trackingLine.GetClosestPoint(target.position + samplingOffset, out float progress);

                Vector3 offset = Vector3.Scale(((target.position + samplingOffset) - t.position), offsetMultiplier);

                SetPosition(progress, offset);
            }
        }

        public void SetPosition(float progress, Vector3 offset)
        {
            var p = cameraLine.GetPoint(progress);

            Vector3 targetPosition = p.position + positionOffset + offset;
            Quaternion targetRotation = p.rotation * Quaternion.Euler(rotationOffset);

            if (Application.isPlaying)
            {
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref positionVelocity, positionDamping);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 1/(rotationDamping+0.01f));
            }
            else
            {
                transform.position = targetPosition;
                transform.rotation = targetRotation;
            }


            Debug.DrawLine(target.position + samplingOffset, p.position, Color.cyan);
            Debug.DrawLine(p.position, p.position, Color.yellow);
        }

        private void OnDrawGizmos()
        {
            if (cameraLine && target)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(target.position + samplingOffset, 0.1f);

                //GizmosUtilities.DrawRotation(transform.position, transform.rotation, 3);

                int res = Mathf.RoundToInt(cameraLine.totalDistance / 10);
                for (int i = 0; i < res; i++)
                {
                    float t = (float)i / res;

                    Debug.DrawLine(cameraLine.GetPoint(t).position, trackingLine.GetPoint(t).position, Color.yellow*0.5f);
                }
            }
        }

        public void SetLine(LineBehavior2 lineBehavior)
        {
            trackingLine = lineBehavior;
            cameraLine = lineBehavior;
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(DollyCartLineBehavior))]
    public class DollyCartLineBehaviorEditor : UnityEditor.Editor
    {
        DollyCartLineBehavior dollyCart;

        float previewValue = 0;

        private void OnEnable()
        {
            dollyCart = (DollyCartLineBehavior)target;
        }

        private void OnDisable()
        {
            dollyCart.preview = false;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if(GUILayout.Button(dollyCart.preview ? "Stop previewing" : "Star preview"))
            {
                dollyCart.preview = !dollyCart.preview;

                if(dollyCart.preview)
                {
                    if(dollyCart.TryGetComponent<CinemachineCamera>(out var cam))
                    {
                        cam.Prioritize();
                    }
                }

                if (!dollyCart.preview) dollyCart.Update();
            }


            if(dollyCart.preview)
            {
                float newValue = EditorGUILayout.Slider(previewValue, 0, 1);
                if(newValue != previewValue)
                {
                    previewValue = newValue;
                }
                dollyCart.SetPosition(newValue, Vector3.zero);
            }
        }

        private void OnSceneGUI()
        {
            if (!dollyCart.target || !dollyCart.cameraLine || !dollyCart.trackingLine) return;

            var p = dollyCart.trackingLine.GetClosestPoint(dollyCart.target.position, out float progress);
            Handles.Label(p.position + Vector3.up*0.25f, progress.ToString());

            dollyCart.target.position = Handles.PositionHandle(dollyCart.target.position, Quaternion.identity);
        }
    }
#endif
}
#endif