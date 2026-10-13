namespace Sirstrap.Core.Settings
{
    public sealed class SettingsStore(SirstrapSettings settings, string filePath)
    {
        private void ApplyLine(IniLine line, SettingsSection section)
        {
            SettingDefinition? definition = SettingsCatalog.FindByKey(line.Key, section);

            if (definition != null)
            {
                Log.Debug("Loaded setting (Key: {0}, Value: {1}).", line.Key, line.Value);

                definition.Apply(settings, line.Value);

                return;
            }

            SettingDefinition? migratedDefinition = SettingsCatalog.FindByLegacyKey(line.Key, section);

            if (migratedDefinition == null)
            {
                return;
            }

            Log.Information("Moved an old setting to its new name (Old: {0}, New: {1}).", line.Key, migratedDefinition.Key);

            migratedDefinition.Apply(settings, line.Value);
        }

        public void Load()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    Save();
                }

                SettingsSection? currentSection = null;

                foreach (string line in File.ReadAllLines(FilePath))
                {
                    IniLine parsedLine = IniLine.Parse(line);

                    if (parsedLine.Kind == IniLineKind.SectionHeader)
                    {
                        currentSection = parsedLine.Section;
                    }
                    else if (parsedLine.Kind == IniLineKind.KeyValue && currentSection is
                    {
                    } section)
                    {
                        ApplyLine(parsedLine, section);
                    }
                }

                Log.Information("Loaded the settings (Path: {0}).", FilePath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not load the settings, using the default values (Path: {0}).", FilePath);
            }
        }

        public void Save()
        {
            try
            {
                string? directory = Path.GetDirectoryName(FilePath);

                if (!string.IsNullOrWhiteSpace(directory))
                {
                    _ = Directory.CreateDirectory(directory);
                }

                string[] existingLines = File.Exists(FilePath) ? File.ReadAllLines(FilePath) : [];

                File.WriteAllLines(FilePath, SettingsFileMerger.Merge(existingLines, settings));

                Log.Information("Saved the settings (Path: {0}).", FilePath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not save the settings (Path: {0}).", FilePath);
            }
        }

        public string FilePath { get; } = filePath;
    }
}