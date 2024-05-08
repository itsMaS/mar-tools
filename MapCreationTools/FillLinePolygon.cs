namespace MarTools
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    #if UNITY_EDITOR
    using UnityEditor;
    using System.Linq;
    using System.Runtime.InteropServices.WindowsRuntime;
#endif

    [RequireComponent(typeof(LineBehavior))]
    public class FillLinePolygon : MonoBehaviour
    {
        public Pallete pallete;

        [SerializeField] private int density = 10;
        [SerializeField] private int seed;
    
        public List<TagSO> Tags = new List<TagSO>();
    
        public List<Vector3> CurrentPositions = new List<Vector3>();
        private string elementName => $"Elements {this.GetComponentIndex()}";
    
        public void Fill()
        {
            Transform holder = transform.Find(elementName);
    
            var line = GetComponent<LineBehavior>();
            CurrentPositions = line.GetPointsInsideShape(density, seed);
            FilterPositions();
    
            Clear();
    
            holder = (new GameObject(elementName)).transform;
            holder.parent = transform;
    
            foreach (var position in CurrentPositions) 
            {
                pallete.Place(this, position, Quaternion.Euler(0, Random.value * 360, 0));
            }
        }
    
        internal void NewSeed()
        {
            seed = Random.Range(int.MinValue, int.MaxValue);
            var line = GetComponent<LineBehavior>();
            CurrentPositions = line.GetPointsInsideShape(density, seed);

            FilterPositions();
        }

        internal void FilterPositions()
        {
            CurrentPositions = CurrentPositions.Where(item =>
            {
                if(Physics.Raycast(item + Vector3.up * 100, Vector3.down, out RaycastHit hit))
                {
                    return (pallete.placeSurfaceMask.value & (1 << hit.collider.gameObject.layer)) != 0;
                }
                return false;
            }).ToList();
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
    
            if(GUILayout.Button("Populate"))
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