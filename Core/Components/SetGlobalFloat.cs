using UnityEngine;

public class SetGlobalFloat : MonoBehaviour
{
    public string shaderID;
    public float startingValue = 0;


    public void SetValue(float value)
    {
        Shader.SetGlobalFloat(shaderID, value);
    }

    private void OnValidate()
    {
        SetValue(startingValue);
    }
}
