namespace MarTools
{
    using UnityEngine;
    #if UNITY_EDITOR
    using UnityEditor;
    #endif
    
    // Runtime component to store points
    [RequireComponent(typeof(LineRenderer))]
    [ExecuteAlways]
    public class LineRendererLineBehavior : MonoBehaviour
    {
        public float verticalTilling = 10;
    
        private LineRenderer lr;
        private LineBehavior lb;

        MaterialPropertyBlock block;

        private void Awake()
        {
            Initialize();
        }
    
        private void Initialize()
        {
            lr = GetComponent<LineRenderer>();
            lb = GetComponentInParent<LineBehavior>();
        }
    
        private void Update()
        {
            UpdateLine();
        }

        public void UpdateLine()
        {
            if (!lb || !lr) Initialize();

            Vector3 rot = lr.transform.rotation.eulerAngles;
            rot.x = 90;

            lr.transform.rotation = Quaternion.Euler(rot);
            lr.useWorldSpace = false;
            lr.alignment = LineAlignment.TransformZ;

            float length = lb.CalculateLength();
            lr.textureScale = new Vector2(verticalTilling * length, lr.textureScale.y);

            var positions = lb.smoothWorldPoints.ConvertAll(item => transform.InverseTransformPoint(item));

            lr.positionCount = positions.Count;
            lr.SetPositions(positions.ToArray());


            if (block == null)
            {
                block = new MaterialPropertyBlock();
                if (lr.HasPropertyBlock()) lr.GetPropertyBlock(block);
            }
            block.SetFloat("_LineLength", length);
            lr.SetPropertyBlock(block);
        }
    }
    
    #if UNITY_EDITOR
    // Editor script to draw handles
    [CustomEditor(typeof(LineRendererLineBehavior))]
    public class PointsHolderEditor : Editor
    {
    }
    #endif
    
}