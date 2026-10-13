namespace Sirstrap.Core.Update
{
    public sealed class SirstrapUpdater(SirstrapReleaseFeed releaseFeed, UpdateInstaller updateInstaller, SirstrapSettings settings, SirstrapVersionInfo versionInfo, ITelemetry telemetry)
    {
        private Task<bool>? _pendingDownload;

        private async Task<UpdateOutcome> CheckAndDownloadAsync(SirstrapEdition edition)
        {
            if (!settings.SirstrapAutoUpdate)
            {
                Log.Information("Automatic updates are turned off, skipping the update check.");

                return UpdateOutcome.Disabled;
            }

            if (await IsUpToDateAsync())
            {
                return UpdateOutcome.UpToDate;
            }

            bool isDownloaded = await TryDownloadAsync(edition);

            return isDownloaded ? UpdateOutcome.Downloaded : UpdateOutcome.Failed;
        }

        private async Task<bool> DownloadAsync(SirstrapEdition edition)
        {
            using ITelemetryOperation operation = telemetry.StartOperation("update.check", new Dictionary<string, object>
            {
                ["sirstrapType"] = edition.ToString()
            });

            UpdateOutcome outcome;

            try
            {
                outcome = await CheckAndDownloadAsync(edition);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not update Sirstrap because of an unexpected error.");

                outcome = UpdateOutcome.Failed;
            }

            if (outcome == UpdateOutcome.Failed)
            {
                operation.MarkFailed();
            }

            operation.SetOutcome(outcome.ToString());

            return outcome == UpdateOutcome.Downloaded;
        }

        private async Task<(ReleaseTag Tag, SirstrapRelease Release)?> FindLatestReleaseAsync()
        {
            (ReleaseTag Tag, SirstrapRelease Release)? latest = null;

            foreach (SirstrapRelease release in await releaseFeed.GetReleasesAsync())
            {
                if (release.IsDraft || !ReleaseTag.TryParse(release.TagName, out ReleaseTag tag) || !tag.IsOnChannel(versionInfo.Channel))
                {
                    continue;
                }

                if (latest == null || tag.Version > latest.Value.Tag.Version)
                {
                    latest = (tag, release);
                }
            }

            return latest;
        }

        private static string GetAssetName(SirstrapEdition edition)
        {
            return edition == SirstrapEdition.CLI ? "Sirstrap.CLI.zip" : "Sirstrap.UI.zip";
        }

        private async Task<bool> IsUpToDateAsync()
        {
            try
            {
                Version currentVersion = SirstrapVersionInfo.Current;
                string currentChannel = versionInfo.Channel;

                if (await FindLatestReleaseAsync() is not
                    {
                    } latest)
                {
                    Log.Warning("No Sirstrap release found for this channel, skipping the update (Channel: {0}).", string.IsNullOrEmpty(currentChannel) ? "stable" : currentChannel);

                    return true;
                }

                if (latest.Tag.Version > currentVersion)
                {
                    Log.Information("A new Sirstrap version is available, downloading it now (Current: v{0}{1}, Latest: v{2}{3}).", currentVersion, currentChannel, latest.Tag.Version, latest.Tag.Channel);

                    return false;
                }

                Log.Information("Sirstrap is up to date (Version: v{0}{1}).", currentVersion, currentChannel);

                return true;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not check for Sirstrap updates, continuing with the current version.");

                return true;
            }
        }

        private async Task<bool> TryDownloadAsync(SirstrapEdition edition)
        {
            try
            {
                string assetName = GetAssetName(edition);
                string? downloadUri = (await FindLatestReleaseAsync())?.Release.FindAssetDownloadUri(assetName);

                if (string.IsNullOrEmpty(downloadUri))
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "The latest release does not contain the file {0}.", assetName));
                }

                await updateInstaller.DownloadAsync(downloadUri);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not download the Sirstrap update, continuing with the current version.");

                return false;
            }
        }

        public async Task ApplyPendingUpdateAsync()
        {
            if (_pendingDownload == null || !await _pendingDownload)
            {
                return;
            }

            try
            {
                updateInstaller.Apply();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not install the Sirstrap update, it will be downloaded again the next time Sirstrap starts.");
            }
        }

        public async Task<string> GetLatestChangelogAsync()
        {
            return (await FindLatestReleaseAsync())?.Release.Changelog ?? string.Empty;
        }

        public void StartDownload(SirstrapEdition edition)
        {
            _pendingDownload ??= Task.Run(() => DownloadAsync(edition));
        }
    }
}