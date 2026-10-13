namespace Sirstrap.SDK.Roblox
{
    public sealed class PackageArchiveWriter(ZipArchive archive, CompressionLevel packageCompressionLevel) : IDisposable
    {
        private readonly SemaphoreSlim _archiveLock = new(1, 1);

        private async Task AddExtractedPackageAsync(string root, byte[] packageBytes)
        {
            using MemoryStream packageStream = new(packageBytes);
            using ZipArchive packageArchive = new(packageStream, ZipArchiveMode.Read);

            foreach (ZipArchiveEntry entry in packageArchive.Entries.Where(entry => !string.IsNullOrEmpty(entry.FullName)))
            {
                await WriteEntryAsync($"{root}{entry.FullName.Replace('\\', '/')}", await ReadEntryAsync(entry), packageCompressionLevel);
            }
        }

        private static async Task<byte[]> ReadEntryAsync(ZipArchiveEntry entry)
        {
            byte[] entryBytes = new byte[checked((int)entry.Length)];
            await using Stream entryStream = await entry.OpenAsync();

            await entryStream.ReadExactlyAsync(entryBytes);

            return entryBytes;
        }

        private async Task WriteEntryAsync(string entryPath, byte[] entryBytes, CompressionLevel compressionLevel)
        {
            await _archiveLock.WaitAsync();

            try
            {
                await using Stream entryStream = await archive.CreateEntry(entryPath, compressionLevel).OpenAsync();

                await entryStream.WriteAsync(entryBytes);
            }
            finally
            {
                _ = _archiveLock.Release();
            }
        }

        public async Task AddPackageAsync(string packageName, byte[] packageBytes)
        {
            if (PackageRootCatalog.TryGetRoot(packageName, out string? root))
            {
                await AddExtractedPackageAsync(root, packageBytes);
            }
            else
            {
                await WriteEntryAsync(packageName, packageBytes, packageCompressionLevel);
            }
        }

        public Task AddTextEntryAsync(string entryPath, string content)
        {
            return WriteEntryAsync(entryPath, Encoding.UTF8.GetBytes(content), CompressionLevel.Optimal);
        }

        public void Dispose()
        {
            _archiveLock.Dispose();
        }
    }
}