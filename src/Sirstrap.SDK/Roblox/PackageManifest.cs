namespace Sirstrap.SDK.Roblox
{
    public sealed partial record PackageManifest(bool IsValid, IReadOnlyList<string> Packages, IReadOnlyDictionary<string, string> Md5Checksums)
    {
        private static bool IsPackageLine(string line)
        {
            return line.Contains('.') && line.EndsWith(".zip", StringComparison.OrdinalIgnoreCase);
        }

        [GeneratedRegex("^[0-9a-fA-F]{32}$")]
        private static partial Regex Md5Pattern();

        public static PackageManifest Parse(string? manifestContent)
        {
            if (string.IsNullOrEmpty(manifestContent))
            {
                return Empty;
            }

            string[] lines = manifestContent.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
            bool isValid = lines.Length > 0 && lines[0].Trim().Equals("v0", StringComparison.OrdinalIgnoreCase);

            List<string> packages = [.. lines.Where(IsPackageLine).Select(line => line.Trim())];
            Dictionary<string, string> checksums = new(StringComparer.OrdinalIgnoreCase);

            for (int index = 0; index < lines.Length - 1; index++)
            {
                string nextLine = lines[index + 1].Trim();

                if (IsPackageLine(lines[index]) && Md5Pattern().IsMatch(nextLine))
                {
                    checksums[lines[index].Trim()] = nextLine.ToLowerInvariant();
                }
            }

            return new PackageManifest(isValid, packages, checksums);
        }

        public static PackageManifest Empty { get; } = new(false, [], new Dictionary<string, string>());
    }
}