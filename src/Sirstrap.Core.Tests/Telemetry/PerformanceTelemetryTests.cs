namespace Sirstrap.Core.Tests.Telemetry
{
    public class PerformanceTelemetryTests
    {
        [Fact]
        public void NullPerformanceTelemetry_IsSingleton_AndScopeIsUsable()
        {
            Assert.Same(NullPerformanceTelemetry.Instance, NullPerformanceTelemetry.Instance);

            var exception = Record.Exception(() =>
            {
                NullPerformanceTelemetry.Instance.RecordCounter("counter");
                NullPerformanceTelemetry.Instance.RecordDistribution("op.duration", 1, "millisecond");
                NullPerformanceTelemetry.Instance.SetTag("k", "v");
                NullPerformanceTelemetry.Instance.SetContext("c", new Dictionary<string, object> { ["a"] = 1 });

                using ITelemetryScope scope = NullPerformanceTelemetry.Instance.Measure("op");
                scope.SetTag("k", "v");
                scope.SetOutcome("Cached");
                scope.MarkFailed("Boom");
            });

            Assert.Null(exception);
        }

        [Fact]
        public void SentryPerformanceTelemetry_DoesNotThrow_WhenSentryNotInitialized()
        {
            SentryPerformanceTelemetry telemetry = new();

            var exception = Record.Exception(() =>
            {
                telemetry.RecordCounter("counter", new Dictionary<string, object> { ["a"] = 1 });
                telemetry.RecordDistribution("op.bytes", 5, "byte", new Dictionary<string, object> { ["a"] = 1 });
                telemetry.SetTag("k", "v");
                telemetry.SetContext("c", new Dictionary<string, object> { ["a"] = 1 });

                using ITelemetryScope scope = telemetry.Measure("op", new Dictionary<string, object> { ["a"] = 1 });
                scope.SetTag("k", 42);
                scope.MarkFailed("Boom");
                scope.Dispose();
                scope.Dispose();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void SentryPerformanceTelemetry_Measure_ReturnsScope_WithoutTags()
        {
            SentryPerformanceTelemetry telemetry = new();

            using ITelemetryScope scope = telemetry.Measure("op");

            Assert.NotNull(scope);
        }

        [Theory]
        [InlineData("-beta", "beta")]
        [InlineData("-Alpha", "alpha")]
        [InlineData("", "production")]
        [InlineData(null, "production")]
        public void GetEnvironment_MapsTheChannel(string? channel, string expected)
            => Assert.Equal(expected, SentryLoggerConfigurationExtensions.GetEnvironment(channel));
    }
}
