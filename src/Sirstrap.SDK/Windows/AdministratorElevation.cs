namespace Sirstrap.SDK.Windows
{
    public static class AdministratorElevation
    {
        public static bool IsRunningAsAdministrator()
        {
            try
            {
                using WindowsIdentity identity = WindowsIdentity.GetCurrent();

                return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not check whether Sirstrap is running as administrator.");

                return false;
            }
        }

        public static bool RestartAsAdministrator(string[] arguments)
        {
            try
            {
                string? executablePath = Environment.ProcessPath;

                if (string.IsNullOrEmpty(executablePath))
                {
                    Log.Error("Could not restart Sirstrap as administrator because its file path is unknown.");

                    return false;
                }

                Log.Information("Restarting Sirstrap as administrator (Path: {0}).", executablePath);

                _ = Process.Start(new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = string.Join(" ", arguments.Select(argument => $"\"{argument}\"")),
                    UseShellExecute = true,
                    Verb = "runas"
                });

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not restart Sirstrap as administrator.");

                return false;
            }
        }

        public static bool RunOrRestartElevated(Func<bool> operation, string[] restartArguments, string operationDescription)
        {
            try
            {
                if (operation())
                {
                    Log.Debug("Operation completed (Operation: {0}).", operationDescription);

                    return true;
                }

                if (IsRunningAsAdministrator())
                {
                    Log.Error("Operation failed even with administrator rights (Operation: {0}).", operationDescription);

                    return false;
                }

                Log.Information("Administrator rights are needed, Windows will ask for permission (Operation: {0}).", operationDescription);

                if (RestartAsAdministrator(restartArguments))
                {
                    Environment.Exit(0);
                }

                Log.Error("Could not get administrator rights (Operation: {0}).", operationDescription);

                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Operation failed (Operation: {0}).", operationDescription);

                return false;
            }
        }
    }
}