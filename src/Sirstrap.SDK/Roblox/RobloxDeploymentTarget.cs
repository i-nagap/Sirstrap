namespace Sirstrap.SDK.Roblox
{
    public sealed record RobloxDeploymentTarget(RobloxBinaryType BinaryType, string ChannelName, string BlobDirectory, string VersionHash)
    {
        public bool IsLiveChannel => ChannelName.Equals("LIVE", StringComparison.OrdinalIgnoreCase);
    }
}