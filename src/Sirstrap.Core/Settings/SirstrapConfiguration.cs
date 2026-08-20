namespace Sirstrap.Core.Settings
{
    public class SirstrapConfiguration
    {
        public static string GetDefaultInstallationPath() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap", "Versions");

        public bool CleanerCleanOnExit { get; set; }

        public bool CleanerCleanOnLaunch { get; set; }

        public bool CleanerCleanProtectedFiles { get; set; }

        public bool CleanerCleanTempFolders { get; set; } = true;

        public bool CleanerEnabled { get; set; }

        public string SirstrapAccentColor { get; set; } = "#454ee6";

        public bool SirstrapAutoUpdate { get; set; } = true;

        public string SirstrapChannel { get; set; } = "-beta";

        public string SirstrapFontFamily { get; set; } = "JetBrains Mono";

        public string SirstrapLanguage { get; set; } = string.Empty;

        public bool SirstrapTelemetry { get; set; } = true;

        public TrayMode SirstrapTrayMode { get; set; } = TrayMode.None;

        public bool RobloxFastFlagsEnabled { get; set; } = true;

        public bool RobloxIncognito { get; set; }

        public string RobloxInstallationPath { get; set; } = GetDefaultInstallationPath();

        public bool RobloxMultiInstance { get; set; } = true;

        public string RobloxPreviousInstallationPath { get; set; } = string.Empty;

        public string RobloxVersionOverride { get; set; } = string.Empty;

        public string RobloxVersionSource { get; set; } = RobloxVersionSources.SirHurt;

        public string RobloxCdnUriOverride { get; set; } = string.Empty;

        public string ResolvedRobloxCdnUri { get; set; } = RobloxCdnService.DefaultBaseUri;

        public IReadOnlyList<string> ResolvedRobloxCdnUris { get; set; } = [RobloxCdnService.DefaultBaseUri];
    }
}
