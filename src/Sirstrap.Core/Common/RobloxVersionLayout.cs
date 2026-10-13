namespace Sirstrap.Core.Common
{
    public sealed class RobloxVersionLayout(SirstrapSettings settings, InstanceWorkspace workspace)
    {
        private const string PlayerExecutableName = "RobloxPlayerBeta.exe";

        public const string RetiredSuffix = ".retired";

        public const string StagingSuffix = ".staging";

        public string GetDownloadArchivePath(string versionHash)
        {
            _ = Directory.CreateDirectory(workspace.Downloads);

            return Path.Combine(workspace.Downloads, $"{versionHash}.zip");
        }

        public string GetPlayerExecutablePath(string versionHash)
        {
            return Path.Combine(GetVersionDirectory(versionHash), PlayerExecutableName);
        }

        public string GetRetiredDirectory(string versionHash)
        {
            return $"{GetVersionDirectory(versionHash)}.{workspace.Id}{RetiredSuffix}";
        }

        public string GetStagingDirectory(string versionHash)
        {
            return $"{GetVersionDirectory(versionHash)}.{workspace.Id}{StagingSuffix}";
        }

        public string GetVersionDirectory(string versionHash)
        {
            return Path.Combine(settings.RobloxInstallationPath, versionHash);
        }
    }
}