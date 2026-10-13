namespace Sirstrap.Core.Bootstrap
{
    public sealed class IncognitoRobloxData
    {
        private readonly string _cacheDirectory;

        private bool _isRobloxDataInCache;

        private readonly Lock _moveLock = new();

        private readonly string _robloxDataDirectory;

        public IncognitoRobloxData(RobloxSingletonMutex singletonMutex, string robloxDataDirectory, string cacheDirectory)
        {
            _robloxDataDirectory = robloxDataDirectory;
            _cacheDirectory = cacheDirectory;

            singletonMutex.RoleChanged += OnRoleChanged;
        }

        private void OnRoleChanged(object? sender, RobloxInstanceRole newRole)
        {
            if (newRole == RobloxInstanceRole.Master || !_isRobloxDataInCache)
            {
                return;
            }

            Log.Information("Incognito mode ended, restoring your Roblox data (Role: {0}).", newRole);

            _ = RestoreFromCache();
        }

        public bool MoveToCache()
        {
            lock (_moveLock)
            {
                try
                {
                    if (_isRobloxDataInCache)
                    {
                        Log.Debug("Your Roblox data is already set aside for Incognito mode.");

                        return true;
                    }

                    if (!Directory.Exists(_robloxDataDirectory))
                    {
                        Log.Debug("No Roblox data to set aside for Incognito mode (Path: {0}).", _robloxDataDirectory);

                        return true;
                    }

                    _ = Directory.CreateDirectory(Path.GetDirectoryName(_cacheDirectory)!);

                    if (Directory.Exists(_cacheDirectory))
                    {
                        Log.Debug("Removing the previous Incognito backup (Path: {0}).", _cacheDirectory);

                        ResilientFileSystem.DeleteDirectory(_cacheDirectory);
                    }

                    _ = RobloxProcesses.WaitForAllToExit();

                    ResilientFileSystem.MoveDirectory(_robloxDataDirectory, _cacheDirectory);

                    _isRobloxDataInCache = true;

                    Log.Information("Moved your Roblox data aside for Incognito mode (From: {0}, To: {1}).", _robloxDataDirectory, _cacheDirectory);

                    return true;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Could not move your Roblox data aside, Incognito mode is not active.");

                    return false;
                }
            }
        }

        public bool RestoreFromCache()
        {
            lock (_moveLock)
            {
                try
                {
                    if (!_isRobloxDataInCache)
                    {
                        Log.Debug("Your Roblox data was not moved, nothing to restore.");

                        return true;
                    }

                    if (!Directory.Exists(_cacheDirectory))
                    {
                        Log.Warning("The Incognito backup of your Roblox data is missing, nothing to restore (Path: {0}).", _cacheDirectory);

                        _isRobloxDataInCache = false;

                        return true;
                    }

                    if (Directory.Exists(_robloxDataDirectory))
                    {
                        Log.Debug("Deleting the Roblox data created during Incognito mode (Path: {0}).", _robloxDataDirectory);

                        _ = RobloxProcesses.WaitForAllToExit();

                        ResilientFileSystem.DeleteDirectory(_robloxDataDirectory);
                    }

                    ResilientFileSystem.MoveDirectory(_cacheDirectory, _robloxDataDirectory);

                    _isRobloxDataInCache = false;

                    Log.Information("Restored your Roblox data after Incognito mode (From: {0}, To: {1}).", _cacheDirectory, _robloxDataDirectory);

                    return true;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Could not restore your Roblox data after Incognito mode. It is still saved in the backup folder (Path: {0}).", _cacheDirectory);

                    return false;
                }
            }
        }
    }
}