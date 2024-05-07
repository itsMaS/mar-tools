namespace MarTools
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    
    [RequireComponent(typeof(Renderer))]
    public class MaterialParameterLerp : TweenCore
    {
        public string materialID;
        public int materialIndex = 0;
    
        public float from = 0;
        public float to = 1;
    
        Material mat;
    
        public override void SetPose(float t)
        {
            if (!Application.isPlaying) return;
    
            if(!mat)
            {
                var renderer = GetComponent<Renderer>();
    
                mat = renderer.materials[materialIndex];
            }
    
            mat.SetFloat(materialID, Mathf.Lerp(from,to, t));
        }
    }
    
}