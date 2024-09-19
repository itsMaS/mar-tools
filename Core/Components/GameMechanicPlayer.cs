#if UNITY_EDITOR
using UnityEditor;
#endif
using MarTools;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameMechanicPlayer;
using UnityEngine.Events;

public interface IGameMechanic
{
    public void Execute();
    public GameObject gameObject { get; }
    public string name { get; }
}

public class GameMechanicPlayer : MonoBehaviour
{
    [System.Serializable]
    public class GameMechanicParams
    {
        public GameObject parent;
        public int componentIndex = 0;
        public bool enabled = true;
        public float executionDelay = 0;

        public IGameMechanic GetMechanic()
        {
            return parent.GetComponentAtIndex(componentIndex) as IGameMechanic;
        }
    }

    public UnityEvent OnPlayed;

    [HideInInspector]
    public List<GameMechanicParams> GameMechanicBindings = new List<GameMechanicParams>();
    [HideInInspector]
    public bool getChildMechanics = false;

    bool initialized = false;

    private Dictionary<GameMechanicParams, IGameMechanic> LoadedMechanics = new Dictionary<GameMechanicParams, IGameMechanic>();

    public void Initialize()
    {
        if (initialized) return;

        initialized = true;

        foreach (var item in GameMechanicBindings)
        {
            LoadedMechanics.Add(item, item.GetMechanic());
        }
    }

    public void Play()
    {
        Initialize();

        foreach (var item in GameMechanicBindings)
        {
            if(item.enabled)
            {
                if(item.executionDelay > 0)
                {
                    this.DelayedAction(item.executionDelay, () =>
                    {
                        LoadedMechanics[item].Execute();
                    });
                }
                else
                {
                    LoadedMechanics[item].Execute();
                }

            }
        }

        OnPlayed.Invoke();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(GameMechanicPlayer))]
public class GameFeelControllerEditor : Editor
{
    bool autoRefresh = false;

    GameMechanicPlayer script;
    private void OnEnable()
    {
        script = (GameMechanicPlayer)target;
        RefreshComponents();
    }
    public override void OnInspectorGUI()
    {
        autoRefresh = EditorGUILayout.Toggle("Auto refresh", autoRefresh);
        if(!autoRefresh)
        {
            if(GUILayout.Button("Refresh"))
            {
                RefreshComponents();
            }
        }
        else
        {
            RefreshComponents();
        }


        base.OnInspectorGUI();

        EditorGUI.BeginChangeCheck();

        GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
        //btnStyle.fontSize = 15;
        btnStyle.alignment = TextAnchor.MiddleLeft;

        if(Application.isPlaying)
        {
            if(GUILayout.Button("Execute"))
            {
                script.Play();
            }
        }

        GUILayout.Space(10);

        //base.OnInspectorGUI();

        if (GUILayout.Button($"Children included {(script.getChildMechanics ? "YES":"NO")}", btnStyle))
        {
            script.getChildMechanics = !script.getChildMechanics;
            RefreshComponents();
        }

        GUILayout.Space(10);

        for (int i = 0; i < script.GameMechanicBindings.Count; i++)
        {
            GUILayout.BeginHorizontal();
            var binding = script.GameMechanicBindings[i];
            var mechanic = script.GameMechanicBindings[i].GetMechanic();

            EditorGUILayout.ObjectField("", mechanic as MonoBehaviour, typeof(MonoBehaviour));
            //GUILayout.Button($"{binding.parent.name}/{mechanic.GetType().Name}", btnStyle);
            script.GameMechanicBindings[i].enabled = GUILayout.Toggle(binding.enabled, "");
            script.GameMechanicBindings[i].executionDelay = EditorGUILayout.FloatField(binding.executionDelay);

            GUILayout.EndHorizontal();

            GUILayout.Space(5);
        }

        if(EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(script);
        }
    }

    private void RefreshComponents()
    {
        var newGameMechanics = script.getChildMechanics ? script.GetComponentsInChildren<IGameMechanic>() : script.GetComponents<IGameMechanic>();
        List<GameMechanicParams> newParams = new List<GameMechanicParams>();

        foreach (var item in newGameMechanics)
        {
            GameObject go = item.gameObject;
            int componentIndex = go.GetComponents<Component>().ToList().IndexOf(item as Component);

            GameMechanicPlayer.GameMechanicParams param = null;
            param = script.GameMechanicBindings.Find(x => x.parent == go && x.componentIndex == componentIndex);

            if (param == null)
            {
                param = new GameMechanicPlayer.GameMechanicParams()
                {
                    parent = go,
                    componentIndex = componentIndex,
                };
            }

            newParams.Add(param);
        }

        script.GameMechanicBindings = newParams;
    }
}
#endif
