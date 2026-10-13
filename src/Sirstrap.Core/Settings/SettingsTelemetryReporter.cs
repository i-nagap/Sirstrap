namespace Sirstrap.Core.Settings
{
    public sealed class SettingsTelemetryReporter(SirstrapSettings settings, ITelemetry telemetry)
    {
        public void Report()
        {
            try
            {
                Dictionary<string, object> snapshot = SettingsCatalog.All.Select(definition => definition.Metric).OfType<SettingMetric>().ToDictionary(metric => metric.Name, metric => metric.Read(settings));

                telemetry.RecordCounter("settings.snapshot", snapshot);
                telemetry.SetContext("Settings", snapshot);
            }
            catch (Exception ex)
            {
                Log.Debug(ex, "Could not send the settings to telemetry.");
            }
        }
    }
}