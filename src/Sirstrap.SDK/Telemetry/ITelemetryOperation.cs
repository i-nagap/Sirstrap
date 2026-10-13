namespace Sirstrap.SDK.Telemetry
{
    public interface ITelemetryOperation : IDisposable
    {
        void MarkFailed(string? outcome = null);

        void SetOutcome(string outcome);

        void SetTag(string key, object value);
    }
}