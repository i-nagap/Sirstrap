namespace Sirstrap.Core.Telemetry
{
    public static class SentryLoggerConfigurationExtensions
    {
        private const string DSN = "https://0cd56ab3e5eac300ecf1380dd6ad0a92@o4510907426471936.ingest.de.sentry.io/4510907479490640";

        public static LoggerConfiguration WriteToSentry(this LoggerConfiguration loggerConfiguration, SirstrapConfiguration sirstrapConfiguration, SirstrapType sirstrapType, string runId)
        {
            if (!sirstrapConfiguration.SirstrapTelemetry)
                return loggerConfiguration;

            return loggerConfiguration.WriteTo.Sentry(x =>
            {
                x.Dsn = DSN;
                x.Environment = GetEnvironment(sirstrapConfiguration.SirstrapChannel);
                x.AutoSessionTracking = true;
                x.EnableLogs = true;
                x.CaptureFailedRequests = false;
                x.MaxBreadcrumbs = 200;

                x.TracesSampleRate = 1.0;
                x.ProfilesSampleRate = 0.5;
                x.AddIntegration(new Sentry.Profiling.ProfilingIntegration());

                x.DefaultTags["sirstrap.type"] = sirstrapType.ToString();
                x.DefaultTags["run.id"] = runId;
            });
        }

        public static string GetEnvironment(string? channel)
        {
            string normalized = channel?.Trim().TrimStart('-').ToLowerInvariant() ?? string.Empty;

            return string.IsNullOrEmpty(normalized) ? "production" : normalized;
        }
    }
}
