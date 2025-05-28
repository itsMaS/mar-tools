
namespace MarTools
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    public class ConditionalActivator : MonoBehaviour
    {
        [System.Serializable]
        public class Condition
        {
            [SerializeReference] public IConditional condition;
            public UnityEvent OnSuccess;
        }

        public List<Condition> Conditions = new List<Condition>();
        public UnityEvent OnElse;


        public void Activate()
        {
            foreach (var item in Conditions)
            {
                if(item.condition.IsTrue())
                {
                    item.OnSuccess.Invoke();
                    return;
                }
            }
            OnElse.Invoke();
        }

        private void Reset()
        {
            Conditions.Add(new Condition());
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(ConditionalActivator))]
    public class ConditionalActivatorEditor : MarToolsEditor<ConditionalActivator>
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // TODO : Make a nice looking if tree visually
        }
    }
#endif
}
