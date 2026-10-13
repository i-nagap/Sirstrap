namespace Sirstrap.UI
{
    public partial class App : Application
    {
        private static void ScheduleConfiguredCleanups(IClassicDesktopStyleApplicationLifetime desktop, SirstrapSettings settings, CleanupService cleanupService)
        {
            void RunCleanup(string trigger)
            {
                cleanupService.Run(trigger, settings.CleanerCleanTempFolders, settings.CleanerCleanProtectedFiles);
            }

            if (settings.CleanerEnabled && settings.CleanerCleanOnLaunch)
            {
                _ = Task.Run(() => RunCleanup("launch"));
            }

            desktop.Exit += (_, _) =>
            {
                if (settings.CleanerEnabled && settings.CleanerCleanOnExit)
                {
                    RunCleanup("exit");
                }
            };
        }

        private static void StartDesktop(IClassicDesktopStyleApplicationLifetime desktop, IServiceProvider services)
        {
            SirstrapSettings settings = services.GetRequiredService<SirstrapSettings>();
            SystemTray systemTray = services.GetRequiredService<SystemTray>();

            services.GetRequiredService<SettingsStore>().Load();
            services.GetRequiredService<AppAppearance>().Apply();

            MainWindow mainWindow = new()
            {
                DataContext = services.GetRequiredService<MainWindowViewModel>(),
                SystemTray = systemTray
            };

            desktop.MainWindow = mainWindow;

            systemTray.SetEnabled(settings.SirstrapTrayMode != TrayMode.None);

            if (settings.SirstrapTrayMode == TrayMode.OnLaunch)
            {
                void HideToTrayOnFirstOpen(object? sender, EventArgs e)
                {
                    mainWindow.Opened -= HideToTrayOnFirstOpen;

                    systemTray.HideToTray(mainWindow);
                }

                mainWindow.Opened += HideToTrayOnFirstOpen;
            }

            ScheduleConfiguredCleanups(desktop, settings, services.GetRequiredService<CleanupService>());
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

#if DEBUG
            this.AttachDeveloperTools();
#endif
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                StartDesktop(desktop, Program.CreateServices(desktop.Args ?? []));
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}