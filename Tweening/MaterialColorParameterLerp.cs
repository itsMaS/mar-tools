namespace MarTools
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class MaterialColorParameterLerp : MaterialParameterLerp<Color>
    {
        protected override void SetParameter(Material mat, string parameterID, Color from, Color to, float t)
        {
            mat.SetColor(parameterID, Color.Lerp(from, to, t));
        }
    }
}
