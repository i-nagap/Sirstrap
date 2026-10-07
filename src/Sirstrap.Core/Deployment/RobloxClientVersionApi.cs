namespace Sirstrap.Core.Deployment
{
    public sealed class RobloxClientVersionApi(HttpClient httpClient)
    {
#pragma warning disable S1075 // URIs should not be hardcoded - External API endpoint.
        private static readonly string[] ROBLOX_API_URIS =
        [
            "https://clientsettingscdn.roblox.com/v2/client-version/WindowsPlayer",
            "https://clientsettings.roblox.com/v2/client-version/WindowsPlayer"
        ];
#pragma warning restore S1075

        public async Task<string> GetVersionAsync()
        {
            foreach (string uri in ROBLOX_API_URIS)
            {
                string version = await GetVersionAsync(uri);

                if (!string.IsNullOrEmpty(version))
                    return version;
            }

            return string.Empty;
        }

        private async Task<string> GetVersionAsync(string uri)
        {
            try
            {
                string response = await httpClient.GetStringAsync(uri);

                using JsonDocument jsonDocument = JsonDocument.Parse(response);

                if (jsonDocument.RootElement.TryGetProperty("clientVersionUpload", out var version))
                    return version.GetString() ?? string.Empty;

                Log.Error("[!] The clientVersionUpload field was not found in the Roblox API response from {Uri}.", uri);

                return string.Empty;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to retrieve the Roblox version from {Uri}.", uri);

                return string.Empty;
            }
        }
    }
}
