namespace Sirstrap.Core.Telemetry
{
    public sealed class SentryPerformanceTelemetry : IPerformanceTelemetry
    {
        public void RecordCounter(string name, IReadOnlyDictionary<string, object>? tags = null)
            => Emit(name, () => SentrySdk.Metrics.EmitCounter(name, 1, Copy(tags)));

        public void RecordDistribution(string name, double value, string unit, IReadOnlyDictionary<string, object>? tags = null)
            => Emit(name, () => SentrySdk.Metrics.EmitDistribution(name, value, unit, Copy(tags)));

        public ITelemetryScope Measure(string operation, IReadOnlyDictionary<string, object>? tags = null)
        {
            ISpan? span = null;
            bool isRoot = false;

            try
            {
                ISpan? current = SentrySdk.GetSpan();

                if (current != null)
                    span = current.StartChild(operation);
                else
                {
                    ITransactionTracer transaction = SentrySdk.StartTransaction(operation, "task");

                    SentrySdk.ConfigureScope(scope => scope.Transaction = transaction);

                    span = transaction;
                    isRoot = true;
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[!] Failed to start the telemetry span for {Operation}.", operation);
            }

            SentryScope telemetryScope = new(this, operation, span, isRoot);

            if (tags != null)
                foreach (var kvp in tags)
                    telemetryScope.SetTag(kvp.Key, kvp.Value);

            return telemetryScope;
        }

        public void SetTag(string key, string value)
            => Emit(key, () => SentrySdk.ConfigureScope(scope => scope.SetTag(key, value)));

        public void SetContext(string name, IReadOnlyDictionary<string, object> values)
            => Emit(name, () => SentrySdk.ConfigureScope(scope => scope.Contexts[name] = Copy(values)));

        private static Dictionary<string, object> Copy(IReadOnlyDictionary<string, object>? tags)
            => tags == null ? [] : new Dictionary<string, object>(tags);

        private static void Emit(string name, Action emit)
        {
            try
            {
                emit();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[!] Failed to emit the telemetry {TelemetryName}.", name);
            }
        }

        private sealed class SentryScope(SentryPerformanceTelemetry telemetry, string operation, ISpan? span, bool isRoot) : ITelemetryScope
        {
            private readonly Dictionary<string, object> _attributes = [];
            private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
            private bool _failed;
            private string? _outcome;
            private bool _disposed;

            public void MarkFailed(string? outcome = null)
            {
                _failed = true;
                _outcome = outcome ?? _outcome;
            }

            public void SetOutcome(string outcome) => _outcome = outcome;

            public void SetTag(string key, object value)
            {
                _attributes[key] = value;

                Emit(key, () => span?.SetTag(key, value?.ToString() ?? string.Empty));
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;
                _stopwatch.Stop();

                string outcome = _outcome ?? (_failed ? "Failed" : "Success");

                _attributes["outcome"] = outcome;
                _attributes["success"] = !_failed;

                telemetry.RecordDistribution($"{operation}.duration", _stopwatch.Elapsed.TotalMilliseconds, "millisecond", _attributes);

                Emit(operation, () =>
                {
                    if (span == null)
                        return;

                    span.SetTag("outcome", outcome);
                    span.Finish(_failed ? SpanStatus.InternalError : SpanStatus.Ok);

                    if (isRoot)
                        SentrySdk.ConfigureScope(scope =>
                        {
                            if (ReferenceEquals(scope.Transaction, span))
                                scope.Transaction = null;
                        });
                });
            }
        }
    }
}
