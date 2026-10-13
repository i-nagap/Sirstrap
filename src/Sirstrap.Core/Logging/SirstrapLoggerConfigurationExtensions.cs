namespace Sirstrap.Core.Logging
{
    public static class SirstrapLoggerConfigurationExtensions
    {
        private const string SentryDsn = "https://0cd56ab3e5eac300ecf1380dd6ad0a92@o4510907426471936.ingest.de.sentry.io/4510907479490640";

        public static string GetSentryEnvironment(string channel)
        {
            string normalizedChannel = ReleaseTag.NormalizeChannel(channel).ToLowerInvariant();

            return string.IsNullOrEmpty(normalizedChannel) ? "production" : normalizedChannel;
        }

        public static LoggerConfiguration SessionLog(this LoggerSinkConfiguration sinkConfiguration, SessionLogSink sink)
        {
            return sinkConfiguration.Sink(sink);
        }

        public static LoggerConfiguration WriteToSentryWhenEnabled(this LoggerConfiguration loggerConfiguration, SirstrapSettings settings, SirstrapEdition edition, string runId)
        {
            return !settings.SirstrapTelemetry ? loggerConfiguration : loggerConfiguration.WriteTo.Sentry(options =>
            {
                options.Dsn = SentryDsn;
                options.Environment = GetSentryEnvironment(settings.SirstrapChannel);
                options.AutoSessionTracking = true;
                options.EnableLogs = true;
                options.CaptureFailedRequests = false;
                options.MaxBreadcrumbs = 200;
                options.TracesSampleRate = 1.0;
                options.DefaultTags["sirstrap.type"] = edition.ToString();
                options.DefaultTags["run.id"] = runId;
            });
        }
    }
}