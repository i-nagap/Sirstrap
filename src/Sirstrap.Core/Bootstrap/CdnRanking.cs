namespace Sirstrap.Core.Bootstrap
{
    public sealed class CdnRanking
    {
        public IReadOnlyList<string> BaseUrisFastestFirst { get; set; } = [RobloxCdn.DefaultBaseUri];

        public string PrimaryBaseUri { get; set; } = RobloxCdn.DefaultBaseUri;
    }
}