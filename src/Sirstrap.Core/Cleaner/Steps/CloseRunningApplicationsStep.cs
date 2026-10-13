namespace Sirstrap.Core.Cleaner.Steps
{
    public sealed class CloseRunningApplicationsStep(IUserPrompt userPrompt, CleanerOptions options) : ICleanupStep
    {
        private bool TryCloseRunningApplications()
        {
            List<string> runningProcesses = [.. options.ProcessesToClose.Where(ProcessKiller.IsRunning)];

            if (runningProcesses.Count == 0)
            {
                Log.Information("Roblox and SirHurt are not running.");

                return true;
            }

            foreach (string processName in runningProcesses)
            {
                Log.Information("Found a running program that must be closed (Process: {0}).", processName);
            }

            if (!userPrompt.Confirm("Roblox and SirHurt must be closed before cleaning.", defaultAnswer: true))
            {
                Log.Information("Left the running programs open, as requested.");

                return false;
            }

            List<string> stillRunningProcesses = [.. runningProcesses.Where(processName => !ProcessKiller.TryKillAll(processName))];

            foreach (string processName in stillRunningProcesses)
            {
                Log.Warning("Could not close every instance of a program (Process: {0}).", processName);
            }

            return stillRunningProcesses.Count == 0;
        }

        public void Execute()
        {
            if (!TryCloseRunningApplications())
            {
                Log.Warning("Some programs are still running, files in use may not be removed.");
            }
        }

        public string Name => "Close running applications";
    }
}