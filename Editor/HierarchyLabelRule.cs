using UnityEditor;
using UnityEngine;

namespace HierarchyLabels
{
    public abstract class HierarchyLabelRule : IHierarchyLabelRule, ISerializationCallbackReceiver
    {
        public abstract bool GetLabel(Component component, out string label);

        public virtual void OnBeforeSerialize()
        {
            EditorApplication.RepaintHierarchyWindow();
        }

        public virtual void OnAfterDeserialize()
        {
        }
    }
}