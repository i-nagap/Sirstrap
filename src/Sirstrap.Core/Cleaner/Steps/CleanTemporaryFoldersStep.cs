namespace Sirstrap.Core.Cleaner.Steps
{
    public sealed class CleanTemporaryFoldersStep(UserProfileLocator userProfileLocator, CleanerOptions options) : ICleanupStep
    {
        private static void TryDeleteFile(string filePath)
        {
            try
            {
                File.Delete(filePath);
            }
            catch (Exception ex)
            {
                Log.Debug(ex, "Skipped temporary file, it is probably in use (Path: {0}).", filePath);
            }
        }

        public static void CleanTemporaryFolder(string temporaryFolder)
        {
            try
            {
                if (!Directory.Exists(temporaryFolder))
                {
                    Log.Debug("Temporary folder not found, nothing to empty (Path: {0}).", temporaryFolder);

                    return;
                }

                foreach (string filePath in Directory.EnumerateFiles(temporaryFolder))
                {
                    TryDeleteFile(filePath);
                }

                foreach (string folderPath in Directory.EnumerateDirectories(temporaryFolder))
                {
                    FolderRemover.Remove(folderPath);
                }

                int remainingEntryCount = Directory.EnumerateFileSystemEntries(temporaryFolder).Count();

                if (remainingEntryCount == 0)
                {
                    Log.Information("Emptied temporary folder (Path: {0}).", temporaryFolder);
                }
                else
                {
                    Log.Information("Partly emptied temporary folder, some items are in use by other programs (Path: {0}, Items left: {1}).", temporaryFolder, remainingEntryCount);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Warning(ex, "Could not empty temporary folder because access was denied (Path: {0}).", temporaryFolder);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not empty temporary folder (Path: {0}).", temporaryFolder);
            }
        }

        public void Execute()
        {
            if (!options.CleanTempFolders)
            {
                Log.Information("Skipped, emptying temporary folders is turned off.");

                return;
            }

            string currentUserTempFolder = Path.Combine(userProfileLocator.CurrentUserProfile, options.UserTempFolder);

            Log.Information("Emptying the temporary folder of the current user (User: {0}, Path: {1}).", Environment.UserName, currentUserTempFolder);

            CleanTemporaryFolder(currentUserTempFolder);

            foreach (string userProfile in userProfileLocator.GetOtherUserProfiles())
            {
                string userTempFolder = Path.Combine(userProfile, options.UserTempFolder);

                if (!Directory.Exists(userTempFolder))
                {
                    continue;
                }

                Log.Information("Emptying the temporary folder of another user (User: {0}, Path: {1}).", Path.GetFileName(userProfile), userTempFolder);

                CleanTemporaryFolder(userTempFolder);
            }

            string systemTempFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp");

            Log.Information("Emptying the Windows temporary folder (Path: {0}).", systemTempFolder);

            CleanTemporaryFolder(systemTempFolder);
        }

        public string Name => "Clean temporary folders";
    }
}