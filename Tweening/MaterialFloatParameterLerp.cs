namespace MarTools
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class MaterialFloatParameterLerp : MaterialParameterLerp<float>
    {
        protected override void SetParameter(Material mat, string parameterID, float from, float to, float t)
        {
            mat.SetFloat(parameterID, Mathf.Lerp(from, to, t));
        }
    }
}

