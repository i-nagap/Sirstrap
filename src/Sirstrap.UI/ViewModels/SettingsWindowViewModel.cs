namespace Sirstrap.UI.ViewModels
{
    public partial class SettingsWindowViewModel : ViewModelBase
    {
        private const string SirHurtBootstrapperFileName = "bootstrapper.exe";

        private static readonly SettingsTabOption _fastFlagsTab = new(Strings.Instance.Get("Tab_FastFlags"), "FastFlags", IsPreview: true);

        private readonly AppAppearance _appearance;

        private readonly CleanupService _cleanupService;

        private readonly SirstrapDirectories _directories;

        private readonly SystemTray _systemTray;

        private readonly SirstrapUninstaller _uninstaller;

        private readonly SirstrapUpdater _updater;

        private readonly VersionSourceCatalog _versionSourceCatalog;

        public SettingsWindowViewModel(EditableSettings settings, SirstrapVersionInfo versionInfo, SirHurtAccount sirHurtAccount, SirstrapDirectories directories, SirstrapUpdater updater, CleanupService cleanupService, SirstrapUninstaller uninstaller, VersionSourceCatalog versionSourceCatalog, AppAppearance appearance, SystemTray systemTray)
        {
            Settings = settings;
            FastFlagsEditor = new FastFlagsEditor(settings);
            CurrentFullVersion = versionInfo.GetDisplayVersion();
            SirHurtPath = sirHurtAccount.GetInstallationDirectory();
            CanRunSirHurt = File.Exists(SirHurtBootstrapperPath);

            _directories = directories;
            _updater = updater;
            _cleanupService = cleanupService;
            _uninstaller = uninstaller;
            _versionSourceCatalog = versionSourceCatalog;
            _appearance = appearance;
            _systemTray = systemTray;

            SelectedSettingsTab = SettingsTabs[0];
            SelectedLanguage = Languages.FirstOrDefault(language => string.Equals(language.Value, settings.SirstrapLanguage, StringComparison.OrdinalIgnoreCase)) ?? Languages[0];

            LoadFontFamilies();

            _ = LoadChangelogAsync();
            _ = LoadVersionSourcesAsync();
        }

        [RelayCommand]
        private void AddFastFlag()
        {
            if (FastFlagsEditor.AddNewFlag())
            {
                SearchText = string.Empty;
            }
        }

        [RelayCommand]
        private async Task BrowseInstallationPathAsync()
        {
            try
            {
                if (MainWindow is not
                    {
                    } mainWindow)
                {
                    return;
                }

                IStorageProvider storageProvider = mainWindow.StorageProvider;
                IReadOnlyList<IStorageFolder> selectedFolders = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "Choose the Roblox installation folder",
                    AllowMultiple = false,
                    SuggestedStartLocation = await storageProvider.TryGetFolderFromPathAsync(Settings.RobloxInstallationPath)
                });

                if (selectedFolders.Count > 0)
                {
                    Settings.RobloxInstallationPath = selectedFolders[0].Path.LocalPath;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not open the folder picker for the Roblox installation path.");
            }
        }

        private async Task LoadChangelogAsync()
        {
            try
            {
                string changelog = (await _updater.GetLatestChangelogAsync()).Trim();

                _ = await Dispatcher.UIThread.InvokeAsync(() => Changelog = changelog);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not load the changelog.");
            }
        }

        private void LoadFontFamilies()
        {
            try
            {
                FontFamilies = [AppAppearance.BundledFontFamilyName, .. FontManager.Current.SystemFonts.Select(fontFamily => fontFamily.Name).Distinct().Order()];
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not load the installed fonts.");
            }
        }

        private async Task LoadVersionSourcesAsync()
        {
            IReadOnlyList<VersionSourceOption> options = await _versionSourceCatalog.LoadOptionsAsync();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                VersionSources = [.. options];
                SelectedVersionSource = VersionSourceCatalog.FindOrFirst(options, Settings.RobloxVersionSource);
            });
        }

        partial void OnSearchTextChanged(string value)
        {
            FastFlagsEditor.Filter(value);
        }

        partial void OnSelectedFastFlagsTabChanged(FastFlagsTab value)
        {
            if (value == FastFlagsTab.Raw)
            {
                FastFlagsEditor.RefreshRawJson();
            }
        }

        partial void OnSelectedLanguageChanged(LanguageOption value)
        {
            if (value is not null)
            {
                Settings.SirstrapLanguage = value.Value;
            }
        }

        partial void OnSelectedVersionSourceChanged(VersionSourceOption? value)
        {
            if (value is not null)
            {
                Settings.RobloxVersionSource = value.Value;
            }
        }

        [RelayCommand]
        private async Task OpenSettingsFileAsync()
        {
            try
            {
                string settingsFilePath = _directories.SettingsFile;

                await Task.Run(() =>
                {
                    if (!File.Exists(settingsFilePath))
                    {
                        File.Create(settingsFilePath).Dispose();
                    }

                    ShellLauncher.Open(settingsFilePath);
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not open the settings file.");
            }
        }

        [RelayCommand]
        private async Task RunCleanerAsync()
        {
            try
            {
                if (!await WarningDialog.ConfirmAsync("Run SirHurt Cleaner", "SirHurt Cleaner will:\n  • Close Roblox and SirHurt\n  • Delete the Roblox installation, its data and its registry entries\n  • Delete the Sirstrap data folder (%LocalAppData%\\Sirstrap)\n\nThis cannot be undone. Select Yes to continue."))
                {
                    return;
                }

                IsCleanerRunning = true;

                bool cleanTempFolders = Settings.CleanerCleanTempFolders;
                bool cleanProtectedFiles = Settings.CleanerCleanProtectedFiles;

                await Task.Run(() => _cleanupService.Run("manual", cleanTempFolders, cleanProtectedFiles));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not run SirHurt Cleaner.");
            }
            finally
            {
                IsCleanerRunning = false;
            }
        }

        [RelayCommand]
        private async Task RunSirHurtAsync()
        {
            try
            {
                string bootstrapperPath = SirHurtBootstrapperPath;

                await Task.Run(() =>
                {
                    if (File.Exists(bootstrapperPath))
                    {
                        ShellLauncher.Run(bootstrapperPath);
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not start SirHurt.");
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            try
            {
                await Settings.SaveAsync();

                _appearance.Apply();
                _systemTray.SetEnabled(Settings.SirstrapTrayMode != TrayMode.None);

                CloseOwnedWindow<SettingsWindow>();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not save the settings.");
            }
        }

        [RelayCommand]
        private async Task UninstallAsync()
        {
            try
            {
                if (!await WarningDialog.ConfirmAsync("Uninstall Sirstrap", "Uninstalling Sirstrap will:\n  • Stop Sirstrap from opening Roblox links\n  • Delete the Sirstrap data folder (%LocalAppData%\\Sirstrap)\n  • Delete the Sirstrap program\n\nThis cannot be undone. Select Yes to continue."))
                {
                    return;
                }

                await Task.Run(_uninstaller.Uninstall);

                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not uninstall Sirstrap.");
            }
        }

        private string SirHurtBootstrapperPath => Path.Combine(SirHurtPath, SirHurtBootstrapperFileName);

        public bool CanRunSirHurt { get; }

        [ObservableProperty]
        public partial string Changelog { get; set; } = string.Empty;

        public string CurrentFullVersion { get; }

        public FastFlagsEditor FastFlagsEditor { get; }

        public IReadOnlyList<FastFlagsTab> FastFlagsTabs { get; } = Enum.GetValues<FastFlagsTab>();

        [ObservableProperty]
        public partial ObservableCollection<string> FontFamilies { get; set; } = [];

        [ObservableProperty]
        public partial bool IsCleanerRunning { get; set; }

        public bool IsFastFlagsListVisible => IsFastFlagsTabSelected && SelectedFastFlagsTab == FastFlagsTab.List;

        public bool IsFastFlagsRawVisible => IsFastFlagsTabSelected && SelectedFastFlagsTab == FastFlagsTab.Raw;

        public bool IsFastFlagsTabSelected => SelectedSettingsTab == _fastFlagsTab;

        public bool IsSearchVisible => !IsFastFlagsRawVisible;

        public IReadOnlyList<LanguageOption> Languages { get; } = Strings.Languages;

        [ObservableProperty]
        public partial string SearchText { get; set; } = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsFastFlagsListVisible))]
        [NotifyPropertyChangedFor(nameof(IsFastFlagsRawVisible))]
        [NotifyPropertyChangedFor(nameof(IsSearchVisible))]
        public partial FastFlagsTab SelectedFastFlagsTab { get; set; } = FastFlagsTab.List;

        [ObservableProperty]
        public partial LanguageOption SelectedLanguage { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsFastFlagsTabSelected))]
        [NotifyPropertyChangedFor(nameof(IsFastFlagsListVisible))]
        [NotifyPropertyChangedFor(nameof(IsFastFlagsRawVisible))]
        [NotifyPropertyChangedFor(nameof(IsSearchVisible))]
        public partial SettingsTabOption SelectedSettingsTab { get; set; }

        [ObservableProperty]
        public partial VersionSourceOption? SelectedVersionSource { get; set; }

        public EditableSettings Settings { get; }

        public IReadOnlyList<SettingsTabOption> SettingsTabs { get; } =
        [
            new(Strings.Instance.Get("Tab_All"), null),
            _fastFlagsTab,
            new(Strings.Instance.Get("Tab_Roblox"), "Roblox"),
            new(Strings.Instance.Get("Tab_SirHurt"), "SirHurt"),
            new(Strings.Instance.Get("Tab_Sirstrap"), "Sirstrap")
        ];

        public string SirHurtPath { get; }

        public IReadOnlyList<TrayMode> TrayModes { get; } = Enum.GetValues<TrayMode>();

        [ObservableProperty]
        public partial ObservableCollection<VersionSourceOption> VersionSources { get; set; } = [];
    }
}