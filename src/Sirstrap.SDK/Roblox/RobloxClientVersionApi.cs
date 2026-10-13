namespace Sirstrap.SDK.Roblox
{
    public sealed class RobloxClientVersionApi(HttpClient httpClient)
    {
        private static readonly string[] _endpoints = ["https://clientsettingscdn.roblox.com/v2/client-version/WindowsPlayer", "https://clientsettings.roblox.com/v2/client-version/WindowsPlayer"];

        private async Task<string> GetVersionFromAsync(string endpoint)
        {
            try
            {
                using JsonDocument response = JsonDocument.Parse(await httpClient.GetStringAsync(endpoint));

                if (response.RootElement.TryGetProperty("clientVersionUpload", out JsonElement version))
                {
                    return version.GetString() ?? string.Empty;
                }

                Log.Warning("Roblox did not include the version in its response (URL: {0}, Field: clientVersionUpload).", endpoint);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not get the Roblox version from Roblox (URL: {0}).", endpoint);
            }

            return string.Empty;
        }

        public async Task<string> GetLatestVersionAsync()
        {
            foreach (string endpoint in _endpoints)
            {
                string version = await GetVersionFromAsync(endpoint);

                if (!string.IsNullOrEmpty(version))
                {
                    return version;
                }
            }

            return string.Empty;
        }
    }
}