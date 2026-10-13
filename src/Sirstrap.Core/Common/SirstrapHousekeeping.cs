namespace Sirstrap.Core.Common
{
    public sealed class SirstrapHousekeeping(SirstrapDirectories directories, InstanceWorkspace workspace, SirstrapSettings settings, SettingsStore settingsStore)
    {
        private const int RetainedInstanceCount = 20;

        private static void TryDelete(string path)
        {
            try
            {
                ResilientFileSystem.DeleteDirectory(path);
            }
            catch (Exception ex)
            {
                Log.Debug(ex, "Could not delete a leftover folder (Path: {0}).", path);
            }
        }

        public void PruneInstances()
        {
            try
            {
                if (!Directory.Exists(directories.Instances))
                {
                    return;
                }

                List<DirectoryInfo> finishedNewestFirst = [.. new DirectoryInfo(directories.Instances).GetDirectories().Where(instance => instance.Name != workspace.Id && !InstanceWorkspace.IsAlive(directories, instance.Name)).OrderByDescending(instance => instance.Name, StringComparer.Ordinal)];

                foreach (DirectoryInfo instance in finishedNewestFirst.Take(RetainedInstanceCount))
                {
                    TryDelete(Path.Combine(instance.FullName, "Downloads"));
                }

                foreach (DirectoryInfo instance in finishedNewestFirst.Skip(RetainedInstanceCount))
                {
                    TryDelete(instance.FullName);
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not clean up the folders of closed Sirstrap instances (Folder: {0}).", directories.Instances);
            }
        }

        public void RemoveAbandonedInstallFolders()
        {
            try
            {
                if (!Directory.Exists(settings.RobloxInstallationPath))
                {
                    return;
                }

                foreach (string folder in Directory.EnumerateDirectories(settings.RobloxInstallationPath))
                {
                    string name = Path.GetFileName(folder);
                    string? suffix = new[]
                    {
                        RobloxVersionLayout.StagingSuffix,
                        RobloxVersionLayout.RetiredSuffix
                    }.FirstOrDefault(candidate => name.EndsWith(candidate, StringComparison.OrdinalIgnoreCase));

                    if (suffix == null)
                    {
                        continue;
                    }

                    string ownerId = Path.GetExtension(name[..^suffix.Length]).TrimStart('.');

                    if (ownerId != workspace.Id && !InstanceWorkspace.IsAlive(directories, ownerId))
                    {
                        TryDelete(folder);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not clean up unfinished Roblox installations (Folder: {0}).", settings.RobloxInstallationPath);
            }
        }

        public void RemovePreviousRobloxInstallation()
        {
            try
            {
                string previousPath = settings.RobloxPreviousInstallationPath;

                if (string.IsNullOrWhiteSpace(previousPath) || !Directory.Exists(previousPath) || string.Equals(previousPath, settings.RobloxInstallationPath, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                ResilientFileSystem.DeleteDirectory(previousPath);

                settings.RobloxPreviousInstallationPath = string.Empty;

                settingsStore.Save();

                Log.Information("Deleted the previous Roblox installation folder (Path: {0}).", previousPath);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not delete the previous Roblox installation folder (Path: {0}).", settings.RobloxPreviousInstallationPath);
            }
        }
    }
}