namespace Sirstrap.Core.Update
{
    public sealed class SirstrapReleaseClient(HttpClient httpClient)
    {
#pragma warning disable S1075 // URIs should not be hardcoded - External API endpoint.
        private const string RELEASES_URI = "https://sirstrap.com/releases/";
#pragma warning restore S1075

        public async Task<IReadOnlyList<SirstrapRelease>> GetReleasesAsync()
        {
            try
            {
                using var jsonDocument = JsonDocument.Parse(await httpClient.GetStringAsync(RELEASES_URI));

                return [.. jsonDocument.RootElement.EnumerateArray().Select(SirstrapRelease.FromJson)];
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[*] Failed to fetch the Sirstrap releases from {ReleasesUri}.", RELEASES_URI);

                return [];
            }
        }
    }
}
