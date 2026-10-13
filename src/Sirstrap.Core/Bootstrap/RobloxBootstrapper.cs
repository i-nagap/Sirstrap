namespace Sirstrap.Core.Bootstrap
{
    public sealed class RobloxBootstrapper(SirstrapUpdater updater, RobloxVersionResolver versionResolver, CdnSelector cdnSelector, RobloxArchiveDownloader archiveDownloader, RobloxInstaller installer, RobloxLauncher launcher, RobloxVersionLayout layout, ITelemetry telemetry)
    {
        private async Task<string> DeployAsync(LaunchRequest request)
        {
            if (request.IsWindowsPlayer && File.Exists(layout.GetPlayerExecutablePath(request.VersionHash)))
            {
                Log.Information("Roblox is already installed, skipping the download (Version: {0}).", request.VersionHash);

                if (launcher.Launch(request))
                {
                    return "Cached";
                }

                Log.Warning("Could not start the installed copy of Roblox, downloading it again (Version: {0}).", request.VersionHash);
            }

            _ = await cdnSelector.SelectAsync(request.Target);

            await archiveDownloader.DownloadAsync(request.Target);

            if (request.IsWindowsPlayer)
            {
                installer.Install(request.Target);

                _ = launcher.Launch(request);
            }

            return "Success";
        }

        private async Task<string> DeployConfiguredSourceVersionAsync(LaunchRequest request, Exception overrideFailure)
        {
            Log.Warning(overrideFailure, "Could not install the Roblox version set in the settings, using the version source instead (Version: {0}).", request.VersionHash);

            request = request.WithVersionHash(await versionResolver.ResolveIgnoringOverrideAsync());

            return string.IsNullOrEmpty(request.VersionHash) ? throw new InvalidOperationException("Could not determine the Roblox version from the version source.", overrideFailure) : await DeployAsync(request);
        }

        public async Task RunAsync(string[] arguments, SirstrapEdition edition)
        {
            using ITelemetryOperation operation = telemetry.StartOperation("sirstrap.execute", new Dictionary<string, object>
            {
                ["sirstrapType"] = edition.ToString()
            });

            try
            {
                updater.StartDownload(edition);

                LaunchRequest request = LaunchArgumentsParser.Parse(arguments);

                string binaryType = request.Target.BinaryType.ToString();

                operation.SetTag("channel", request.Target.ChannelName);
                operation.SetTag("binaryType", binaryType);

                telemetry.SetGlobalTag("roblox.channel", request.Target.ChannelName);
                telemetry.SetGlobalTag("roblox.binary_type", binaryType);

                bool usesVersionOverride = string.IsNullOrEmpty(request.VersionHash) && versionResolver.HasVersionOverride;

                if (string.IsNullOrEmpty(request.VersionHash))
                {
                    request = request.WithVersionHash(await versionResolver.ResolveAsync());

                    if (string.IsNullOrEmpty(request.VersionHash))
                    {
                        Log.Error("Could not determine which Roblox version to install. Check your internet connection and try again.");

                        operation.MarkFailed("VersionResolutionFailed");

                        return;
                    }
                }

                string outcome;

                try
                {
                    outcome = await DeployAsync(request);
                }
                catch (Exception ex) when (usesVersionOverride)
                {
                    operation.SetTag("versionOverrideFallback", true);

                    outcome = await DeployConfiguredSourceVersionAsync(request, ex);
                }

                operation.SetOutcome(outcome);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not start Roblox because of an unexpected error.");

                operation.MarkFailed();

                Environment.ExitCode = 1;
            }
        }
    }
}