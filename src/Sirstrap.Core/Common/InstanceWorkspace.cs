namespace Sirstrap.Core.Common
{
    public sealed class InstanceWorkspace(SirstrapDirectories directories) : IDisposable
    {
        private const string LockFileName = ".lock";

        private FileStream? _lock;

        public void Dispose()
        {
            _lock?.Dispose();

            _lock = null;
        }

        public static bool IsAlive(SirstrapDirectories directories, string instanceId)
        {
            string lockPath = Path.Combine(directories.Instances, instanceId, LockFileName);

            if (!File.Exists(lockPath))
            {
                return false;
            }

            try
            {
                using FileStream probe = new(lockPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

                return false;
            }
            catch (IOException)
            {
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return true;
            }
        }

        public void Open()
        {
            if (_lock != null)
            {
                return;
            }

            _ = Directory.CreateDirectory(Logs);
            _ = Directory.CreateDirectory(Downloads);

            _lock = new FileStream(Path.Combine(Root, LockFileName), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        }

        public string Downloads => Path.Combine(Root, "Downloads");

        public string Id { get; } = string.Create(CultureInfo.InvariantCulture, $"{DateTime.Now:yyyyMMdd-HHmmss}-{Environment.ProcessId}");

        public string Logs => Path.Combine(Root, "Logs");

        public string Root => Path.Combine(directories.Instances, Id);

        public string Update => Path.Combine(Root, "Update");

        public string UpdateScript => Path.Combine(Root, "Update.cmd");
    }
}