namespace Sirstrap.Core.Bootstrap
{
    public sealed class RobloxArchiveDownloader(HttpClient httpClient, CdnRanking ranking, RobloxVersionLayout layout, ITelemetry telemetry)
    {
        private const string AppSettingsEntryName = "AppSettings.xml";

        private const string AppSettingsXml = """<?xml version="1.0" encoding="UTF-8"?><Settings><ContentFolder>content</ContentFolder><BaseUrl>http://www.roblox.com</BaseUrl></Settings>""";

        private async Task<T?> DownloadFromRankedCdnsAsync<T>(string resourceDescription, Func<string, Task<T?>> downloadFromBaseUri, Func<T?, bool> isUsable) where T : class
        {
            string primaryBaseUri = ranking.PrimaryBaseUri;

            T? result = await downloadFromBaseUri(primaryBaseUri);

            if (isUsable(result))
            {
                return result;
            }

            foreach (string fallbackBaseUri in ranking.BaseUrisFastestFirst.Where(baseUri => !baseUri.Equals(primaryBaseUri, StringComparison.OrdinalIgnoreCase)))
            {
                Log.Warning("Could not download the {0} from the fastest server, trying another one (Failed server: {1}, Next server: {2}).", resourceDescription, primaryBaseUri, fallbackBaseUri);

                result = await downloadFromBaseUri(fallbackBaseUri);

                if (isUsable(result))
                {
                    return result;
                }
            }

            return null;
        }

        private async Task DownloadMacArchiveAsync(RobloxDeploymentTarget target)
        {
            string archiveName = target.BinaryType.GetMacArchiveName();

            Dictionary<string, object> archiveTag = new()
            {
                ["archive"] = archiveName
            };

            using ITelemetryOperation operation = telemetry.StartOperation("packages.download.mac", archiveTag);

            try
            {
                Log.Information("Downloading Roblox for Mac (Archive: {0}).", archiveName);

                byte[] archiveBytes = await DownloadPackageAsync(target, archiveName, expectedMd5: null) ?? throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "No download server provided the archive {0}.", archiveName));

                await File.WriteAllBytesAsync(layout.GetDownloadArchivePath(target.VersionHash), archiveBytes);

                Log.Information("Downloaded Roblox for Mac (Archive: {0}, Size: {1:0.0} MB).", archiveName, archiveBytes.Length / 1_048_576.0);

                telemetry.RecordDistribution("packages.download.mac.bytes", archiveBytes.Length, MeasurementUnit.Information.Byte, archiveTag);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not download Roblox for Mac (Archive: {0}).", archiveName);

                operation.MarkFailed();

                throw new InvalidOperationException("Could not download Roblox for Mac.", ex);
            }
        }

        private Task<string?> DownloadManifestAsync(RobloxDeploymentTarget target)
        {
            return DownloadFromRankedCdnsAsync("package list", baseUri => httpClient.GetStringOrNullAsync(RobloxPackageUris.ForManifest(target, baseUri)), manifest => manifest != null);
        }

        private Task<byte[]?> DownloadPackageAsync(RobloxDeploymentTarget target, string packageName, string? expectedMd5)
        {
            return DownloadFromRankedCdnsAsync("package " + packageName, baseUri => httpClient.GetBytesOrNullAsync(RobloxPackageUris.ForPackage(target, baseUri, packageName)), packageBytes => PackageChecksum.IsIntact(packageName, packageBytes, expectedMd5));
        }

        private async Task<int> DownloadPackageIntoArchiveAsync(RobloxDeploymentTarget target, string packageName, string? expectedMd5, PackageArchiveWriter archiveWriter)
        {
            try
            {
                Log.Debug("Downloading package (Package: {0}).", packageName);

                byte[] packageBytes = await DownloadPackageAsync(target, packageName, expectedMd5) ?? throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "No download server provided the package {0}.", packageName));

                await archiveWriter.AddPackageAsync(packageName, packageBytes);

                return packageBytes.Length;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not download package (Package: {0}).", packageName);

                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Could not download the package {0}.", packageName), ex);
            }
        }

        private async Task<long> DownloadPackagesIntoArchiveAsync(RobloxDeploymentTarget target, PackageManifest manifest, PackageArchiveWriter archiveWriter)
        {
            int maximumConcurrentDownloads = Math.Max(Environment.ProcessorCount, 8);
            long totalBytes = 0;
            int downloadedPackageCount = 0;
            using SemaphoreSlim downloadSlots = new(maximumConcurrentDownloads, maximumConcurrentDownloads);

            await Task.WhenAll(manifest.Packages.Select(async packageName =>
            {
                await downloadSlots.WaitAsync();

                try
                {
                    int packageBytes = await DownloadPackageIntoArchiveAsync(target, packageName, manifest.Md5Checksums.GetValueOrDefault(packageName), archiveWriter);

                    _ = Interlocked.Add(ref totalBytes, packageBytes);

                    Log.Information("Downloaded package {0} of {1} (Package: {2}, Size: {3:N0} KB).", Interlocked.Increment(ref downloadedPackageCount), manifest.Packages.Count, packageName, packageBytes / 1024);
                }
                finally
                {
                    _ = downloadSlots.Release();
                }
            }));

            return totalBytes;
        }

        private async Task DownloadWindowsPackagesAsync(RobloxDeploymentTarget target)
        {
            using ITelemetryOperation operation = telemetry.StartOperation("packages.download.windows");

            try
            {
                Log.Information("Downloading Roblox (Version: {0}).", target.VersionHash);

                PackageManifest manifest = PackageManifest.Parse(await DownloadManifestAsync(target));

                if (!manifest.IsValid)
                {
                    operation.MarkFailed("ManifestInvalid");

                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "The package list of the Roblox version {0} is missing or not valid.", target.VersionHash));
                }

                operation.SetTag("packageCount", manifest.Packages.Count);

                string archivePath = layout.GetDownloadArchivePath(target.VersionHash);

                ResilientFileSystem.DeleteFile(archivePath);

                using ZipArchive archive = await ZipFile.OpenAsync(archivePath, ZipArchiveMode.Create);
                using PackageArchiveWriter archiveWriter = new(archive, target.BinaryType == RobloxBinaryType.WindowsPlayer ? CompressionLevel.NoCompression : CompressionLevel.Fastest);

                await archiveWriter.AddTextEntryAsync(AppSettingsEntryName, AppSettingsXml);

                long totalBytes = await DownloadPackagesIntoArchiveAsync(target, manifest, archiveWriter);

                Log.Information("Downloaded Roblox (Version: {0}, Packages: {1}, Size: {2:0.0} MB).", target.VersionHash, manifest.Packages.Count, totalBytes / 1_048_576.0);

                telemetry.RecordDistribution("packages.download.windows.bytes", totalBytes, MeasurementUnit.Information.Byte, new Dictionary<string, object>
                {
                    ["packageCount"] = manifest.Packages.Count
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not download Roblox (Version: {0}).", target.VersionHash);

                operation.MarkFailed();

                throw new InvalidOperationException("Could not download Roblox for Windows.", ex);
            }
        }

        public Task DownloadAsync(RobloxDeploymentTarget target)
        {
            return target.BinaryType.IsMac() ? DownloadMacArchiveAsync(target) : DownloadWindowsPackagesAsync(target);
        }
    }
}