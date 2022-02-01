using System;
using System.ComponentModel;
using UnityEngine;
using Component = UnityEngine.Component;

namespace HierarchyLabels.BuiltInRules
{
    [Serializable,
     DisplayName("Is Type"),
     Description("Displays a label if the component script is of the specified type.")]
    internal class DisplaySpecificComponentTypeHierarchyRule : HierarchyLabelRule
    {
        [SerializeField] private string _typeName;

        public override bool GetLabel(Component component, out string label, out GUIStyle style)
        {
            style = StyleProvider.GetStyle(component);
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