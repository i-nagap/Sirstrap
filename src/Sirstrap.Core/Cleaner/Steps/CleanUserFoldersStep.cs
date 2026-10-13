namespace Sirstrap.Core.Cleaner.Steps
{
    public sealed class CleanUserFoldersStep(SelectiveFolderCleaner selectiveFolderCleaner, UserProfileLocator userProfileLocator, CleanerOptions options) : ICleanupStep
    {
        private void CleanProfile(string profilePath)
        {
            foreach (string userFolder in options.UserFolders)
            {
                try
                {
                    selectiveFolderCleaner.Clean(Path.GetFullPath(Path.Combine(profilePath, userFolder)));
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Could not clean folder (Folder: {0}, Profile: {1}).", userFolder, profilePath);
                }
            }
        }

        public void Execute()
        {
            Log.Information("Cleaning the app data of the current user (User: {0}).", Environment.UserName);

            CleanProfile(userProfileLocator.CurrentUserProfile);

            foreach (string userProfile in userProfileLocator.GetOtherUserProfiles())
            {
                Log.Information("Cleaning the app data of another user (User: {0}).", Path.GetFileName(userProfile));

                try
                {
                    CleanProfile(userProfile);
                }
                catch (UnauthorizedAccessException ex)
                {
                    Log.Warning(ex, "Could not clean the app data of another user because access was denied (Profile: {0}).", userProfile);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Could not clean the app data of another user (Profile: {0}).", userProfile);
                }
            }
        }

        public string Name => "Clean user folders";
    }
}