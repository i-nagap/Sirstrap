namespace Sirstrap.SDK.Roblox
{
    [SuppressMessage("Major Code Smell", "S1075", Justification = "Roblox CDN hosts are fixed endpoints.")]
    public static class RobloxCdn
    {
        public const string DefaultBaseUri = "https://setup.rbxcdn.com";

        public static string NormalizeBaseUri(string baseUri)
        {
            string trimmedBaseUri = baseUri.Trim().TrimEnd('/');

            if (string.IsNullOrEmpty(trimmedBaseUri))
            {
                return string.Empty;
            }

            if (!Uri.TryCreate(trimmedBaseUri, UriKind.Absolute, out Uri? uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) || string.IsNullOrWhiteSpace(uri.Host))
            {
                Log.Warning("Ignored the Roblox download server set in the settings because it is not a valid address (Value: {0}).", baseUri);

                return string.Empty;
            }

            return uri.GetLeftPart(UriPartial.Path).TrimEnd('/');
        }

        public static IReadOnlyList<string> KnownBaseUris { get; } = [DefaultBaseUri, "https://setup-aws.rbxcdn.com", "https://setup-ak.rbxcdn.com", "https://roblox-setup.cachefly.net", "https://s3.amazonaws.com/setup.roblox.com"];
    }
}