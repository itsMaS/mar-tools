using UnityEngine;

public class SetGlobalFloat : MonoBehaviour
{
    public string shaderID;

    public void SetValue(float value)
    {
        Shader.SetGlobalFloat(shaderID, value);
    }
}
