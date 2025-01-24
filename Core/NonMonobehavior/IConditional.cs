#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;

namespace MarTools
{

    public interface IConditional<T>
    {
        public bool IsTrue(T value);
    }

    public interface IConditional
    {
        public bool IsTrue();
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(IConditional), true)]
    public class IConditionalDrawer : PolymorphicDrawer<IConditional> { }
#endif

    public interface IGameObjectConditional : IConditional<GameObject> {}
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(IGameObjectConditional), true)]
    public class IGameObjectConditionalDrawer : PolymorphicDrawer<IGameObjectConditional> { }
#endif

    [NameAttribute("Unity/Compare Tag")]
    [System.Serializable]
    public class CompareTag : IGameObjectConditional
    {
        public string tag;
        public bool IsTrue(GameObject value)
        {
            return value.CompareTag(tag);

        }
    }

    [NameAttribute("Unity/Compare Scene Reference")]
    [System.Serializable]
    public class CompareSceneReference : IGameObjectConditional
    {
        public GameObject sceneReference;
        public bool IsTrue(GameObject value)
        {
            return value == sceneReference;
        }
    }

    [NameAttribute("Generic/Composite")]
    [System.Serializable]
    public class CompositeObjectConditional : IGameObjectConditional
    {
        [SerializeReference]
        public List<IGameObjectConditional> Conditionals = new List<IGameObjectConditional>();
        public bool IsTrue(GameObject value)
        {
            return Conditionals.TrueForAll(x => x.IsTrue(value));
        }
    }

    [NameAttribute("Unity/Name Check")]
    public class NameChecker : IGameObjectConditional
    {
        [System.Serializable]
        public enum CheckOperation
        {
            Exact,
            Contains,
        }

        public string name;
        public CheckOperation operation;

        public bool IsTrue(GameObject value)
        {
            return operation == CheckOperation.Exact ? value.name == name : value.name.Contains(name);
        }
    }
}
