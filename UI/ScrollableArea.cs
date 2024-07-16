using MarTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollableArea : MonoBehaviour
{
    [SerializeField] RectTransform contentRect;
    [SerializeField] RectTransform displayRect;

    UIManager manager;
    private void Awake()
    {
        manager = GetComponentInParent<UIManager>();
        displayRect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        // TODO

        var selected = manager.lastNotNull;

        if(selected != null)
        {
            Vector2 delta = selected.transform.position - contentRect.position;

            if(Mathf.Abs(delta.x) > displayRect.rect.width / 2)
            {
                contentRect.transform.position = Vector3.MoveTowards(contentRect.transform.position, selected.transform.position, Time.deltaTime * 200);
            }
        }
    }
}
