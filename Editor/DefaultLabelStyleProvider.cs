using System;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;
using Component = UnityEngine.Component;

namespace HierarchyLabels
{
    [Serializable, DisplayName("Default Label Style")]
    public class DefaultLabelStyleProvider : ILabelStyleProvider
    {
        private static GUIStyle _style;
        public static GUIStyle Style =>
            _style ??= new GUIStyle(EditorStyles.whiteLabel)
            {
                name = nameof(DefaultLabelStyleProvider),
                fontSize = EditorStyles.whiteLabel.fontSize,
                normal =
                {
                    background = Texture2D.grayTexture
                },
                alignment = TextAnchor.MiddleCenter
            };

        public GUIStyle GetStyle(Component component)
        {
            return Style;
        }
    }
}