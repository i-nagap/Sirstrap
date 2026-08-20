namespace Sirstrap.Core.Common
{
    public static class GitHubAccounts
    {
        public static IReadOnlyList<string> All { get; } = ["i-nagap", "massimopaganigh"];

        public static string Primary => All[0];

        public static string Repository => $"{Primary}/Sirstrap";

        public static async Task<T?> ResolveAsync<T>(Func<string, Task<T?>> attemptAsync) where T : class
        {
            foreach (var account in All)
            {
                try
                {
                    var result = await attemptAsync(account);

                    if (result is not null)
                        return result;

                    Log.Warning("[*] No result from the {Account} GitHub account, falling back to the next one.", account);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "[*] Request to the {Account} GitHub account failed, falling back to the next one.", account);
                }
            }

            return null;
        }
    }
}
