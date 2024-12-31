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
            if (hovered && hovered.CanBeInteracted(this))
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

            Interactable newHovered = null;

            var Hits = Physics.SphereCastAll(raycastPosition.position, raycastWidth, raycastPosition.forward, raycastDistance, interactionBlockMask, QueryTriggerInteraction.Collide);
            //foreach (var item in Physics.OverlapSphere(raycastPosition.position, raycastWidth, interactionBlockMask, QueryTriggerInteraction.UseGlobal))
            //{
            //    Colliders.Insert(0, item);
            //}

            //Debug.Log(string.Join("\n", Hits.Select(hit => hit.collider.name)));


            foreach (var item in Hits)
            {
                // Skip if collider is inside this gameobject's hierarchy
                if (ChildrenColliders.Contains(item.collider)) continue;

                Vector3 point = (item.point == Vector3.zero ? item.collider.ClosestPoint(raycastPosition.position) : item.point);

                //Debug.DrawLine(point, raycastPosition.position, Color.cyan);

                if(item.collider.TryGetComponent<Interactable>(out var inter))
                {
                    Vector3 toInteractable = point  - raycastPosition.position;
                    if (!Physics.Raycast(raycastPosition.position, toInteractable, out RaycastHit hitInfo, toInteractable.magnitude, interactionBlockMask, QueryTriggerInteraction.Ignore))
                    {
                        if (!inter.CanBeInteracted(this)) continue;

                        newHovered = inter;
                        break;
                    }
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