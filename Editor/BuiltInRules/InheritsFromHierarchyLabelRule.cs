using System;
using System.ComponentModel;
using UnityEngine;
using Component = UnityEngine.Component;
using Object = System.Object;

namespace HierarchyLabels.BuiltInRules
{
    [Serializable,
     DisplayName("Inherits From"),
     Description("Displays a label if the component inherits from the specified type name.")]
    public class InheritsFromHierarchyLabelRule : HierarchyLabelRule
    {
        [SerializeField] private string _baseTypeName;

        public override bool GetLabel(Component component, out string label, out GUIStyle style)
        {
            style = StyleProvider.GetStyle(component);
            label = string.Empty;
            var componentType = component.GetType();
            
            if (!string.IsNullOrWhiteSpace(_baseTypeName) && IsBaseType(_baseTypeName, componentType))
            {
                label = componentType.Name;
                return true;
            }

            return false;
        }

        private bool IsBaseType(string typeName, Type type)
        {
            while (true)
            {
                if (type.BaseType == null || type.BaseType == typeof(MonoBehaviour))
                {
                    return false;
                }

                if (type.BaseType.Name.Equals(typeName))
                {
                    return true;
                }

                type = type.BaseType;
            }
        }
    }
}