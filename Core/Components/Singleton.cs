using UnityEngine;

namespace MarTools
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance
        {
            get
            {
                if(!_instance)
                {
                    _instance = FindFirstObjectByType<T>();
                }
                if(!_instance)
                {
                    Debug.LogError("Not a single instance of the singleton class exists in the scene");
                }

                return _instance;
            }
        }

        private static T _instance;
    }
}