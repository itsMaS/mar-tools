namespace MarTools
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [RequireComponent(typeof(Button))]
    public class SelectionGroupItem : MonoBehaviour
    {
        Button btn;
        private void Awake()
        {
            btn = GetComponent<Button>();
        }
    }
}

