using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Component = UnityEngine.Component;

namespace HierarchyLabels.Rules
{
    [Serializable, DisplayName("Is From Assembly"),
     Description("Displays a label if the component script is from the specified assembly.")]
    internal class DisplayScriptsFromSpecifiedAssemblyHierarchyRule : HierarchyLabelRule
    {
        private Dictionary<Type, string> _types;

        [SerializeField] public string AssemblyName;
        [SerializeField] private bool _assemblyFullName;

        public override bool GetLabel(Component component, out string label)
        {
            if (_types.ContainsKey(component.GetType()))
            {
                label = component.GetType().Name;
                return true;
            }

            label = string.Empty;
            return false;
        }

        public override void OnBeforeSerialize()
        {
            GatherTypes();
            base.OnBeforeSerialize();
        }

        public override void OnAfterDeserialize()
        {
            GatherTypes();
            base.OnAfterDeserialize();
        }

        private void GatherTypes()
        {
            _types = TypeCache.GetTypesDerivedFrom<MonoBehaviour>()
                .Where(e => !string.IsNullOrEmpty(AssemblyName)
                            && (_assemblyFullName
                                ? e.Assembly.GetName().Name.Equals(AssemblyName)
                                : e.Assembly.GetName().Name.Contains(AssemblyName)))
                .ToDictionary(e => e, e => e.Name);
        }
    }
}