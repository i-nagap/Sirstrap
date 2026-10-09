namespace Sirstrap.Core.Activity
{
    public sealed class ServerLocationService(HttpClient httpClient, IPerformanceTelemetry performanceTelemetry) : IServerLocationService
    {
        private static readonly TimeSpan _requestTimeout = TimeSpan.FromSeconds(5);
        private readonly ConcurrentDictionary<string, string> _locationCache = new();

        public void ClearCache() => _locationCache.Clear();

        public async Task<string> GetServerLocationAsync(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                return string.Empty;

            if (_locationCache.TryGetValue(ipAddress, out var cachedLocation))
                return cachedLocation;

            var stopwatch = Stopwatch.StartNew();

            try
            {
                using var timeout = new CancellationTokenSource(_requestTimeout);
                var response = await httpClient.GetAsync($"https://ipinfo.io/{ipAddress}/json", timeout.Token);

                if (!response.IsSuccessStatusCode)
                {
                    RecordDuration(stopwatch, $"Http{(int)response.StatusCode}");

                    return string.Empty;
                }

                var location = ParseLocation(await response.Content.ReadAsStringAsync(timeout.Token));

                _locationCache[ipAddress] = location;

                Log.Information("[*] Resolved the server location for IP {IpAddress}: {Location}.", ipAddress, location);
                RecordDuration(stopwatch, string.IsNullOrEmpty(location) ? "Unparsed" : "Success");

                return location;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[!] Failed to resolve the server location for IP {IpAddress}.", ipAddress);

                RecordDuration(stopwatch, ex.GetType().Name);

                return string.Empty;
            }
        }

        #region PRIVATE METHODS
        private static string GetString(JsonElement root, string propertyName) => root.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() ?? string.Empty : string.Empty;

        private void RecordDuration(Stopwatch stopwatch, string outcome)
            => performanceTelemetry.RecordDistribution("server.location.duration", stopwatch.Elapsed.TotalMilliseconds, "millisecond", new Dictionary<string, object> { ["outcome"] = outcome });

        private static string ParseLocation(string json)
        {
            try
            {
                using var jsonDocument = JsonDocument.Parse(json);
                var root = jsonDocument.RootElement;
                var city = GetString(root, "city");
                var region = GetString(root, "region");
                var country = GetString(root, "country");

                if (string.IsNullOrWhiteSpace(region)
                    || string.IsNullOrWhiteSpace(country))
                    return string.Empty;

                if (string.IsNullOrWhiteSpace(city)
                    || city.Equals(region, StringComparison.InvariantCultureIgnoreCase))
                    return $"{region}, {country}";

                return $"{city}, {region}, {country}";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[!] Failed to parse the server location response.");

                return string.Empty;
            }
        }
        #endregion
    }
}
