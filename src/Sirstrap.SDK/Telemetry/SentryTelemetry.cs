namespace Sirstrap.SDK.Telemetry
{
    public sealed class SentryTelemetry : ITelemetry
    {
        private static Dictionary<string, object> Copy(IReadOnlyDictionary<string, object>? values)
        {
            return values == null ? [] : new(values);
        }

        private static void EmitSafely(string name, Action emit)
        {
            try
            {
                emit();
            }
            catch (Exception ex)
            {
                Log.Debug(ex, "Could not send telemetry (Name: {0}).", name);
            }
        }

        private static (ISpan? Span, bool IsTransaction) StartSpan(string name)
        {
            try
            {
                ISpan? parentSpan = SentrySdk.GetSpan();

                if (parentSpan != null)
                {
                    return (parentSpan.StartChild(name), false);
                }

                ITransactionTracer transaction = SentrySdk.StartTransaction(name, "task");

                SentrySdk.ConfigureScope(scope => scope.Transaction = transaction);

                return (transaction, true);
            }
            catch (Exception ex)
            {
                Log.Debug(ex, "Could not start telemetry tracking (Operation: {0}).", name);

                return (null, false);
            }
        }

        public void RecordCounter(string name, IReadOnlyDictionary<string, object>? attributes = null)
        {
            EmitSafely(name, () => SentrySdk.Metrics.EmitCounter(name, 1, Copy(attributes)));
        }

        public void RecordDistribution(string name, double value, MeasurementUnit unit, IReadOnlyDictionary<string, object>? attributes = null)
        {
            EmitSafely(name, () => SentrySdk.Metrics.EmitDistribution(name, value, unit, Copy(attributes)));
        }

        public void SetContext(string name, IReadOnlyDictionary<string, object> values)
        {
            EmitSafely(name, () => SentrySdk.ConfigureScope(scope => scope.Contexts[name] = Copy(values)));
        }

        public void SetGlobalTag(string key, string value)
        {
            EmitSafely(key, () => SentrySdk.ConfigureScope(scope => scope.SetTag(key, value)));
        }

        public ITelemetryOperation StartOperation(string name, IReadOnlyDictionary<string, object>? tags = null)
        {
            (ISpan? span, bool isTransaction) = StartSpan(name);

            SentryOperation operation = new(this, name, span, isTransaction);

            foreach ((string key, object value) in tags ?? new Dictionary<string, object>())
            {
                operation.SetTag(key, value);
            }

            return operation;
        }

        private sealed class SentryOperation(SentryTelemetry telemetry, string name, ISpan? span, bool isTransaction) : ITelemetryOperation
        {
            private readonly Dictionary<string, object> _attributes = [];

            private bool _isDisposed;

            private bool _isFailed;

            private string? _outcome;

            private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

            private void FinishSpan(string outcome)
            {
                if (span == null)
                {
                    return;
                }

                span.SetTag("outcome", outcome);
                span.Finish(_isFailed ? SpanStatus.InternalError : SpanStatus.Ok);

                if (isTransaction)
                {
                    SentrySdk.ConfigureScope(scope =>
                    {
                        if (ReferenceEquals(scope.Transaction, span))
                        {
                            scope.Transaction = null;
                        }
                    });
                }
            }

            public void Dispose()
            {
                if (_isDisposed)
                {
                    return;
                }

                _isDisposed = true;

                _stopwatch.Stop();

                string outcome = _outcome ?? (_isFailed ? "Failed" : "Success");

                _attributes["outcome"] = outcome;
                _attributes["success"] = !_isFailed;

                telemetry.RecordDistribution($"{name}.duration", _stopwatch.Elapsed.TotalMilliseconds, MeasurementUnit.Duration.Millisecond, _attributes);

                EmitSafely(name, () => FinishSpan(outcome));
            }

            public void MarkFailed(string? outcome = null)
            {
                _isFailed = true;
                _outcome = outcome ?? _outcome;
            }

            public void SetOutcome(string outcome)
            {
                _outcome = outcome;
            }

            public void SetTag(string key, object value)
            {
                _attributes[key] = value;

                EmitSafely(key, () => span?.SetTag(key, value?.ToString() ?? string.Empty));
            }
        }
    }
}