namespace Sirstrap.Core.Common
{
    [SuppressMessage("Major Code Smell", "S1075", Justification = "Codeberg is a fixed endpoint.")]
    public sealed class CodebergAccounts(HttpClient httpClient)
    {
        public const string ApiUri = "https://codeberg.org/api/v1/";

        private readonly Lazy<Task<string>> _reachableAccount = new(() => FindReachableAccountAsync(httpClient));

        private static async Task<string> FindReachableAccountAsync(HttpClient httpClient)
        {
            return await FirstResultAsync(async account =>
            {
                using HttpResponseMessage response = await httpClient.GetAsync($"{ApiUri}users/{account}", HttpCompletionOption.ResponseHeadersRead);

                return response.IsSuccessStatusCode ? account : null;
            }) ?? Primary;
        }

        public static async Task<T?> FirstResultAsync<T>(Func<string, Task<T?>> tryAccountAsync) where T : class
        {
            foreach (string account in All)
            {
                try
                {
                    T? result = await tryAccountAsync(account);

                    if (result is not null)
                    {
                        return result;
                    }

                    Log.Warning("Codeberg returned no data (Account: {0}).", account);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Could not get a response from Codeberg (Account: {0}).", account);
                }
            }

            return null;
        }

        public Task<string> GetReachableAccountAsync()
        {
            return _reachableAccount.Value;
        }

        public static IReadOnlyList<string> All { get; } = ["Sirstrap"];

        public static string Primary => All[0];
    }
}