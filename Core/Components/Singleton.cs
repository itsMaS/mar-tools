using System;
using UnityEngine;

namespace MarTools
{
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void Cleanup()
        {
            _instance = null;
        }

        private bool initialized = false;

        public static T Instance
        {
            get
            {
                if(!_instance)
                {
                    _instance = FindFirstObjectByType<T>(FindObjectsInactive.Include);
                    if(_instance)
                    {
                        _instance.TryInitialize();
                    }
                }
                if(!_instance)
                {
                    Debug.LogWarning("Not a single instance of the singleton class exists in the scene");
                }

                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance && _instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                TryInitialize();
            }
        }

        public void TryInitialize()
        {
            if(!initialized)
            {
                Initialize();
            }
        }

        protected virtual void Initialize()
        {
            _instance = this as T;
            //Debug.Log($"Initialize {_instance.GetType()}");

            initialized = true;
        }

        private static T _instance;
    }
}
