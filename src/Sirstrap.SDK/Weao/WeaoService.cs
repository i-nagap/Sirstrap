namespace Sirstrap.SDK.Weao
{
    public sealed record WeaoExecutor(string Title, string? RobloxVersion, bool IsUpdated, bool IsDetected);

    public sealed record WeaoWindowsVersions(string? Current, string? Past, string? Future);

    public sealed class WeaoService(WeaoApiClient apiClient)
    {
        private static bool IsWindowsPlatform(string? platform)
        {
            return string.IsNullOrWhiteSpace(platform) || platform.Equals("windows", StringComparison.OrdinalIgnoreCase);
        }

        private static string? NullIfBlank(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        private static WeaoExecutor? ToWindowsExecutor(ExploitStatus exploit)
        {
            return exploit.Hidden || !IsWindowsPlatform(exploit.Platform) || string.IsNullOrWhiteSpace(exploit.Title) ? null : new WeaoExecutor(exploit.Title, NullIfBlank(exploit.RobloxVersion), exploit.UpdateStatus, exploit.Detected);
        }

        public async Task<string?> GetCurrentWindowsVersionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                RobloxVersions versions = await apiClient.GetCurrentVersionsAsync(cancellationToken).ConfigureAwait(false);

                return NullIfBlank(versions.Windows);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not get the current Roblox version from WEAO.");

                return null;
            }
        }

        public async Task<string?> GetExecutorRobloxVersionAsync(string executorTitle, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(executorTitle))
            {
                return null;
            }

            try
            {
                ExploitStatus exploit = await apiClient.GetExploitAsync(executorTitle, cancellationToken).ConfigureAwait(false);

                return NullIfBlank(exploit.RobloxVersion);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not get the Roblox version supported by the executor from WEAO (Executor: {0}).", executorTitle);

                return null;
            }
        }

        public async Task<IReadOnlyList<WeaoExecutor>> GetWindowsExecutorsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                List<ExploitStatus> exploits = await apiClient.GetExploitsAsync(cancellationToken).ConfigureAwait(false);

                return [.. exploits.Select(ToWindowsExecutor).OfType<WeaoExecutor>().OrderBy(executor => executor.Title, StringComparer.OrdinalIgnoreCase)];
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not get the list of executors from WEAO.");

                return [];
            }
        }

        public async Task<WeaoWindowsVersions> GetWindowsVersionsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                Task<RobloxVersions> current = apiClient.GetCurrentVersionsAsync(cancellationToken);
                Task<RobloxVersions> past = apiClient.GetPastVersionsAsync(cancellationToken);
                Task<RobloxVersions> future = apiClient.GetFutureVersionsAsync(cancellationToken);

                _ = await Task.WhenAll(current, past, future).ConfigureAwait(false);

                return new WeaoWindowsVersions(NullIfBlank(current.Result.Windows), NullIfBlank(past.Result.Windows), NullIfBlank(future.Result.Windows));
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not get the Roblox versions from WEAO.");

                return new WeaoWindowsVersions(null, null, null);
            }
        }
    }
}