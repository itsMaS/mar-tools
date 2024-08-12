namespace MarTools
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    
    [RequireComponent(typeof(Renderer))]
    public abstract class MaterialParameterLerp<T> : TweenCore
    {
        public string parameterID;
        public int materialIndex = 0;
    
        public T from;
        public T to;
    
        Material mat;
    
        public override void SetPose(float t)
        {
            if (!Application.isPlaying) return;
    
            if(!mat)
            {
                var renderer = GetComponent<Renderer>();
                mat = renderer.materials[materialIndex];
            }
    
            SetParameter(mat, parameterID, from, to, t);
        }

        protected abstract void SetParameter(Material mat, string parameterID, T from, T to, float t);
    }
    
}