namespace MarTools
{
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using System.Linq;
    using System.Reflection;
    using System;
    using System.Linq.Expressions;
    using static MarTools.ReactorBehavior;

    public class ReactorBehavior : MonoBehaviour
    {
        [System.Serializable]
        public class EventReference
        {
            public GameObject gameObject;
            public string componentType;
            public string fieldName;
            public int indexWithinType;

            [SerializeReference] public IMechanic activatedMechanic;

            public Component GetComponent()
            {
                if (gameObject == null || string.IsNullOrEmpty(componentType))
                    return null;

                Component[] components = gameObject.GetComponents(Type.GetType(componentType));
                if (components.Length > indexWithinType && indexWithinType >= 0)
                    return components[indexWithinType];
                return null;
            }

            public UnityEventBase GetEvent()
            {
                Component component = GetComponent();
                if (component == null)
                {
                    Debug.LogError("Component not found.");
                    return null;
                }

                FieldInfo eventField = component.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (eventField == null || !typeof(UnityEventBase).IsAssignableFrom(eventField.FieldType))
                {
                    Debug.LogError("Event field not found or is not of type UnityEventBase.");
                    return null;
                }

                return eventField.GetValue(component) as UnityEventBase;
            }
        }

        public List<EventReference> Components = new List<EventReference>();

        private void Awake()
        {
            foreach (var item in Components)
            {
                SubscribeToEvent(item.GetEvent(), item.activatedMechanic);
            }
        }

        public void SubscribeToEvent(UnityEventBase ev, IMechanic mechanic)
        {
            if (ev == null)
            {
                Debug.LogError("Provided UnityEventBase is null.");
                return;
            }

            Type eventType = ev.GetType();

            if (eventType == typeof(UnityEvent))
            {
                ((UnityEvent)ev).AddListener(() => HandleEventWithParams(mechanic));
            }
            else if (eventType.IsGenericType)
            {
                var genericArguments = eventType.GetGenericArguments();
                Type actionType = genericArguments.Length switch
                {
                    1 => typeof(UnityAction<>).MakeGenericType(genericArguments),
                    2 => typeof(UnityAction<,>).MakeGenericType(genericArguments),
                    3 => typeof(UnityAction<,,>).MakeGenericType(genericArguments),
                    _ => throw new InvalidOperationException("Unsupported number of generic arguments.")
                };

                var parameters = genericArguments.Select(Expression.Parameter).ToArray(); // Create dummy parameters for the lambda expression to match the event signature

                // Create a lambda that calls HandleEventWithParams with the mechanic and any event parameters
                var callExpression = Expression.Call(
                    Expression.Constant(this),
                    nameof(HandleEventWithParams),
                    null, // No generic type arguments needed
                    new Expression[] { Expression.Constant(mechanic) }.Concat(parameters).ToArray());

                var lambdaExpression = Expression.Lambda(actionType, callExpression, parameters);

                Delegate delegateInstance = lambdaExpression.Compile();

                MethodInfo addListenerMethod = eventType.GetMethod("AddListener", new Type[] { actionType });
                if (addListenerMethod == null)
                {
                    Debug.LogError("AddListener method not found.");
                    return;
                }

                addListenerMethod.Invoke(ev, new object[] { delegateInstance });
            }
        }

        public void HandleEventWithParams(IMechanic mechanic, params object[] args)
        {
            Debug.Log("Event triggered with parameters. Mechanic: " + mechanic.ToString());
            // Implement handling logic for both mechanic and event parameters here
        }


        public void HandleEvent(IMechanic mechanic)
        {
            mechanic?.Activate(gameObject);
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(ReactorBehavior))]
    public class ReactorBehaviorEditor : Editor
    {
        ReactorBehavior script;

        private void OnEnable()
        {
            script = (ReactorBehavior)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            foreach (var item in script.GetComponentsInChildren<Transform>())
            {
                foreach (var tr in item.GetComponents<Component>())
                {
                    foreach(var field in tr.GetFieldsOfType<UnityEventBase>(true))
                    {
                        string name = $"{item.gameObject.name}/{field.Name}";
                        string parameters = string.Concat(field.FieldType.GenericTypeArguments.ToList());

                        UnityEventBase ev = (UnityEventBase)field.GetValue(tr);
                        if (GUILayout.Button($"{name}<{parameters}>"))
                        {
                            script.Components.Add(SaveComponentReference(tr, field));
                        }
                    }
                }
            }
        }

        public static EventReference SaveComponentReference(Component component, FieldInfo field)
        {
            EventReference reference = new EventReference();
            reference.gameObject = component.gameObject;
            reference.componentType = component.GetType().AssemblyQualifiedName;  // Use full name to avoid ambiguity between types with the same name.
            Component[] componentsOfType = component.gameObject.GetComponents(component.GetType());
            reference.indexWithinType = System.Array.IndexOf(componentsOfType, component);
            reference.fieldName = field.Name;

            return reference;
        }
    }
#endif
}
