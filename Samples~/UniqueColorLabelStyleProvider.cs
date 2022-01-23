using System;
using System.ComponentModel;
using HierarchyLabels;
using UnityEngine;
using Component = UnityEngine.Component;
using Random = System.Random;

[Serializable, DisplayName("Unique Color per Component")]
public class UniqueColorLabelStyleProvider : ILabelStyleProvider
{
    public GUIStyle GetStyle(Component component)
    {
        var random = new Random(component.GetInstanceID());
        return new GUIStyle(DefaultLabelStyleProvider.Style)
        {
            normal =
            {
                textColor = new Color((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble())
            }
        };
    }
}