namespace Sirstrap.UI.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private const int ActivePollingIntervalMilliseconds = 100;

        private const int IdlePollingIntervalMilliseconds = 10000;

        private static readonly TimeSpan _recentLogActivityWindow = TimeSpan.FromSeconds(30);

        private readonly RobloxBootstrapper _bootstrapper;

        private readonly CodebergAccounts _codebergAccounts;

        private readonly Func<SettingsWindowViewModel> _createSettingsWindowViewModel;

        private int _displayedLogEntryCount;

        private bool _hasMovedAsideForRoblox;

        private readonly HttpClient _httpClient;

        private bool _isRobloxRunning;

        private readonly LaunchArguments _launchArguments;

        private readonly System.Timers.Timer _pollingTimer = new(ActivePollingIntervalMilliseconds);

        private readonly RobloxServerLocationWatcher _serverLocationWatcher;

        private readonly SirstrapSession _session;

        private readonly SessionLogSink _sessionLogSink;

        private readonly Task _sessionStartup;

        private readonly SettingsStore _settingsStore;

        private readonly SirHurtAccount _sirHurtAccount;

        private readonly SirstrapSettings _sirstrapSettings;

        private readonly SystemTray _systemTray;

        private readonly ITelemetry _telemetry;

        private readonly VersionSourceCatalog _versionSourceCatalog;

        public MainWindowViewModel(LaunchArguments launchArguments, SirstrapSettings sirstrapSettings, SettingsStore settingsStore, SirstrapSession session, SessionLogSink sessionLogSink, RobloxBootstrapper bootstrapper, RobloxServerLocationWatcher serverLocationWatcher, SirHurtAccount sirHurtAccount, SirstrapVersionInfo versionInfo, CodebergAccounts codebergAccounts, HttpClient httpClient, ITelemetry telemetry, VersionSourceCatalog versionSourceCatalog, SystemTray systemTray, Func<SettingsWindowViewModel> createSettingsWindowViewModel)
        {
            _launchArguments = launchArguments;
            _sirstrapSettings = sirstrapSettings;
            _settingsStore = settingsStore;
            _session = session;
            _sessionLogSink = sessionLogSink;
            _bootstrapper = bootstrapper;
            _serverLocationWatcher = serverLocationWatcher;
            _sirHurtAccount = sirHurtAccount;
            _codebergAccounts = codebergAccounts;
            _httpClient = httpClient;
            _telemetry = telemetry;
            _versionSourceCatalog = versionSourceCatalog;
            _systemTray = systemTray;
            _createSettingsWindowViewModel = createSettingsWindowViewModel;

            CurrentFullVersion = versionInfo.GetDisplayVersion();
            SirHurtUser = sirHurtAccount.GetUserName();
            ShowLaunchButton = launchArguments.IsEmpty;

            _ = LoadVersionSourcesAsync();
            _ = LoadProfileImageAsync();

            _pollingTimer.Elapsed += (_, _) => OnPollingTick();

            _pollingTimer.Start();

            _sessionStartup = Task.Run(StartSessionAsync);

            if (!launchArguments.IsEmpty)
            {
                _ = Task.Run(RunAsync);
            }
        }

        private void AdjustPollingInterval()
        {
            bool hasRecentLogActivity = _sessionLogSink.LastTimestamp is
            {
            } lastTimestamp && DateTimeOffset.Now - lastTimestamp <= _recentLogActivityWindow;
            int pollingInterval = hasRecentLogActivity ? ActivePollingIntervalMilliseconds : IdlePollingIntervalMilliseconds;

            if ((int)_pollingTimer.Interval != pollingInterval)
            {
                _pollingTimer.Interval = pollingInterval;
            }
        }

        private async Task<Bitmap?> DownloadImageAsync(string uri)
        {
            try
            {
                using MemoryStream stream = new(await _httpClient.GetByteArrayAsync(uri));

                return new Bitmap(stream);
            }
            catch (Exception ex)
            {
                Log.Debug(ex, "Could not load an image (URL: {0}).", uri);

                return null;
            }
        }

        private async Task LoadProfileImageAsync()
        {
            Bitmap? profileImage = await CodebergAccounts.FirstResultAsync(account => DownloadImageAsync(CodebergLinks.ProfileImage(account)));

            _ = await Dispatcher.UIThread.InvokeAsync(() => ProfileImage = profileImage);
        }

        private async Task LoadVersionSourcesAsync()
        {
            IReadOnlyList<VersionSourceOption> options = await _versionSourceCatalog.LoadOptionsAsync();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                VersionSources = [.. options];
                SelectedVersionSource = VersionSourceCatalog.FindOrFirst(options, _sirstrapSettings.RobloxVersionSource);
            });
        }

        [RelayCommand]
        private void Logout()
        {
            try
            {
                if (_sirHurtAccount.LogOut())
                {
                    SirHurtUser = _sirHurtAccount.GetUserName();
                }

                RecordUserAction(nameof(Logout));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not log out of SirHurt.");
            }
        }

        private void MoveMainWindowAsideForRoblox()
        {
            if (MainWindow is not
                {
                } mainWindow)
            {
                return;
            }

            if (_sirstrapSettings.SirstrapTrayMode == TrayMode.OnRoblox)
            {
                _systemTray.HideToTray(mainWindow);
            }
            else
            {
                mainWindow.WindowState = WindowState.Minimized;
            }
        }

        private void OnPollingTick()
        {
            try
            {
                RefreshSessionLog();
                TrackRobloxProcess();
                AdjustPollingInterval();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not update the session log and the Roblox status.");
            }
        }

        partial void OnSelectedVersionSourceChanged(VersionSourceOption? value)
        {
            if (value is null || string.Equals(value.Value, _sirstrapSettings.RobloxVersionSource, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _sirstrapSettings.RobloxVersionSource = value.Value;

            _settingsStore.Save();

            Log.Information("Changed the Roblox version source from the launch menu (Source: {0}).", value.Value);
        }

        [RelayCommand]
        private async Task OpenProfileAsync()
        {
            try
            {
                ShellLauncher.Open(CodebergLinks.Profile(await _codebergAccounts.GetReachableAccountAsync()));

                RecordUserAction(nameof(OpenProfileAsync));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not open the Sirstrap profile on Codeberg.");
            }
        }

        [RelayCommand]
        private async Task OpenRepositoryAsync()
        {
            try
            {
                ShellLauncher.Open(CodebergLinks.Repository(await _codebergAccounts.GetReachableAccountAsync()));

                RecordUserAction(nameof(OpenRepositoryAsync));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not open the Sirstrap repository on Codeberg.");
            }
        }

        [RelayCommand]
        private void OpenSettings()
        {
            try
            {
                if (MainWindow is not
                    {
                    } owner)
                {
                    return;
                }

                _ = new SettingsWindow
                {
                    DataContext = _createSettingsWindowViewModel()
                }.ShowDialog(owner);

                RecordUserAction(nameof(OpenSettings));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not open the settings window.");
            }
        }

        private void RecordUserAction(string action)
        {
            _telemetry.RecordCounter("ui.action", new Dictionary<string, object>
            {
                ["action"] = action
            });
        }

        private void RefreshSessionLog()
        {
            int entryCount = _sessionLogSink.EntryCount;

            if (entryCount == _displayedLogEntryCount)
            {
                return;
            }

            _displayedLogEntryCount = entryCount;

            string sessionLog = string.Join(Environment.NewLine, _sessionLogSink.GetEntries());

            Dispatcher.UIThread.Post(() => SessionLog = sessionLog);
        }

        [RelayCommand]
        private async Task RunAsync()
        {
            ShowLaunchButton = false;

            await _sessionStartup;

            try
            {
                if (!SirstrapVersionInfo.IsDebugBuild)
                {
                    await _bootstrapper.RunAsync(_launchArguments.Values, SirstrapEdition.UI);
                }

                Environment.ExitCode = 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Sirstrap stopped because of an unexpected error.");

                Environment.ExitCode = 1;
            }
            finally
            {
                if (!SirstrapVersionInfo.IsDebugBuild)
                {
                    await _session.StopAsync();

                    Environment.Exit(Environment.ExitCode);
                }
            }
        }

        private async Task StartSessionAsync()
        {
            try
            {
                await _session.StartAsync(SirstrapEdition.UI, _launchArguments.Values);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not finish starting Sirstrap.");
            }
        }

        private void TrackRobloxProcess()
        {
            bool isRobloxRunning = RobloxProcesses.CountGameProcesses() > 0;

            if (isRobloxRunning != _isRobloxRunning)
            {
                _isRobloxRunning = isRobloxRunning;

                if (isRobloxRunning)
                {
                    _serverLocationWatcher.Start();
                }
                else
                {
                    _serverLocationWatcher.Stop();
                }
            }

            if (isRobloxRunning && !_hasMovedAsideForRoblox)
            {
                _hasMovedAsideForRoblox = true;

                Dispatcher.UIThread.Post(MoveMainWindowAsideForRoblox);
            }
        }

        public string CurrentFullVersion { get; }

        [ObservableProperty]
        public partial Bitmap? ProfileImage { get; set; }

        [ObservableProperty]
        public partial VersionSourceOption? SelectedVersionSource { get; set; }

        [ObservableProperty]
        public partial string SessionLog { get; set; } = string.Empty;

        [ObservableProperty]
        public partial bool ShowLaunchButton { get; set; }

        [ObservableProperty]
        public partial string SirHurtUser { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<VersionSourceOption> VersionSources { get; set; } = [];
    }
}