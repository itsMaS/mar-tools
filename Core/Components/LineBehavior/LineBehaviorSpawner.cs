namespace MarTools
{
    using Palmmedia.ReportGenerator.Core.Reporting.Builders;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
    using System;
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

        public List<SpawnPosition> SpawnPositions = new List<SpawnPosition>();
        public GameObject prefab;

        public void UpdateEditor()
        {
            SpawnPositions = UpdatePositions();
        }

        public GameObject AddElement(GameObject go, Vector3 position, Quaternion rotation, Vector3 scale)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && PrefabUtility.GetPrefabAssetType(go) != PrefabAssetType.NotAPrefab)
            {
                GameObject instantiated = UnityEditor.PrefabUtility.InstantiatePrefab(go) as GameObject;
                instantiated.transform.position = position;
                instantiated.transform.rotation = rotation;
                instantiated.transform.localScale = scale;

                instantiated.transform.parent = parent;

                return instantiated;
            }
            else
            {
#endif
                GameObject instantiated = Instantiate(go, position, rotation, parent);
                instantiated.transform.localScale = scale;
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

        private void OnValidate()
        {
            UpdateEditor();
        }

        public abstract List<SpawnPosition> UpdatePositions();

        internal void SpawnObjects()
        {
            ClearElements();

            foreach (var item in SpawnPositions)
            {
                AddElement(prefab, item.position, item.rotation, item.scale);
            }
        }

        internal void UpdateShape()
        {
            UpdateEditor();
            SpawnObjects();
        }
    }

#if UNITY_EDITOR
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
        }

        private void OnSceneGUI()
        {
            var script = (target as LineBehaviorSpawner);
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

