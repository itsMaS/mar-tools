namespace MarTools
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class MatchCameraViewport : MonoBehaviour
    {
        [SerializeField] public Camera cam;
        RectTransform rt;

        void Update()
        {
            if (!cam) return;
            if (!rt) rt = GetComponent<RectTransform>();

            rt.anchorMin = new Vector2(cam.rect.x, cam.rect.y);
            rt.anchorMax = new Vector2(cam.rect.x + cam.rect.width, cam.rect.y + cam.rect.height);
        }
    }
}
