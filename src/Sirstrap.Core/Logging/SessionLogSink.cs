namespace Sirstrap.Core.Logging
{
    public sealed class SessionLogSink : ILogEventSink
    {
        private readonly List<string> _entries = [];

        private readonly Lock _entriesLock = new();

        private static string GetLevelAbbreviation(LogEventLevel level)
        {
            return level switch
            {
                LogEventLevel.Verbose => "VRB",
                LogEventLevel.Debug => "DBG",
                LogEventLevel.Information => "INF",
                LogEventLevel.Warning => "WRN",
                LogEventLevel.Error => "ERR",
                LogEventLevel.Fatal => "FTL",
                _ => level.ToString().ToUpperInvariant()
            };
        }

        public void Emit(LogEvent logEvent)
        {
            string message = logEvent.RenderMessage();

            LastMessage = message;
            LastTimestamp = logEvent.Timestamp;
            LastLevel = logEvent.Level;

            string entry = $"[{logEvent.Timestamp:HH:mm:ss} {GetLevelAbbreviation(logEvent.Level)}] {message}";

            if (logEvent.Exception != null)
            {
                entry = $"{entry}{Environment.NewLine}{logEvent.Exception}";
            }

            lock (_entriesLock)
            {
                _entries.Add(entry);
            }
        }

        public IReadOnlyList<string> GetEntries()
        {
            lock (_entriesLock)
            {
                return [.. _entries];
            }
        }

        public int EntryCount
        {
            get
            {
                lock (_entriesLock)
                {
                    return _entries.Count;
                }
            }
        }

        public LogEventLevel? LastLevel { get; private set; }

        public string LastMessage { get; private set; } = string.Empty;

        public DateTimeOffset? LastTimestamp { get; private set; }
    }
}