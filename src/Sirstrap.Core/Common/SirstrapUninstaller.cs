namespace Sirstrap.Core.Common
{
    public sealed class SirstrapUninstaller(SirstrapDirectories directories)
    {
        private static readonly string[] _registeredProtocols = ["roblox-player"];

        public void ScheduleFileRemoval()
        {
            string executablePath = Environment.ProcessPath ?? string.Empty;
            string scriptPath = Path.Combine(Path.GetTempPath(), $"sirstrap_uninstall_{Guid.NewGuid():N}.cmd");

            StringBuilder script = new();

            _ = script.AppendLine("@echo off");
            _ = script.AppendLine("timeout /t 3 /nobreak >nul");

            if (Directory.Exists(directories.Root))
            {
                _ = script.AppendLine($"rmdir /s /q \"{directories.Root}\"");
            }

            if (!string.IsNullOrEmpty(executablePath) && File.Exists(executablePath))
            {
                _ = script.AppendLine($"del /f /q \"{executablePath}\"");
            }

            _ = script.AppendLine("del /f /q \"%~f0\"");

            File.WriteAllText(scriptPath, script.ToString());

            _ = Process.Start(new ProcessStartInfo
            {
                FileName = Path.Combine(Environment.SystemDirectory, "cmd.exe"),
                Arguments = $"/c \"{scriptPath}\"",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = true
            });
        }

        public void Uninstall()
        {
            Log.Information("Uninstalling Sirstrap.");

            UnregisterProtocols();
            ScheduleFileRemoval();

            Log.Information("Sirstrap was uninstalled. It will now close and its files will be deleted (Folder: {0}).", directories.Root);
        }

        public static void UnregisterProtocols()
        {
            foreach (string protocol in _registeredProtocols)
            {
                ProtocolHandlerRegistrar.Unregister(protocol);
            }
        }
    }
}