namespace Sirstrap.Core
{
    public static class SirstrapServiceCollectionExtensions
    {
        private const string UserAgent = "Sirstrap";

        private static void AddBootstrap(IServiceCollection services)
        {
            services.TryAddSingleton<RobloxVersionLayout>();
            services.TryAddSingleton<CdnRanking>();
            services.TryAddSingleton<CdnSelector>();
            services.TryAddSingleton<RobloxVersionResolver>();
            services.TryAddSingleton<RobloxArchiveDownloader>();
            services.TryAddSingleton<RobloxInstaller>();
            services.TryAddSingleton(provider => new FastFlagsFile(provider.GetRequiredService<SirstrapDirectories>().FastFlagsFile));
            services.TryAddSingleton<FastFlagsService>();
            services.TryAddSingleton<RobloxSingletonMutex>();
            services.TryAddSingleton(provider => new IncognitoRobloxData(provider.GetRequiredService<RobloxSingletonMutex>(), RobloxFolders.LocalData, provider.GetRequiredService<SirstrapDirectories>().IncognitoBackup));
            services.TryAddSingleton<RobloxLauncher>();
            services.TryAddSingleton<RobloxBootstrapper>();
        }

        private static void AddIntegrations(IServiceCollection services)
        {
            services.TryAddSingleton<RobloxClientVersionApi>();
            services.TryAddSingleton<SirHurtStatusApi>();
            services.TryAddSingleton(SirHurtAccount.ForCurrentUser());
            services.TryAddSingleton(_ => new WeaoApiClient());
            services.TryAddSingleton<WeaoService>();
        }

        private static void AddSettings(IServiceCollection services)
        {
            services.TryAddSingleton<SirstrapSettings>();
            services.TryAddSingleton(provider => new SettingsStore(provider.GetRequiredService<SirstrapSettings>(), provider.GetRequiredService<SirstrapDirectories>().SettingsFile));
            services.TryAddSingleton<SettingsTelemetryReporter>();
            services.TryAddSingleton<SirstrapHousekeeping>();
        }

        private static void AddUpdate(IServiceCollection services)
        {
            services.TryAddSingleton<SirstrapVersionInfo>();
            services.TryAddSingleton<SirstrapReleaseFeed>();
            services.TryAddSingleton<UpdateInstaller>();
            services.TryAddSingleton<SirstrapUpdater>();
        }

        private static HttpClient CreateHttpClient()
        {
            HttpClient httpClient = new(new SentryHttpMessageHandler())
            {
                Timeout = TimeSpan.FromMinutes(5)
            };

            httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);

            return httpClient;
        }

        public static IServiceCollection AddSirHurtCleaner(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.TryAddSingleton<CleanerOptions>();
            services.TryAddSingleton<IStatusLine, ConsoleStatusLine>();
            services.TryAddSingleton<IUserPrompt, ConsoleUserPrompt>();
            services.TryAddSingleton(UserProfileLocator.ForThisMachine());
            services.TryAddSingleton<SelectiveFolderCleaner>();

            _ = services.AddSingleton<ICleanupStep, CloseRunningApplicationsStep>();
            _ = services.AddSingleton<ICleanupStep, RemoveSystemFoldersStep>();
            _ = services.AddSingleton<ICleanupStep, CleanUserFoldersStep>();
            _ = services.AddSingleton<ICleanupStep, RemoveRegistryKeysStep>();
            _ = services.AddSingleton<ICleanupStep, CleanTemporaryFoldersStep>();

            services.TryAddSingleton<CleanupRunner>();

            return services;
        }

        public static IServiceCollection AddSirstrapCore(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.TryAddSingleton(SirstrapDirectories.ForCurrentUser());
            services.TryAddSingleton<InstanceWorkspace>();
            services.TryAddSingleton(CreateHttpClient());
            services.TryAddSingleton<ITelemetry, SentryTelemetry>();
            services.TryAddSingleton<SessionLogSink>();

            AddSettings(services);
            AddIntegrations(services);
            AddBootstrap(services);
            AddUpdate(services);

            _ = services.AddSirHurtCleaner();

            services.TryAddSingleton<CleanupService>();
            services.TryAddTransient(provider => new RobloxServerLocationWatcher(provider.GetRequiredService<ServerLocationLookup>(), RobloxFolders.Logs));
            services.TryAddSingleton<ServerLocationLookup>();
            services.TryAddSingleton<SirstrapUninstaller>();
            services.TryAddSingleton<CodebergAccounts>();
            services.TryAddSingleton<IpcServer>();
            services.TryAddSingleton<SirstrapSession>();

            return services;
        }
    }
}