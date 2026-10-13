namespace Sirstrap.Core.Settings
{
    public sealed record SettingMetric(string Name, Func<SirstrapSettings, object> Read);

    public sealed record SettingDefinition(string Key, SettingsSection Section, Func<SirstrapSettings, string> Read, Action<SirstrapSettings, string> Write)
    {
        public void Apply(SirstrapSettings settings, string rawValue)
        {
            Write(settings, MigrateValue(rawValue));
        }

        public IReadOnlyList<string> LegacyKeys { get; init; } = [];

        public SettingMetric? Metric { get; init; }

        public Func<string, string> MigrateValue { get; init; } = value => value;
    }
}