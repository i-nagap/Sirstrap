using System.Globalization;

namespace Sirstrap.UI.Models
{
    public sealed class VersionSourceCatalog(WeaoService weaoService)
    {
        private async Task AddWeaoOptionsAsync(List<VersionSourceOption> options)
        {
            WeaoWindowsVersions versions = await weaoService.GetWindowsVersionsAsync();

            if (!string.IsNullOrWhiteSpace(versions.Current))
            {
                options.Add(new(string.Format(CultureInfo.InvariantCulture, "WEAO: current version {0}", versions.Current), RobloxVersionSources.Weao));
            }

            if (!string.IsNullOrWhiteSpace(versions.Past))
            {
                options.Add(new(string.Format(CultureInfo.InvariantCulture, "WEAO: previous version {0}", versions.Past), RobloxVersionSources.VersionPrefix + versions.Past));
            }

            if (!string.IsNullOrWhiteSpace(versions.Future) && !string.IsNullOrWhiteSpace(versions.Current) && versions.Future != versions.Current)
            {
                options.Add(new(string.Format(CultureInfo.InvariantCulture, "WEAO: future version {0}", versions.Future), RobloxVersionSources.VersionPrefix + versions.Future));
            }

            IReadOnlyList<WeaoExecutor> executors = await weaoService.GetWindowsExecutorsAsync();

            options.AddRange(executors.Select(executor => new VersionSourceOption(string.Format(CultureInfo.InvariantCulture, "WEAO: version supported by {0}", executor.Title), RobloxVersionSources.ExecutorPrefix + executor.Title)));
        }

        public static VersionSourceOption? FindOrFirst(IReadOnlyList<VersionSourceOption> options, string value)
        {
            return options.FirstOrDefault(option => string.Equals(option.Value, value, StringComparison.OrdinalIgnoreCase)) ?? options.FirstOrDefault();
        }

        public async Task<IReadOnlyList<VersionSourceOption>> LoadOptionsAsync()
        {
            List<VersionSourceOption> options = [new("SirHurt: supported version", RobloxVersionSources.SirHurt), new("Roblox: latest version", RobloxVersionSources.Roblox)];

            try
            {
                await AddWeaoOptionsAsync(options);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not load the version sources from WEAO.");
            }

            return options;
        }
    }
}