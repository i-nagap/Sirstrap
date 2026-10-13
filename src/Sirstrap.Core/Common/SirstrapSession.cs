namespace Sirstrap.Core.Common
{
    public sealed class SirstrapSession(InstanceWorkspace workspace, SirstrapSettings settings, SettingsStore settingsStore, SettingsTelemetryReporter settingsTelemetryReporter, SirstrapHousekeeping housekeeping, SirstrapVersionInfo versionInfo, SirHurtAccount sirHurtAccount, SessionLogSink sessionLogSink, IpcServer ipcServer, SirstrapUpdater updater)
    {
        private const string IpcPipeName = "SirstrapIpc";

        private const string LogOutputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] [Thread: {ThreadId}, {ThreadName}] [User: {SirHurtUser}] {Message:lj}{NewLine}{Exception}";

        private const long MaximumLogFileBytes = 1_048_576;

        private const int RetainedLogFileCount = 5;

        private const string RobloxPlayerProtocol = "roblox-player";

        private const string StartupBanner = @"
   ▄████████  ▄█     ▄████████    ▄████████     ███        ▄████████    ▄████████    ▄███████▄
  ███    ███ ███    ███    ███   ███    ███ ▀█████████▄   ███    ███   ███    ███   ███    ███
  ███    █▀  ███▌   ███    ███   ███    █▀     ▀███▀▀██   ███    ███   ███    ███   ███    ███
  ███        ███▌  ▄███▄▄▄▄██▀   ███            ███   ▀  ▄███▄▄▄▄██▀   ███    ███   ███    ███
▀███████████ ███▌ ▀▀███▀▀▀▀▀   ▀███████████     ███     ▀▀███▀▀▀▀▀   ▀███████████ ▀█████████▀
         ███ ███  ▀███████████          ███     ███     ▀███████████   ███    ███   ███ {0}
   ▄█    ███ ███    ███    ███    ▄█    ███     ███       ███    ███   ███    ███   ███ {1}
 ▄████████▀  █▀     ███    ███  ▄████████▀     ▄████▀     ███    ███   ███    █▀   ▄████▀ {2}
                    ███    ███                            ███    ███ by SirHurt CSR Team";

        private Logger CreateLogger(SirstrapEdition edition)
        {
            string runId = Guid.NewGuid().ToString("N");

            LoggerConfiguration configuration = new LoggerConfiguration().Enrich.WithThreadId().Enrich.WithThreadName().Enrich.WithProperty("SirHurtUser", sirHurtAccount.GetUserName()).WriteTo.Console(outputTemplate: LogOutputTemplate).WriteTo.File(GetLogFilePath("Sirstrap"), outputTemplate: LogOutputTemplate, fileSizeLimitBytes: MaximumLogFileBytes, rollOnFileSizeLimit: true, retainedFileCountLimit: RetainedLogFileCount).WriteTo.File(GetLogFilePath("Errors"), restrictedToMinimumLevel: LogEventLevel.Error, outputTemplate: LogOutputTemplate, fileSizeLimitBytes: MaximumLogFileBytes, rollOnFileSizeLimit: true, retainedFileCountLimit: RetainedLogFileCount).WriteTo.SessionLog(sessionLogSink);

            return SirstrapVersionInfo.IsDebugBuild ? configuration.CreateLogger() : configuration.WriteToSentryWhenEnabled(settings, edition, runId).CreateLogger();
        }

        private string GetLogFilePath(string fileName)
        {
            return Path.Combine(workspace.Logs, $"{fileName}.log");
        }

        public async Task StartAsync(SirstrapEdition edition, string[] launchArguments)
        {
            workspace.Open();
            housekeeping.PruneInstances();
            settingsStore.Load();

            Log.Logger = CreateLogger(edition);

            Log.Information(StartupBanner, versionInfo.GetDisplayVersion(), AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName, Environment.OSVersion);
            Log.Information("Started a new Sirstrap instance (Instance: {0}, Folder: {1}).", workspace.Id, workspace.Root);

            settingsStore.Load();
            settingsTelemetryReporter.Report();
            housekeeping.RemovePreviousRobloxInstallation();
            housekeeping.RemoveAbandonedInstallFolders();

            await ipcServer.StartAsync(IpcPipeName);

            _ = ProtocolHandlerRegistrar.EnsureRegistered(RobloxPlayerProtocol, launchArguments);
        }

        public async Task StopAsync()
        {
            await ipcServer.StopAsync();
            await updater.ApplyPendingUpdateAsync();
            await Log.CloseAndFlushAsync();

            workspace.Dispose();
        }
    }
}