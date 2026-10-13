namespace Sirstrap.SDK.Roblox
{
    public sealed class RobloxSingletonMutex
    {
        private const string MutexName = "ROBLOX_singletonMutex";

        private RobloxInstanceRole _currentRole = RobloxInstanceRole.None;

        private Mutex? _mutex;

        private readonly Lock _stateLock = new();

        public event EventHandler<RobloxInstanceRole>? RoleChanged;

        private void BecomeSlave()
        {
            ChangeRole(RobloxInstanceRole.Slave);

            _mutex?.Dispose();

            _mutex = null;
        }

        private void ChangeRole(RobloxInstanceRole role)
        {
            if (_currentRole == role)
            {
                return;
            }

            _currentRole = role;

            Log.Debug("The Roblox instance role changed (Role: {0}).", role);

            RoleChanged?.Invoke(this, role);
        }

        public void Release()
        {
            lock (_stateLock)
            {
                if (_mutex == null)
                {
                    Log.Debug("The multi-instance lock is not held, nothing to release.");

                    return;
                }

                try
                {
                    _mutex.Dispose();

                    _mutex = null;

                    ChangeRole(RobloxInstanceRole.None);

                    Log.Debug("Released the multi-instance lock (Lock: {0}).", MutexName);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Could not release the multi-instance lock (Lock: {0}).", MutexName);
                }
            }
        }

        public bool TryAcquire()
        {
            lock (_stateLock)
            {
                if (_mutex != null)
                {
                    Log.Debug("The multi-instance lock is already held by Sirstrap.");

                    return true;
                }

                try
                {
                    _mutex = new Mutex(true, MutexName, out bool isCreatedByThisInstance);

                    if (isCreatedByThisInstance)
                    {
                        Log.Information("Multi-instance is active, more Roblox windows can be opened (Lock: {0}).", MutexName);

                        ChangeRole(RobloxInstanceRole.Master);

                        RobloxProcesses.KillAll();

                        return true;
                    }

                    Log.Information("Another Roblox window is already open, this one will start as an additional instance (Lock: {0}).", MutexName);
                }
                catch (UnauthorizedAccessException ex)
                {
                    Log.Warning(ex, "Could not take the multi-instance lock because access was denied. Another Roblox may be running as administrator (Lock: {0}).", MutexName);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Could not take the multi-instance lock (Lock: {0}).", MutexName);
                }

                BecomeSlave();

                return false;
            }
        }

        public RobloxInstanceRole CurrentRole
        {
            get
            {
                lock (_stateLock)
                {
                    return _currentRole;
                }
            }
        }

        public bool IsHeld => _mutex != null;
    }
}