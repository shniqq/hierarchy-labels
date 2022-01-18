using System;
using UnityEditor;
using UnityEngine;

namespace HierarchyLabels
{
    [Serializable]
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