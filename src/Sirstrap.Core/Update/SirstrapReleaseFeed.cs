namespace Sirstrap.Core.Update
{
    [SuppressMessage("Major Code Smell", "S1075", Justification = "The release feeds are fixed endpoints.")]
    public sealed class SirstrapReleaseFeed(HttpClient httpClient)
    {
        public const string SirstrapReleasesUri = "https://sirstrap.com/releases/";

        private async Task<IReadOnlyList<SirstrapRelease>?> GetReleasesFromAsync(string releasesUri)
        {
            using JsonDocument document = JsonDocument.Parse(await httpClient.GetStringAsync(releasesUri));

            List<SirstrapRelease> releases = [.. document.RootElement.EnumerateArray().Select(SirstrapRelease.FromJson)];

            return releases.Count > 0 ? releases : null;
        }

        public async Task<IReadOnlyList<SirstrapRelease>> GetReleasesAsync()
        {
            try
            {
                IReadOnlyList<SirstrapRelease>? releases = await GetReleasesFromAsync(SirstrapReleasesUri);

                if (releases is not null)
                {
                    return releases;
                }

                Log.Warning("No Sirstrap releases found on the website, checking Codeberg instead (URL: {0}).", SirstrapReleasesUri);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not reach the Sirstrap website, checking Codeberg instead (URL: {0}).", SirstrapReleasesUri);
            }

            return await CodebergAccounts.FirstResultAsync(account => GetReleasesFromAsync($"{CodebergAccounts.ApiUri}repos/{account}/Sirstrap/releases?limit=50")) ?? [];
        }
    }
}