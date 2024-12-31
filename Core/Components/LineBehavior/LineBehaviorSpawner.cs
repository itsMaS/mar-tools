namespace MarTools
{
    using System.Collections.Generic;
    using UnityEngine;
    using System.Linq;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    public abstract class LineBehaviorSpawner : MonoBehaviour
    {
        [System.Serializable]
        public class SpawnPosition
        {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale = Vector3.one;
        }

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

        public List<LineBehavior> OutsideOf;
        public List<LineBehavior> InsideOf;

        [HideInInspector] public List<SpawnPosition> SpawnPositions = new List<SpawnPosition>();
        [HideInInspector] public RandomUtilities.WeightedList<GameObject> Options = new RandomUtilities.WeightedList<GameObject>();
        public bool useLocalPosition = true;

        public void UpdateEditor()
        {
            SpawnPositions = UpdatePositions();

            if(OutsideOf != null && OutsideOf.Count > 0)
            {
                SpawnPositions.RemoveAll(x => OutsideOf.Any(y => y.IsPointInsideShape(x.position)));
            }

            if(InsideOf != null && InsideOf.Count > 0)
            {
                SpawnPositions.RemoveAll(x => !InsideOf.Any(y => y.IsPointInsideShape(x.position)));
            }
        }

        public GameObject AddElement(GameObject go, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            GameObject instantiated = null;

#if UNITY_EDITOR
            if (!Application.isPlaying && PrefabUtility.GetPrefabAssetType(go) != PrefabAssetType.NotAPrefab && !go.transform.parent)
            {
                instantiated = UnityEditor.PrefabUtility.InstantiatePrefab(go) as GameObject;
                instantiated.transform.position = position;
                instantiated.transform.rotation = rotation;
                instantiated.transform.parent = parent;
            }
            else
            {
#endif
                instantiated = Instantiate(go, position, rotation, parent);
#if UNITY_EDITOR
            }
#endif
            instantiated.gameObject.SetActive(true);
            instantiated.transform.localScale = scale;
            return instantiated;

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

        private void OnValidate()
        {
            if(gameObject.activeInHierarchy && enabled)
            {
                UpdateEditor();
            }
        }

        public abstract List<SpawnPosition> UpdatePositions();

        internal void SpawnObjects()
        {
            if (Options.Options.Count <= 0) return;

            ClearElements();
            foreach (var item in SpawnPositions)
            {
                var option = Options.PickRandom();

                Vector3 rot = item.rotation.eulerAngles + option.transform.localRotation.eulerAngles;
                Vector3 scale = Vector3.Scale(option.transform.localScale, item.scale);
                Vector3 position = item.position + (useLocalPosition ?  option.transform.localPosition : Vector3.zero);

                AddElement(option, position, Quaternion.Euler(rot), scale);
            }
        }

        internal void UpdateShape()
        {
            UpdateEditor();
            SpawnObjects();
        }
    }

#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(LineBehaviorSpawner), true)]
    public class LineBehaviorSpawnerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            LineBehaviorSpawner spawner = target as LineBehaviorSpawner;
            if(GUILayout.Button("Spawn Objects"))
            {
                spawner.SpawnObjects();
            }
            if(GUILayout.Button("Clear Objects"))
            {
                spawner.ClearElements();
            }

            for (int i = 0; i < spawner.Options.Options.Count; i++)
            {
                var element = spawner.Options.Options[i];
                GUILayout.BeginHorizontal();

                spawner.Options.Options[i].element = (GameObject)EditorGUILayout.ObjectField("Game Object", element.element, typeof(GameObject), true);
                spawner.Options.Options[i].weight = EditorGUILayout.Slider(element.weight, 0, 1);
                if(GUILayout.Button("-"))
                {
                    spawner.Options.Options.RemoveAt(i);
                    break;
                }

                GUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+"))
            {
                spawner.Options.Options.Add(new RandomUtilities.WeightedOption<GameObject>(null, 0.5f));
            }
        }

        private void OnSceneGUI()
        {
            var script = (target as LineBehaviorSpawner);

            if (script.SpawnPositions.Count > 500) return;
            foreach (var item in script.SpawnPositions)
            {
                Vector3 forward = item.rotation * Vector3.forward;
                Vector3 right = item.rotation * Vector3.right;
                Vector3 up = item.rotation * Vector3.up;

                Handles.color = Color.blue;
                Handles.DrawLine(item.position, item.position + forward * item.scale.z);
                Handles.color = Color.red;
                Handles.DrawLine(item.position, item.position + right * item.scale.x);
                Handles.color = Color.green;
                Handles.DrawLine(item.position, item.position + up * item.scale.y);
            }
        }
    }
#endif
}

