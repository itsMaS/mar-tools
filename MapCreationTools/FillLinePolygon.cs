namespace MarTools
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    #if UNITY_EDITOR
    using UnityEditor;
    using System.Linq;
    using System.Runtime.InteropServices.WindowsRuntime;
    using static UnityEditor.Progress;
#endif

    [RequireComponent(typeof(LineBehavior))]
    public class FillLinePolygon : MonoBehaviour
    {
        public Pallete pallete;
        public LayerMask LayerMask = int.MaxValue;
        public TagMask tagMask;

        [SerializeField] private int density = 10;
        [SerializeField] private int seed;
    
        public List<TagSO> Tags = new List<TagSO>();
    
        public List<Vector3> CurrentPositions = new List<Vector3>();
        public bool raycastPlacement = true;
        public void Fill()
        {
            var line = GetComponent<LineBehavior>();
            CurrentPositions = line.GetPointsInsideShape(density, seed);
            RaycastPositions();
    
            Clear();
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

            RaycastPositions();
        }

        internal void RaycastPositions()
        {
            if (!raycastPlacement) return;

            List<Vector3> NewPositions = new List<Vector3>();
            for (int i = 0; i < CurrentPositions.Count; i++)
            {
                Vector3 point = CurrentPositions[i];

                if (Physics.Raycast(point + Vector3.up * 100, Vector3.down, out RaycastHit hit, Mathf.Infinity, LayerMask))
                {
                    if(tagMask.Check(hit.collider.gameObject))
                    {
                        NewPositions.Add(hit.point);
                    }
                }
            }

            CurrentPositions = NewPositions;
        }
    
        public void Clear()
        {
            pallete.Clear(this);
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