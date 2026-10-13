namespace Sirstrap.Core.Activity
{
    public sealed class ServerLocationLookup(HttpClient httpClient, ITelemetry telemetry)
    {
        private static readonly TimeSpan _requestTimeout = TimeSpan.FromSeconds(5);

        private readonly ConcurrentDictionary<string, string> _locationsByAddress = new();

        private static string ReadString(JsonElement root, string propertyName)
        {
            return root.TryGetProperty(propertyName, out JsonElement property) && property.ValueKind == JsonValueKind.String ? property.GetString() ?? string.Empty : string.Empty;
        }

        private void RecordDuration(Stopwatch stopwatch, string outcome)
        {
            telemetry.RecordDistribution("server.location.duration", stopwatch.Elapsed.TotalMilliseconds, MeasurementUnit.Duration.Millisecond, new Dictionary<string, object>
            {
                ["outcome"] = outcome
            });
        }

        public void ClearCache()
        {
            _locationsByAddress.Clear();
        }

        public static string FormatLocation(string ipInfoJson)
        {
            try
            {
                using JsonDocument document = JsonDocument.Parse(ipInfoJson);

                string city = ReadString(document.RootElement, "city");
                string region = ReadString(document.RootElement, "region");
                string country = ReadString(document.RootElement, "country");

                return city switch
                {
                    _ when string.IsNullOrWhiteSpace(region) || string.IsNullOrWhiteSpace(country) => string.Empty,
                    _ when string.IsNullOrWhiteSpace(city) || city.Equals(region, StringComparison.InvariantCultureIgnoreCase) => $"{region}, {country}",
                    _ => $"{city}, {region}, {country}"
                };
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not read the server location response.");

                return string.Empty;
            }
        }

        public async Task<string> GetLocationAsync(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                return string.Empty;
            }

            if (_locationsByAddress.TryGetValue(ipAddress, out string? cachedLocation))
            {
                return cachedLocation;
            }

            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                using CancellationTokenSource timeout = new(_requestTimeout);
                using HttpResponseMessage response = await httpClient.GetAsync($"https://ipinfo.io/{ipAddress}/json", timeout.Token);

                if (!response.IsSuccessStatusCode)
                {
                    Log.Warning("Could not look up the server location (IP: {0}, HTTP status: {1}).", ipAddress, (int)response.StatusCode);

                    RecordDuration(stopwatch, $"Http{(int)response.StatusCode}");

                    return string.Empty;
                }

                string location = FormatLocation(await response.Content.ReadAsStringAsync(timeout.Token));

                _locationsByAddress[ipAddress] = location;

                if (string.IsNullOrEmpty(location))
                {
                    Log.Warning("Could not determine where the Roblox server is located (IP: {0}).", ipAddress);
                }
                else
                {
                    Log.Information("Connected to a Roblox server in {0} (IP: {1}).", location, ipAddress);
                }

                RecordDuration(stopwatch, string.IsNullOrEmpty(location) ? "Unparsed" : "Success");

                return location;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not look up the server location (IP: {0}).", ipAddress);

                RecordDuration(stopwatch, ex.GetType().Name);

                return string.Empty;
            }
        }
    }
}