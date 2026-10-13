namespace Sirstrap.Core.Bootstrap
{
    public sealed class RobloxLauncher(SirstrapSettings settings, RobloxVersionLayout layout, RobloxSingletonMutex singletonMutex, IncognitoRobloxData incognitoRobloxData, FastFlagsService fastFlagsService, ITelemetry telemetry)
    {
        private static bool Fail(ITelemetryOperation operation, string outcome)
        {
            operation.MarkFailed(outcome);

            return false;
        }

        private static bool StartPlayer(string playerExecutablePath, string launchUri)
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = playerExecutablePath,
                WorkingDirectory = Path.GetDirectoryName(playerExecutablePath),
                UseShellExecute = true
            };

            if (string.IsNullOrEmpty(launchUri))
            {
                Log.Information("Starting Roblox.");
            }
            else
            {
                Log.Information("Starting Roblox and joining the game.");

                startInfo.Arguments = launchUri;
            }

            using Process? process = Process.Start(startInfo);

            if (process == null)
            {
                return false;
            }

            WaitForInputIdle(process);

            return true;
        }

        private void WaitForGameToExit(HashSet<int> gameProcessIdsBeforeLaunch)
        {
            Log.Information("Sirstrap will stay open until Roblox is closed, so that more Roblox windows can be opened.");

            IReadOnlyList<int> launchedGameProcessIds = RobloxProcesses.FindNewGameProcessIds(gameProcessIdsBeforeLaunch);

            if (launchedGameProcessIds.Count == 0)
            {
                Log.Warning("Could not find the Roblox window that was just started, so Sirstrap will not wait for it to close.");

                return;
            }

            while (singletonMutex.IsHeld && RobloxProcesses.AnyGameProcessRunning(launchedGameProcessIds))
            {
                Thread.Sleep(100);
            }

            Log.Information("Stopped waiting for Roblox (Multi-instance lock held: {0}, Started game closed: {1}, Roblox windows still open: {2}).", singletonMutex.IsHeld, !RobloxProcesses.AnyGameProcessRunning(launchedGameProcessIds), RobloxProcesses.CountGameProcesses());
            RobloxProcesses.LogRunningGameProcesses();
        }

        private static void WaitForInputIdle(Process process)
        {
            Log.Debug("Waiting for Roblox to finish starting up.");

            try
            {
                _ = process.WaitForInputIdle();
            }
            catch (Exception ex)
            {
                Log.Debug(ex, "Could not wait for Roblox to finish starting up, the process may have already exited.");
            }
        }

        public bool Launch(LaunchRequest request)
        {
            using ITelemetryOperation operation = telemetry.StartOperation("roblox.launch", new Dictionary<string, object>
            {
                ["multiInstance"] = settings.RobloxMultiInstance,
                ["incognito"] = settings.RobloxIncognito,
                ["fastFlags"] = settings.RobloxFastFlagsEnabled,
                ["hasLaunchUri"] = !string.IsNullOrEmpty(request.LaunchUri)
            });
            string playerExecutablePath = layout.GetPlayerExecutablePath(request.VersionHash);

            if (!File.Exists(playerExecutablePath))
            {
                Log.Error("Could not find the Roblox program (Path: {0}).", playerExecutablePath);

                return Fail(operation, "NotFound");
            }

            fastFlagsService.ApplyTo(layout.GetVersionDirectory(request.VersionHash));

            bool holdsSingleton = false;

            try
            {
                HashSet<int> gameProcessIdsBeforeLaunch = RobloxProcesses.SnapshotGameProcessIds();

                if (settings.RobloxMultiInstance)
                {
                    holdsSingleton = singletonMutex.TryAcquire();

                    if (settings.RobloxIncognito && singletonMutex.CurrentRole == RobloxInstanceRole.Master)
                    {
                        _ = incognitoRobloxData.MoveToCache();
                    }
                }

                if (!StartPlayer(playerExecutablePath, request.LaunchUri))
                {
                    Log.Error("Could not start Roblox, Windows did not create the process (Path: {0}).", playerExecutablePath);

                    return Fail(operation, "ProcessStartFailed");
                }

                Log.Information("Roblox started.");

                if (holdsSingleton)
                {
                    WaitForGameToExit(gameProcessIdsBeforeLaunch);
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not start Roblox.");

                return Fail(operation, ex.GetType().Name);
            }
            finally
            {
                if (holdsSingleton)
                {
                    singletonMutex.Release();
                }
            }
        }
    }
}