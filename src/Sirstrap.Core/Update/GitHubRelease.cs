namespace Sirstrap.Core.Update
{
    public sealed record GitHubReleaseAsset(string Name, string DownloadUri);

    public sealed record GitHubRelease(string TagName, bool IsDraft, string Body, IReadOnlyList<GitHubReleaseAsset> Assets)
    {
        public static GitHubRelease FromJson(JsonElement element)
        {
            var tagName = GetString(element, "tagName", "tag_name");
            var isDraft = TryGetProperty(element, out JsonElement draftElement, "isDraft", "draft") && draftElement.GetBoolean();
            var body = GetString(element, "changelog", "body");

            return new GitHubRelease(tagName, isDraft, body, ParseAssets(element));
        }

        public string FindAssetDownloadUri(string assetName)
            => Assets.FirstOrDefault(asset => asset.Name.Equals(assetName, StringComparison.OrdinalIgnoreCase))?.DownloadUri ?? string.Empty;

        private static List<GitHubReleaseAsset> ParseAssets(JsonElement element)
        {
            List<GitHubReleaseAsset> assets = [];

            if (!element.TryGetProperty("assets", out JsonElement assetsElement))
                return assets;

            foreach (JsonElement assetElement in assetsElement.EnumerateArray())
            {
                var name = GetString(assetElement, "fileName", "name");

                if (string.IsNullOrEmpty(name))
                    continue;

                assets.Add(new GitHubReleaseAsset(name, GetString(assetElement, "downloadUrl", "browser_download_url")));
            }

            return assets;
        }

        private static string GetString(JsonElement element, params string[] propertyNames)
            => TryGetProperty(element, out JsonElement propertyElement, propertyNames) ? propertyElement.GetString() ?? string.Empty : string.Empty;

        private static bool TryGetProperty(JsonElement element, out JsonElement propertyElement, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                if (element.TryGetProperty(propertyName, out propertyElement))
                    return true;
            }

            propertyElement = default;

            return false;
        }
    }
}
