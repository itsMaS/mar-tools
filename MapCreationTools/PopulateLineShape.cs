using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(LineBehavior))]
public class PopulateLineShape : MonoBehaviour
{
    [SerializeField] public GameObject prefab;
    [SerializeField] private int density = 10;

    public void Fill()
    {
        var line = GetComponent<LineBehavior>();
        List<Vector3> positions = line.GetPointsInsideShape(density);

        Transform holder = transform.Find("Elements");

        if(holder != null)
        {
            if (Application.isPlaying) Destroy(holder.gameObject);
            else DestroyImmediate(holder.gameObject);
        }

        holder = (new GameObject("Elements")).transform;
        holder.parent = transform;

        foreach (var position in positions) 
        {
            Instantiate(prefab, position, Quaternion.Euler(0, Random.value * 360, 0), holder);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(PopulateLineShape))]
public class PopulateLineShapeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        PopulateLineShape line = (PopulateLineShape)target;

        if(line.prefab && GUILayout.Button("Populate"))
        {
            line.Fill();
        }
    }
}
#endif
