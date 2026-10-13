namespace Sirstrap.SDK.Roblox
{
    public static class PackageChecksum
    {
        [SuppressMessage("Security", "S4790", Justification = "Roblox publishes MD5 package checksums.")]
        public static bool IsIntact(string packageName, [NotNullWhen(true)] byte[]? packageBytes, string? expectedMd5)
        {
            if (packageBytes == null)
            {
                return false;
            }

            if (expectedMd5 == null)
            {
                return true;
            }

            string actualMd5 = Convert.ToHexStringLower(MD5.HashData(packageBytes));

            if (actualMd5 == expectedMd5)
            {
                return true;
            }

            Log.Warning("A downloaded package is corrupted (Package: {0}, Expected MD5: {1}, Actual MD5: {2}).", packageName, expectedMd5, actualMd5);

            return false;
        }
    }
}