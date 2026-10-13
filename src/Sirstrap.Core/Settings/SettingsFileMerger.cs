namespace Sirstrap.Core.Settings
{
    public static class SettingsFileMerger
    {
        private static IEnumerable<string> GetMissingLines(SettingsSection section, HashSet<string> writtenKeys, SirstrapSettings settings)
        {
            return SettingsCatalog.InSection(section).Where(definition => !writtenKeys.Contains(definition.Key)).Select(definition => $"{definition.Key}={definition.Read(settings)}");
        }

        public static List<string> Merge(IEnumerable<string> existingLines, SirstrapSettings settings)
        {
            List<string> mergedLines = [];
            Dictionary<SettingsSection, int> sectionContentEnds = [];
            Dictionary<SettingsSection, HashSet<string>> writtenKeys = Enum.GetValues<SettingsSection>().ToDictionary(section => section, _ => new HashSet<string>(StringComparer.OrdinalIgnoreCase));
            SettingsSection? currentSection = null;

            foreach (string line in existingLines)
            {
                IniLine parsedLine = IniLine.Parse(line);

                if (parsedLine.Kind == IniLineKind.SectionHeader)
                {
                    currentSection = parsedLine.Section;

                    mergedLines.Add(line);

                    if (currentSection is
                        {
                        } headerSection)
                    {
                        sectionContentEnds[headerSection] = mergedLines.Count;
                    }

                    continue;
                }

                if (parsedLine.Kind == IniLineKind.KeyValue && currentSection is { } keySection)
                {
                    SettingDefinition? definition = SettingsCatalog.FindByKey(parsedLine.Key, keySection);

                    if (definition == null)
                    {
                        continue;
                    }

                    mergedLines.Add($"{parsedLine.Key}={definition.Read(settings)}");

                    _ = writtenKeys[keySection].Add(definition.Key);

                    sectionContentEnds[keySection] = mergedLines.Count;

                    continue;
                }

                mergedLines.Add(line);

                if (currentSection is
                    {
                    } ownerSection && !string.IsNullOrWhiteSpace(line))
                {
                    sectionContentEnds[ownerSection] = mergedLines.Count;
                }
            }

            foreach ((SettingsSection section, int contentEnd) in sectionContentEnds.OrderByDescending(entry => entry.Value))
            {
                mergedLines.InsertRange(contentEnd, GetMissingLines(section, writtenKeys[section], settings));
            }

            foreach (SettingsSection section in Enum.GetValues<SettingsSection>().Where(section => !sectionContentEnds.ContainsKey(section)))
            {
                if (mergedLines.Count > 0)
                {
                    mergedLines.Add(string.Empty);
                }

                mergedLines.Add(section.ToIniHeader());
                mergedLines.AddRange(GetMissingLines(section, writtenKeys[section], settings));
            }

            return mergedLines;
        }
    }
}