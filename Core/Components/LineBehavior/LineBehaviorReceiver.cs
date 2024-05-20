namespace MarTools
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public abstract class LineBehaviorReceiver : MonoBehaviour
    {
        public LineBehavior line
        {
            get
            {
                if(!_line)
                {
                    _line = GetComponentInParent<LineBehavior>();
                    if (!_line) Debug.LogError("This gameobject or its parents do not have a LineBehavior script");
                }
                return _line;
            }
        }

        private LineBehavior _line;

        private Transform parent
        {
            get
            {
                string name = groupName.Length > 0 ? $"{groupName}" : $"Elements {this.GetComponentIndex()}";
                var p = transform.Find(name);
                if(!p)
                {
                    p = new GameObject(name).transform;
                    p.parent = transform;
                }

                return p;
            }
        }

        public string groupName = "";

        /// <summary>
        /// Is sent by the linebehavior component
        /// </summary>
        [ContextMenu("Refresh")]
        public virtual void UpdateEditor()
        {
        }

        public GameObject AddElement(GameObject go, Vector3 position, Quaternion rotation)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                GameObject instantiated = UnityEditor.PrefabUtility.InstantiatePrefab(go) as GameObject;
                instantiated.transform.position = position;
                instantiated.transform.rotation = rotation;

                instantiated.transform.parent = parent;

                return instantiated;
            }
            else
            {
#endif
                GameObject instantiated = Instantiate(go, position, rotation, parent);
                return instantiated;
#if UNITY_EDITOR
            }
#endif

        }

        public void ClearElements()
        {
            if(Application.isPlaying)
            {
                Destroy(parent.gameObject);
            }
            else
            {
                DestroyImmediate(parent.gameObject);
            }
        }
    }
}

