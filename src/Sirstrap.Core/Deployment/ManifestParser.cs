namespace Sirstrap.Core.Deployment
{
    public static partial class ManifestParser
    {
        public static Manifest Parse(string? manifestContext)
        {
            if (string.IsNullOrEmpty(manifestContext))
                return new Manifest();

            var lines = manifestContext.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);

            return new Manifest
            {
                IsValid = IsValidManifest(lines),
                Packages = GetPackages(lines),
                Checksums = GetChecksums(lines)
            };
        }

        #region PRIVATE METHODS
        private static bool IsPackageLine(string line) => line.Contains('.') && line.EndsWith(".zip", StringComparison.OrdinalIgnoreCase);

        private static List<string> GetPackages(string[] lines) => [.. lines.Where(IsPackageLine).Select(line => line.Trim())];

        private static Dictionary<string, string> GetChecksums(string[] lines)
        {
            Dictionary<string, string> checksums = new(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < lines.Length - 1; i++)
            {
                string checksum = lines[i + 1].Trim();

                if (IsPackageLine(lines[i]) && Md5Regex().IsMatch(checksum))
                    checksums[lines[i].Trim()] = checksum.ToLowerInvariant();
            }

            return checksums;
        }

        [GeneratedRegex("^[0-9a-fA-F]{32}$")]
        private static partial Regex Md5Regex();

        private static bool IsValidManifest(string[] lines) => lines.Length > 0 && lines[0].Trim().Equals("v0", StringComparison.OrdinalIgnoreCase);
        #endregion
    }
}
