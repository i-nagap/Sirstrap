namespace Sirstrap.Core.Cleaner
{
    public sealed class SelectiveFolderCleaner(IUserPrompt userPrompt, CleanerOptions options)
    {
        private readonly HashSet<string> _foldersHoldingProtectedFiles = options.FilesRequiringConfirmation.Select(Path.GetDirectoryName).OfType<string>().Where(directory => directory.Length > 0).ToHashSet(StringComparer.Ordinal);

        private void CleanSubFolder(string subFolderPath, string rootFolderPath)
        {
            string subFolderName = Path.GetFileName(subFolderPath);

            if (options.ExcludedSubFolderNames.Contains(subFolderName) || IsRobloxStudioBuildFolder(subFolderName))
            {
                Log.Debug("Keeping excluded folder (Path: {0}).", subFolderPath);

                return;
            }

            if (options.FoldersRequiringConfirmation.Contains(Path.GetRelativePath(rootFolderPath, subFolderPath)))
            {
                DeleteFolderAfterConfirmation(subFolderPath);

                return;
            }

            if (!_foldersHoldingProtectedFiles.Contains(subFolderName))
            {
                FolderRemover.Remove(subFolderPath);

                return;
            }

            foreach (string filePath in Directory.EnumerateFiles(subFolderPath))
            {
                DeleteFile(filePath, rootFolderPath);
            }

            DeleteFolderIfEmpty(subFolderPath);
        }

        private void DeleteFile(string filePath, string rootFolderPath)
        {
            try
            {
                if (!options.FilesRequiringConfirmation.Contains(Path.GetRelativePath(rootFolderPath, filePath)))
                {
                    File.Delete(filePath);
                    Log.Debug("Deleted file (Path: {0}).", filePath);

                    return;
                }

                if (!userPrompt.Confirm(string.Format(CultureInfo.InvariantCulture, "The file {0} contains login or configuration data and will be deleted.", filePath), options.CleanProtectedFiles))
                {
                    Log.Information("Kept file with login or configuration data (Path: {0}).", filePath);

                    return;
                }

                File.Delete(filePath);
                Log.Information("Deleted file with login or configuration data (Path: {0}).", filePath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not delete file (Path: {0}).", filePath);
            }
        }

        private void DeleteFolderAfterConfirmation(string folderPath)
        {
            if (!userPrompt.Confirm(string.Format(CultureInfo.InvariantCulture, "The folder {0} contains login or configuration data and will be deleted.", folderPath), options.CleanProtectedFiles))
            {
                Log.Information("Kept folder with login or configuration data (Path: {0}).", folderPath);

                return;
            }

            FolderRemover.Remove(folderPath);
        }

        private static void DeleteFolderIfEmpty(string folderPath)
        {
            if (!Directory.Exists(folderPath) || Directory.EnumerateFileSystemEntries(folderPath).Any())
            {
                return;
            }

            try
            {
                Directory.Delete(folderPath, recursive: false);
                Log.Debug("Deleted empty folder (Path: {0}).", folderPath);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not delete empty folder (Path: {0}).", folderPath);
            }
        }

        private static bool IsRobloxStudioBuildFolder(string folderName)
        {
            return folderName.Length > 0 && folderName.All(char.IsDigit);
        }

        public void Clean(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Log.Debug("Folder not found, nothing to clean (Path: {0}).", folderPath);

                return;
            }

            Log.Information("Cleaning folder (Path: {0}).", folderPath);

            try
            {
                foreach (string filePath in Directory.EnumerateFiles(folderPath))
                {
                    if (options.ExcludedFileNames.Contains(Path.GetFileName(filePath)))
                    {
                        Log.Debug("Keeping excluded file (Path: {0}).", filePath);

                        continue;
                    }

                    DeleteFile(filePath, folderPath);
                }

                foreach (string subFolderPath in Directory.EnumerateDirectories(folderPath))
                {
                    CleanSubFolder(subFolderPath, folderPath);
                }

                DeleteFolderIfEmpty(folderPath);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Warning(ex, "Could not clean folder because access was denied (Path: {0}).", folderPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not clean folder (Path: {0}).", folderPath);
            }
        }
    }
}