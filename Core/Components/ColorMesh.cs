using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ColorMesh : MonoBehaviour
{
    public Color color;

    private MaterialPropertyBlock block;

    private void Awake()
    {
        SetColor();
    }

    private void OnValidate()
    {
        SetColor();
    }

    private void SetColor()
    {
        var rend = GetComponent<Renderer>();


        if(block == null)
        {
            block = new MaterialPropertyBlock();
        }

        block.SetColor("_BaseColor", color);

        rend.SetPropertyBlock(block);
    }
}
