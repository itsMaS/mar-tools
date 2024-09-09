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

        List<Collider> ChildrenColliders = new List<Collider>();

        private void Awake()
        {
            ChildrenColliders = GetComponentsInChildren<Collider>().ToList();
        }
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

            // REWRITE

            Interactable newHovered = null;

            List<Collider> Colliders = Physics.SphereCastAll(raycastPosition.position, raycastWidth, raycastPosition.forward, raycastDistance).ToList().OrderBy(x => Vector3.Distance(x.point, raycastPosition.position)).ToList().ConvertAll(x => x.collider);
            foreach (var item in Physics.OverlapSphere(raycastPosition.position, raycastWidth))
            {
                Colliders.Insert(0,item);
            }

            foreach (var item in Colliders)
            {
                // Skip if collider is inside this gameobject's hierarchy
                if (ChildrenColliders.Contains(item)) continue;

                if(item.TryGetComponent<Interactable>(out var inter))
                {
                    if (!inter.CanBeInteracted(this)) continue;

                    newHovered = inter;
                    break;
                }
                else
                {

                    if (item.isTrigger) continue;
                    else break;
                }
            }
            //

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
    
        public List<Interactable> CastFromEye(float range, float width, LayerMask interactionBlockMask, Func<Interactable, bool> checkFunction = null)
        {
            List<Interactable> Items = new List<Interactable>();
            Debug.DrawLine(raycastPosition.position, raycastPosition.position + raycastPosition.forward * range, Color.yellow);
            foreach (var t in Physics.SphereCastAll(raycastPosition.transform.position, width, raycastPosition.transform.forward, range, interactionBlockMask))
            {
                GameObject target = t.rigidbody ? t.rigidbody.gameObject : t.collider.gameObject;
                if (target.TryGetComponent<Interactable>(out var item) && (checkFunction == null || checkFunction.Invoke(item)))
                {
                    Items.Add(item);
                }
            }
            return Items;
        }
    
        public bool TryCastFirst(out Interactable item, LayerMask interactionBlockMask, float range = 5, float width = 5, Func<Interactable, bool> checkFunction = null)
        {

            item = null;
            var found = CastFromEye(raycastDistance, width, interactionBlockMask, checkFunction);
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
            Gizmos.DrawLine(raycastPosition.position, raycastPosition.position + raycastPosition.forward * raycastDistance);
            Gizmos.DrawWireSphere(raycastPosition.position + raycastPosition.forward * raycastDistance, raycastWidth);
        }
    }
    
}