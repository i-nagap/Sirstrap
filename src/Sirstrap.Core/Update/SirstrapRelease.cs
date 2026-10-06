namespace Sirstrap.Core.Update
{
    public sealed record SirstrapReleaseAsset(string Name, string DownloadUri);

    public sealed record SirstrapRelease(string TagName, bool IsDraft, string Body, IReadOnlyList<SirstrapReleaseAsset> Assets)
    {
        public static SirstrapRelease FromJson(JsonElement element)
        {
            var tagName = element.TryGetProperty("tag_name", out JsonElement tagNameElement)
                ? tagNameElement.GetString() ?? string.Empty
                : string.Empty;
            var isDraft = element.TryGetProperty("draft", out JsonElement draftElement) && draftElement.GetBoolean();
            var body = element.TryGetProperty("body", out JsonElement bodyElement)
                ? bodyElement.GetString() ?? string.Empty
                : string.Empty;

            return new SirstrapRelease(tagName, isDraft, body, ParseAssets(element));
        }

        public string FindAssetDownloadUri(string assetName)
            => Assets.FirstOrDefault(asset => asset.Name.Equals(assetName, StringComparison.OrdinalIgnoreCase))?.DownloadUri ?? string.Empty;

        private static List<SirstrapReleaseAsset> ParseAssets(JsonElement element)
        {
            List<SirstrapReleaseAsset> assets = [];

            if (!element.TryGetProperty("assets", out JsonElement assetsElement))
                return assets;

            foreach (JsonElement assetElement in assetsElement.EnumerateArray())
            {
                if (!assetElement.TryGetProperty("name", out JsonElement nameElement))
                    continue;

                var name = nameElement.GetString();

                if (string.IsNullOrEmpty(name))
                    continue;

                var downloadUri = assetElement.TryGetProperty("browser_download_url", out JsonElement uriElement)
                    ? uriElement.GetString() ?? string.Empty
                    : string.Empty;

                assets.Add(new SirstrapReleaseAsset(name, downloadUri));
            }

            return assets;
        }
    }
}
