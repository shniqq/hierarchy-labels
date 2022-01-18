using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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
        public ILabelStyleProvider StyleProvider => _labelStyleProvider;
        public string Separator => _separator;
        public IHierarchyLabelRule[] HierarchyLabelRules => _hierarchyLabelRules.ToArray();

        [SerializeField] private bool _enabled = true;
        [SerializeField] private Vector2Int _labelAlignment = new(18, 0);
        [SerializeField, Range(0, 1)] private float _fontSizeFactor = 0.8f;
        [SerializeField] private string _separator = "|";
        [SerializeReference] private List<IHierarchyLabelRule> _hierarchyLabelRules = new();
        [SerializeReference] private ILabelStyleProvider _labelStyleProvider;

        [SerializeField] private bool _foldoutStyling;
        [SerializeField] private bool _foldoutRules;
        [SerializeField] private int _selectedRuleIndex;

        private static Dictionary<string, Type> _availableRules;
        private static SerializedObject _serializedObject;

        [MenuItem("Tools/Toggle Hierarchy Labels")]
        private static void ToggleLabels()
        {
            instance.Enabled = !instance.Enabled;
        }

        [InitializeOnLoadMethod]
        private static void GetOrCreateEditor()
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
        }

        public static void DrawSettings()
        {
            _serializedObject ??= new SerializedObject(instance);
            _serializedObject.UpdateIfRequiredOrScript();

            EditorGUI.BeginChangeCheck();
            instance._enabled = EditorGUILayout.Toggle("Enabled", instance._enabled);
            instance._labelAlignment = EditorGUILayout.Vector2IntField("Label Alignment", instance._labelAlignment);
            instance._fontSizeFactor = EditorGUILayout.Slider("Font Scale", instance._fontSizeFactor, 0.1f, 1);
            instance._separator = EditorGUILayout.TextField("Separator", instance._separator);

            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Hierarchy Label Rules:", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawActiveRules();
            EditorGUILayout.Separator();
            DrawRulesAdditionUI();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Separator();

            DrawStylingUI();

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
            while (hierarchyRulesEnumerator.MoveNext())
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.BeginVertical();
                var currentHierarchyLabelRule = (SerializedProperty)hierarchyRulesEnumerator.Current;
                var hierarchyRuleType = currentHierarchyLabelRule.managedReferenceValue.GetType();
                var displayName = hierarchyRuleType
                    .GetCustomAttribute<DisplayNameAttribute>() is { } displayNameAttribute
                    ? displayNameAttribute.DisplayName
                    : hierarchyRuleType.Name;
                EditorGUILayout.LabelField($"Name: {displayName}");

                var hierarchyRuleChildrenEnumerator = currentHierarchyLabelRule.GetEnumerator();
                while (hierarchyRuleChildrenEnumerator.MoveNext())
                {
                    EditorGUILayout.PropertyField((SerializedProperty)hierarchyRuleChildrenEnumerator.Current, true);
                }

                EditorGUILayout.EndVertical();

                if (GUILayout.Button("Remove"))
                {
                    hierarchyRulesSerializedProperty.DeleteArrayElementAtIndex(index);
                    break;
                }

                EditorGUILayout.EndHorizontal();
                index++;
            }
        }

        private static void DrawRulesAdditionUI()
        {
            instance._foldoutRules = EditorGUILayout.BeginFoldoutHeaderGroup(instance._foldoutRules, "Label Rules:");
            if (instance._foldoutRules)
            {
                instance._selectedRuleIndex = EditorGUILayout.Popup(new GUIContent("Select which rule to add:"),
                    instance._selectedRuleIndex,
                    _availableRules.Keys.ToArray());

                var selectedRule = _availableRules.ElementAt(instance._selectedRuleIndex);
                if (selectedRule.Value.GetCustomAttribute<DescriptionAttribute>(false) is { } descriptionAttribute)
                {
                    GUILayout.Label("Rule Description:");
                    EditorGUILayout.LabelField(descriptionAttribute.Description, EditorStyles.wordWrappedLabel);
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Add selected Rule"))
                {
                    var newInstance = Activator.CreateInstance(selectedRule.Value);
                    instance._hierarchyLabelRules.Add(newInstance as IHierarchyLabelRule);
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private static void DrawStylingUI()
        {
            var labelStyleProvider = _serializedObject.FindProperty(nameof(_labelStyleProvider));
            EditorGUILayout.PropertyField(labelStyleProvider, true);
            
            instance._foldoutStyling = EditorGUILayout.BeginFoldoutHeaderGroup(instance._foldoutStyling, "Styling:");
            if (instance._foldoutStyling)
            {
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
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void Sanitize()
        {
            _hierarchyLabelRules.RemoveAll(e => e is null);
        }
    }
}