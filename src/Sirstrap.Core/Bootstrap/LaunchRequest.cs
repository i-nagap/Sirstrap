namespace Sirstrap.Core.Bootstrap
{
    public sealed record LaunchRequest(RobloxDeploymentTarget Target, string LaunchUri)
    {
        public LaunchRequest WithVersionHash(string versionHash)
        {
            return this with
            {
                Target = Target with
                {
                    VersionHash = versionHash
                }
            };
        }

        public bool IsWindowsPlayer => Target.BinaryType == RobloxBinaryType.WindowsPlayer;

        public string VersionHash => Target.VersionHash;
    }
}