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
    
        Button selected;
        public Button lastNotNull { get; private set; }
        List<Button> MouseHoveredButtons = new List<Button>();
    
        //public string lastMap 
        PointerEventData pointerData;
        Canvas canvas;
        Vector2 pointerPosition;

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
    
            if(aim.magnitude > 0.01f && selected)
            {
                var other = GetComponentsInChildren<Button>().ToList().Where(item => item != selected && item.navigational).ToList();

                Vector3 aimDirection = aim.x * transform.right + aim.y * transform.up;

                Debug.DrawLine(selected.transform.position, selected.transform.position + aimDirection * 1000, Color.green, 1f);

                foreach (var x in other)
                {
                    Vector3 direction = x.transform.position - selected.transform.position;
                    Debug.DrawLine(selected.transform.position, selected.transform.position + direction, Color.yellow, 0.5f);
                }

                var directional = other.FindAll(x =>
                {
                    Vector3 direction = x.transform.position - selected.transform.position;
                    Debug.DrawLine(selected.transform.position, selected.transform.position + direction, Color.yellow, 0.5f);

                    return Vector3.Angle(direction, aimDirection) < 45f;
                });

                if (directional.Count > 0)
                {
                    var target = directional.FindClosest(selected.transform.position, x => x.transform.position, out float closestDistance);
                    SelectButton(target);
                }
            }
        }
        private void SubmitStart(InputAction.CallbackContext obj)
        {
            if(obj.control.device != Mouse.current)
            {
                SetNavigationType(NavigationType.Axis);
            }

            holdingSubmit = true;

            if (selected != null)
            {
                selected.Click();
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
                        Button button = result.gameObject.GetComponent<Button>();
                        if (button != null)
                        {
                            MouseHoveredButtons.Add(button);
                        }
                    }

                    if(!holdingSubmit)
                        SelectButton(MouseHoveredButtons.FirstOrDefault());
                }
            }

            if(selected)
            {
                if(selected.TryGetComponent<Image>(out var img))
                {
                    Vector2 screenPosition = GetScreenPosition(img.transform.position);
                    Vector2 diff = pointerPosition - screenPosition;

                    diff.x = Mathf.Clamp(diff.x, -img.rectTransform.sizeDelta.x/2, img.rectTransform.sizeDelta.x/2);
                    diff.y = Mathf.Clamp(diff.y, -img.rectTransform.sizeDelta.y/2, img.rectTransform.sizeDelta.y/2);


                    Vector2 diffNormalized = diff;
                    diffNormalized.x /= img.rectTransform.sizeDelta.x;
                    diffNormalized.y /= img.rectTransform.sizeDelta.y;
                    diffNormalized += Vector2.one * 0.5f;

                    selected.SetCursorPosition(diffNormalized, diff);
                }
            }

            if(currentNavigationType == NavigationType.Axis && !selected)
            {
                SelectButton(GetComponentsInChildren<Button>().FirstOrDefault(x => x.navigational));
            }
        }
    
        public void SelectButton(Button button)
        {
            if (selected == button) return;

            DeselectButton(selected);
    
            selected = button;
            
            if(button != null)
            {
                if(button.navigational)
                {
                    lastNotNull = button;
                }
                OnSelect.Invoke();
                selected.Select();
            }
        }
    
        public void DeselectButton(Button button)
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
                    SelectButton(GetComponentsInChildren<Button>(false).FirstOrDefault(x => x.navigational));
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