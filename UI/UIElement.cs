namespace MarTools
{
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.InputSystem;
    using UnityEngine.UI;

    [RequireComponent(typeof(Image), typeof(RectTransform))]
    public class UIElement : MonoBehaviour
    {
        public UnityEvent OnSelected;
        public UnityEvent OnDeselected;
        public UnityEvent OnSubmit;

        public UIManager manager
        {
            get
            {
                if(_manager == null)
                {
                    _manager = GetComponentInParent<UIManager>();
                    if (_manager == null) Debug.LogError("Parent does not contain a UIManager");
                }
                return _manager;
            }
        }

        public Image image
        {
            get
            {
                if(!_image) _image = GetComponent<Image>();
                return _image;
            }
        }

        public RectTransform rectTr
        {
            get
            {
                if(!_rectTr) _rectTr = GetComponent<RectTransform>();
                return _rectTr;
            }
        }

        public Vector2 cursorPositionNormalized
        {
            get
            {
                Vector2 normalized = (cursorPosition + rectTr.sizeDelta / 2);
                normalized.x /= rectTr.sizeDelta.x;
                normalized.y /= rectTr.sizeDelta.y;

                return normalized;
            }
        }
        public Vector2 cursorPositionNormalizedClamped
        {
            get
            {
                Vector2 normalized = cursorPositionNormalized;
                return new Vector2(Mathf.Clamp01(normalized.x), Mathf.Clamp01(normalized.y));
            }
        }
        public Vector2 cursorPosition
        {
            get
            {
                Vector2 pos = elementCanvasPosition;
                Vector2 delta = manager.pointerPosition - pos;
                return delta;
            }
        }

        public Vector2 elementCanvasPosition
        {
            get
            {
                Vector2 screenPosition = manager.cam.WorldToScreenPoint(rectTr.position);
                return screenPosition;
            }
        }

        public Vector2 cursorPositionClamped
        {
            get
            {
                Vector2 clampedPosition = cursorPosition;
                Vector2 size = rectTr.sizeDelta;
                clampedPosition = new Vector2(Mathf.Clamp(clampedPosition.x, -size.x/2, size.x/2), Mathf.Clamp(clampedPosition.y, -size.y / 2, size.y / 2));


                return clampedPosition;
            }
        }


        private UIManager _manager;
        private Image _image;
        private RectTransform _rectTr;

        [Tooltip("Whether this element can only be selected by a mouse")]
        public bool mouseOnly = false;
        public InputActionReference alternativeSubmitAction;
        public bool selected => manager.selected == this;

        private void Start()
        {
            if(alternativeSubmitAction != null) 
            {
                manager.SubscribeInput(alternativeSubmitAction, x =>
                {
                    if (x.phase == InputActionPhase.Performed) Submit();
                });
            }
        }

        public void Select()
        {
            OnSelectedInternal();
        }

        public void Deselect()
        {
            OnDeselectedInternal();
        }
        public void Submit()
        {
            OnSubmitedInternal();
        }

        public void Navigate(Vector2 direction)
        {
            OnNavigateInternal(direction);
        }

        protected virtual void OnNavigateInternal(Vector2 direction)
        {
            manager.MoveSelection(direction);
        }

        protected virtual void OnSelectedInternal()
        {
            OnSelected.Invoke();
        }

        protected virtual void OnDeselectedInternal()
        {
            OnDeselected.Invoke();
        }

        protected virtual void OnSubmitedInternal()
        {
            OnSubmit.Invoke();
        }

        private void OnEnable()
        {
            manager.Subscribe(this);
        }

        private void OnDisable()
        {
            manager.Unsubscribe(this);
        }
    }
}
