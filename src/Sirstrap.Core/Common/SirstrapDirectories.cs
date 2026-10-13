namespace Sirstrap.Core.Common
{
    public sealed class SirstrapDirectories(string root)
    {
        public static SirstrapDirectories ForCurrentUser()
        {
            return new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap"));
        }

        public string Config => Path.Combine(Root, "Config");

        public string DefaultRobloxVersions => Path.Combine(Root, "Roblox", "Versions");

        public string FastFlagsFile => Path.Combine(Config, "FastFlags.json");

        public string IncognitoBackup => Path.Combine(Root, "Roblox", "IncognitoBackup");

        public string Instances => Path.Combine(Root, "Instances");

        public string Root { get; } = root;

        public string SettingsFile => Path.Combine(Config, "Settings.ini");
    }
}