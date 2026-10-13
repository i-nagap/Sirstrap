namespace Sirstrap.Core.Cleaner
{
    public sealed class UserProfileLocator(string usersDirectory, string currentUserProfile)
    {
        private static readonly string[] _systemProfileNames = ["Public", "Default", "Default User", "All Users"];

        public static UserProfileLocator ForThisMachine()
        {
            string systemDrive = Environment.GetEnvironmentVariable("SystemDrive") ?? "C:";

            return new(Path.Combine(systemDrive + Path.DirectorySeparatorChar, "Users"), Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        }

        public IEnumerable<string> GetOtherUserProfiles()
        {
            return !Directory.Exists(usersDirectory) ? [] : Directory.EnumerateDirectories(usersDirectory).Where(profile => !profile.Equals(CurrentUserProfile, StringComparison.OrdinalIgnoreCase) && !_systemProfileNames.Contains(Path.GetFileName(profile), StringComparer.OrdinalIgnoreCase));
        }

        public string CurrentUserProfile { get; } = currentUserProfile;
    }
}