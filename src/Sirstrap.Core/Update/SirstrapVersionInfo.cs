namespace Sirstrap.Core.Update
{
    public sealed class SirstrapVersionInfo(SirstrapSettings settings)
    {
        public string GetDisplayVersion()
        {
            return IsDebugBuild ? $"v{Guid.NewGuid().ToString().ToUpperInvariant()}" : $"v{Current}{Channel}";
        }

        public string Channel => settings.SirstrapChannel;

        public static Version Current { get; } = Version.TryParse(typeof(SirstrapVersionInfo).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0].Split('-')[0], out Version? version) ? version : typeof(SirstrapVersionInfo).Assembly.GetName().Version ?? new Version();

#if DEBUG
        public static bool IsDebugBuild => true;
#else
        public static bool IsDebugBuild => false;
#endif
    }
}