namespace MarTools
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    #if UNITY_EDITOR
    using UnityEditor;
    #endif
    
    [RequireComponent(typeof(LineBehavior))]
    public class FillLinePolygon : MonoBehaviour
    {
        [System.Serializable]
        public class Prefab
        {
            public float weight;
            public GameObject prefab;
        }
    
        [SerializeField] private int density = 10;
        [SerializeField] private int seed;
    
        public List<Prefab> Prefabs = new List<Prefab>();
    
        public List<Vector3> CurrentPositions = new List<Vector3>();
        private string elementName => $"Elements {this.GetComponentIndex()}";
    
        public void Fill()
        {
            Transform holder = transform.Find(elementName);
    
            var line = GetComponent<LineBehavior>();
            CurrentPositions = line.GetPointsInsideShape(density, seed);
    
            Clear();
    
            holder = (new GameObject(elementName)).transform;
            holder.parent = transform;
    
            var weighted = Prefabs.ConvertToWeighted(item => item.weight);
    
            foreach (var position in CurrentPositions) 
            {
                Instantiate(weighted.PickRandom().element.prefab, position, Quaternion.Euler(0, Random.value * 360, 0), holder);
            }
        }
    
        internal void NewSeed()
        {
            seed = Random.Range(int.MinValue, int.MaxValue);
            var line = GetComponent<LineBehavior>();
            CurrentPositions = line.GetPointsInsideShape(density, seed);
        }
    
        public void Clear()
        {
            Transform holder = transform.Find(elementName);
            if(holder)
            {
                if (Application.isPlaying) Destroy(holder.gameObject);
                else DestroyImmediate(holder.gameObject);
            }
        }
    }
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(FillLinePolygon))]
    public class PopulateLineShapeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
    
            FillLinePolygon line = (FillLinePolygon)target;
    
            if(line.Prefabs.Count > 0 && GUILayout.Button("Populate"))
            {
                line.Fill();
            }
            if(GUILayout.Button("New Seed"))
            {
                line.NewSeed();
            }
            if(GUILayout.Button("Clear"))
            {
                line.Clear();
            }
        }
    
        private void OnSceneGUI()
        {
            FillLinePolygon line = (FillLinePolygon)target;
            foreach (var position in line.CurrentPositions)
            {
                Handles.DrawWireDisc(position,Vector3.up, 1f);
            }
        }
    }
    #endif
    
}