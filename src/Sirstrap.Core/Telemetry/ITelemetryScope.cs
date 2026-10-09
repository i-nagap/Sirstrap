namespace Sirstrap.Core.Telemetry
{
    public interface ITelemetryScope : IDisposable
    {
        void MarkFailed(string? outcome = null);

        void SetOutcome(string outcome);

        void SetTag(string key, object value);
    }
}
