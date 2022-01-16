using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HierarchyLabels.Rules
{
    [Serializable]
    internal class DisplayScriptsFromSpecifiedAssemblyHierarchyRule : HierarchyLabelRule
    {
        private Dictionary<Type, string> _types;

        [SerializeField] public string AssemblyName;

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
                .Where(e => !string.IsNullOrEmpty(AssemblyName) && e.Assembly.FullName.Contains(AssemblyName))
                .ToDictionary(e => e, e => e.Name);
        }
    }
}