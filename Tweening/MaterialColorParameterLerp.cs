namespace MarTools
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class MaterialColorParameterLerp : MaterialParameterLerp<Color>
    {
        protected override void SetParameter(ref MaterialPropertyBlock block, string parameterID, Color from, Color to, float t)
        {
            block.SetColor(parameterID, Color.Lerp(from, to, t));
        }
    }
}
