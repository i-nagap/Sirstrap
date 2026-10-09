namespace Sirstrap.Core.Tests.Support
{
    public sealed class RecordingPerformanceTelemetry : IPerformanceTelemetry
    {
        public List<(string Name, IReadOnlyDictionary<string, object>? Tags)> Counters { get; } = [];

        public List<(string Name, double Value, string Unit, IReadOnlyDictionary<string, object>? Tags)> Distributions { get; } = [];

        public List<RecordingScope> Scopes { get; } = [];

        public Dictionary<string, string> Tags { get; } = [];

        public Dictionary<string, IReadOnlyDictionary<string, object>> Contexts { get; } = [];

        public void RecordCounter(string name, IReadOnlyDictionary<string, object>? tags = null) => Counters.Add((name, tags));

        public void RecordDistribution(string name, double value, string unit, IReadOnlyDictionary<string, object>? tags = null) => Distributions.Add((name, value, unit, tags));

        public ITelemetryScope Measure(string operation, IReadOnlyDictionary<string, object>? tags = null)
        {
            RecordingScope scope = new(operation, tags);

            Scopes.Add(scope);

            return scope;
        }

        public void SetTag(string key, string value) => Tags[key] = value;

        public void SetContext(string name, IReadOnlyDictionary<string, object> values) => Contexts[name] = values;

        public sealed class RecordingScope(string operation, IReadOnlyDictionary<string, object>? tags) : ITelemetryScope
        {
            public string Operation { get; } = operation;

            public IReadOnlyDictionary<string, object>? Tags { get; } = tags;

            public Dictionary<string, object> SetTags { get; } = [];

            public bool Failed { get; private set; }

            public string? Outcome { get; private set; }

            public bool Disposed { get; private set; }

            public void Dispose() => Disposed = true;

            public void MarkFailed(string? outcome = null)
            {
                Failed = true;
                Outcome = outcome ?? Outcome;
            }

            public void SetOutcome(string outcome) => Outcome = outcome;

            public void SetTag(string key, object value) => SetTags[key] = value;
        }
    }
}
