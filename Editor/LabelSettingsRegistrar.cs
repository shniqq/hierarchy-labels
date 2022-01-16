using System.Collections.Generic;
using UnityEditor;

namespace HierarchyLabels
{
    internal class LabelSettingsRegistrar
    {
        private static readonly string SettingsPath = $"Project/{Constants.Name}";
        
        [SettingsProvider]
        public static SettingsProvider CreateAutoDarkModeSettingsProvider()
        {
            var provider = new SettingsProvider(SettingsPath, SettingsScope.Project)
            {
                guiHandler = _ =>
                {
                    HierarchyLabelsSettings.DrawSettings();
                },
                keywords = new HashSet<string>(new[] {"Label", "Labels", "Hierarchy"})
            };

            return provider;
        }

        public static void Open()
        {
            SettingsService.OpenUserPreferences(SettingsPath);
        }
    }
}