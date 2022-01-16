using System;
using System.Collections.Generic;
using System.Linq;
using HierarchyLabels.Rules;
using UnityEditor;
using UnityEngine;

namespace HierarchyLabels
{
    [FilePath("ProjectSettings/Hierarchy Labels/Settings.asset", FilePathAttribute.Location.ProjectFolder)]
    internal class HierarchyLabelsSettings : ScriptableSingleton<HierarchyLabelsSettings>
    {
        private const string HierarchyLabelsSettingsAssetPath = "Assets/Hierarchy Labels Settings.asset";

        public bool Enabled
        {
            get => _enabled;
            private set
            {
                _enabled = value;
                EditorApplication.RepaintHierarchyWindow();
            }
        }

        public Vector2 Spacing => _labelSpacing;
        public float FontSizeFactory => _fontSizeFactor;
        public ILabelStyleProvider StyleProvider => _labelStyleProvider;
        public string Separator => _separator;
        public IHierarchyLabelRule[] HierarchyLabelRules => _hierarchyLabelRules.ToArray();

        [SerializeField] private bool _enabled = true;
        [SerializeField] private Vector2Int _labelSpacing = new(18, -1);
        [SerializeField, Range(0, 1)] private float _fontSizeFactor = 0.9f;
        [SerializeField] private string _separator = "|";
        [SerializeReference] private List<IHierarchyLabelRule> _hierarchyLabelRules = new();
        [SerializeReference] private ILabelStyleProvider _labelStyleProvider;

        private static Editor _editor;

        [MenuItem("Tools/Toggle Hierarchy Labels")]
        private static void ToggleLabels()
        {
            instance.Enabled = !instance.Enabled;
        }

        [InitializeOnLoadMethod]
        private static void Init()
        {
            _editor = Editor.CreateEditor(instance);
        }

        public static void DrawSettings()
        {
            instance.hideFlags = HideFlags.None;

            _editor.OnInspectorGUI();

            foreach (var type in TypeCache.GetTypesDerivedFrom<IHierarchyLabelRule>()
                         .Where(e => e.IsClass)
                         .Where(e => !e.IsAbstract))
            {
                if (GUILayout.Button($"Add {type.Name}"))
                {
                    var newInstance = Activator.CreateInstance(type);
                    instance._hierarchyLabelRules.Add(newInstance as IHierarchyLabelRule);
                }
            }

            EditorGUILayout.Separator();

            foreach (var type in TypeCache.GetTypesDerivedFrom<ILabelStyleProvider>()
                         .Where(e => e.IsClass)
                         .Where(e => !e.IsAbstract))
            {
                if (GUILayout.Button($"Use {type.Name}"))
                {
                    var newInstance = Activator.CreateInstance(type);
                    instance._labelStyleProvider = newInstance as ILabelStyleProvider;
                }
            }

            instance.Save(true);
        }
    }
}