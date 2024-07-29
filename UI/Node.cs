using MarTools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Node : MonoBehaviour
{
    private List<Node> Children = new List<Node>();

    /// <summary>
    /// Populates the parent container using this node as an example
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public List<Node> Populate<T>(IEnumerable<T> Collection, Action<T, int, Node> InitializeFunction = null, Action<T, int, Node> SubmitFunction = null, Action<T, int, Node> SelectFunction = null)
    {
        foreach (var child in Children)
        {
            Destroy(child.gameObject);
        }
        Children.Clear();

        List<Node> Nodes = new List<Node>();

        Transform parent = transform.parent;
        var list = Collection.ToList();

        for (int i = 0; i < list.Count; i++)
        {
            T element = list[i];
            int index = i;

            var node = Instantiate(gameObject, parent).GetComponent<Node>();
            var btn = node.GetComponent<MarTools.Button>();

            Nodes.Add(node);


            if(InitializeFunction != null)
                InitializeFunction.Invoke(element, index, node);

            if(btn)
            {
                if(SubmitFunction != null)
                    btn.OnClick.AddListener(() => SubmitFunction(element, index, node));
            
                if(SelectFunction != null) 
                    btn.OnSelected.AddListener(() => SelectFunction(element, index, node));
            }

            Children.Add(node);
            node.gameObject.SetActive(true);
        }

        gameObject.SetActive(false);

        return Nodes;
    }

    public Image image;
    public TextMeshProUGUI text;

    public Image[] Images;
    public TextMeshProUGUI[] Texts;

    public MarTools.Button button => GetComponent<MarTools.Button>();
}
