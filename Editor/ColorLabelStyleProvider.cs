using System;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;
using Component = UnityEngine.Component;

namespace HierarchyLabels
{
    [Serializable, DisplayName("Colored Label")]
    public class ColorLabelStyleProvider : ILabelStyleProvider, ISerializationCallbackReceiver
    {
        [SerializeField] private Color _color = Color.black;

        public GUIStyle GetStyle(Component component)
        {
            var style = new GUIStyle(DefaultLabelStyleProvider.Style)
            {
                normal =
                {
                    textColor = _color
                }
            };
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