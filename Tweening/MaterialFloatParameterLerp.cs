namespace MarTools
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class MaterialFloatParameterLerp : MaterialParameterLerp<float>
    {
        protected override void SetParameter(ref MaterialPropertyBlock block, string parameterID, float from, float to, float t)
        {
            block.SetFloat(parameterID, Mathf.LerpUnclamped(from, to, t));
        }
    }
}

