namespace Sirstrap.Core.Deployment
{
    public class Manifest
    {
        public bool IsValid { get; set; }

        public List<string> Packages { get; set; } = [];

        public Dictionary<string, string> Checksums { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
