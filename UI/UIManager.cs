using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas), typeof(GraphicRaycaster))]
public class UIManager : MonoBehaviour
{
    public bool allowNoSelected = false;

    private GraphicRaycaster graphicRaycaster;
    private EventSystem eventSystem;

    Button selected;
    List<Button> MouseHoveredButtons = new List<Button>();

    private void Awake()
    {
        eventSystem = GetComponent<EventSystem>();
        graphicRaycaster = GetComponent<GraphicRaycaster>();
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



        if(Input.GetMouseButtonDown(0))
        {
            if(selected != null)
            {
                selected.Click();
            }
        }
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
