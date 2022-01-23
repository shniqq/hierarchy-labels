using UnityEngine;

namespace HierarchyLabels
{
    public interface IHierarchyLabelRule
    {
        bool GetLabel(Component component, out string label, out GUIStyle style);
    }
}