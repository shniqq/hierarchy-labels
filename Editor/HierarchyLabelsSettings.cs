using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using HierarchyLabels.Utils;
using UnityEditor;
using UnityEngine;

namespace HierarchyLabels
{
    [FilePath(HierarchyLabelsSettingsAssetPath, FilePathAttribute.Location.ProjectFolder)]
    internal class HierarchyLabelsSettings : ScriptableSingleton<HierarchyLabelsSettings>
    {
        private const string HierarchyLabelsSettingsAssetPath = "ProjectSettings/Hierarchy Labels/Settings.asset";

        public bool Enabled
        {
            get => _enabled;
            private set
            {
                _enabled = value;
                EditorApplication.RepaintHierarchyWindow();
            }
        }

        public Vector2 Alignment => _labelAlignment;
        public float FontSizeFactory => _fontSizeFactor;
        public string Separator => _separator;
        public IHierarchyLabelRule[] HierarchyLabelRules => _hierarchyLabelRules.ToArray();

        [SerializeField] private bool _enabled = true;
        [SerializeField] private Vector2Int _labelAlignment = new(18, 0);
        [SerializeField, Range(0, 1)] private float _fontSizeFactor = 0.8f;
        [SerializeField] private string _separator = "|";
        [SerializeReference] private List<IHierarchyLabelRule> _hierarchyLabelRules = new();

        [SerializeField] private bool _foldoutRules;
        [SerializeField] private int _selectedRuleIndex;

        private static Dictionary<string, Type> _availableRules;
        private static List<Tuple<Type, string>> _availableStyles;
        private static SerializedObject _serializedObject;

        [MenuItem("Tools/Toggle Hierarchy Labels")]
        private static void ToggleLabels()
        {
            instance.Enabled = !instance.Enabled;
        }

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            _availableRules = TypeCache.GetTypesDerivedFrom<IHierarchyLabelRule>()
                .Where(e => e.IsClass)
                .Where(e => !e.IsAbstract)
                .ToDictionary(
                    e => e.GetCustomAttribute<DisplayNameAttribute>(false)
                        is { } displayNameAttribute
                        ? displayNameAttribute.DisplayName
                        : e.Name,
                    e => e);

            _availableStyles = TypeCache.GetTypesDerivedFrom<ILabelStyleProvider>()
                .Where(e => e.IsClass && !e.IsAbstract)
                .Select(e => new Tuple<Type, string>(
                    e, e.GetCustomAttribute<DisplayNameAttribute>(false)
                        is { } displayNameAttribute
                        ? displayNameAttribute.DisplayName
                        : e.Name))
                .ToList();
        }

        public static void DrawSettings()
        {
            _serializedObject = new SerializedObject(instance);
            _serializedObject.UpdateIfRequiredOrScript();

            EditorGUI.BeginChangeCheck();
            instance._enabled = EditorGUILayout.Toggle("Enabled", instance._enabled);
            instance._labelAlignment = EditorGUILayout.Vector2IntField("Label Alignment", instance._labelAlignment);
            instance._fontSizeFactor = EditorGUILayout.Slider("Font Scale", instance._fontSizeFactor, 0.1f, 1);
            instance._separator = EditorGUILayout.TextField("Separator", instance._separator);

            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Hierarchy Label Rules:", EditorStyles.boldLabel);
            using (new VerticalLayoutBlock(EditorStyles.helpBox))
            {
                DrawActiveRules();
                EditorGUILayout.Separator();
                DrawRulesAdditionUI();
            }

            instance.Sanitize();

            if (_serializedObject.hasModifiedProperties)
            {
                _serializedObject.ApplyModifiedProperties();
            }

            if (EditorGUI.EndChangeCheck())
            {
                instance.Save(true);
            }
        }

