namespace Sirstrap.Core.Update
{
    public sealed class UpdateInstaller(HttpClient httpClient, InstanceWorkspace workspace)
    {
        private const string DownloadedArchiveFileName = "Sirstrap.zip";

        private async Task DownloadAndExtractAsync(string downloadUri, string updateDirectory)
        {
            Log.Information("Downloading the Sirstrap update (URL: {0}).", downloadUri);

            string archivePath = Path.Combine(updateDirectory, DownloadedArchiveFileName);

            await File.WriteAllBytesAsync(archivePath, await httpClient.GetByteArrayAsync(downloadUri));
            await Task.Run(() => ZipFile.ExtractToDirectory(archivePath, updateDirectory, overwriteFiles: true));

            File.Delete(archivePath);
        }

        private string PrepareUpdateDirectory()
        {
            string updateDirectory = workspace.Update;

            if (!Directory.Exists(updateDirectory))
            {
                Log.Debug("Creating the update folder (Path: {0}).", updateDirectory);

                _ = Directory.CreateDirectory(updateDirectory);

                return updateDirectory;
            }

            Log.Debug("Emptying the update folder (Path: {0}).", updateDirectory);

            try
            {
                Directory.Delete(updateDirectory, recursive: true);

                _ = Directory.CreateDirectory(updateDirectory);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not empty the update folder (Path: {0}).", updateDirectory);
            }

            return updateDirectory;
        }

        public void Apply()
        {
            string executablePath = Environment.ProcessPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.FriendlyName);
            string executableDirectory = Path.GetDirectoryName(executablePath) ?? AppDomain.CurrentDomain.BaseDirectory;
            string scriptPath = workspace.UpdateScript;

            File.WriteAllText(scriptPath, BuildUpdateScript(workspace.Update, executableDirectory, Path.GetFileName(executablePath)));
            Log.Information("Installing the Sirstrap update once every Sirstrap window is closed, it will be used the next time Sirstrap starts (Folder: {0}).", executableDirectory);

            _ = Process.Start(new ProcessStartInfo
            {
                FileName = scriptPath,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = true
            });
        }

        public static string BuildUpdateScript(string updateDirectory, string executableDirectory, string executableName)
        {
            return $@"
@echo off
echo Updating Sirstrap.
:wait
tasklist /FI ""IMAGENAME eq {executableName}"" 2>nul | find /I ""{executableName}"" >nul && (timeout /t 1 /nobreak >nul & goto wait)
xcopy ""{updateDirectory}\*"" ""{executableDirectory}"" /E /Y
rmdir /s /q ""{updateDirectory}""
del /f /q ""%~f0""
";
        }

        public async Task DownloadAsync(string downloadUri)
        {
            string updateDirectory = PrepareUpdateDirectory();

            await DownloadAndExtractAsync(downloadUri, updateDirectory);

            Log.Information("Downloaded the Sirstrap update, it will be installed when Sirstrap closes (Folder: {0}).", updateDirectory);
        }
    }
}