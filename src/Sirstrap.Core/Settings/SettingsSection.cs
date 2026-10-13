namespace Sirstrap.Core.Settings
{
    public enum SettingsSection
    {
        Settings,

        State
    }

    public static class SettingsSectionExtensions
    {
        public static string ToIniHeader(this SettingsSection section)
        {
            return section switch
            {
                SettingsSection.Settings => "[SETTINGS]",
                SettingsSection.State => "[STATE]",
                _ => throw new ArgumentOutOfRangeException(nameof(section))
            };
        }
    }
}