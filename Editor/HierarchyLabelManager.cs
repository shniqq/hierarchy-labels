using System;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HierarchyLabels
{
    internal static class HierarchyLabelManager
    {
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnDrawHierarchyItem;
            EditorSceneManager.sceneDirtied += _ => EditorApplication.RepaintHierarchyWindow();
        }

        private static void OnDrawHierarchyItem(int instanceID, Rect selectionRect)
        {
            if (!HierarchyLabelsSettings.instance.Enabled)
            {
                return;
            }

            var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

            if (gameObject != null)
            {
                selectionRect.x += HierarchyLabelsSettings.instance.StyleProvider.GetStyle().CalcSize(new GUIContent(gameObject.name)).x +
                                   HierarchyLabelsSettings.instance.Alignment.x;
                selectionRect.y += HierarchyLabelsSettings.instance.Alignment.y;

                ApplyLabel(selectionRect, GetLabel(gameObject));
            }
        }

        private static void ApplyLabel(Rect selectionRect, string label)
        {
            if (string.IsNullOrEmpty(label))
            {
                return;
            }

            var labelStyle = HierarchyLabelsSettings.instance.StyleProvider.GetStyle();
            labelStyle.fontSize =
                Convert.ToInt32(labelStyle.fontSize * HierarchyLabelsSettings.instance.FontSizeFactory);
            var size = labelStyle.CalcSize(new GUIContent(label));

            selectionRect.y = selectionRect.y + (selectionRect.height - size.y)/2f;
            selectionRect.width = size.x;
            selectionRect.height = size.y;

            GUI.Box(selectionRect, label, labelStyle);
            selectionRect.x += size.x;
        }

        private static string GetLabel(GameObject gameObject)
        {
            var label = new StringBuilder();

            foreach (var component in gameObject.GetComponents<Component>())
            {
                var componentLabel = string.Empty;
                if (HierarchyLabelsSettings.instance.HierarchyLabelRules != null &&
                    HierarchyLabelsSettings.instance.HierarchyLabelRules
                        .Where(e => e != null)
                        .Any(rule => rule.GetLabel(component, out componentLabel)))
                {
                    if (label.Length > 0)
                    {
                        label.Append(HierarchyLabelsSettings.instance.Separator);
                    }

                    label.Append(componentLabel);
                }
            }

            return label.ToString();
        }
    }
}