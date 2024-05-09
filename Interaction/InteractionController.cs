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

    
        [SerializeField] Transform raycastPosition;
        [SerializeField] float raycastDistance = 10;
        [SerializeField] float raycastWidth = 5;
    
        public Interactable hovered { get; private set; }
        public Interactable interactable { get; private set; }
        
        public bool active = true;
    


        public void BeginInteract()
        {
            if (hovered)
            {
                interactable = hovered;
                hovered.InteractStart(this);
            }
            OnInteractStart.Invoke(hovered);
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
            if(interactable && !interactable.available)
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
    
            TryCastFirst(out Interactable newHovered, raycastDistance, raycastWidth, item => item.available);
    
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
    
        public List<Interactable> CastFromEye(float range, float width, Func<Interactable, bool> checkFunction = null)
        {
            List<Interactable> Items = new List<Interactable>();
            Debug.DrawLine(raycastPosition.position, raycastPosition.position + raycastPosition.forward * range, Color.yellow);
            foreach (var t in Physics.SphereCastAll(raycastPosition.transform.position, width, raycastPosition.transform.forward, range))
            {
                GameObject target = t.rigidbody ? t.rigidbody.gameObject : t.collider.gameObject;
                if (target.TryGetComponent<Interactable>(out var item) && (checkFunction == null || checkFunction.Invoke(item)))
                {
                    Items.Add(item);
                }
            }
            return Items;
        }
    
        public bool TryCastFirst(out Interactable item, float range = 5, float width = 5, Func<Interactable, bool> checkFunction = null)
        {
            item = null;
            var found = CastFromEye(raycastDistance, width, checkFunction);
            float minDistance = float.MaxValue;
            
            if (found.Count > 0)
            {
                foreach (var f in found)
                {
                    if (f == interactable)
                    {
                        item = f;
                        return true;
                    }

                    float distance = Vector3.Distance(f.transform.position, raycastPosition.position + raycastPosition.forward * range);
                    if(distance < minDistance)
                    {
                        minDistance = distance;
                        item = f;
                    }
                }
                return true;
            }
            return false;
        }
    
        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(raycastPosition.position + raycastPosition.forward * raycastDistance, raycastWidth);
        }
    }
    
}