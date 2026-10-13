namespace Sirstrap.Core.Cleaner.Steps
{
    public sealed class RemoveSystemFoldersStep(CleanerOptions options) : ICleanupStep
    {
        public void Execute()
        {
            Log.Information("Removing the system folders (Folders: {0}).", options.SystemFolders.Count);

            foreach (string folderPath in options.SystemFolders)
            {
                FolderRemover.Remove(folderPath);
            }
        }

        public string Name => "Clean system folders";
    }
}