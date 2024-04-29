using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
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

    public NavigationType currentNavigationType;

    public InputActionReference submitAction;
    public InputActionReference navigationVector;
    public InputActionReference cancelAction;

    private GraphicRaycaster graphicRaycaster;
    private EventSystem eventSystem;

    Button selected;
    Button lastNotNull;
    List<Button> MouseHoveredButtons = new List<Button>();

    //public string lastMap 
    PointerEventData pointerData;

    private void Awake()
    {
        eventSystem = GetComponent<EventSystem>();
        graphicRaycaster = GetComponent<GraphicRaycaster>();

        submitAction.action.Enable();
        navigationVector.action.Enable();
        pointerData = new PointerEventData(eventSystem);

    }

    private void OnEnable()
    {
        submitAction.action.performed += Submit;
        navigationVector.action.performed += Navigate;
    }

    private void OnDisable()
    {
        submitAction.action.performed += Submit;
        navigationVector.action.performed -= Navigate;
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
            var other = GetComponentsInChildren<Button>().ToList().Where(item => item != selected).ToList();

            if(other.Count > 0)
            {
                var btn = other.FindClosest<Button>((Vector2)selected.transform.position + aim, item => item.transform.position, out float distance);
                SelectButton(btn);
            }
        }
    }
    private void Submit(InputAction.CallbackContext obj)
    {
        if (selected != null)
        {
            if(currentNavigationType == NavigationType.Pointer)
            {
                if(MouseHoveredButtons.Count > 0)
                {
                    selected.Click();
                }
            }
            else
            {
                selected.Click();
            }

        }
    }

    void Update()
    {
        MouseHoveredButtons.Clear();

        Vector2 pointerPosition = Input.mousePosition;
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
            SelectButton(MouseHoveredButtons.FirstOrDefault());
        }
    }

    public void SelectButton(Button button)
    {
        if (selected == button) return;

        DeselectButton(selected);

        selected = button;
        
        if(button != null)
        {
            lastNotNull = button;
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
        }
    }
}
