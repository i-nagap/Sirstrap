namespace Sirstrap.Core.Settings
{
    public enum IniLineKind
    {
        Other,

        SectionHeader,

        KeyValue
    }

    public readonly record struct IniLine(IniLineKind Kind, SettingsSection? Section, string Key, string Value)
    {
        private static SettingsSection? ParseSection(string header)
        {
            foreach (SettingsSection section in Enum.GetValues<SettingsSection>())
            {
                if (section.ToIniHeader().Equals(header, StringComparison.InvariantCultureIgnoreCase))
                {
                    return section;
                }
            }

            return null;
        }

        public static IniLine Parse(string line)
        {
            string trimmedLine = line.Trim();

            if (trimmedLine.StartsWith('['))
            {
                return new(IniLineKind.SectionHeader, ParseSection(trimmedLine), string.Empty, string.Empty);
            }

            int separatorIndex = trimmedLine.IndexOf('=');

            if (separatorIndex < 0)
            {
                return new(IniLineKind.Other, null, string.Empty, string.Empty);
            }

            string key = trimmedLine[..separatorIndex].Trim();

            return key.Length == 0 ? new(IniLineKind.Other, null, string.Empty, string.Empty) : new(IniLineKind.KeyValue, null, key, trimmedLine[(separatorIndex + 1)..].Trim());
        }
    }
}