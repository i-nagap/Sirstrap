namespace Sirstrap.Core.Cleaner
{
    public sealed class CleanupService(CleanerOptions options, CleanupRunner runner, ITelemetry telemetry)
    {
        public void Run(string trigger, bool cleanTempFolders, bool cleanProtectedFiles)
        {
            using ITelemetryOperation operation = telemetry.StartOperation("cleaner.run", new Dictionary<string, object>
            {
                ["trigger"] = trigger,
                ["cleanTempFolders"] = cleanTempFolders,
                ["cleanProtectedFiles"] = cleanProtectedFiles
            });

            try
            {
                options.CleanTempFolders = cleanTempFolders;
                options.CleanProtectedFiles = cleanProtectedFiles;

                runner.Run();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not run the cleanup (Trigger: {0}).", trigger);

                operation.MarkFailed(ex.GetType().Name);
            }
        }
    }
}