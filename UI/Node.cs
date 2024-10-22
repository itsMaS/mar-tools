namespace MarTools 
{
    using MarTools;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using TMPro;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;

    public class Node : MonoBehaviour
    {
        private List<Node> Children = new List<Node>();
        private UnityAction cleanupAction = null;

        /// <summary>
        /// Populates the parent container using this node as an example
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public List<Node> Populate<T>(IEnumerable<T> Collection, Action<T, int, Node> InitializeFunction = null, Action<T, int, Node> SubmitFunction = null, Action<T, int, Node> SelectFunction = null)
        {
            cleanupAction?.Invoke();
            cleanupAction = null;

            foreach (var item in Children)
            {
                item.gameObject.SetActive(false);
            }


            List<Node> Nodes = new List<Node>();
            Transform parent = transform.parent;
            var list = Collection.ToList();

            for (int i = 0; i < list.Count; i++)
            {
                T element = list[i];
                int index = i;

                Node node = null;
                if(Children.Count <= i)
                {
                    node = Instantiate(gameObject, parent).GetComponent<Node>();
                }
                else
                {
                    node = Children[i];
                    node.gameObject.SetActive(true);
                }
                var btn = node.GetComponent<MarTools.Button>();

                Nodes.Add(node);


                if(InitializeFunction != null)
                    InitializeFunction.Invoke(element, index, node);

                if(btn)
                {
                    if(SubmitFunction != null)
                    {
                        UnityAction a = () => SubmitFunction(element, index, node);
                        btn.OnClick.AddListener(a);

                        cleanupAction += () => btn.OnSelected.RemoveListener(a);
                    }
            
                    if(SelectFunction != null)
                    {
                        UnityAction a = () => SelectFunction(element, index, node);
                        btn.OnSelected.AddListener(a);

                        cleanupAction += () => btn.OnSelected.RemoveListener(a);
                    }
                }

                Children.Add(node);
                node.gameObject.SetActive(true);
            }

            gameObject.SetActive(false);

            return Nodes;
        }

        public Node Add()
        {
            var node = Instantiate(gameObject, transform.parent).GetComponent<Node>();
            node.gameObject.SetActive(true);
            return node;
        }

        public Image image;
        public TextMeshProUGUI text;

        public Image[] Images;
        public TextMeshProUGUI[] Texts;

        public MarTools.Button button => GetComponent<MarTools.Button>();
    }
}
