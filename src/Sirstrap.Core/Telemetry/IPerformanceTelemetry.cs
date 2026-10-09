namespace Sirstrap.Core.Telemetry
{
    public interface IPerformanceTelemetry
    {
        void RecordCounter(string name, IReadOnlyDictionary<string, object>? tags = null);

        void RecordDistribution(string name, double value, string unit, IReadOnlyDictionary<string, object>? tags = null);

        ITelemetryScope Measure(string operation, IReadOnlyDictionary<string, object>? tags = null);

        void SetTag(string key, string value);

        void SetContext(string name, IReadOnlyDictionary<string, object> values);
    }
}
