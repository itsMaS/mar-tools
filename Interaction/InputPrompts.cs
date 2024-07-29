namespace MarTools
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;

    [DefaultExecutionOrder(999)]
    public class InputPrompts : MonoBehaviour
    {
        private InteractionController _controller;
        public InteractionController controller
        {
            get
            {
                return _controller;
            }
            set
            {
                if (value == _controller) return;
    
                if(_controller != null)
                {
                    _controller.OnHover.RemoveListener(Hover);
                    _controller.OnUnhover.RemoveListener(Unhover);
                }
    
                _controller = value;
                if(value != null)
                {
                    _controller.OnHover.AddListener(Hover);
                    _controller.OnUnhover.AddListener(Unhover);
                }
            }
        }



        public Camera cam;
    
        public Interactable hoveredInteractable { get; private set; }
        [SerializeField] RectTransform promptTr;
        [SerializeField] Image interactionFillImage;
        [SerializeField] Image lockImage;
    
        public UnityEvent<Interactable> OnHover;
        public UnityEvent<Interactable> OnUnhover;
        public UnityEvent<Interactable> OnInteract;
    
        private void Awake()
        {
            controller = GetComponentInParent<InteractionController>();
        }
    
        private void Start()
        {
            if (!cam) cam = Camera.main;
        }
    
        private void Unhover(Interactable arg0)
        {
            OnUnhover.Invoke(arg0);
        }
    
        private void Hover(Interactable arg0)
        {
            hoveredInteractable = arg0;
            if(arg0 != null)
            {
                UpdatePointPosition();
                OnHover.Invoke(arg0);
            }
        }
    
        private void LateUpdate()
        {
            if(hoveredInteractable)
            {
                UpdatePointPosition();
                interactionFillImage.fillAmount = hoveredInteractable.interactionProgressNormalized;
                lockImage.gameObject.SetActive(!hoveredInteractable.IsUnlocked(controller));
            }
        }
    
        private void UpdatePointPosition()
        {
            promptTr.position = cam.WorldToScreenPoint(hoveredInteractable.promptPosition);
        }
    }
    
}