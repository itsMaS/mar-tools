using MarTools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class Pallete : RandomUtilities.WeightedList<PalleteItem> 
{
    public GameObject Place(MonoBehaviour instantiator, Vector3 position, Quaternion rotation)
    {
        string id = $"ELEMENTS_{instantiator.GetComponentIndex()}";
        Transform holder = instantiator.transform.Find(id);
        if(!holder)
        {
            holder = (new GameObject(id)).transform;
            holder.transform.parent = instantiator.transform;
        }

        var picked = Options.PickRandom().element;
        return GameObject.Instantiate(picked.prefab, position, rotation, holder);
    }

    public void Clear(MonoBehaviour instantiator)
    {
        string id = $"ELEMENTS_{instantiator.GetComponentIndex()}";
        Transform holder = instantiator.transform.Find(id);
        if (holder)
        {
            GameObject.DestroyImmediate(holder.gameObject);
        }
    }
}
[System.Serializable]
public class PalleteItem
{
    public GameObject prefab;
}


[RequireComponent(typeof(LineBehavior))]
public class FillLineEdge : MonoBehaviour
{
    public Pallete pallete;

    public int points = 10;

    public List<Vector3> Points = new List<Vector3>();
    public List<Vector3> Normals = new List<Vector3>();

    LineBehavior lineBehavior;
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        foreach (var item in Points)
        {
            Gizmos.DrawWireSphere(item, 0.5f);
        }
    }
    internal void UpdatePoints()
    {
        if(!lineBehavior) lineBehavior = GetComponent<LineBehavior>();
        var result = lineBehavior.GetPointAlongPath(points);
        
        Points = result.Item1;
        Normals = result.Item2;
    }

    internal void Populate()
    {
        Clear();
        for (int i = 0; i < Points.Count; i++)
        {
            Vector3 point = Points[i];
            Vector3 normal = Normals[i];

            pallete.Place(this, point, Quaternion.LookRotation(normal, Vector3.up));
        }
    }
    internal void Clear()
    {
        pallete.Clear(this);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(FillLineEdge))]
public class FillLineEdgeEditor : Editor 
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var script = (FillLineEdge)target;
        if(GUILayout.Button("Populate"))
        {
            script.Populate();
        }
        if (GUILayout.Button("Clear"))
        {
            script.Clear();
        }
    }
    public void OnSceneGUI()
    {
        var script = (FillLineEdge)target;

        script.UpdatePoints();
    }
}
#endif

