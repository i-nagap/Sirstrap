namespace Sirstrap.SDK.SirHurt
{
    public sealed class SirHurtAccount(string sirhuiDirectory)
    {
        private const string DllPathFileName = "dllpath.dat";

        private const string UserNameFileName = "sirhurta.dat";

        private static readonly string[] _credentialFileNames = [UserNameFileName, "sirhurtp.dat"];

        public static SirHurtAccount ForCurrentUser()
        {
            return new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "sirhurt", "sirhui"));
        }

        public string GetInstallationDirectory()
        {
            try
            {
                string dllPathFile = Path.Combine(sirhuiDirectory, DllPathFileName);

                if (!File.Exists(dllPathFile))
                {
                    return string.Empty;
                }

                DirectoryInfo? installationDirectory = Directory.GetParent(File.ReadAllText(dllPathFile));

                if (installationDirectory == null || !installationDirectory.Exists)
                {
                    return string.Empty;
                }

                Log.Debug("Found the SirHurt folder (Path: {0}).", installationDirectory.FullName);

                return installationDirectory.FullName;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not find the SirHurt folder (Source: {0}).", DllPathFileName);

                return string.Empty;
            }
        }

        public string GetUserName()
        {
            try
            {
                string userNameFile = Path.Combine(sirhuiDirectory, UserNameFileName);

                if (!File.Exists(userNameFile))
                {
                    return string.Empty;
                }

                string userName = File.ReadAllText(userNameFile).Trim();

                if (string.IsNullOrWhiteSpace(userName))
                {
                    return string.Empty;
                }

                Log.Debug("Found the SirHurt user (User: {0}).", userName);

                return userName;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not read the SirHurt user name (Source: {0}).", UserNameFileName);

                return string.Empty;
            }
        }

        public bool LogOut()
        {
            try
            {
                List<string> deletedFiles = [];

                foreach (string fileName in _credentialFileNames)
                {
                    string filePath = Path.Combine(sirhuiDirectory, fileName);

                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);

                        deletedFiles.Add(fileName);

                        Log.Debug("Deleted a SirHurt login file (Path: {0}).", filePath);
                    }
                    else
                    {
                        Log.Debug("SirHurt login file not found (Path: {0}).", filePath);
                    }
                }

                if (deletedFiles.Count == 0)
                {
                    Log.Information("No SirHurt login found, nothing to log out of (Folder: {0}).", sirhuiDirectory);
                }
                else
                {
                    Log.Information("Logged out of SirHurt (Deleted: {0}).", string.Join(", ", deletedFiles));
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not log out of SirHurt.");

                return false;
            }
        }
    }
}