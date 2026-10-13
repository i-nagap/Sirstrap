namespace Sirstrap.Core.Update
{
    public sealed record SirstrapReleaseAsset(string Name, string DownloadUri);

    public sealed record SirstrapRelease(string TagName, bool IsDraft, string Changelog, IReadOnlyList<SirstrapReleaseAsset> Assets)
    {
        private static List<SirstrapReleaseAsset> ReadAssets(JsonElement release)
        {
            return !release.TryGetProperty("assets", out JsonElement assets) ? [] : [.. assets.EnumerateArray().Select(asset => new SirstrapReleaseAsset(ReadString(asset, "fileName", "name"), ReadString(asset, "downloadUrl", "browser_download_url"))).Where(asset => !string.IsNullOrEmpty(asset.Name))];
        }

        private static string ReadString(JsonElement element, params string[] propertyNames)
        {
            return TryGetFirstProperty(element, out JsonElement property, propertyNames) ? property.GetString() ?? string.Empty : string.Empty;
        }

        private static bool TryGetFirstProperty(JsonElement element, out JsonElement property, params string[] propertyNames)
        {
            foreach (string propertyName in propertyNames)
            {
                if (element.TryGetProperty(propertyName, out property))
                {
                    return true;
                }
            }

            property = default;

            return false;
        }

        public string FindAssetDownloadUri(string assetName)
        {
            return Assets.FirstOrDefault(asset => asset.Name.Equals(assetName, StringComparison.OrdinalIgnoreCase))?.DownloadUri ?? string.Empty;
        }

        public static SirstrapRelease FromJson(JsonElement release)
        {
            bool isDraft = TryGetFirstProperty(release, out JsonElement draftProperty, "isDraft", "draft") && draftProperty.GetBoolean();

            return new SirstrapRelease(ReadString(release, "tagName", "tag_name"), isDraft, ReadString(release, "changelog", "body"), ReadAssets(release));
        }
    }
}