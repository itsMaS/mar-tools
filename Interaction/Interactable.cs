namespace MarTools
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using UnityEngine;
    using UnityEngine.Events;

    [SelectionBase]
    public class Interactable : MonoBehaviour
    {
        public UnityEvent<InteractionController> OnHover;
        public UnityEvent<InteractionController> OnUnhover;
    
        public UnityEvent<InteractionController> OnInteractStart;
        public UnityEvent<InteractionController> OnInteractionCancelled;
        public UnityEvent<float> OnInteractionTick;
        public UnityEvent<InteractionController> OnInteracted;
        public UnityEvent<InteractionController> OnInteractFailed;
    
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

        public string promptText;

        public float interactionProgressNormalized { get; private set; }

        public Vector3 promptOffset = Vector3.zero;
        public Vector3 promptPosition => transform.position + transform.right * promptOffset.x + transform.up * promptOffset.y + transform.forward * promptOffset.z;

        public InteractionController currentInteractor;

        public Func<InteractionController, bool> availabilityFilter { get; set; } = null;
        public Func<InteractionController, bool> isUnlocked { get; set; } = v => true;

        private Coroutine cooldownCoroutine;

        private List<InteractionController> CurrentControllers = new List<InteractionController>();

        public void Hover(InteractionController controller)
        {
            CurrentControllers.Add(controller);

            if(CurrentControllers.Count == 1)
            {
                OnHover.Invoke(controller);
            }
        }
    
        public void Unhover(InteractionController controller)
        {
            CurrentControllers.Remove(controller);

            if(CurrentControllers.Count == 0)
            {
                OnUnhover.Invoke(controller);
            }
        }
    
        public bool InteractStart(InteractionController controller)
        {
            if(IsUnlocked(controller))
            {
                OnInteractStart.Invoke(controller);
                currentInteractor = controller;

                if (interactionDuration <= 0) Interacted();
                return true;
            }
            else
            {
                OnInteractFailed.Invoke(controller);
                return false;
            }
        }

        public bool IsUnlocked(InteractionController controller)
        {
            return isUnlocked.Invoke(controller);
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

            if(!enabled)
            {
                if(cooldownCoroutine != null)
                {
                    StopCoroutine(cooldownCoroutine);
                    cooldownCoroutine = null;
                }
                unlocksAfterCooldown = -1;
            }
        }

        public void ToggleAvailability()
        {
            if(available)
            {
                SetAvailability(false);
            }
            else
            {
                SetAvailability(true);
            }
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
                    interactionProgressNormalized = 0;
                }

                if(interactionProgressNormalized >= 1 && interactionDuration > 0)
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
            if(lockOnInteraction)
            {
                available = false;

                if(unlocksAfterCooldown > 0)
                {
                    cooldownCoroutine = this.DelayedAction(unlocksAfterCooldown, () =>
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
            OnInteracted.Invoke(currentInteractor);
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(promptPosition, 0.2f);
        }
    }
    
}