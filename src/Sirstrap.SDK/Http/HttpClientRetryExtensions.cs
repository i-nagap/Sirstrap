namespace Sirstrap.SDK.Http
{
    public static class HttpClientRetryExtensions
    {
        private const int MaximumAttempts = 3;

        private static async Task<T?> GetOrNullAsync<T>(Func<Task<T>> sendRequest, string uri) where T : class
        {
            for (int attempt = 1; attempt <= MaximumAttempts; attempt++)
            {
                try
                {
                    return await sendRequest();
                }
                catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    Log.Debug(ex, "The server does not have the requested file (URL: {0}, HTTP status: 404).", uri);

                    return null;
                }
                catch (Exception ex) when (attempt < MaximumAttempts)
                {
                    TimeSpan retryDelay = TimeSpan.FromSeconds(Math.Pow(2, attempt));

                    Log.Warning(ex, "Download failed, retrying in {0} seconds (URL: {1}, Attempt: {2} of {3}).", retryDelay.TotalSeconds, uri, attempt, MaximumAttempts);

                    await Task.Delay(retryDelay);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Download failed after {0} attempts (URL: {1}).", MaximumAttempts, uri);
                }
            }

            return null;
        }

        public static Task<byte[]?> GetBytesOrNullAsync(this HttpClient httpClient, string uri)
        {
            return GetOrNullAsync(() => httpClient.GetByteArrayAsync(uri), uri);
        }

        public static Task<string?> GetStringOrNullAsync(this HttpClient httpClient, string uri)
        {
            return GetOrNullAsync(() => httpClient.GetStringAsync(uri), uri);
        }
    }
}