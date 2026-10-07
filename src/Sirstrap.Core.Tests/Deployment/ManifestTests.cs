namespace Sirstrap.Core.Tests.Deployment
{
    public class ManifestTests
    {
        [Fact]
        public void Parse_ReturnsInvalidEmptyManifest_ForNullOrEmpty()
        {
            Manifest fromNull = ManifestParser.Parse(null);
            Manifest fromEmpty = ManifestParser.Parse(string.Empty);

            Assert.False(fromNull.IsValid);
            Assert.Empty(fromNull.Packages);
            Assert.False(fromEmpty.IsValid);
        }

        [Fact]
        public void Parse_MarksValid_WhenFirstLineIsV0_AndCollectsZipPackages()
        {
            Manifest manifest = ManifestParser.Parse("v0\r\nRobloxApp.zip\r\nLibraries.zip\r\nignored-line\r\nchecksum");

            Assert.True(manifest.IsValid);
            Assert.Equal(["RobloxApp.zip", "Libraries.zip"], manifest.Packages);
        }

        [Fact]
        public void Parse_CollectsChecksums_FromLineAfterPackage()
        {
            Manifest manifest = ManifestParser.Parse("v0\nRobloxApp.zip\nABCDEF0123456789abcdef0123456789\n100\n200\nLibraries.zip\nnot-a-hash\n");

            Assert.Equal("abcdef0123456789abcdef0123456789", manifest.Checksums["RobloxApp.zip"]);
            Assert.False(manifest.Checksums.ContainsKey("Libraries.zip"));
        }

        [Fact]
        public void Parse_MarksInvalid_WhenFirstLineIsNotV0()
        {
            Manifest manifest = ManifestParser.Parse("v1\nRobloxApp.zip");

            Assert.False(manifest.IsValid);
            Assert.Equal(["RobloxApp.zip"], manifest.Packages);
        }

        [Fact]
        public void Manifest_DefaultsToInvalidWithEmptyPackages()
        {
            Manifest manifest = new();

            Assert.False(manifest.IsValid);
            Assert.Empty(manifest.Packages);
        }
    }
}
