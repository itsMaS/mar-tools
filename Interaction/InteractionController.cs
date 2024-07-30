namespace MarTools
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.InputSystem;
    
    public class InteractionController : MonoBehaviour
    {
        public UnityEvent<Interactable> OnHover;
        public UnityEvent<Interactable> OnUnhover;
        public UnityEvent<Interactable> OnInteractStart;
        public UnityEvent<Interactable> OnInteracted;
        public UnityEvent<Interactable> OnInteractCancelled;
        public UnityEvent<Interactable, float> OnInteractTick;

    
        [SerializeField] public Transform raycastPosition;
        [SerializeField] float raycastDistance = 10;
        [SerializeField] float raycastWidth = 5;
        [SerializeField] LayerMask interactionBlockMask;
    
        public Interactable hovered { get; private set; }
        public Interactable interactable { get; private set; }
        
        public bool active = true;

        private Transform raycastTransform => raycastPosition ? raycastPosition : transform;


        public void BeginInteract()
        {
            if (hovered)
            {
                interactable = hovered;
                if(hovered.InteractStart(this))
                {
                    OnInteractStart.Invoke(hovered);
                }
            }
        }

        public void StopInteract()
        {
            if(interactable)
            {
                interactable.InteractEnd(this);
                OnInteractTick.Invoke(interactable, interactable.interactionProgressNormalized);
                interactable = null;
            }
        }
    
        private void Update()
        {
            if(interactable && !interactable.CanBeInteracted(this))
            {
                StopInteract();
            }

            if(interactable)
            {
                OnInteractTick.Invoke(interactable, interactable.interactionProgressNormalized);
            }


            if(!active)
            {
                if(hovered)
                {
                    hovered.Unhover(this);
                    OnUnhover.Invoke(hovered);
                    hovered = null;
                }
    
                return;
            }

            //TryCastFirst(out Interactable newHovered, interactionBlockMask, raycastDistance, raycastWidth, item => item.CanBeInteracted(this));

            Interactable newHovered = null;
            foreach (var item in Physics.SphereCastAll(raycastTransform.position, raycastWidth, raycastTransform.forward, raycastDistance).ToList().OrderBy(x => Vector3.Distance(x.point, raycastTransform.position)))
            {
                if(item.collider.TryGetComponent<Interactable>(out var inter))
                {
                    if (!inter.CanBeInteracted(this)) continue;

                    newHovered = inter;
                    break;
                }
                else
                {
                    break;
                }
            }

            if(newHovered != hovered)
            {
                if(hovered)
                {
                    hovered.Unhover(this);
                    OnUnhover.Invoke(hovered);
                }
    
                if(newHovered)
                {
                    newHovered.Hover(this);
                    OnHover.Invoke(newHovered);
                }
                hovered = newHovered;
            }
        }
    
        private void OnDrawGizmos()
        {
            Gizmos.DrawLine(raycastTransform.position, raycastTransform.position + raycastTransform.forward * raycastDistance);
            Gizmos.DrawWireSphere(raycastTransform.position + raycastTransform.forward * raycastDistance, raycastWidth);
        }
    }
    
}