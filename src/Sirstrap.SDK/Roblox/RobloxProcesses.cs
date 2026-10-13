namespace Sirstrap.SDK.Roblox
{
    public static class RobloxProcesses
    {
        private static readonly string[] _allProcessNames = ["RobloxCrashHandler", "RobloxPlayerBeta"];

        private static readonly string[] _gameProcessNames = ["RobloxPlayerBeta"];

        private static bool AnyProcessRunning(string[] processNames)
        {
            bool anyRunning = false;

            foreach (Process process in GetProcesses(processNames))
            {
                anyRunning = true;

                process.Dispose();
            }

            return anyRunning;
        }

        private static IEnumerable<Process> GetProcesses(string[] processNames)
        {
            return processNames.SelectMany(Process.GetProcessesByName);
        }

        public static bool AnyGameProcessRunning(IEnumerable<int> processIds)
        {
            HashSet<int> runningIds = SnapshotGameProcessIds();

            return processIds.Any(runningIds.Contains);
        }

        public static int CountGameProcesses()
        {
            return SnapshotGameProcessIds().Count;
        }

        public static IReadOnlyList<int> FindNewGameProcessIds(IReadOnlySet<int> knownProcessIds, int maximumAttempts = 20)
        {
            for (int attempt = 0; attempt < maximumAttempts; attempt++)
            {
                List<int> newProcessIds = [.. SnapshotGameProcessIds().Where(id => !knownProcessIds.Contains(id))];

                if (newProcessIds.Count > 0)
                {
                    return newProcessIds;
                }

                Thread.Sleep(100);
            }

            return [];
        }

        public static void KillAll()
        {
            try
            {
                foreach (Process process in GetProcesses(_allProcessNames))
                {
                    try
                    {
                        Log.Information("Closing a Roblox that was already open (Process: {0}, PID: {1}, Started: {2}, Memory: {3} MB, Window: {4}).", process.ProcessName, process.Id, process.StartTime.ToString("yyyy-MM-dd HH:mm:ss"), process.WorkingSet64 / 1024 / 1024, process.MainWindowTitle);

                        process.Kill();
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Could not close a Roblox that was already open (Process: {0}, PID: {1}).", process.ProcessName, process.Id);
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not close the Roblox programs that were already open.");
            }
        }

        public static void LogRunningGameProcesses()
        {
            foreach (Process process in GetProcesses(_gameProcessNames))
            {
                try
                {
                    Log.Debug("Roblox is still running (Process: {0}, PID: {1}, Started: {2}, Memory: {3} MB, Window: {4}).", process.ProcessName, process.Id, process.StartTime.ToString("yyyy-MM-dd HH:mm:ss"), process.WorkingSet64 / 1024 / 1024, process.MainWindowTitle);
                }
                catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception)
                {
                    Log.Debug(ex, "A Roblox process exited or cannot be inspected (PID: {0}).", process.Id);
                }
                finally
                {
                    process.Dispose();
                }
            }
        }

        public static HashSet<int> SnapshotGameProcessIds()
        {
            HashSet<int> processIds = [];

            foreach (Process process in GetProcesses(_gameProcessNames))
            {
                _ = processIds.Add(process.Id);

                process.Dispose();
            }

            return processIds;
        }

        public static bool WaitForAllToExit(int timeoutMilliseconds = 5000)
        {
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                while (stopwatch.ElapsedMilliseconds < timeoutMilliseconds)
                {
                    if (!AnyProcessRunning(_allProcessNames))
                    {
                        Log.Debug("All Roblox programs are closed.");

                        return true;
                    }

                    Thread.Sleep(100);
                }

                Log.Warning("Roblox is still running after waiting {0} seconds.", timeoutMilliseconds / 1000);

                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not check whether Roblox is still running.");

                return false;
            }
        }
    }
}