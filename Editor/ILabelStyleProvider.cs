using System;
using UnityEditor;
using UnityEngine;

namespace HierarchyLabels
{
    public interface ILabelStyleProvider
    {
        GUIStyle GetStyle();
    }

    [Serializable]
    public class ColorLabelStyleProvider : ILabelStyleProvider, ISerializationCallbackReceiver
    {
        [SerializeField] private Color _color;

        public GUIStyle GetStyle()
        {
            var style = new DefaultLabelStyleProvider().GetStyle();
            style.normal.textColor = _color;
            return style;
        }

        public virtual void OnBeforeSerialize()
        {
            EditorApplication.RepaintHierarchyWindow();
        }

        public void OnAfterDeserialize()
        {
        }
    }
}