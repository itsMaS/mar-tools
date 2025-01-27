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
    
        MaterialPropertyBlock block;
        Renderer rend;

        public override void SetPose(float t)
        {
            if(!rend)
            {
                rend = GetComponent<Renderer>();
            }

            if (block == null)
            {
                block = new MaterialPropertyBlock();
                if (rend.HasPropertyBlock()) rend.GetPropertyBlock(block);
            }

            SetParameter(ref block, parameterID, from, to, t);
            rend.SetPropertyBlock(block, materialIndex);
        }

        protected abstract void SetParameter(ref MaterialPropertyBlock block, string parameterID, T from, T to, float t);
    }
    
}