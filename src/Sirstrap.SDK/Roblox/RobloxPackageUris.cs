namespace Sirstrap.SDK.Roblox
{
    public static class RobloxPackageUris
    {
        private const string ManifestFileName = "rbxPkgManifest.txt";

        private static string GetVersionPrefix(RobloxDeploymentTarget target, string baseUri)
        {
            string normalizedBaseUri = RobloxCdn.NormalizeBaseUri(baseUri);

            if (string.IsNullOrEmpty(normalizedBaseUri))
            {
                normalizedBaseUri = RobloxCdn.DefaultBaseUri;
            }

            string channelSegment = target.IsLiveChannel ? string.Empty : $"/channel/{target.ChannelName}";

            return $"{normalizedBaseUri}{channelSegment}{target.BlobDirectory}{target.VersionHash}-";
        }

        public static string ForManifest(RobloxDeploymentTarget target, string baseUri)
        {
            return $"{GetVersionPrefix(target, baseUri)}{ManifestFileName}";
        }

        public static string ForPackage(RobloxDeploymentTarget target, string baseUri, string packageName)
        {
            return $"{GetVersionPrefix(target, baseUri)}{packageName}";
        }

        public static string ForProbe(RobloxDeploymentTarget target, string baseUri)
        {
            return target.BinaryType.IsMac() ? ForPackage(target, baseUri, target.BinaryType.GetMacArchiveName()) : ForManifest(target, baseUri);
        }
    }
}