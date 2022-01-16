using System;
using UnityEditor;
using UnityEngine;

namespace HierarchyLabels
{
    [Serializable]
    public class DefaultLabelStyleProvider : ILabelStyleProvider
    {
        public GUIStyle GetStyle()
        {
            var style = new GUIStyle(EditorStyles.whiteLabel)
            {
                normal =
                {
                    background = Texture2D.grayTexture
                },
                alignment = TextAnchor.MiddleCenter
            };
            return style;
        }
    }
}