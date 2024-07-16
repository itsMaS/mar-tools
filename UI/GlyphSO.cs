using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GlyphSO : MonoBehaviour
{
}

#if UNITY_EDITOR
[CustomEditor(typeof(GlyphSO))]
public class GlyphSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }

}
#endif
