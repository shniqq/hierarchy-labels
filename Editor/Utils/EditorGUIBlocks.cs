using System;
using UnityEditor;
using UnityEngine;

namespace HierarchyLabels.Utils
{
    public class HorizontalLayoutBlock : IDisposable
    {
        public HorizontalLayoutBlock(GUIStyle style, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginHorizontal(style, options);
        }

        public HorizontalLayoutBlock(params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginHorizontal(options);
        }

        public void Dispose()
        {
            EditorGUILayout.EndHorizontal();
        }
    }

    public class VerticalLayoutBlock : IDisposable
    {
        public VerticalLayoutBlock(GUIStyle style, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical(style, options);
        }

        public VerticalLayoutBlock(params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical(options);
        }

        public void Dispose()
        {
            EditorGUILayout.EndVertical();
        }
    }

    public class FoldoutHeaderGroupBlock : IDisposable
    {
        public FoldoutHeaderGroupBlock(ref bool foldout,
            string content,
            GUIStyle style = null,
            Action<Rect> menuAction = null,
            GUIStyle menuIcon = null)
        {
            foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, content, style, menuAction, menuIcon);
        }

        public FoldoutHeaderGroupBlock(ref bool foldout,
            GUIContent content,
            GUIStyle style = null,
            Action<Rect> menuAction = null,
            GUIStyle menuIcon = null)
        {
            foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, content, style, menuAction, menuIcon);
        }

        public void Dispose()
        {
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}