namespace Sirstrap.SDK.Weao
{
    public sealed class WeaoApiClient : IDisposable
    {
        public const string UserAgent = "WEAO-3PService";

        private static readonly string[] _defaultMirrors = ["https://weao.xyz", "https://whatexpsare.online", "https://whatexploitsaretra.sh", "https://weao.gg"];

        private readonly HttpClient _httpClient;

        private readonly IReadOnlyList<string> _mirrors;

        private readonly bool _ownsHttpClient;

        public WeaoApiClient(IReadOnlyList<string>? mirrors = null) : this(CreateHttpClient(), ownsHttpClient: true, mirrors)
        {
        }

        public WeaoApiClient(HttpClient httpClient, bool ownsHttpClient = false, IReadOnlyList<string>? mirrors = null)
        {
            ArgumentNullException.ThrowIfNull(httpClient);

            _httpClient = httpClient;
            _ownsHttpClient = ownsHttpClient;
            _mirrors = mirrors is
            {
                Count: > 0
            } ? mirrors : _defaultMirrors;

            if (!_httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(UserAgent))
            {
                _httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
            }
        }

        private static HttpClient CreateHttpClient()
        {
            return new(new SocketsHttpHandler
            {
                AutomaticDecompression = DecompressionMethods.All,
                ConnectTimeout = TimeSpan.FromSeconds(10)
            })
            {
                Timeout = TimeSpan.FromSeconds(20)
            };
        }

        private async Task<T> GetFromAnyMirrorAsync<T>(string path, JsonTypeInfo<T> typeInfo, CancellationToken cancellationToken)
        {
            Exception? lastError = null;

            for (int index = 0; index < _mirrors.Count; index++)
            {
                string uri = _mirrors[index] + path;

                try
                {
                    return await GetFromMirrorAsync(uri, typeInfo, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
                {
                    lastError = ex;

                    Log.Warning(ex, "Could not reach WEAO, trying the next mirror (URL: {0}, Mirror: {1} of {2}).", uri, index + 1, _mirrors.Count);
                }
            }

            throw new WeaoRequestException("No WEAO mirror responded.", statusCode: null, innerException: lastError);
        }

        private async Task<T> GetFromMirrorAsync<T>(string uri, JsonTypeInfo<T> typeInfo, CancellationToken cancellationToken)
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new WeaoRateLimitException(await TryReadRateLimitAsync(response, cancellationToken).ConfigureAwait(false));
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new WeaoRequestException(string.Format(CultureInfo.InvariantCulture, "WEAO returned HTTP {0} {1} (URL: {2}).", (int)response.StatusCode, response.ReasonPhrase, uri), response.StatusCode);
            }

            await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                return await JsonSerializer.DeserializeAsync(stream, typeInfo, cancellationToken).ConfigureAwait(false) ?? throw new WeaoRequestException(string.Format(CultureInfo.InvariantCulture, "WEAO returned an empty response (URL: {0}).", uri), response.StatusCode);
            }
            catch (JsonException ex)
            {
                throw new WeaoRequestException(string.Format(CultureInfo.InvariantCulture, "Could not read the WEAO response (URL: {0}).", uri), response.StatusCode, ex);
            }
        }

        private static async Task<RateLimitInfo?> TryReadRateLimitAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            try
            {
                await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

                WeaoErrorResponse? error = await JsonSerializer.DeserializeAsync(stream, WeaoJsonContext.Default.WeaoErrorResponse, cancellationToken).ConfigureAwait(false);

                return error?.RateLimitInfo;
            }
            catch (Exception ex) when (ex is JsonException or HttpRequestException or IOException)
            {
                return null;
            }
        }

        public void Dispose()
        {
            if (_ownsHttpClient)
            {
                _httpClient.Dispose();
            }
        }

        public Task<RobloxVersions> GetCurrentVersionsAsync(CancellationToken cancellationToken = default)
        {
            return GetFromAnyMirrorAsync("/api/versions/current", WeaoJsonContext.Default.RobloxVersions, cancellationToken);
        }

        public Task<ExploitStatus> GetExploitAsync(string exploitName, CancellationToken cancellationToken = default)
        {
            return string.IsNullOrWhiteSpace(exploitName) ? throw new ArgumentException("Exploit name must be provided.", nameof(exploitName)) : GetFromAnyMirrorAsync($"/api/status/exploits/{Uri.EscapeDataString(exploitName)}", WeaoJsonContext.Default.ExploitStatus, cancellationToken);
        }

        public Task<List<ExploitStatus>> GetExploitsAsync(CancellationToken cancellationToken = default)
        {
            return GetFromAnyMirrorAsync("/api/status/exploits", WeaoJsonContext.Default.ListExploitStatus, cancellationToken);
        }

        public Task<RobloxVersions> GetFutureVersionsAsync(CancellationToken cancellationToken = default)
        {
            return GetFromAnyMirrorAsync("/api/versions/future", WeaoJsonContext.Default.RobloxVersions, cancellationToken);
        }

        public Task<RobloxVersions> GetPastVersionsAsync(CancellationToken cancellationToken = default)
        {
            return GetFromAnyMirrorAsync("/api/versions/past", WeaoJsonContext.Default.RobloxVersions, cancellationToken);
        }

        public Task<SuncData> GetSuncAsync(ExploitStatus exploit, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(exploit);

            return exploit.Sunc?.Scrap is not
            {
            } scrap || exploit.Sunc?.Key is not
            {
            } key ? throw new WeaoException(string.Format(CultureInfo.InvariantCulture, "The executor {0} does not provide sUNC credentials.", exploit.Title)) : GetSuncAsync(scrap, key, cancellationToken);
        }

        public Task<SuncData> GetSuncAsync(string scrap, string key, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(scrap);
            ArgumentException.ThrowIfNullOrWhiteSpace(key);

            return GetFromAnyMirrorAsync($"/api/sunc?scrap={Uri.EscapeDataString(scrap)}&key={Uri.EscapeDataString(key)}", WeaoJsonContext.Default.SuncData, cancellationToken);
        }
    }
}