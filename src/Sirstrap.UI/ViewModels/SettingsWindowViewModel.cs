namespace Sirstrap.UI.ViewModels
{
    public partial class SettingsWindowViewModel : ViewModelBase
    {
#pragma warning disable S1075 // fixed Sirstrap project endpoint, not a deployment specific path
        private const string ANNOUNCEMENTS_URI = "https://raw.githubusercontent.com/massimopaganigh/Sirstrap/main/announcements.txt";
#pragma warning restore S1075

        private static readonly SettingsTabOption _fastFlagsTab = new("FastFlags", "FastFlags", IsPreview: true);

        private readonly HttpClient _httpClient;
        private readonly ICleanupService _cleanupService;
        private readonly IFastFlagService _fastFlagService;
        private readonly IUninstallService _uninstallService;
        private readonly IWeaoService _weaoService;

        private bool _isRefreshingRawText;

        [ObservableProperty]
        private string _announcements = string.Empty;

        [ObservableProperty]
        private string _currentFullVersion;

        [ObservableProperty]
        private ObservableCollection<FastFlagEntry> _fastFlags = [];

        [ObservableProperty]
        private ObservableCollection<string> _fontFamilies = [];

        [ObservableProperty]
        private string _newFastFlagName = string.Empty;

        [ObservableProperty]
        private string _newFastFlagValue = string.Empty;

        [ObservableProperty]
        private string _rawText = string.Empty;

        [ObservableProperty]
        private ObservableCollection<VersionSourceOption> _versionSources = [];

        [ObservableProperty]
        private VersionSourceOption? _selectedVersionSource;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsFastFlagsListVisible))]
        [NotifyPropertyChangedFor(nameof(IsFastFlagsRawVisible))]
        [NotifyPropertyChangedFor(nameof(IsSearchVisible))]
        private FastFlagsTab _selectedFastFlagsTab = FastFlagsTab.List;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsFastFlagsTabSelected))]
        [NotifyPropertyChangedFor(nameof(IsFastFlagsListVisible))]
        [NotifyPropertyChangedFor(nameof(IsFastFlagsRawVisible))]
        [NotifyPropertyChangedFor(nameof(IsSearchVisible))]
        private SettingsTabOption _selectedSettingsTab;

        [ObservableProperty]
        private Settings _settings;

        [ObservableProperty]
        private bool _isCleanerRunning;

        public SettingsWindowViewModel(HttpClient httpClient, Settings settings, ISirstrapVersion sirstrapVersion, IUninstallService uninstallService, IWeaoService weaoService, ICleanupService cleanupService, IFastFlagService fastFlagService)
        {
            _httpClient = httpClient;
            _settings = settings;
            _currentFullVersion = sirstrapVersion.GetFullVersion();
            _uninstallService = uninstallService;
            _weaoService = weaoService;
            _cleanupService = cleanupService;
            _fastFlagService = fastFlagService;
            _selectedSettingsTab = SettingsTabs[0];

            GetFontFamilies();
            ApplyFastFlagsSearch();

            _ = LoadAnnouncementsAsync();
            _ = LoadVersionSourcesAsync();
        }

        public bool IsFastFlagsTabSelected => SelectedSettingsTab == _fastFlagsTab;

        public bool IsFastFlagsListVisible => IsFastFlagsTabSelected && SelectedFastFlagsTab == FastFlagsTab.List;

        public bool IsFastFlagsRawVisible => IsFastFlagsTabSelected && SelectedFastFlagsTab == FastFlagsTab.Raw;

        public bool IsSearchVisible => !IsFastFlagsRawVisible;

        private async Task LoadAnnouncementsAsync()
        {
            try
            {
                var announcements = (await _httpClient.GetStringAsync(ANNOUNCEMENTS_URI)).Trim();

                await Dispatcher.UIThread.InvokeAsync(() => Announcements = announcements);
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(LoadAnnouncementsAsync));
            }
        }

        private async Task LoadVersionSourcesAsync()
        {
            try
            {
                var options = await VersionSourceCatalog.BuildAsync(_weaoService);

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    VersionSources = new ObservableCollection<VersionSourceOption>(options);
                    SelectedVersionSource = VersionSources.FirstOrDefault(option => string.Equals(option.Value, Settings.RobloxVersionSource, StringComparison.OrdinalIgnoreCase))
                        ?? VersionSources.FirstOrDefault();
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(LoadVersionSourcesAsync));
            }
        }

        partial void OnRawTextChanged(string value)
        {
            if (_isRefreshingRawText)
                return;

            var flags = string.IsNullOrWhiteSpace(value) ? new Dictionary<string, string>() : _fastFlagService.DeserializeFlags(value);

            if (flags == null)
                return;

            Settings.RobloxFastFlags = [.. flags.Select(flag => new FastFlagEntry { Name = flag.Key, Value = flag.Value })];

            ApplyFastFlagsSearch();
        }

        partial void OnSearchTextChanged(string value) => ApplyFastFlagsSearch();

        partial void OnSelectedFastFlagsTabChanged(FastFlagsTab value)
        {
            if (value != FastFlagsTab.Raw)
                return;

            _isRefreshingRawText = true;

            RawText = _fastFlagService.SerializeFlags(GetFastFlags());

            _isRefreshingRawText = false;
        }

        partial void OnSelectedVersionSourceChanged(VersionSourceOption? value)
        {
            if (value != null)
                Settings.RobloxVersionSource = value.Value;
        }

        private void ApplyFastFlagsSearch()
        {
            var term = SearchText.Trim();
            var hasTerm = !string.IsNullOrEmpty(term);

            foreach (var entry in Settings.RobloxFastFlags)
                entry.Opacity = !hasTerm || Matches(entry, term) ? 1 : 0.4;

            IEnumerable<FastFlagEntry> entries = Settings.RobloxFastFlags;

            if (hasTerm)
                entries = entries.OrderByDescending(entry => Matches(entry, term));

            FastFlags = [.. entries];
        }

        private Dictionary<string, string> GetFastFlags()
        {
            Dictionary<string, string> flags = new(StringComparer.Ordinal);

            foreach (var entry in Settings.RobloxFastFlags.Where(entry => !string.IsNullOrWhiteSpace(entry.Name)))
                flags[entry.Name.Trim()] = entry.Value;

            return flags;
        }

        private static bool Matches(FastFlagEntry entry, string term) => entry.Name.Contains(term, StringComparison.OrdinalIgnoreCase);

        [RelayCommand]
        private void AddFastFlag()
        {
            if (string.IsNullOrWhiteSpace(NewFastFlagName))
                return;

            FastFlagEntry entry = new() { Name = NewFastFlagName.Trim(), Value = NewFastFlagValue };

            var index = 0;

            while (index < Settings.RobloxFastFlags.Count
                && string.Compare(Settings.RobloxFastFlags[index].Name, entry.Name, StringComparison.Ordinal) < 0)
                index++;

            Settings.RobloxFastFlags.Insert(index, entry);

            NewFastFlagName = string.Empty;
            NewFastFlagValue = string.Empty;
            SearchText = string.Empty;

            ApplyFastFlagsSearch();
        }

        [RelayCommand]
        private void RemoveFastFlag(FastFlagEntry entry)
        {
            Settings.RobloxFastFlags.Remove(entry);

            ApplyFastFlagsSearch();
        }

        [RelayCommand]
        private async Task BrowseInstallationPathAsync()
        {
            try
            {
                var mainWindow = GetMainWindow();

                if (mainWindow == null)
                    return;

                var storageProvider = mainWindow.StorageProvider;

                var startFolder = await storageProvider.TryGetFolderFromPathAsync(Settings.RobloxInstallationPath);

                var result = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "Select Roblox installation path",
                    AllowMultiple = false,
                    SuggestedStartLocation = startFolder
                });

                if (result.Count > 0)
                    Settings.RobloxInstallationPath = result[0].Path.LocalPath;
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(BrowseInstallationPathAsync));
            }
        }

        private void GetFontFamilies()
        {
            try
            {
                var fontFamilies = new List<string>
                {
                    "JetBrains Mono"
                };

                fontFamilies.AddRange(FontManager.Current.SystemFonts.Select(x => x.Name).Distinct().OrderBy(x => x));

                FontFamilies = new ObservableCollection<string>(fontFamilies);
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(GetFontFamilies));
            }
        }

        [RelayCommand]
        private async Task OpenIniFileAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    var iniPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sirstrap", "Sirstrap.ini");

                    if (!File.Exists(iniPath))
                        File.Create(iniPath).Close();

                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = iniPath,
                        UseShellExecute = true,
                        Verb = "open"
                    };

                    using var process = new Process
                    {
                        StartInfo = processStartInfo
                    };

                    process.Start();
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(OpenIniFileAsync));
            }
        }

        [RelayCommand]
        private async Task RunCleanerAsync()
        {
            try
            {
                const uint MB_YESNO = 0x00000004;
                const uint MB_ICONWARNING = 0x00000030;
                const int IDYES = 6;

                var result = await Task.Run(() =>
                    MessageBoxW(
                        IntPtr.Zero,
                        "This will:\n  • Close every running Roblox and SirHurt application\n  • Delete the Roblox installation, data and registry entries\n  • Delete the Sirstrap data folder (%LocalAppData%\\Sirstrap)\n\nThis action cannot be undone. Are you sure?",
                        "Run SirHurt Cleaner",
                        MB_YESNO | MB_ICONWARNING));

                if (result != IDYES)
                    return;

                IsCleanerRunning = true;

                await Task.Run(() => _cleanupService.Run("manual", Settings.CleanerCleanTempFolders, Settings.CleanerCleanProtectedFiles));
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(RunCleanerAsync));
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
                await Task.Run(() =>
                {
                    var sirHurt = Path.Combine(Settings.SirHurtPath, "bootstrapper.exe");

                    if (!File.Exists(sirHurt))
                        return;

                    ProcessStartInfo processStartInfo = new()
                    {
                        FileName = sirHurt
                    };

                    using Process process = new()
                    {
                        StartInfo = processStartInfo
                    };

                    process.Start();
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(RunSirHurtAsync));
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int MessageBoxW(IntPtr hWnd, string text, string caption, uint type);

        [RelayCommand]
        private async Task UninstallAsync()
        {
            try
            {
                const uint MB_YESNO = 0x00000004;
                const uint MB_ICONWARNING = 0x00000030;
                const int IDYES = 6;

                var result = await Task.Run(() =>
                    MessageBoxW(
                        IntPtr.Zero,
                        "This will:\n  • Remove Sirstrap protocol handler from the registry\n  • Delete the Sirstrap data folder (%LocalAppData%\\Sirstrap)\n  • Delete the Sirstrap executable\n\nThis action cannot be undone. Are you sure?",
                        "Uninstall Sirstrap",
                        MB_YESNO | MB_ICONWARNING));

                if (result != IDYES)
                    return;

                await Task.Run(_uninstallService.Uninstall);

                await Dispatcher.UIThread.InvokeAsync(() => Environment.Exit(0));
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(UninstallAsync));
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    Settings.Set();

                    App.ApplyFontFamily();

                    Dispatcher.UIThread.Invoke(() =>
                    {
                        App.ApplyAccentColor();
                        App.SetTray(Settings.SirstrapTrayMode != TrayMode.None);

                        CloseSpecificWindow<SettingsWindow>();
                    });
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(SaveAsync));
            }
        }

        public IReadOnlyList<FastFlagsTab> FastFlagsTabs { get; } = Enum.GetValues<FastFlagsTab>();

        public IReadOnlyList<SettingsTabOption> SettingsTabs { get; } =
        [
            new("All", null),
            _fastFlagsTab,
            new("Roblox", "Roblox"),
            new("SirHurt", "SirHurt"),
            new("Sirstrap", "Sirstrap")
        ];

        public IReadOnlyList<TrayMode> TrayModes { get; } = Enum.GetValues<TrayMode>();
    }
}
