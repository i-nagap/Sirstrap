namespace Sirstrap.Core.Cleaner
{
    public static class FolderRemover
    {
        private static void RemoveAfterClearingReadOnly(string folderPath)
        {
            try
            {
                foreach (string filePath in Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories))
                {
                    FileAttributes attributes = File.GetAttributes(filePath);

                    if (attributes.HasFlag(FileAttributes.ReadOnly))
                    {
                        File.SetAttributes(filePath, attributes & ~FileAttributes.ReadOnly);
                    }
                }

                Directory.Delete(folderPath, recursive: true);
                Log.Information("Deleted folder after removing its read-only protection (Path: {0}).", folderPath);
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
            {
                Log.Warning(ex, "Could not delete folder because it is in use by another program (Path: {0}).", folderPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not delete folder (Path: {0}).", folderPath);
            }
        }

        public static void Remove(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Log.Debug("Folder not found, nothing to delete (Path: {0}).", folderPath);

                return;
            }

            try
            {
                Directory.Delete(folderPath, recursive: true);
                Log.Information("Deleted folder (Path: {0}).", folderPath);
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
            {
                Log.Debug(ex, "Folder is locked or read-only, retrying after removing its read-only protection (Path: {0}).", folderPath);

                RemoveAfterClearingReadOnly(folderPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not delete folder (Path: {0}).", folderPath);
            }
        }
    }
}