namespace Sirstrap.Core.Deployment
{
    public sealed class PackageManager(HttpClient httpClient, IRobloxUriFactory robloxUriFactory, IPathManager pathManager, IPerformanceTelemetry performanceTelemetry, SirstrapConfiguration sirstrapConfiguration) : IPackageManager
    {
        private const string APP_SETTINGS_XML = """<?xml version="1.0" encoding="UTF-8"?><Settings><ContentFolder>content</ContentFolder><BaseUrl>http://www.roblox.com</BaseUrl></Settings>""";

        public async Task DownloadMacArchiveAsync(Configuration configuration)
        {
            string archiveName = configuration.IsMacPlayer() ? "RobloxPlayer.zip" : "RobloxStudioApp.zip";

            using ITelemetryScope scope = performanceTelemetry.Measure("packages.download.mac", new Dictionary<string, object>
            {
                ["archive"] = archiveName
            });

            try
            {
                Log.Information("[*] Downloading the Mac archive {ArchiveName}...", archiveName);

                byte[]? archiveBytes = await GetPackageBytesAsync(configuration, archiveName)
                    ?? throw new InvalidOperationException($"No bytes were downloaded for the package for Mac: {archiveName}.");

                int byteCount = archiveBytes.Length;

                await File.WriteAllBytesAsync(pathManager.GetOutputPath(configuration), archiveBytes);

                Log.Information("[*] Downloaded the Mac archive {ArchiveName}.", archiveName);

                performanceTelemetry.RecordDistribution("packages.download.mac.bytes", byteCount, "byte", new Dictionary<string, object>
                {
                    ["archive"] = archiveName
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to download the Mac archive {ArchiveName}.", archiveName);

                scope.MarkFailed();

                throw new InvalidOperationException("An error occurred while downloading the package for Mac.", ex);
            }
        }

        public async Task DownloadWindowsArchiveAsync(Configuration configuration)
        {
            using ITelemetryScope scope = performanceTelemetry.Measure("packages.download.windows");

            try
            {
                Log.Information("[*] Downloading the Windows packages...");

                Manifest manifest = ManifestParser.Parse(await GetManifestContentAsync(configuration));

                if (!manifest.IsValid)
                {
                    scope.MarkFailed("ManifestInvalid");

                    throw new InvalidOperationException($"The manifest for the version {configuration.VersionHash} is unavailable or invalid.");
                }

                int packageCount = manifest.Packages.Count;

                scope.SetTag("packageCount", packageCount);

                string outputPath = pathManager.GetOutputPath(configuration);

                FileSystemOperations.DeleteFile(outputPath);

                using ZipArchive archive = await ZipFile.OpenAsync(outputPath, ZipArchiveMode.Create);
                using PackageArchiveWriter archiveWriter = new(archive, GetEntryCompressionLevel(configuration));

                await archiveWriter.AddTextEntryAsync("AppSettings.xml", APP_SETTINGS_XML);

                long totalBytes = await DownloadPackagesAsync(configuration, manifest, archiveWriter);

                Log.Information("[*] Downloaded all the Windows packages.");

                performanceTelemetry.RecordDistribution("packages.download.windows.bytes", totalBytes, "byte", new Dictionary<string, object>
                {
                    ["packageCount"] = packageCount
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to download the Windows packages.");

                scope.MarkFailed();

                throw new InvalidOperationException("An error occurred while downloading packages for Windows.", ex);
            }
        }

        private async Task<int> DownloadPackageAsync(Configuration configuration, string package, string? expectedChecksum, PackageArchiveWriter archiveWriter)
        {
            try
            {
                Log.Information("[*] Downloading the package {Package}...", package);

                byte[]? packageBytes = await GetPackageBytesAsync(configuration, package, expectedChecksum)
                    ?? throw new InvalidOperationException($"No bytes were downloaded for the package: {package}.");

                int byteCount = packageBytes.Length;

                await archiveWriter.AddPackageAsync(package, packageBytes);

                Log.Information("[*] Downloaded the package {Package}.", package);

                return byteCount;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to download the package {Package}.", package);

                throw new InvalidOperationException($"An error occurred while downloading the package: {package}.", ex);
            }
        }

        private async Task<long> DownloadPackagesAsync(Configuration configuration, Manifest manifest, PackageArchiveWriter archiveWriter)
        {
            int downloadConcurrency = Math.Max(Environment.ProcessorCount, 8);

            using SemaphoreSlim semaphore = new(downloadConcurrency, downloadConcurrency);
            long totalBytes = 0;

            IEnumerable<Task> downloadTasks = manifest.Packages.Select(async package =>
            {
                await semaphore.WaitAsync();

                try
                {
                    int bytes = await DownloadPackageAsync(configuration, package, manifest.Checksums.GetValueOrDefault(package), archiveWriter);

                    Interlocked.Add(ref totalBytes, bytes);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(downloadTasks);

            return totalBytes;
        }

        private static CompressionLevel GetEntryCompressionLevel(Configuration configuration)
            => configuration.IsWindowsPlayer()
                ? CompressionLevel.NoCompression
                : CompressionLevel.Fastest;

        private async Task<byte[]?> GetPackageBytesAsync(Configuration configuration, string package, string? expectedChecksum = null)
        {
            byte[]? packageBytes = await HttpClientExtension.GetByteArrayAsync(httpClient, robloxUriFactory.GetPackageUri(configuration, package));

            if (IsIntact(package, packageBytes, expectedChecksum))
                return packageBytes;

            string primaryCdnUri = sirstrapConfiguration.ResolvedRobloxCdnUri;

            foreach (string fallbackCdnUri in sirstrapConfiguration.ResolvedRobloxCdnUris)
            {
                if (fallbackCdnUri.Equals(primaryCdnUri, StringComparison.OrdinalIgnoreCase))
                    continue;

                Log.Warning("[!] The package {Package} is unavailable on {PrimaryCdnUri}, trying {FallbackCdnUri}...", package, primaryCdnUri, fallbackCdnUri);

                packageBytes = await HttpClientExtension.GetByteArrayAsync(httpClient, robloxUriFactory.GetPackageUri(configuration, package, fallbackCdnUri));

                if (IsIntact(package, packageBytes, expectedChecksum))
                    return packageBytes;
            }

            return null;
        }

        private static bool IsIntact(string package, byte[]? packageBytes, string? expectedChecksum)
        {
            if (packageBytes == null)
                return false;

            if (expectedChecksum == null)
                return true;

#pragma warning disable S4790 // Use a stronger hashing algorithm - Roblox manifests only publish MD5 checksums.
            string actualChecksum = Convert.ToHexStringLower(System.Security.Cryptography.MD5.HashData(packageBytes));
#pragma warning restore S4790

            if (actualChecksum == expectedChecksum)
                return true;

            Log.Warning("[!] The package {Package} failed the checksum verification (expected {ExpectedChecksum}, got {ActualChecksum}).", package, expectedChecksum, actualChecksum);

            return false;
        }

        private async Task<string?> GetManifestContentAsync(Configuration configuration)
        {
            string? manifestContent = await HttpClientExtension.GetStringAsync(httpClient, robloxUriFactory.GetManifestUri(configuration));

            if (manifestContent != null)
                return manifestContent;

            string primaryCdnUri = sirstrapConfiguration.ResolvedRobloxCdnUri;

            foreach (string fallbackCdnUri in sirstrapConfiguration.ResolvedRobloxCdnUris)
            {
                if (fallbackCdnUri.Equals(primaryCdnUri, StringComparison.OrdinalIgnoreCase))
                    continue;

                Log.Warning("[!] The manifest is unavailable on {PrimaryCdnUri}, trying {FallbackCdnUri}...", primaryCdnUri, fallbackCdnUri);

                manifestContent = await HttpClientExtension.GetStringAsync(httpClient, robloxUriFactory.GetManifestUri(configuration, fallbackCdnUri));

                if (manifestContent != null)
                    return manifestContent;
            }

            return null;
        }
    }
}
