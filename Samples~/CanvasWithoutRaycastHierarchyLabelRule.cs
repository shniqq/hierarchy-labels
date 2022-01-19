using System;
using System.ComponentModel;
using HierarchyLabels;
using UnityEngine;
using UnityEngine.UI;
using Component = UnityEngine.Component;

[Serializable,
DisplayName("Missing Raycaster on Canvas"),
Description("Shows a label if a Canvas is attached but no GraphicalRaycaster is present or it is disabled.")]
public class CanvasWithoutRaycastHierarchyLabelRule : HierarchyLabelRule
{
    [SerializeField] private bool _includeDisabled;

    public override bool GetLabel(Component component, out string label)
    {
        label = string.Empty;

        if (component is Canvas && IsRaycastDisabledOrMissing(component))
        {
            label = "Missing GraphicRaycaster!";
            return true;
        }

        return false;
    }

    private bool IsRaycastDisabledOrMissing(Component component)
    {
        return !component.GetComponent<GraphicRaycaster>() ||
               _includeDisabled && !component.GetComponent<GraphicRaycaster>().enabled;
    }
}