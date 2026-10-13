namespace Sirstrap.UI.Models
{
    public sealed record LaunchArguments(string[] Values)
    {
        public bool IsEmpty => Values.Length == 0;
    }
}