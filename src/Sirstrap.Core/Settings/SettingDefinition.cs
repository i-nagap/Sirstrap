namespace Sirstrap.Core.Settings
{
    public sealed record SettingDefinition(string Key, SettingsSection Section, Func<string> Getter, Action<string> Setter, IReadOnlyList<string> LegacyKeys, Func<string, string>? ValueMigrator, SettingMetric? Metric);

    public sealed record SettingMetric(string Name, Func<object> GetValue);
}
