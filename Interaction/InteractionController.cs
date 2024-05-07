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
        public UnityEvent<Interactable> OnInteract;
        public InputActionReference interactInput;
    
        [SerializeField] Transform raycastPosition;
        [SerializeField] float raycastDistance = 10;
        [SerializeField] float raycastWidth = 5;
    
        public Interactable hovered { get; private set; }
        public bool active = true;
    
        private void Awake()
        {
            if(interactInput != null)
                interactInput.action.performed += Interact;
        }
    
        private void Interact(InputAction.CallbackContext obj)
        {
            Interact();
        }
    
        public void Interact()
        {
            if (hovered) hovered.InteractStart(this);
            OnInteract.Invoke(hovered);
        }
    
        private void Update()
        {
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
    
            TryCastFirst<Interactable>(out Interactable newHovered, raycastDistance, raycastWidth, item => item.available);
    
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
    
        public List<T> CastFromEye<T>(float range, float width, Func<T, bool> checkFunction = null) where T : Component
        {
            List<T> Items = new List<T>();
            Debug.DrawLine(raycastPosition.position, raycastPosition.position + raycastPosition.forward * range, Color.yellow);
            foreach (var t in Physics.SphereCastAll(raycastPosition.transform.position, width, raycastPosition.transform.forward, range))
            {
                GameObject target = t.rigidbody ? t.rigidbody.gameObject : t.collider.gameObject;
                if (target.TryGetComponent<T>(out var item) && (checkFunction == null || checkFunction.Invoke(item)))
                {
                    Items.Add(item);
                }
            }
            return Items;
        }
    
        public bool TryCastFirst<T>(out T item, float range = 5, float width = 5, Func<T, bool> checkFunction = null) where T : Component
        {
            item = null;
            var found = CastFromEye<T>(raycastDistance, width, checkFunction);
            float minDistance = float.MaxValue;
            if (found.Count > 0)
            {
                foreach (var f in found)
                {
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