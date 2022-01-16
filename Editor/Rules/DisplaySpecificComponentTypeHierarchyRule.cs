using System;
using UnityEngine;

namespace HierarchyLabels.Rules
{
    [Serializable]
    internal class DisplaySpecificComponentTypeHierarchyRule : HierarchyLabelRule
    {
        [SerializeField] private string _typeName;

        public override bool GetLabel(Component component, out string label)
        {
            if (component.GetType().Name == _typeName)
            {
                label = _typeName;
                return true;
            }

            label = string.Empty;
            return false;
        }
    }
}