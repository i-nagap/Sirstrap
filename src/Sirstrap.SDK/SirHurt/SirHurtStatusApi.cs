namespace Sirstrap.SDK.SirHurt
{
    public sealed record SirHurtSupportedVersion(string RobloxVersion, bool IsOutdated);

    public sealed class SirHurtStatusApi(HttpClient httpClient)
    {
        private const int OutdatedAfterDays = 10;

        private const string StatusEndpoint = "https://sirhurt.net/status/fetch_2.php?exploit=SirHurt%20V5";

        private static bool IsOutdated(JsonElement sirHurtStatus)
        {
            if (!sirHurtStatus.TryGetProperty("last_update_unix", out JsonElement lastUpdateElement))
            {
                return false;
            }

            DateTimeOffset lastUpdate = DateTimeOffset.FromUnixTimeSeconds(lastUpdateElement.GetInt64());

            return (DateTimeOffset.UtcNow - lastUpdate).TotalDays > OutdatedAfterDays;
        }

        public async Task<SirHurtSupportedVersion?> GetSupportedRobloxVersionAsync()
        {
            try
            {
                using JsonDocument response = JsonDocument.Parse(await httpClient.GetStringAsync(StatusEndpoint));

                if (response.RootElement.ValueKind != JsonValueKind.Array || response.RootElement.GetArrayLength() == 0)
                {
                    Log.Warning("SirHurt returned an unexpected response (URL: {0}, Expected: JSON array).", StatusEndpoint);

                    return null;
                }

                if (!response.RootElement[0].TryGetProperty("SirHurt V5", out JsonElement sirHurtStatus))
                {
                    Log.Warning("SirHurt did not include its status in the response (URL: {0}, Field: SirHurt V5).", StatusEndpoint);

                    return null;
                }

                string? robloxVersion = sirHurtStatus.TryGetProperty("roblox_version", out JsonElement versionElement) ? versionElement.GetString() : null;

                if (string.IsNullOrEmpty(robloxVersion))
                {
                    Log.Warning("SirHurt did not include the supported Roblox version in the response (URL: {0}, Field: roblox_version).", StatusEndpoint);

                    return null;
                }

                return new SirHurtSupportedVersion(robloxVersion, IsOutdated(sirHurtStatus));
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not reach SirHurt to get the supported Roblox version (URL: {0}).", StatusEndpoint);

                return null;
            }
        }
    }
}