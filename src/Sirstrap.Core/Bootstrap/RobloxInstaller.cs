namespace Sirstrap.Core.Bootstrap
{
    public sealed class RobloxInstaller(RobloxVersionLayout layout, ITelemetry telemetry)
    {
        private static void Publish(string stagingDirectory, string versionDirectory, string retiredDirectory)
        {
            if (Directory.Exists(versionDirectory))
            {
                try
                {
                    ResilientFileSystem.MoveDirectory(versionDirectory, retiredDirectory);
                }
                catch (InvalidOperationException ex)
                {
                    Log.Information(ex, "This Roblox version is in use by another Roblox window, keeping the installed copy (Folder: {0}).", versionDirectory);
                    ResilientFileSystem.DeleteDirectory(stagingDirectory);

                    return;
                }
            }

            ResilientFileSystem.MoveDirectory(stagingDirectory, versionDirectory);

            try
            {
                ResilientFileSystem.DeleteDirectory(retiredDirectory);
            }
            catch (InvalidOperationException ex)
            {
                Log.Debug(ex, "Could not delete the replaced Roblox folder, it will be removed later (Folder: {0}).", retiredDirectory);
            }
        }

        public void Install(RobloxDeploymentTarget target)
        {
            using ITelemetryOperation operation = telemetry.StartOperation("install", new Dictionary<string, object>
            {
                ["binaryType"] = target.BinaryType.ToString()
            });

            try
            {
                string versionDirectory = layout.GetVersionDirectory(target.VersionHash);
                string stagingDirectory = layout.GetStagingDirectory(target.VersionHash);
                string archivePath = layout.GetDownloadArchivePath(target.VersionHash);

                Log.Information("Installing Roblox (Folder: {0}).", versionDirectory);
                ResilientFileSystem.DeleteDirectory(stagingDirectory);

                int extractedFileCount;

                try
                {
                    using ZipArchive archive = ZipFile.OpenRead(archivePath);

                    extractedFileCount = archive.Entries.Count(entry => !string.IsNullOrEmpty(entry.Name));

                    archive.ExtractToDirectory(stagingDirectory, overwriteFiles: true);
                }
                finally
                {
                    ResilientFileSystem.DeleteFile(archivePath);
                }

                Publish(stagingDirectory, versionDirectory, layout.GetRetiredDirectory(target.VersionHash));

                operation.SetTag("entryCount", extractedFileCount);

                Log.Information("Installed Roblox (Folder: {0}, Files: {1}).", versionDirectory, extractedFileCount);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not install Roblox (Version: {0}).", target.VersionHash);

                operation.MarkFailed(ex.GetType().Name);

                throw new InvalidOperationException("Could not install Roblox.", ex);
            }
        }
    }
}