namespace Sirstrap.Core.Settings
{
    public static class SettingsCatalog
    {
        private static string AutoOrCustom(string value)
        {
            return string.IsNullOrEmpty(value) ? "Auto" : "Custom";
        }

        private static SettingDefinition Flag(string key, Func<SirstrapSettings, bool> read, Action<SirstrapSettings, bool> write, string? metricName = null)
        {
            return new(key, SettingsSection.Settings, settings => read(settings).ToString(), (settings, value) =>
            {
                if (bool.TryParse(value, out bool parsedValue))
                {
                    write(settings, parsedValue);
                }
            })
            {
                Metric = metricName == null ? null : new(metricName, settings => read(settings))
            };
        }

        private static SettingDefinition Text(string key, Func<SirstrapSettings, string> read, Action<SirstrapSettings, string> write)
        {
            return new(key, SettingsSection.Settings, read, write);
        }

        private static void WriteTrayMode(SirstrapSettings settings, string value)
        {
            if (Enum.TryParse(value, true, out TrayMode trayMode))
            {
                settings.SirstrapTrayMode = trayMode;
            }
        }

        public static SettingDefinition? FindByKey(string key, SettingsSection section)
        {
            return InSection(section).FirstOrDefault(definition => definition.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
        }

        public static SettingDefinition? FindByLegacyKey(string legacyKey, SettingsSection section)
        {
            return InSection(section).FirstOrDefault(definition => definition.LegacyKeys.Contains(legacyKey, StringComparer.OrdinalIgnoreCase));
        }

        public static IEnumerable<SettingDefinition> InSection(SettingsSection section)
        {
            return All.Where(definition => definition.Section == section);
        }

        public static IReadOnlyList<SettingDefinition> All { get; } =
        [
            Flag("CLEANER_CLEAN_ON_EXIT", settings => settings.CleanerCleanOnExit, (settings, value) => settings.CleanerCleanOnExit = value, "CleanerCleanOnExit"),
            Flag("CLEANER_CLEAN_ON_LAUNCH", settings => settings.CleanerCleanOnLaunch, (settings, value) => settings.CleanerCleanOnLaunch = value, "CleanerCleanOnLaunch"),
            Flag("CLEANER_CLEAN_PROTECTED_FILES", settings => settings.CleanerCleanProtectedFiles, (settings, value) => settings.CleanerCleanProtectedFiles = value, "CleanerCleanProtectedFiles"),
            Flag("CLEANER_CLEAN_TEMP_FOLDERS", settings => settings.CleanerCleanTempFolders, (settings, value) => settings.CleanerCleanTempFolders = value, "CleanerCleanTempFolders"),
            Flag("CLEANER_ENABLED", settings => settings.CleanerEnabled, (settings, value) => settings.CleanerEnabled = value, "CleanerEnabled"),
            Text("ROBLOX_CDN_URI_OVERRIDE", settings => settings.RobloxCdnUriOverride, (settings, value) => settings.RobloxCdnUriOverride = value) with
            {
                Metric = new("RobloxCdnUriOverride", settings => AutoOrCustom(settings.RobloxCdnUriOverride))
            },
            Flag("ROBLOX_FAST_FLAGS_ENABLED", settings => settings.RobloxFastFlagsEnabled, (settings, value) => settings.RobloxFastFlagsEnabled = value, "RobloxFastFlagsEnabled"),
            Flag("ROBLOX_INCOGNITO", settings => settings.RobloxIncognito, (settings, value) => settings.RobloxIncognito = value, "RobloxIncognito"),
            Text("ROBLOX_INSTALLATION_PATH", settings => settings.RobloxInstallationPath, (settings, value) => settings.RobloxInstallationPath = string.IsNullOrWhiteSpace(value) ? SirstrapSettings.DefaultRobloxInstallationPath : value),
            Flag("ROBLOX_MULTI_INSTANCE", settings => settings.RobloxMultiInstance, (settings, value) => settings.RobloxMultiInstance = value, "RobloxMultiInstance"),
            Text("ROBLOX_VERSION_OVERRIDE", settings => settings.RobloxVersionOverride, (settings, value) => settings.RobloxVersionOverride = value.Trim()) with
            {
                Metric = new("RobloxVersionOverride", settings => AutoOrCustom(settings.RobloxVersionOverride))
            },
            Text("ROBLOX_VERSION_SOURCE", settings => settings.RobloxVersionSource, (settings, value) => settings.RobloxVersionSource = value) with
            {
                Metric = new("RobloxVersionSource", settings => settings.RobloxVersionSource)
            },
            Text("SIRSTRAP_ACCENT_COLOR", settings => settings.SirstrapAccentColor, (settings, value) => settings.SirstrapAccentColor = value) with
            {
                Metric = new("SirstrapAccentColor", settings => settings.SirstrapAccentColor)
            },
            Flag("SIRSTRAP_AUTO_UPDATE", settings => settings.SirstrapAutoUpdate, (settings, value) => settings.SirstrapAutoUpdate = value, "SirstrapAutoUpdate"),
            Text("SIRSTRAP_CHANNEL", settings => settings.SirstrapChannel, (settings, value) => settings.SirstrapChannel = value) with
            {
                Metric = new("SirstrapChannel", settings => settings.SirstrapChannel)
            },
            Text("SIRSTRAP_FONT_FAMILY", settings => settings.SirstrapFontFamily, (settings, value) => settings.SirstrapFontFamily = value) with
            {
                Metric = new("SirstrapFontFamily", settings => settings.SirstrapFontFamily)
            },
            Text("SIRSTRAP_LANGUAGE", settings => settings.SirstrapLanguage, (settings, value) => settings.SirstrapLanguage = value) with
            {
                Metric = new("SirstrapLanguage", settings => string.IsNullOrEmpty(settings.SirstrapLanguage) ? "System" : settings.SirstrapLanguage)
            },
            Flag("SIRSTRAP_TELEMETRY", settings => settings.SirstrapTelemetry, (settings, value) => settings.SirstrapTelemetry = value),
            new SettingDefinition("SIRSTRAP_TRAY_MODE", SettingsSection.Settings, settings => settings.SirstrapTrayMode.ToString(), WriteTrayMode)
            {
                Metric = new("SirstrapTrayMode", settings => settings.SirstrapTrayMode.ToString())
            },
            new SettingDefinition("ROBLOX_PREVIOUS_INSTALLATION_PATH", SettingsSection.State, settings => settings.RobloxPreviousInstallationPath, (settings, value) => settings.RobloxPreviousInstallationPath = value)
        ];
    }
}