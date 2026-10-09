namespace Sirstrap.Core.Cleaner
{
    public sealed class CleanupService(CleanerConfig cleanerConfig, ICleanupOrchestrator cleanupOrchestrator, IPerformanceTelemetry performanceTelemetry) : ICleanupService
    {
        public void Run(string trigger, bool cleanTempFolders, bool cleanProtectedFiles)
        {
            using ITelemetryScope scope = performanceTelemetry.Measure("cleaner.run", new Dictionary<string, object>
            {
                ["trigger"] = trigger,
                ["cleanTempFolders"] = cleanTempFolders,
                ["cleanProtectedFiles"] = cleanProtectedFiles
            });

            try
            {
                cleanerConfig.CleanTempFolders = cleanTempFolders;
                cleanerConfig.CleanProtectedFiles = cleanProtectedFiles;

                cleanupOrchestrator.Run();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to run the {Trigger} cleanup.", trigger);

                scope.MarkFailed(ex.GetType().Name);
            }
        }
    }
}
