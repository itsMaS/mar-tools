namespace MarTools
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    
    public class Interactable : MonoBehaviour
    {
        public UnityEvent<InteractionController> OnHover;
        public UnityEvent<InteractionController> OnUnhover;
    
        public UnityEvent<InteractionController> OnInteractStart;
    
        public string _actionText = "Interact";
        
        public string actionText
        {
            get
            {
                return _actionText;
            }
        }
    
        public bool available { get; set; } = true;
        public bool lockOnInteraction = true;
        public float unlocksAfterCooldown = -1;
    
        public Vector3 promptOffset = Vector3.zero;
        public Vector3 promptPosition => transform.position + transform.right * promptOffset.x + transform.up * promptOffset.y + transform.forward * promptOffset.z;
    
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
    
            if(lockOnInteraction)
            {
                available = false;
                if(unlocksAfterCooldown > 0)
                {
                    this.DelayedAction(unlocksAfterCooldown, () =>
                    {
                        available = true;
                    });
                }
            }
        }
    
        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(promptPosition, 0.2f);
        }
    }
    
}