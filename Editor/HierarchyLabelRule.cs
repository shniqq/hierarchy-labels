using System;
using HierarchyLabels.BuiltInStyles;
using UnityEditor;
using UnityEngine;

namespace HierarchyLabels
{
    [Serializable]
    public abstract class HierarchyLabelRule : IHierarchyLabelRule, ISerializationCallbackReceiver
    {
        [SerializeReference] private ILabelStyleProvider _styleProviderProvider = new DefaultLabelStyleProvider();
        protected ILabelStyleProvider StyleProvider => _styleProviderProvider;

        public abstract bool GetLabel(Component component, out string label, out GUIStyle style);

        public virtual void OnBeforeSerialize()
        {
            EditorApplication.RepaintHierarchyWindow();
        }

        public virtual void OnAfterDeserialize()
        {
        }
    }
}