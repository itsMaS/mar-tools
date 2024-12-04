namespace MarTools
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
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

        public List<UIElement> EnabledUIElements = new List<UIElement>();

        private Coroutine navigationComboCoroutine;

        public bool isPointerOverUI { get; private set; } = false;
        private void Awake()
        {
            eventSystem = GetComponent<EventSystem>();
            graphicRaycaster = GetComponent<GraphicRaycaster>();

            canvas = GetComponent<Canvas>();
            pointerData = new PointerEventData(eventSystem);

            if (!cam) cam = GetComponentInParent<Camera>();
            if (!cam) cam = FindObjectOfType<Camera>();

        }

        private void Start()
        {
            SubscribeInput(submitAction, Submit);
            SubscribeInput(navigationVector, Navigate);

            SubscribeInput(nextTabAction, NextTab);
            SubscribeInput(previousTabAction, PreviousTab);
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
            playerIndex = index;
            EnableInput();
        }

        private void OnEnable()
        {
            EnableInput();
        }
        private void OnDisable()
        {
            DisableInput();
        }

        private void PreviousTab(InputAction.CallbackContext obj)
        {
            if (obj.phase != InputActionPhase.Performed) return;

            GetComponentsInChildren<TabGroup>().ToList().ForEach(x =>
            {
                if (x.gameObject.activeInHierarchy) x.PreviousTab();
            });
        }
        private void NextTab(InputAction.CallbackContext obj)
        {
            if (obj.phase != InputActionPhase.Performed) return;

            GetComponentsInChildren<TabGroup>().ToList().ForEach(x =>
            {
                if (x.gameObject.activeInHierarchy) x.NextTab();
            });
        }
        private void Navigate(InputAction.CallbackContext obj)
        {
            SetNavigationType(NavigationType.Axis);

            if (navigationComboCoroutine != null)
            {
                StopCoroutine(navigationComboCoroutine);
                navigationComboCoroutine = null;
            }

            if (obj.phase == InputActionPhase.Performed)
            {
                navigationComboCoroutine = StartCoroutine(NavigationCombo(obj.action));
            }
        }

        private void Navigate(Vector2 direction)
        {
            if (lastNotNull != null && !lastNotNull.gameObject.activeInHierarchy) lastNotNull = null;
            if (selected == null && lastNotNull != null)
            {
                SelectButton(lastNotNull);
            }
            if (selected == null) return;
            selected.Navigate(direction);
        }

        IEnumerator NavigationCombo(InputAction action)
        {
            Navigate(action.ReadValue<Vector2>());

            float timeStep = 0.6f;
            float elapsed = 0;

            while(true)
            {
                if (elapsed > timeStep) 
                {
                    elapsed = 0;
                    Navigate(action.ReadValue<Vector2>());
                    timeStep = Mathf.Max(timeStep * 0.5f, 0.1f);
                }
                else
                {
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }
            }
        }

        public UIElement GetAdjacent(UIElement origin, Vector2 direction)
        {
            // IMPROVE THIS FUNCTION IT'S STILL PRETTY BAD


            Vector3 realDirection = transform.right * direction.x + transform.up * direction.y;
            Debug.DrawLine(origin.transform.position, origin.transform.position + realDirection * 1000, Color.cyan, 1f);

            var AllPotentialTargets = GetComponentsInChildren<UIElement>().ToList().Where(x => !x.mouseOnly && origin != x);

            float angleCheck = 50f;

            AllPotentialTargets = AllPotentialTargets.Where(x =>
            {
                Vector3 toTarget = x.transform.position - origin.transform.position;
                float angle = Vector3.Angle(toTarget, realDirection);
                float availableAngle = 80;
                if (angle > angleCheck)
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

                    float costToReach = distanceNormalized + (angle/angleCheck)*0.2f;
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

        private void Submit(InputAction.CallbackContext obj)
        {
            if(obj.phase == InputActionPhase.Started)
            {
                SubmitStart(obj);
            }
            else if(obj.phase == InputActionPhase.Canceled)
            {
                SubmitEnd(obj);
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
    
                    isPointerOverUI = results.Count > 0;

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
                else
                {
                    isPointerOverUI = false;
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

        internal void Subscribe(UIElement uIElement)
        {
            EnabledUIElements.Add(uIElement);
        }

        internal void Unsubscribe(UIElement uIElement)
        {
            EnabledUIElements.Remove(uIElement);
        }

        private Dictionary<string, UnityEvent<InputAction.CallbackContext>> EventDictionary = new Dictionary<string, UnityEvent<InputAction.CallbackContext>>();
        Action inputCleanupAction = null;
        private void EnableInput()
        {
            DisableInput();

            PlayerInput player = PlayerInput.GetPlayerByIndex(playerIndex);
            // If player exists get asset held by the player, if not, use the action asset 
            InputActionAsset asset = player ? player.actions : submitAction.asset;

            var actionMap = asset.FindActionMap("UI");
            actionMap.actionTriggered += ActionTriggered;
            actionMap.Enable();

            inputCleanupAction = () => actionMap.actionTriggered -= ActionTriggered;

            foreach (var item in actionMap.actions)
            {
                if(EventDictionary.TryAdd(item.name, new UnityEvent<InputAction.CallbackContext>()))
                {

                }
            }
        }
        private void DisableInput()
        {
            inputCleanupAction?.Invoke();
        }

        private void ActionTriggered(InputAction.CallbackContext obj)
        {
            EventDictionary[obj.action.name].Invoke(obj);
        }

        public void SubscribeInput<T>(InputActionReference r, UnityAction<T> action, InputActionPhase phase = InputActionPhase.Performed) where T : struct
        {
            if(EventDictionary.TryGetValue(r.action.name, out var context))
            {
                context.AddListener(x =>
                {
                    if(x.phase == phase)
                    {
                        action.Invoke(x.ReadValue<T>());
                    }
                });
            }
            else
            {
                Debug.LogError($"The UI action map does not contain {r.name} from {r.action.actionMap}");
            }
        }

        public void SubscribeInput(InputActionReference r, UnityAction<InputAction.CallbackContext> action)
        {
            if (r == null) return;

            if (EventDictionary.TryGetValue(r.action.name, out var context))
            {
                context.AddListener(action);
            }
            else
            {
                string all = "";
                EventDictionary.Keys.ToList().ForEach(x => all += $"{x}\n");

                Debug.LogError($"The UI action map does not contain {r.name} [{r.action.name}] from {r.action.actionMap} It contains these actions:\n");

            }
        }
    }
    
}