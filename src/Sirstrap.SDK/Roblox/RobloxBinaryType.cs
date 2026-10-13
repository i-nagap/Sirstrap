namespace Sirstrap.SDK.Roblox
{
    public enum RobloxBinaryType
    {
        WindowsPlayer,

        WindowsStudio64,

        MacPlayer,

        MacStudio
    }

    public static class RobloxBinaryTypeExtensions
    {
        public static string GetDefaultBlobDirectory(this RobloxBinaryType binaryType)
        {
            return binaryType.IsMac() ? "/mac/" : "/";
        }

        public static string GetMacArchiveName(this RobloxBinaryType binaryType)
        {
            return binaryType == RobloxBinaryType.MacPlayer ? "RobloxPlayer.zip" : "RobloxStudioApp.zip";
        }

        public static bool IsMac(this RobloxBinaryType binaryType)
        {
            return binaryType is RobloxBinaryType.MacPlayer or RobloxBinaryType.MacStudio;
        }
    }
}