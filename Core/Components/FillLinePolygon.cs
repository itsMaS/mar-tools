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

        [SerializeField] private float density = 10;
        [SerializeField] private int seed;
    
        public List<TagSO> Tags = new List<TagSO>();
    
        public List<Vector3> CurrentPositions = new List<Vector3>();
        public bool raycastPlacement = true;
        public Vector3 offset = Vector3.zero;
        public void Fill()
        {
            var line = GetComponent<LineBehavior>();
            if(CurrentPositions.Count <= 0) 
            {
                Debug.LogError("No positions to fill");
                return;
            }

            RaycastPositions();
    
            Clear();
            foreach (var position in CurrentPositions) 
            {
                pallete.Place(this, position, transform.rotation);
            }
        }
    
        internal void NewSeed()
        {
            seed = Random.Range(int.MinValue, int.MaxValue);
            var line = GetComponent<LineBehavior>();
            CurrentPositions = line.GetPointsInsideShape((int)density, seed);

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

        internal void GenerateGrid()
        {
            var line = GetComponent<LineBehavior>();
            CurrentPositions = line.GetPointsInsideGrid(density).ConvertAll(i => i + offset);
        }
    }
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(FillLinePolygon))]
    public class PopulateLineShapeEditor : Editor
    {
        Transform parentTransform;

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
            if(GUILayout.Button("Generate grid"))
            {
                line.GenerateGrid();
            }
            if(GUILayout.Button("Clear"))
            {
                line.Clear();
            }

            parentTransform = (Transform)EditorGUILayout.ObjectField("Target Transform", parentTransform, typeof(Transform), true);

            if (parentTransform && GUILayout.Button("Populate pallete"))
            {
                PopulatePallete(line);
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

        private void PopulatePallete(FillLinePolygon line)
        {
            line.pallete.Options.Clear();
            foreach (Transform item in parentTransform)
            {
                line.pallete.Options.Add(new RandomUtilities.WeightedOption<PalleteItem>(new PalleteItem() { prefab = item.gameObject }, 1));
            }
        }
    }
    #endif
    
}