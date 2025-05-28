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

    [DefaultExecutionOrder(-200)]
    [RequireComponent(typeof(Canvas), typeof(GraphicRaycaster))]
    public class UIManager : MonoBehaviour
    {
        public enum NavigationType
        {
            Axis,
            Pointer
        }

        public NavigationType currentNavigationType;

        public InputActionReference submitAction;
        public InputActionReference navigationVector;
        public InputActionReference cancelAction;
        public InputActionReference nextTabAction;
        public InputActionReference previousTabAction;
        public InputActionReference extraLeftAction; // Left trigger
        public InputActionReference extraRightAction; // Right trigger
        public InputActionReference extraButtonAction; // Left facing key
        public InputActionReference pauseAction;

        private GraphicRaycaster graphicRaycaster;
        private EventSystem eventSystem;

        public UIElement selected;
        public UIElement lastNotNull { get; private set; }
        List<UIElement> MouseHoveredButtons = new List<UIElement>();

        public PlayerInput playerInput;

        //public string lastMap 
        PointerEventData pointerData;
        public Canvas canvas { get; private set; }

        public Vector2 pointerPosition { get; private set; }
        public bool holdingSubmit { get; private set; } = false;
        public Camera cam;

        public UnityEvent OnSelect;
        public UnityEvent OnSelectLocked;
        public UnityEvent OnSubmit;
        public UnityEvent OnSubmitLocked;

        public List<UIElement> EnabledUIElements = new List<UIElement>();

        public bool blockNavigation { get; private set; } = false;
        private Coroutine navigationComboCoroutine;

        public bool isPointerOverUI { get; private set; } = false;

        [HideInInspector] public System.Action<InputAction.CallbackContext> OnSubmitHandover;
        [HideInInspector] public System.Action<InputAction.CallbackContext> OnBackHandover;
        [HideInInspector] public System.Action<InputAction.CallbackContext> OnNavigateHandover;
        [HideInInspector] public System.Action<InputAction.CallbackContext> OnNextTabHandover;
        [HideInInspector] public System.Action<InputAction.CallbackContext> OnPreviousTabHandover;
        [HideInInspector] public System.Action<InputAction.CallbackContext> OnExtraButtonHandover;
        [HideInInspector] public System.Action<InputAction.CallbackContext> OnExtraLeftHandover;
        [HideInInspector] public System.Action<InputAction.CallbackContext> OnExtraRightHandover;
        [HideInInspector] public System.Action<InputAction.CallbackContext> OnPauseHandover;
        private bool _handover = false;
        
        private void Awake()
        {
            blockNavigation = false;
            eventSystem = GetComponent<EventSystem>();
            graphicRaycaster = GetComponent<GraphicRaycaster>();

            canvas = GetComponent<Canvas>();
            pointerData = new PointerEventData(eventSystem);

            if (!cam) cam = GetComponentInParent<Camera>();
            if (!cam) cam = FindObjectOfType<Camera>();
        }

        private void SubscribeDefaultInputs()
        {
            SubscribeInput(submitAction, Submit);
            SubscribeInput(navigationVector, Navigate);

            SubscribeInput(nextTabAction, NextTab);
            SubscribeInput(previousTabAction, PreviousTab);

            SubscribeInput(cancelAction, Cancel);
            SubscribeInput(extraLeftAction, ExtraLeft);
            SubscribeInput(extraRightAction, ExtraRight);
            SubscribeInput(extraButtonAction, ExtraButton);

            SubscribeInput(pauseAction, Pause);
        }

        private void UnsubscribeDefaultInputs()
        {
            UnsubscribeInput(submitAction, Submit);
            UnsubscribeInput(navigationVector, Navigate);

            UnsubscribeInput(nextTabAction, NextTab);
            UnsubscribeInput(previousTabAction, PreviousTab);

            UnsubscribeInput(cancelAction, Cancel);
            UnsubscribeInput(extraLeftAction, ExtraLeft);
            UnsubscribeInput(extraRightAction, ExtraRight);
            UnsubscribeInput(extraButtonAction, ExtraButton);
            
            UnsubscribeInput(pauseAction, Pause);
        }

        public InputAction ResolveAction(InputActionReference reference)
        {
            if(!playerInput)
                return reference.action;
                
            if (!playerInput.actions) return null;
            return playerInput.actions[reference.name];
        }

        public void SetPlayerInput(int playerIndex)
        {
            playerInput = PlayerInput.GetPlayerByIndex(playerIndex);
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

        private void Cancel(InputAction.CallbackContext obj) 
        {
            if (_handover)
            {
                if (obj.phase != InputActionPhase.Performed && obj.phase != InputActionPhase.Canceled)
                    return;

                OnBackHandover.Invoke(obj);
                return;
            }
        }
        private void ExtraLeft(InputAction.CallbackContext obj) 
        {
            if (_handover)
            {
                if (obj.phase != InputActionPhase.Performed && obj.phase != InputActionPhase.Canceled)
                    return;

                OnExtraLeftHandover.Invoke(obj);
                return;
            }
        }
        private void ExtraRight(InputAction.CallbackContext obj) 
        {
            if (_handover)
            {
                if (obj.phase != InputActionPhase.Performed && obj.phase != InputActionPhase.Canceled)
                    return;

                OnExtraRightHandover.Invoke(obj);
                return;
            }
        }

        private void Pause(InputAction.CallbackContext obj)
        {
            if (blockNavigation)
                return;

            if (_handover)
            {
                if (obj.phase != InputActionPhase.Performed && obj.phase != InputActionPhase.Canceled)
                    return;

                OnPauseHandover.Invoke(obj);
                return;
            }
        }

        private void ExtraButton(InputAction.CallbackContext obj)
        {
            if (_handover)
            {
                if (obj.phase != InputActionPhase.Performed && obj.phase != InputActionPhase.Canceled)
                    return;

                OnExtraButtonHandover.Invoke(obj);
                return;
            }
        }

        private void PreviousTab(InputAction.CallbackContext obj)
        {
            if (blockNavigation)
                return;

            if(_handover)
            {
                if (obj.phase != InputActionPhase.Performed && obj.phase != InputActionPhase.Canceled)
                    return;

                OnPreviousTabHandover.Invoke(obj);
                return;
            }
            
            if (obj.phase != InputActionPhase.Performed) return;

            GetComponentsInChildren<TabGroup>().ToList().ForEach(x =>
            {
                if (x.gameObject.activeInHierarchy) x.PreviousTab();
            });
        }

        private void NextTab(InputAction.CallbackContext obj)
        {
            if (blockNavigation)
                return;

            if (_handover)
            {
                if (obj.phase != InputActionPhase.Performed && obj.phase != InputActionPhase.Canceled)
                    return;

                OnNextTabHandover.Invoke(obj);
                return;
            }

            if (obj.phase != InputActionPhase.Performed) return;

            GetComponentsInChildren<TabGroup>().ToList().ForEach(x =>
            {
                if (x.gameObject.activeInHierarchy) x.NextTab();
            });
        }
        private void Navigate(InputAction.CallbackContext obj)
        {
            if (blockNavigation)
                return;
            
            SetNavigationType(NavigationType.Axis);

            if (navigationComboCoroutine != null)
            {
                StopCoroutine(navigationComboCoroutine);
                navigationComboCoroutine = null;
            }
            
            if (obj.phase == InputActionPhase.Performed)
            {
                if (_handover)
                {
                    OnNavigateHandover.Invoke(obj);
                    return;
                }

                navigationComboCoroutine = StartCoroutine(NavigationCombo(obj.action));
            }
            else if (obj.phase == InputActionPhase.Canceled)
            {
                if(_handover)
                {
                    OnNavigateHandover.Invoke(obj);
                    return;
                }
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
                if (blockNavigation)
                    yield break;

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
                float availableAngle = (origin.limitHorizontalToSiblings && direction.x != 0 && direction.y == 0) ? 1f : 80f;
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
                float maxDistance = AllPotentialTargets.Max(x => Vector3.SqrMagnitude(x.transform.position - origin.transform.position));
                AllPotentialTargets = AllPotentialTargets.OrderBy(x =>
                {
                    float distance = Vector3.SqrMagnitude(x.transform.position - origin.transform.position);
                    float distanceNormalized = distance / maxDistance;
                    float angle = Vector3.Angle(x.transform.position - origin.transform.position, realDirection);

                    if ((realDirection.x != 0 && x.captureAlongHeight && angle < 90) ||
                        (realDirection.y != 0 && x.captureAlongWidth && angle < 90))
                    {
                        distanceNormalized = Vector3.SqrMagnitude(Vector3.Project(x.transform.position - origin.transform.position, realDirection)) / maxDistance;
                        angle = 0;
                    }

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
            if (blockNavigation)
                return;

            if (_handover)
            {
                if (obj.phase != InputActionPhase.Performed && obj.phase != InputActionPhase.Canceled)
                    return;

                OnSubmitHandover.Invoke(obj);
                return;
            }

            if (obj.phase == InputActionPhase.Started)
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

                if (selected.locked)
                    OnSubmitLocked.Invoke();
                else
                    OnSubmit.Invoke();
            }
        }

        private void SubmitEnd(InputAction.CallbackContext obj)
        {
            holdingSubmit = false;
        }

        void Update()
        {
            if (_handover)
                return;

            if(selected && !selected.gameObject.activeInHierarchy)
            {
                DeselectButton(selected);
            }

            bool canMouseBeUsed = false; // TODO(Tautvydas): Fix and re-enable, right now it just causes more issues

            if(playerInput)
            {
                if (playerInput.currentControlScheme != "Keyboard&Mouse") canMouseBeUsed = false;
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

                if (button.locked)
                    OnSelectLocked.Invoke();
                else
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

            // If player exists get asset held by the player, if not, use the action asset 
            InputActionAsset asset;
            if(playerInput)
            {
                asset = playerInput.actions;
            }
            else
            {
                asset = Instantiate(submitAction.asset);
                asset.devices = InputSystem.devices;
            }

            var actionMap = asset.FindActionMap("UI");
            actionMap.actionTriggered += ActionTriggered;
            actionMap.Enable();

            inputCleanupAction = () => actionMap.actionTriggered -= ActionTriggered;

            EventDictionary.Clear();

            foreach (var item in actionMap.actions)
            {
                if(EventDictionary.TryAdd(item.name, new UnityEvent<InputAction.CallbackContext>()))
                {

                }
            }

            SubscribeDefaultInputs();
        }
        private void DisableInput()
        {
            inputCleanupAction?.Invoke();

            UnsubscribeDefaultInputs();
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

        public void UnsubscribeInput(InputActionReference r, UnityAction<InputAction.CallbackContext> action)
        {
            if (r == null) return;

            if (EventDictionary.TryGetValue(r.action.name, out var context))
            {
                context.RemoveListener(action);
            }
            else
            {
                string all = "";
                EventDictionary.Keys.ToList().ForEach(x => all += $"{x}\n");

                Debug.LogWarning($"The UI action map does not contain {r.name} [{r.action.name}] from {r.action.actionMap} It contains these actions:\n");

            }
        }

        public void EnableHandover()
        {
            _handover = true;
        }

        public void DisableHandover()
        {
            _handover = false;
            OnSubmitHandover = null;
            OnNavigateHandover = null;
            OnNextTabHandover = null;
            OnPreviousTabHandover = null;
        }

        public void BlockNavigation(bool state)
        {
            blockNavigation = state;
            if (blockNavigation && navigationComboCoroutine != null)
            {
                StopAllCoroutines();
                navigationComboCoroutine = null;
            }
        }
    }
}