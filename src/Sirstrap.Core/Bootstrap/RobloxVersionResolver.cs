namespace Sirstrap.Core.Bootstrap
{
    public sealed class RobloxVersionResolver(RobloxClientVersionApi robloxApi, SirHurtStatusApi sirHurtApi, WeaoService weaoService, SirstrapSettings settings, ITelemetry telemetry)
    {
        private async Task<(string Version, VersionResolutionSource Source)> FromConfiguredSourceAsync(bool honorsOverride)
        {
            string versionOverride = (settings.RobloxVersionOverride ?? string.Empty).Trim();

            if (honorsOverride && !string.IsNullOrEmpty(versionOverride))
            {
                Log.Information("Using the Roblox version set in the settings (Version: {0}).", versionOverride);

                return (versionOverride, VersionResolutionSource.Override);
            }

            string source = (settings.RobloxVersionSource ?? string.Empty).Trim();

            if (TryStripPrefix(source, RobloxVersionSources.VersionPrefix, out string pinnedVersion) && !string.IsNullOrWhiteSpace(pinnedVersion))
            {
                Log.Information("Using the fixed Roblox version chosen as version source (Version: {0}).", pinnedVersion);

                return (pinnedVersion, VersionResolutionSource.Override);
            }

            if (TryStripPrefix(source, RobloxVersionSources.ExecutorPrefix, out string executorTitle))
            {
                return await FromExecutorAsync(executorTitle);
            }

            if (source.Equals(RobloxVersionSources.Roblox, StringComparison.OrdinalIgnoreCase))
            {
                Log.Information("Asking Roblox for the latest Roblox version.");

                return await FromRobloxApiAsync();
            }

            if (source.Equals(RobloxVersionSources.Weao, StringComparison.OrdinalIgnoreCase))
            {
                Log.Information("Asking WEAO for the current Roblox version.");

                return await FromWeaoAsync();
            }

            Log.Information("Asking SirHurt which Roblox version it supports.");

            return await FromSirHurtAsync();
        }

        private async Task<(string Version, VersionResolutionSource Source)> FromExecutorAsync(string executorTitle)
        {
            Log.Information("Asking WEAO which Roblox version the selected executor supports (Executor: {0}).", executorTitle);

            string? version = await weaoService.GetExecutorRobloxVersionAsync(executorTitle);

            if (!string.IsNullOrWhiteSpace(version))
            {
                return (version, VersionResolutionSource.Executor);
            }

            Log.Warning("Could not get the Roblox version supported by the selected executor, using the latest Roblox version instead (Executor: {0}).", executorTitle);

            return await FromRobloxApiAsync();
        }

        private async Task<(string Version, VersionResolutionSource Source)> FromRobloxApiAsync()
        {
            string version = await robloxApi.GetLatestVersionAsync();

            if (!string.IsNullOrWhiteSpace(version))
            {
                return (version, VersionResolutionSource.RobloxApi);
            }

            Log.Warning("Could not get the latest Roblox version from Roblox.");

            return (version, VersionResolutionSource.Failed);
        }

        private async Task<(string Version, VersionResolutionSource Source)> FromSirHurtAsync()
        {
            SirHurtSupportedVersion? supportedVersion = await sirHurtApi.GetSupportedRobloxVersionAsync();

            if (supportedVersion == null)
            {
                Log.Warning("Could not get the supported Roblox version from SirHurt, using the latest Roblox version instead.");

                return await FromRobloxApiAsync();
            }

            if (!supportedVersion.IsOutdated)
            {
                return (supportedVersion.RobloxVersion, VersionResolutionSource.SirHurt);
            }

            Log.Warning("SirHurt has not been updated for more than 10 days, using the latest Roblox version instead (SirHurt version: {0}).", supportedVersion.RobloxVersion);

            string robloxVersion = await robloxApi.GetLatestVersionAsync();

            if (!string.IsNullOrEmpty(robloxVersion))
            {
                return (robloxVersion, VersionResolutionSource.RobloxApi);
            }

            Log.Warning("Could not get the latest Roblox version from Roblox, using the version supported by SirHurt even though it may be outdated (Version: {0}).", supportedVersion.RobloxVersion);

            return (supportedVersion.RobloxVersion, VersionResolutionSource.SirHurtFallback);
        }

        private async Task<(string Version, VersionResolutionSource Source)> FromWeaoAsync()
        {
            string? version = await weaoService.GetCurrentWindowsVersionAsync();

            if (!string.IsNullOrWhiteSpace(version))
            {
                return (version, VersionResolutionSource.Weao);
            }

            Log.Warning("Could not get the current Roblox version from WEAO, using the latest Roblox version instead.");

            return await FromRobloxApiAsync();
        }

        private async Task<string> ResolveAsync(bool honorsOverride)
        {
            using ITelemetryOperation operation = telemetry.StartOperation("version.resolve");

            (string version, VersionResolutionSource source) = await FromConfiguredSourceAsync(honorsOverride);

            operation.SetTag("source", source.ToString());
            operation.SetTag("configuredSource", settings.RobloxVersionSource ?? string.Empty);

            if (string.IsNullOrEmpty(version))
            {
                operation.MarkFailed();

                return version;
            }

            Log.Information("Selected the Roblox version to install (Version: {0}, Source: {1}).", version, source);

            telemetry.SetGlobalTag("roblox.version", version);
            telemetry.SetGlobalTag("roblox.version_source", source.ToString());

            return version;
        }

        private static bool TryStripPrefix(string value, string prefix, out string remainder)
        {
            remainder = value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ? value[prefix.Length..].Trim() : string.Empty;

            return value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }

        public Task<string> ResolveAsync()
        {
            return ResolveAsync(honorsOverride: true);
        }

        public Task<string> ResolveIgnoringOverrideAsync()
        {
            return ResolveAsync(honorsOverride: false);
        }

        public bool HasVersionOverride => !string.IsNullOrWhiteSpace(settings.RobloxVersionOverride);
    }
}