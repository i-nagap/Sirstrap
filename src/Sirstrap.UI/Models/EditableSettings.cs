namespace Sirstrap.UI.Models
{
    public partial class EditableSettings : ObservableObject
    {
        private readonly FastFlagsService _fastFlagsService;

        private readonly SirstrapHousekeeping _housekeeping;

        private readonly SirstrapSettings _settings;

        private readonly SettingsStore _settingsStore;

        public EditableSettings(SirstrapSettings settings, SettingsStore settingsStore, FastFlagsService fastFlagsService, SirstrapHousekeeping housekeeping)
        {
            _settings = settings;
            _settingsStore = settingsStore;
            _fastFlagsService = fastFlagsService;
            _housekeeping = housekeeping;

            _settingsStore.Load();

            CopyFromSettings();
        }

        private void CopyFromSettings()
        {
            if (Color.TryParse(_settings.SirstrapAccentColor, out Color accentColor))
            {
                SirstrapAccentColorValue = accentColor;
            }

            CleanerCleanOnExit = _settings.CleanerCleanOnExit;
            CleanerCleanOnLaunch = _settings.CleanerCleanOnLaunch;
            CleanerCleanProtectedFiles = _settings.CleanerCleanProtectedFiles;
            CleanerCleanTempFolders = _settings.CleanerCleanTempFolders;
            CleanerEnabled = _settings.CleanerEnabled;
            RobloxCdnUriOverride = _settings.RobloxCdnUriOverride;
            RobloxFastFlags = FastFlagEntry.FromDictionary(_fastFlagsService.GetFlags());
            RobloxFastFlagsEnabled = _settings.RobloxFastFlagsEnabled;
            RobloxIncognito = _settings.RobloxIncognito && _settings.RobloxMultiInstance;
            RobloxInstallationPath = _settings.RobloxInstallationPath;
            RobloxMultiInstance = _settings.RobloxMultiInstance;
            RobloxVersionOverride = _settings.RobloxVersionOverride;
            RobloxVersionSource = _settings.RobloxVersionSource;
            SirstrapAutoUpdate = _settings.SirstrapAutoUpdate;
            SirstrapChannel = _settings.SirstrapChannel;
            SirstrapFontFamily = _settings.SirstrapFontFamily;
            SirstrapLanguage = _settings.SirstrapLanguage;
            SirstrapTelemetry = _settings.SirstrapTelemetry;
            SirstrapTrayMode = _settings.SirstrapTrayMode;
        }

        private void CopyToSettings()
        {
            if (!string.Equals(_settings.RobloxInstallationPath, RobloxInstallationPath, StringComparison.OrdinalIgnoreCase))
            {
                _settings.RobloxPreviousInstallationPath = _settings.RobloxInstallationPath;
            }

            RobloxVersionOverride = RobloxVersionOverride.Trim();
            RobloxCdnUriOverride = RobloxCdn.NormalizeBaseUri(RobloxCdnUriOverride);

            _settings.CleanerCleanOnExit = CleanerCleanOnExit;
            _settings.CleanerCleanOnLaunch = CleanerCleanOnLaunch;
            _settings.CleanerCleanProtectedFiles = CleanerCleanProtectedFiles;
            _settings.CleanerCleanTempFolders = CleanerCleanTempFolders;
            _settings.CleanerEnabled = CleanerEnabled;
            _settings.RobloxCdnUriOverride = RobloxCdnUriOverride;
            _settings.RobloxFastFlagsEnabled = RobloxFastFlagsEnabled;
            _settings.RobloxIncognito = RobloxIncognito;
            _settings.RobloxInstallationPath = RobloxInstallationPath;
            _settings.RobloxMultiInstance = RobloxMultiInstance;
            _settings.RobloxVersionOverride = RobloxVersionOverride;
            _settings.RobloxVersionSource = RobloxVersionSource;
            _settings.SirstrapAccentColor = $"#{SirstrapAccentColorValue.R:x2}{SirstrapAccentColorValue.G:x2}{SirstrapAccentColorValue.B:x2}";
            _settings.SirstrapAutoUpdate = SirstrapAutoUpdate;
            _settings.SirstrapChannel = SirstrapChannel;
            _settings.SirstrapFontFamily = SirstrapFontFamily;
            _settings.SirstrapLanguage = SirstrapLanguage;
            _settings.SirstrapTelemetry = SirstrapTelemetry;
            _settings.SirstrapTrayMode = SirstrapTrayMode;
        }

        partial void OnRobloxMultiInstanceChanged(bool value)
        {
            if (!value)
            {
                RobloxIncognito = false;
            }
        }

        public async Task SaveAsync()
        {
            CopyToSettings();

            Dictionary<string, string> fastFlags = FastFlagEntry.ToDictionary(RobloxFastFlags);

            await Task.Run(() =>
            {
                _fastFlagsService.SaveFlags(fastFlags);
                _settingsStore.Save();
                _settingsStore.Load();
                _housekeeping.RemovePreviousRobloxInstallation();
            });
        }

        [ObservableProperty]
        public partial bool CleanerCleanOnExit { get; set; }

        [ObservableProperty]
        public partial bool CleanerCleanOnLaunch { get; set; }

        [ObservableProperty]
        public partial bool CleanerCleanProtectedFiles { get; set; }

        [ObservableProperty]
        public partial bool CleanerCleanTempFolders { get; set; }

        [ObservableProperty]
        public partial bool CleanerEnabled { get; set; }

        [ObservableProperty]
        public partial string RobloxCdnUriOverride { get; set; } = string.Empty;

        [ObservableProperty]
        public partial ObservableCollection<FastFlagEntry> RobloxFastFlags { get; set; } = [];

        [ObservableProperty]
        public partial bool RobloxFastFlagsEnabled { get; set; }

        [ObservableProperty]
        public partial bool RobloxIncognito { get; set; }

        [ObservableProperty]
        public partial string RobloxInstallationPath { get; set; } = string.Empty;

        [ObservableProperty]
        public partial bool RobloxMultiInstance { get; set; }

        [ObservableProperty]
        public partial string RobloxVersionOverride { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string RobloxVersionSource { get; set; } = string.Empty;

        [ObservableProperty]
        public partial Color SirstrapAccentColorValue { get; set; } = Color.Parse("#454ee6");

        [ObservableProperty]
        public partial bool SirstrapAutoUpdate { get; set; }

        [ObservableProperty]
        public partial string SirstrapChannel { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string SirstrapFontFamily { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string SirstrapLanguage { get; set; } = string.Empty;

        [ObservableProperty]
        public partial bool SirstrapTelemetry { get; set; }

        [ObservableProperty]
        public partial TrayMode SirstrapTrayMode { get; set; }
    }
}