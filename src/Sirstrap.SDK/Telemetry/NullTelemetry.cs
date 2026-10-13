namespace Sirstrap.SDK.Telemetry
{
    public sealed class NullTelemetry : ITelemetry
    {
        private NullTelemetry()
        {
        }

        public void RecordCounter(string name, IReadOnlyDictionary<string, object>? attributes = null)
        {
        }

        public void RecordDistribution(string name, double value, MeasurementUnit unit, IReadOnlyDictionary<string, object>? attributes = null)
        {
        }

        public void SetContext(string name, IReadOnlyDictionary<string, object> values)
        {
        }

        public void SetGlobalTag(string key, string value)
        {
        }

        public ITelemetryOperation StartOperation(string name, IReadOnlyDictionary<string, object>? tags = null)
        {
            return NullOperation.Shared;
        }

        public static NullTelemetry Instance { get; } = new();

        private sealed class NullOperation : ITelemetryOperation
        {
            public void Dispose()
            {
            }

            public void MarkFailed(string? outcome = null)
            {
            }

            public void SetOutcome(string outcome)
            {
            }

            public void SetTag(string key, object value)
            {
            }

            public static NullOperation Shared { get; } = new();
        }
    }
}