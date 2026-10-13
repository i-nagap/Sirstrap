namespace Sirstrap.UI.Services
{
    [SuppressMessage("Major Code Smell", "S1075", Justification = "Codeberg is a fixed endpoint.")]
    public static class CodebergLinks
    {
        private const string ProfileUriPrefix = "https://codeberg.org/";

        public static string Profile(string account)
        {
            return $"{ProfileUriPrefix}{account}";
        }

        public static string ProfileImage(string account)
        {
            return $"{Profile(account)}.png";
        }

        public static string Repository(string account)
        {
            return $"{Profile(account)}/Sirstrap";
        }
    }
}