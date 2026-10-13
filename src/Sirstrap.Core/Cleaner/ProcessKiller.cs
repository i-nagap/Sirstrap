namespace Sirstrap.Core.Cleaner
{
    public static class ProcessKiller
    {
        private static Process[] GetOtherProcesses(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);

            foreach (Process currentProcess in processes.Where(process => process.Id == Environment.ProcessId))
            {
                currentProcess.Dispose();
            }

            return [.. processes.Where(process => process.Id != Environment.ProcessId)];
        }

        public static bool IsRunning(string processName)
        {
            Process[] processes = GetOtherProcesses(processName);

            foreach (Process process in processes)
            {
                process.Dispose();
            }

            return processes.Length > 0;
        }

        public static bool TryKillAll(string processName)
        {
            try
            {
                Process[] processes = GetOtherProcesses(processName);

                if (processes.Length == 0)
                {
                    return true;
                }

                Log.Information("Closing {0} (Instances: {1}).", processName, processes.Length);

                bool areAllClosed = true;

                foreach (Process process in processes)
                {
                    using (process)
                    {
                        try
                        {
                            process.Kill(entireProcessTree: true);

                            _ = process.WaitForExit(5000);

                            Log.Information("Closed {0} (PID: {1}).", processName, process.Id);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Could not close {0} (PID: {1}).", processName, process.Id);

                            areAllClosed = false;
                        }
                    }
                }

                return areAllClosed;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not close {0}.", processName);

                return false;
            }
        }
    }
}