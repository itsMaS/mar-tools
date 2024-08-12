namespace MarTools
{
    using System.Collections.Generic;
    using UnityEngine;
    using System.Linq;
    using UnityEngine.UIElements;
    using UnityEngine.Events;
    #if UNITY_EDITOR
    using UnityEditor;
#endif
    public class GroupBehavior : MonoBehaviour
    {
        public UnityEvent OnThisActivated;
        public UnityEvent<GroupBehavior> OnAnyOtherActivated;
        [HideInInspector] public string groupID = "";

        [Tooltip("When this option is available AnyOtherActivated event will only work on siblings of this gameobject")]
        public bool limitScopeToSiblings = true;
    
        public void Activate()
        {
            foreach (var groupObject in GetGroupElements()) 
            {
                if(groupObject != this)
                {
                    groupObject.ActivatedByOther(this);
                }
            }

            OnThisActivated.Invoke();
        }

        public List<GroupBehavior> GetGroupElements()
        {
            var groupObjects = FindObjectsOfType<GroupBehavior>().Where(item => item.groupID == groupID && (!limitScopeToSiblings || item.transform.parent == transform.parent)).ToList();
            return groupObjects;
        }
    
        private void ActivatedByOther(GroupBehavior origin)
        {
            OnAnyOtherActivated.Invoke(origin);
        }

        public static void DeactivateAll(GroupBehavior element)
        {
            foreach (var item in element.GetGroupElements())
            {
                item.ActivatedByOther(null);
            }
        }
    }
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(GroupBehavior))]
    public class GroupBehaviorEditor : Editor
    {
        int selection = 0;
        string newName = string.Empty;
    
        private List<string> GroupIDs
        {
            get
            {
                var list = FindObjectsOfType<GroupBehavior>()
               .Where(g => !string.IsNullOrEmpty(g.groupID))
               .Select(g => g.groupID)
               .Distinct()
               .ToList();
                list.Insert(0, "Create new group...");
                return list;
            }
        }
    
        public override VisualElement CreateInspectorGUI()
        {
            GroupBehavior script = (GroupBehavior)target;
            selection = GroupIDs.IndexOf(script.groupID);
            return base.CreateInspectorGUI();
        }
    
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            GroupBehavior script = (GroupBehavior)target;
    
            var groups = GroupIDs;
    
            if (selection > 0)
            {
                selection = GroupIDs.IndexOf(script.groupID);
            }
            selection = EditorGUILayout.Popup("Select Group", selection, groups.ToArray());
    
            if (selection <= 0)
            {
                newName = EditorGUILayout.TextField("New group name", newName);
                if(GUILayout.Button("Create"))
                {
                    script.groupID = newName;
                    selection = GroupIDs.IndexOf(newName);
                }
            }
            else
            {
                script.groupID = GroupIDs[selection];
    
                var objects = FindObjectsOfType<GroupBehavior>().ToList().Where(item => item.groupID == script.groupID && 
                (!script.limitScopeToSiblings || item.transform.parent == script.transform.parent)).ToList().ConvertAll(item => item.gameObject);
                if (objects.Count > 1 && GUILayout.Button($"Select All [{objects.Count}]"))
                {
                    Selection.objects = objects.ToArray();
                }
            }

            if(GUILayout.Button("Activate"))
            {
                script.Activate();
            }
        }
    }
    
    #endif
    
}