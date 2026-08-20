namespace Sirstrap.Core.Update
{
    public sealed class GitHubReleaseClient(HttpClient httpClient)
    {
#pragma warning disable S1075 // URIs should not be hardcoded - External API endpoint.
        private const string RELEASES_URI_PREFIX = "https://api.github.com/repos/";
#pragma warning restore S1075

        public async Task<IReadOnlyList<GitHubRelease>> GetReleasesAsync()
            => await GitHubAccounts.ResolveAsync(GetReleasesAsync) ?? [];

        private async Task<IReadOnlyList<GitHubRelease>?> GetReleasesAsync(string account)
        {
            using var jsonDocument = JsonDocument.Parse(await httpClient.GetStringAsync($"{RELEASES_URI_PREFIX}{account}/sirstrap/releases"));

            IReadOnlyList<GitHubRelease> releases = [.. jsonDocument.RootElement.EnumerateArray().Select(GitHubRelease.FromJson)];

            return releases.Count > 0 ? releases : null;
        }
    }
}
