namespace Sirstrap.SDK.Roblox
{
    public static class RobloxFolders
    {
        public static string LocalData { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Roblox");

        public static string Logs { get; } = Path.Combine(LocalData, "logs");
    }
}