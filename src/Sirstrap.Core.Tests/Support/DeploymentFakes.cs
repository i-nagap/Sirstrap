namespace Sirstrap.Core.Tests.Support
{
    public sealed class FakeSirstrapUpdateService : ISirstrapUpdateService
    {
        public int UpdateCalls { get; private set; }

        public Task<string> GetLatestChangelogAsync() => Task.FromResult(string.Empty);

        public Task UpdateAsync(SirstrapType sirstrapType, string[] args)
        {
            UpdateCalls++;

            return Task.CompletedTask;
        }
    }

    public sealed class FakeRobloxVersionService(string version, string? sourceVersion = null) : IRobloxVersionService
    {
        public int Calls { get; private set; }

        public int SourceCalls { get; private set; }

        public bool HasVersionOverride { get; init; }

        public Task<string> GetLatestVersionAsync()
        {
            Calls++;

            return Task.FromResult(version);
        }

        public Task<string> GetSourceVersionAsync()
        {
            SourceCalls++;

            return Task.FromResult(sourceVersion ?? version);
        }
    }

    public sealed class FakePackageManager : IPackageManager
    {
        public int WindowsCalls { get; private set; }

        public int MacCalls { get; private set; }

        public string? FailingVersionHash { get; init; }

        public Task DownloadMacArchiveAsync(Configuration configuration)
        {
            MacCalls++;

            ThrowIfFailing(configuration);

            return Task.CompletedTask;
        }

        public Task DownloadWindowsArchiveAsync(Configuration configuration)
        {
            WindowsCalls++;

            ThrowIfFailing(configuration);

            return Task.CompletedTask;
        }

        private void ThrowIfFailing(Configuration configuration)
        {
            if (configuration.VersionHash.Equals(FailingVersionHash, StringComparison.Ordinal))
                throw new InvalidOperationException($"An error occurred while downloading packages for the version: {configuration.VersionHash}.");
        }
    }

    public sealed class FakeCdnResolver : ICdnResolver
    {
        public int Calls { get; private set; }

        public Task<string> ResolveAsync(Configuration configuration, CancellationToken cancellationToken = default)
        {
            Calls++;

            return Task.FromResult(RobloxCdnService.DefaultBaseUri);
        }
    }

    public sealed class FakeInstaller : IInstaller
    {
        public int Calls { get; private set; }

        public void Install(Configuration configuration) => Calls++;
    }

    public sealed class FakeRobloxLauncher(bool result) : IRobloxLauncher
    {
        public int Calls { get; private set; }

        public bool Launch(Configuration configuration)
        {
            Calls++;

            return result;
        }
    }
}
