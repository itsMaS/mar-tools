namespace MarTools
{
    using UnityEngine;
    using UnityEngine.Events;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    public class Activator : MonoBehaviour
    {
        public UnityEvent OnActivated;
        public UnityEvent OnDeactivated;

        bool state = false;

        public void Activate()
        {
            state = true;
            OnActivated.Invoke();
        }
        public void Deactivate()
        {
            state = false;
            OnDeactivated.Invoke();
        }

        public void Toggle()
        {
            if(state)
            {
                Deactivate();
            }
            else
            {
                Activate();
            }
        }

        /// <summary>
        /// Activate or deactivate with a single method
        /// </summary>
        /// <param name="value"></param>
        public void Activate(bool value)
        {
            if(value)
            {
                Activate();
            }
            else
            {
                Deactivate();
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Activator))]
    public class ActivatorEditor : MarToolsEditor<Activator>    
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if(GUILayout.Button("Activate"))
            {
                script.Activate();
            }
            
            if(GUILayout.Button("Deactivated"))
            {
                script.Deactivate();
            }

        }
    }
#endif
}
