using System;
using System.Collections.Generic;
using System.Linq;
using HierarchyLabels.BuiltInStyles;
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
                selectionRect.x += EditorStyles.label.CalcSize(new GUIContent(gameObject.name)).x +
                                   HierarchyLabelsSettings.instance.Alignment.x;
                selectionRect.y += HierarchyLabelsSettings.instance.Alignment.y;

                ApplyLabels(selectionRect, GetLabels(gameObject));
            }
        }

        private static void ApplyLabels(Rect selectionRect, List<Tuple<string, GUIStyle>> labels)
        {
            if (!labels.Any())
            {
                return;
            }

            var isFirst = true;
            foreach (var (label, style) in labels)
            {
                if (!isFirst)
                {
                    DrawLabelWithStyle(ref selectionRect, DefaultLabelStyleProvider.Style,
                        HierarchyLabelsSettings.instance.Separator);
                }

                DrawLabelWithStyle(ref selectionRect, style, label);
                isFirst = false;
            }
        }

        private static void DrawLabelWithStyle(ref Rect selectionRect, GUIStyle style, string label)
        {
            var labelStyle = new GUIStyle(style);
            labelStyle.fontSize =
                Convert.ToInt32(labelStyle.fontSize * HierarchyLabelsSettings.instance.FontSizeFactory);
            var size = labelStyle.CalcSize(new GUIContent(label));

            selectionRect.y += (selectionRect.height - size.y) / 2f;
            selectionRect.width = size.x;
            selectionRect.height = size.y;

            GUI.Box(selectionRect, label, labelStyle);
            selectionRect.x += size.x;
        }

        private static List<Tuple<string, GUIStyle>> GetLabels(GameObject gameObject)
        {
            var list = new List<Tuple<string, GUIStyle>>();
            if (HierarchyLabelsSettings.instance.HierarchyLabelRules == null)
            {
                return list;
            }

            foreach (var component in gameObject.GetComponents<Component>())
            {
                var applicableRules = HierarchyLabelsSettings.instance.HierarchyLabelRules
                    .Where(e => e != null)
                    .Select(rule => rule.GetLabel(component, out var componentLabel, out var style)
                        ? new Tuple<string, GUIStyle>(componentLabel, style)
                        : null)
                    .Where(e => e is not null)
                    .ToList();
                if (applicableRules.Any())
                {
                    list.AddRange(applicableRules);
                }
            }

            return list;
        }
    }
}