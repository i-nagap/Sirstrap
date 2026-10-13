namespace Sirstrap.UI.Services
{
    public static class ShellLauncher
    {
        public static void Open(string target)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = target,
                UseShellExecute = true
            })?.Dispose();
        }

        public static void Run(string executablePath)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = executablePath
            })?.Dispose();
        }
    }
}