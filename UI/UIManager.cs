using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas), typeof(GraphicRaycaster))]
public class UIManager : MonoBehaviour
{
    public InputActionReference submitAction;
    public InputActionReference navigationVector;

    public bool allowNoSelected = false;

    private GraphicRaycaster graphicRaycaster;
    private EventSystem eventSystem;

    Button selected;
    List<Button> MouseHoveredButtons = new List<Button>();

    //public string lastMap 

    private void Awake()
    {
        eventSystem = GetComponent<EventSystem>();
        graphicRaycaster = GetComponent<GraphicRaycaster>();

        submitAction.action.Enable();
        navigationVector.action.Enable();
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
        Vector2 aim = obj.ReadValue<Vector2>();
        Debug.Log($"{aim}");

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
            selected.Click();
        }
    }

    void Update()
    {
        MouseHoveredButtons.Clear();

        PointerEventData pointerData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };

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

        Button newSelected = MouseHoveredButtons.FirstOrDefault();
    }

    public void SelectButton(Button button)
    {
        if (selected == button) return;

        if(button != null || allowNoSelected)
        {
            DeselectButton(selected);
        }

        if(button != null)
        {
            selected = button;
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
}
