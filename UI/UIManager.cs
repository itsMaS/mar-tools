namespace MarTools
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Unity.VisualScripting;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem;
    using UnityEngine.UI;
    
    [RequireComponent(typeof(Canvas), typeof(GraphicRaycaster))]
    public class UIManager : MonoBehaviour
    {
        public enum NavigationType
        {
            Axis,
            Pointer
        }

        public int playerIndex { get; private set; } = -1;

        public NavigationType currentNavigationType;
    
        public InputActionReference submitAction;
        public InputActionReference navigationVector;
        public InputActionReference cancelAction;
        public InputActionReference nextTabAction;
        public InputActionReference previousTabAction;
    
        private GraphicRaycaster graphicRaycaster;
        private EventSystem eventSystem;

        public UIElement selected;
        public UIElement lastNotNull { get; private set; }
        List<UIElement> MouseHoveredButtons = new List<UIElement>();
    
        //public string lastMap 
        PointerEventData pointerData;
        public Canvas canvas { get; private set; }

        public Vector2 pointerPosition { get; private set; }
        public bool holdingSubmit { get; private set; } = false;
        public Camera cam;


        public UnityEvent OnSelect;
        public UnityEvent OnSubmit;


        private void Awake()
        {
            eventSystem = GetComponent<EventSystem>();
            graphicRaycaster = GetComponent<GraphicRaycaster>();

            canvas = GetComponent<Canvas>();
            pointerData = new PointerEventData(eventSystem);

            if (!cam) cam = GetComponentInParent<Camera>();
            if (!cam) cam = FindObjectOfType<Camera>();
        }

        public InputAction ResolveAction(InputActionReference reference)
        {
            PlayerInput input = PlayerInput.GetPlayerByIndex(playerIndex);
            if (playerIndex >= 0)
            {
                if(!input)
                {
                    Debug.LogError($"Player with the index of {playerIndex} does not exist");
                    return null;
                }
                else
                {
                    if (!input.actions) return null;
                    return input.actions[reference.name];
                }
            }
            else
            {
                return reference.action;
            }
        }
        public void SetPlayerIndex(int index)
        {
            UnsusbscribeInput();
            playerIndex = index;
            SubscribeToInput();
        }
        private void SubscribeToInput()
        {
            PlayerInput input = PlayerInput.GetPlayerByIndex(playerIndex);
            if (playerIndex >= 0 && (!input || !input.actions)) return;

            ResolveAction(submitAction).performed += SubmitStart;
            ResolveAction(submitAction).canceled += SubmitEnd;
            ResolveAction(navigationVector).performed += Navigate;
            ResolveAction(nextTabAction).performed += NextTab;
            ResolveAction(previousTabAction).performed += PreviousTab;


            ResolveAction(nextTabAction).Enable();
            ResolveAction(previousTabAction).Enable();
            ResolveAction(submitAction).Enable();
            ResolveAction(navigationVector).Enable();
        }

        private void UnsusbscribeInput()
        {
            PlayerInput input = PlayerInput.GetPlayerByIndex(playerIndex);
            if (playerIndex >= 0 && (!input || !input.actions)) return;

            ResolveAction(submitAction).performed -= SubmitStart;
            ResolveAction(submitAction).canceled -= SubmitEnd;
            ResolveAction(navigationVector).performed -= Navigate;
            ResolveAction(nextTabAction).performed -= NextTab;
            ResolveAction(previousTabAction).performed -= PreviousTab;
        }
        private void OnEnable()
        {
            SubscribeToInput();

            if(!cam)
            {
                cam = GetComponent<Camera>();
            }
        }
        private void OnDisable()
        {
            UnsusbscribeInput();
        }

        private void PreviousTab(InputAction.CallbackContext obj)
        {
            GetComponentsInChildren<TabGroup>().ToList().ForEach(x =>
            {
                if (x.gameObject.activeInHierarchy) x.PreviousTab();
            });
        }
        private void NextTab(InputAction.CallbackContext obj)
        {
            GetComponentsInChildren<TabGroup>().ToList().ForEach(x =>
            {
                if (x.gameObject.activeInHierarchy) x.NextTab();
            });
        }
        private void Navigate(InputAction.CallbackContext obj)
        {
            SetNavigationType(NavigationType.Axis);
    
            if(selected == null)
            {
                SelectButton(lastNotNull);
            }

    
            Vector2 aim = obj.ReadValue<Vector2>();
            selected.Navigate(aim);

        }

        public UIElement GetAdjacent(UIElement origin, Vector2 direction)
        {
            Vector3 realDirection = transform.right * direction.x + transform.up * direction.y;
            Debug.DrawLine(origin.transform.position, origin.transform.position + realDirection * 1000, Color.cyan, 1f);

            var AllPotentialTargets = GetComponentsInChildren<UIElement>().ToList().Where(x => !x.mouseOnly && origin != x);

            AllPotentialTargets = AllPotentialTargets.Where(x =>
            {
                Vector3 toTarget = x.transform.position - origin.transform.position;
                float angle = Vector3.Angle(toTarget, realDirection);

                float availableAngle = 80;
                if (angle > availableAngle)
                {
                    Debug.DrawLine(origin.transform.position, x.transform.position, Color.red, 1f);
                }
                else
                {
                    Debug.DrawLine(origin.transform.position, x.transform.position, Color.yellow, 1f);
                }

                return angle <= availableAngle;
            });

            if(AllPotentialTargets.Count() > 0)
            {
                float maxDistance = AllPotentialTargets.Max(x => Vector3.Distance(x.transform.position, origin.transform.position));
                AllPotentialTargets = AllPotentialTargets.OrderBy(x =>
                {
                    float distance = Vector3.Distance(x.transform.position, origin.transform.position);
                    float distanceNormalized = distance / maxDistance;
                    float angle = Vector3.Angle(x.transform.position - origin.transform.position, realDirection);

                    float costToReach = distanceNormalized;

                    return costToReach;
                });

                var target = AllPotentialTargets.First();
                Debug.DrawLine(origin.transform.position, target.transform.position, Color.green, 1f);
                return target;
            }

            return null;
        }

        public void MoveSelection(Vector2 direction)
        {
            var target = GetAdjacent(selected, direction);

            if(target != null)
            {
                SelectButton(target);
            }
        }

        private void SubmitStart(InputAction.CallbackContext obj)
        {
            if(obj.control.device != Mouse.current)
            {
                SetNavigationType(NavigationType.Axis);
            }

            holdingSubmit = true;

            if (selected != null && selected.gameObject.activeInHierarchy)
            {
                if (obj.control.device == Mouse.current && MouseHoveredButtons.Count == 0) return;

                selected.Submit();
                OnSubmit.Invoke();
            }
        }

        private void SubmitEnd(InputAction.CallbackContext obj)
        {
            holdingSubmit = false;
        }

        void Update()
        {
            if(selected && !selected.gameObject.activeInHierarchy)
            {
                DeselectButton(selected);
            }

            bool canMouseBeUsed = true;

            PlayerInput input = PlayerInput.GetPlayerByIndex(playerIndex);
            if(input)
            {
                if (input.currentControlScheme != "Keyboard&Mouse") canMouseBeUsed = false;
            }

            MouseHoveredButtons.Clear();
    
            if(canMouseBeUsed)
            {
                pointerPosition = Input.mousePosition;
                pointerData.delta = pointerData.position - pointerPosition;
                pointerData.position = pointerPosition;
    
                if(pointerData.delta != Vector2.zero)
                {
                    SetNavigationType(NavigationType.Pointer);
                }
    
    
                if(currentNavigationType == NavigationType.Pointer)
                {
                    List<RaycastResult> results = new List<RaycastResult>();
                    graphicRaycaster.Raycast(pointerData, results);
    
                    foreach (RaycastResult result in results)
                    {
                        UIElement button = result.gameObject.GetComponent<UIElement>();
                        if (button != null)
                        {
                            MouseHoveredButtons.Add(button);
                        }
                    }

                    if(!holdingSubmit)
                        SelectButton(MouseHoveredButtons.FirstOrDefault());
                }
            }

            if(currentNavigationType == NavigationType.Axis && !selected)
            {
                SelectButton(GetComponentsInChildren<UIElement>().FirstOrDefault(x => !x.mouseOnly));
            }
        }
    
        public void SelectButton(UIElement button)
        {
            if (selected == button) return;

            DeselectButton(selected);
    
            selected = button;
            
            if(button != null)
            {
                if(!button.mouseOnly)
                {
                    lastNotNull = button;
                }
                OnSelect.Invoke();
                selected.Select();
            }
        }
    
        public void DeselectButton(UIElement button)
        {
            if(button != null)
            {
                button.Deselect();
                selected = null;
            }
        }
    
        private void SetNavigationType(NavigationType type)
        {
            if (currentNavigationType != type)
            {
                currentNavigationType = type;

                if(type == NavigationType.Axis && selected == null)
                {
                    SelectButton(GetComponentsInChildren<UIElement>(false).FirstOrDefault(x => !x.mouseOnly));
                }
            }
        }

        public Vector2 GetScreenPosition(Vector3 worldPosition)
        {
            if(canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace)
            {
                return cam.WorldToScreenPoint(worldPosition);
            }
            else
            {
                return RectTransformUtility.WorldToScreenPoint(cam, worldPosition);
            }
        }
    }
    
}