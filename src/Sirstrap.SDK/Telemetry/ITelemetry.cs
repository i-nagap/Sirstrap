namespace Sirstrap.SDK.Telemetry
{
    public interface ITelemetry
    {
        void RecordCounter(string name, IReadOnlyDictionary<string, object>? attributes = null);

        void RecordDistribution(string name, double value, MeasurementUnit unit, IReadOnlyDictionary<string, object>? attributes = null);

        void SetContext(string name, IReadOnlyDictionary<string, object> values);

        void SetGlobalTag(string key, string value);

        ITelemetryOperation StartOperation(string name, IReadOnlyDictionary<string, object>? tags = null);
    }
}