namespace Sirstrap.Core.Tests.Cleaner
{
    public class CleanupServiceTests
    {
        [Fact]
        public void Run_AppliesTheRequestedFlags_BeforeOrchestrating()
        {
            CleanerConfig config = new();
            bool? cleanTempFoldersDuringRun = null;
            bool? cleanProtectedFilesDuringRun = null;

            FakeCleanupOrchestrator orchestrator = new(() =>
            {
                cleanTempFoldersDuringRun = config.CleanTempFolders;
                cleanProtectedFilesDuringRun = config.CleanProtectedFiles;
            });

            CleanupService service = new(config, orchestrator, NullPerformanceTelemetry.Instance);

            service.Run("manual", cleanTempFolders: false, cleanProtectedFiles: true);

            Assert.Equal(1, orchestrator.Runs);
            Assert.False(cleanTempFoldersDuringRun);
            Assert.True(cleanProtectedFilesDuringRun);
        }

        [Fact]
        public void Run_MeasuresTheOperation_TaggedWithTheTrigger()
        {
            RecordingPerformanceTelemetry telemetry = new();
            CleanupService service = new(new CleanerConfig(), new FakeCleanupOrchestrator(), telemetry);

            service.Run("launch", cleanTempFolders: true, cleanProtectedFiles: false);

            var scope = Assert.Single(telemetry.Scopes);

            Assert.Equal("cleaner.run", scope.Operation);
            Assert.NotNull(scope.Tags);
            Assert.Equal("launch", Assert.Contains("trigger", scope.Tags));
            Assert.False(scope.Failed);
            Assert.True(scope.Disposed);
        }

        [Fact]
        public void Run_MarksTheScopeFailed_WhenTheOrchestratorThrows()
        {
            RecordingPerformanceTelemetry telemetry = new();
            FakeCleanupOrchestrator orchestrator = new(() => throw new InvalidOperationException("boom"));
            CleanupService service = new(new CleanerConfig(), orchestrator, telemetry);

            Assert.Null(Record.Exception(() => service.Run("exit", cleanTempFolders: true, cleanProtectedFiles: false)));

            var scope = Assert.Single(telemetry.Scopes);

            Assert.True(scope.Failed);
            Assert.True(scope.Disposed);
        }
    }
}
