namespace MarTools
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using UnityEngine;
    using UnityEngine.Events;
    
    public class Interactable : MonoBehaviour
    {
        public UnityEvent<InteractionController> OnHover;
        public UnityEvent<InteractionController> OnUnhover;
    
        public UnityEvent<InteractionController> OnInteractStart;
        public UnityEvent<InteractionController> OnInteractionCancelled;
        public UnityEvent<float> OnInteractionTick;
        public UnityEvent<InteractionController> OnInteracted;
    
        public string _actionText = "Interact";
        
        public string actionText
        {
            get
            {
                return _actionText;
            }
        }

        public bool CanBeInteracted(InteractionController controller)
        {
            return available && (availabilityFilter == null || availabilityFilter.Invoke(controller));
        }

        public bool available = true;
        public bool lockOnInteraction = true;
        public float unlocksAfterCooldown = -1;
        public float interactionDuration = 0;
        public bool resetIneractionTimeOnCancel = false;
        public bool resetInteractionTimeOnInteract = true;
        public float interactionTImeDecayPerSecond = 0;
    
        public float interactionProgressNormalized { get; private set; }

        public Vector3 promptOffset = Vector3.zero;
        public Vector3 promptPosition => transform.position + transform.right * promptOffset.x + transform.up * promptOffset.y + transform.forward * promptOffset.z;

        public InteractionController currentInteractor;

        public Func<InteractionController, bool> availabilityFilter { get; set; } = null;

        public void Hover(InteractionController controller)
        {
            OnHover.Invoke(controller);
        }
    
        public void Unhover(InteractionController controller)
        {
            OnUnhover.Invoke(controller);
        }
    
        public void InteractStart(InteractionController controller)
        {
            OnInteractStart.Invoke(controller);
            currentInteractor = controller;
        }

        public void InteractEnd(InteractionController controller)
        {
            if (resetIneractionTimeOnCancel)
            {
                interactionProgressNormalized = 0;
            }

            OnInteractionCancelled.Invoke(controller);
            currentInteractor = null;
        }

        public void SetAvailability(bool enabled)
        {
            available = enabled;
        }

        private void Update()
        {
            if(currentInteractor)
            {
                if (interactionDuration > 0)
                {
                    interactionProgressNormalized += Time.deltaTime / interactionDuration;
                }
                else
                {
                    interactionProgressNormalized = 1;
                }


                if(interactionProgressNormalized >= 1)
                {
                    Interacted();
                }
            }
            else
            {
                interactionProgressNormalized -= Time.deltaTime * interactionTImeDecayPerSecond;
                interactionProgressNormalized = Mathf.Clamp01(interactionProgressNormalized);
            }
            OnInteractionTick.Invoke(interactionProgressNormalized);
        }

        private void Interacted()
        {
            OnInteracted.Invoke(currentInteractor);
            
            if(lockOnInteraction)
            {
                available = false;

                if(unlocksAfterCooldown > 0)
                {
                    this.DelayedAction(unlocksAfterCooldown, () =>
                    {
                        SetAvailability(true);
                    });
                }
            }

            if(resetInteractionTimeOnInteract)
            {
                interactionProgressNormalized = 0;
            }

            interactionProgressNormalized = Mathf.Clamp01(interactionProgressNormalized);
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(promptPosition, 0.2f);
        }
    }
    
}