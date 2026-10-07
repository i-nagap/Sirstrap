namespace Sirstrap.Core.Update
{
    public sealed class SirstrapReleaseClient(HttpClient httpClient)
    {
#pragma warning disable S1075 // URIs should not be hardcoded - External API endpoint.
        public const string ARUBA_RELEASES_URI = "https://sirstrap.com/releases/";

        private const string GITHUB_RELEASES_URI_PREFIX = "https://api.github.com/repos/";
#pragma warning restore S1075

        public async Task<IReadOnlyList<GitHubRelease>> GetReleasesAsync()
        {
            try
            {
                var releases = await GetReleasesFromUriAsync(ARUBA_RELEASES_URI);

                if (releases is not null)
                    return releases;

                Log.Warning("[*] No releases on {ReleasesUri}, falling back to GitHub.", ARUBA_RELEASES_URI);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[*] Failed to fetch the Sirstrap releases from {ReleasesUri}, falling back to GitHub.", ARUBA_RELEASES_URI);
            }

            return await GitHubAccounts.ResolveAsync(account => GetReleasesFromUriAsync($"{GITHUB_RELEASES_URI_PREFIX}{account}/sirstrap/releases")) ?? [];
        }

        private async Task<IReadOnlyList<GitHubRelease>?> GetReleasesFromUriAsync(string uri)
        {
            using var jsonDocument = JsonDocument.Parse(await httpClient.GetStringAsync(uri));

            IReadOnlyList<GitHubRelease> releases = [.. jsonDocument.RootElement.EnumerateArray().Select(GitHubRelease.FromJson)];

            return releases.Count > 0 ? releases : null;
        }
    }
}