        private static void DrawActiveRules()
        {
            var hierarchyRulesSerializedProperty = _serializedObject.FindProperty(nameof(_hierarchyLabelRules));
            var hierarchyRulesEnumerator = hierarchyRulesSerializedProperty.GetEnumerator();

            if (hierarchyRulesSerializedProperty.arraySize == 0)
            {
                EditorGUILayout.LabelField("No rules added yet..");
            }

            var index = 0;
            while (hierarchyRulesEnumerator.MoveNext() && hierarchyRulesEnumerator.Current != null)
            {
                using (new HorizontalLayoutBlock(EditorStyles.helpBox))
                {
                    using (new VerticalLayoutBlock())
                    {
                        var currentHierarchyLabelRule = (SerializedProperty)hierarchyRulesEnumerator.Current;

                        if (currentHierarchyLabelRule.managedReferenceValue == null)
                        {
                            hierarchyRulesSerializedProperty.DeleteArrayElementAtIndex(index);
                            EditorGUILayout.EndVertical();
                            break;
                        }

                        var hierarchyRuleType = currentHierarchyLabelRule.managedReferenceValue.GetType();
                        var displayName = hierarchyRuleType
                            .GetCustomAttribute<DisplayNameAttribute>() is { } displayNameAttribute
                            ? displayNameAttribute.DisplayName
                            : hierarchyRuleType.Name;
                        EditorGUILayout.LabelField($"Name: {displayName}");

                        var hierarchyRuleChildrenEnumerator = currentHierarchyLabelRule.GetEnumerator();
                        while (hierarchyRuleChildrenEnumerator.MoveNext())
                        {
                            if (hierarchyRuleChildrenEnumerator.Current is not SerializedProperty
                                childSerializedProperty)
                            {
                                continue;
                            }

                            var fullName = typeof(ILabelStyleProvider).FullName ?? nameof(ILabelStyleProvider);
                            if (childSerializedProperty.propertyType == SerializedPropertyType.ManagedReference
                                && childSerializedProperty.managedReferenceFieldTypename.Contains(fullName))
                            {
                                childSerializedProperty.managedReferenceValue ??= new DefaultLabelStyleProvider();

                                var currentStyle =
                                    _availableStyles.FirstOrDefault(e =>
                                        e.Item1 == childSerializedProperty.managedReferenceValue.GetType());
                                var styleIndex = currentStyle != null ? _availableStyles.IndexOf(currentStyle) : 0;
                                var newStyleIndex = EditorGUILayout.Popup(new GUIContent("Style:"),
                                    styleIndex,
                                    _availableStyles.Select(e => e.Item2).ToArray());
                                if (newStyleIndex != styleIndex)
                                {
                                    childSerializedProperty.managedReferenceValue =
                                        Activator.CreateInstance(_availableStyles[newStyleIndex].Item1);
                                }
                            }
                            else
                            {
                                EditorGUILayout.PropertyField(childSerializedProperty, true);
                            }
                        }
                    }

                    if (GUILayout.Button("Remove"))
                    {
                        hierarchyRulesSerializedProperty.DeleteArrayElementAtIndex(index);
                        break;
                    }
                }

                index++;
            }
        }

        private static void DrawRulesAdditionUI()
        {
            using (new FoldoutHeaderGroupBlock(ref instance._foldoutRules, "Available Label Rules:"))
            {
                if (!instance._foldoutRules)
                {
                    return;
                }

                instance._selectedRuleIndex = EditorGUILayout.Popup(new GUIContent("Select which rule to add:"),
                    Math.Min(instance._selectedRuleIndex, _availableRules.Count - 1),
                    _availableRules.Keys.ToArray());

                var selectedRule = _availableRules.ElementAt(instance._selectedRuleIndex);
                if (selectedRule.Value.GetCustomAttribute<DescriptionAttribute>(false) is { } descriptionAttribute)
                {
                    GUILayout.Label("Rule Description:");
                    EditorGUILayout.LabelField(descriptionAttribute.Description, EditorStyles.wordWrappedLabel);
                }

                using (new HorizontalLayoutBlock())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Add selected Rule"))
                    {
                        var newInstance = Activator.CreateInstance(selectedRule.Value);
                        instance._hierarchyLabelRules.Add(newInstance as IHierarchyLabelRule);
                    }

                    GUILayout.FlexibleSpace();
                }
            }
        }

        private void Sanitize()
        {
            _hierarchyLabelRules.RemoveAll(e => e is null);
        }
    }
}