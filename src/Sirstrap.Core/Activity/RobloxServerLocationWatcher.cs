namespace Sirstrap.Core.Activity
{
    public sealed class RobloxServerLocationWatcher(ServerLocationLookup serverLocationLookup, string robloxLogsDirectory)
    {
        private const string UnknownLocation = "UNKNOWN";

        private FileSystemWatcher? _logDirectoryWatcher;

        private CancellationTokenSource? _readingCancellation;

        private Task? _readingTask;

        public event EventHandler<string>? ServerLocationChanged;

        private async Task FollowLogFileAsync(string logFilePath, CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(500, cancellationToken);

                using FileStream logStream = new(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                using StreamReader reader = new(logStream);

                while (!cancellationToken.IsCancellationRequested)
                {
                    string? line = await reader.ReadLineAsync(cancellationToken);

                    if (line is null)
                    {
                        await Task.Delay(1000, cancellationToken);

                        continue;
                    }

                    string? serverAddress = RobloxLogLineParser.TryExtractServerAddress(line);

                    if (!string.IsNullOrEmpty(serverAddress))
                    {
                        _ = UpdateServerLocationAsync(serverAddress);
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                Log.Debug(ex, "Stopped reading the Roblox log file (Path: {0}).", logFilePath);
            }
            catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
            {
                Log.Debug(ex, "The Roblox log file was removed before it could be read (Path: {0}).", logFilePath);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not read the Roblox log file, the server location will not be shown (Path: {0}).", logFilePath);
            }
        }

        private void OnLogFileCreated(object sender, FileSystemEventArgs e)
        {
            try
            {
                StopReading(TimeSpan.FromSeconds(2));
                StartReading(e.FullPath);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not open the new Roblox log file, the server location will not be updated.");
            }
        }

        private void StartReading(string logFilePath)
        {
            _readingCancellation = new CancellationTokenSource();

            CancellationToken cancellationToken = _readingCancellation.Token;

            _readingTask = Task.Run(() => FollowLogFileAsync(logFilePath, cancellationToken), CancellationToken.None);
        }

        private void StopReading(TimeSpan timeout)
        {
            _readingCancellation?.Cancel();

            _ = _readingTask?.Wait(timeout, CancellationToken.None);

            _readingCancellation?.Dispose();

            _readingCancellation = null;
            _readingTask = null;
        }

        private async Task UpdateServerLocationAsync(string serverAddress)
        {
            try
            {
                string location = await serverLocationLookup.GetLocationAsync(serverAddress);

                if (location == CurrentServerLocation)
                {
                    return;
                }

                CurrentServerLocation = location;

                ServerLocationChanged?.Invoke(this, location);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not update the server location (IP: {0}).", serverAddress);
            }
        }

        public void Start()
        {
            try
            {
                if (!Directory.Exists(robloxLogsDirectory))
                {
                    return;
                }

                string? latestLogFile = Directory.GetFiles(robloxLogsDirectory, "*.log", SearchOption.TopDirectoryOnly).OrderByDescending(File.GetLastWriteTime).FirstOrDefault();

                if (latestLogFile != null)
                {
                    StartReading(latestLogFile);
                }

                _logDirectoryWatcher = new FileSystemWatcher(robloxLogsDirectory)
                {
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime,
                    Filter = "*.log",
                    EnableRaisingEvents = true
                };

                _logDirectoryWatcher.Created += OnLogFileCreated;

                Log.Debug("Started following the Roblox log files to detect the server location (Folder: {0}).", robloxLogsDirectory);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not start detecting the server location (Folder: {0}).", robloxLogsDirectory);
            }
        }

        public void Stop()
        {
            try
            {
                StopReading(TimeSpan.FromSeconds(5));

                if (_logDirectoryWatcher != null)
                {
                    _logDirectoryWatcher.Created -= OnLogFileCreated;

                    _logDirectoryWatcher.Dispose();

                    _logDirectoryWatcher = null;
                }

                Log.Debug("Stopped following the Roblox log files.");
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not stop detecting the server location.");
            }
        }

        public string CurrentServerLocation { get; private set; } = UnknownLocation;
    }
}