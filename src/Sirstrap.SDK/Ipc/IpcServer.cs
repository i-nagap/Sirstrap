namespace Sirstrap.SDK.Ipc
{
    public sealed class IpcServer
    {
        private const int LengthPrefixBytes = 4;

        private const int MaximumMessageBytes = 1_048_576;

        private CancellationTokenSource? _listeningCancellation;

        private Task? _listeningTask;

        public event EventHandler<string>? MessageReceived;

        private static async Task DelayUnlessCancelledAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(5000, cancellationToken);
            }
            catch (OperationCanceledException ex)
            {
                Log.Debug(ex, "Stopped waiting to restart the message listener.");
            }
        }

        private async Task HandleClientAsync(NamedPipeServerStream pipe, CancellationToken cancellationToken)
        {
            await using (pipe)
            {
                try
                {
                    Log.Debug("Another Sirstrap connected.");

                    string? message = await ReadMessageAsync(pipe, cancellationToken);

                    if (message == null)
                    {
                        return;
                    }

                    Log.Debug("Received a message from another Sirstrap (Message: {0}).", message);

                    MessageReceived?.Invoke(this, message);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Could not read the message from another Sirstrap.");
                }
            }
        }

        private async Task ListenAsync(string pipeName, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                NamedPipeServerStream? pipe = null;

                try
                {
                    Log.Debug("Waiting for messages from other Sirstrap instances (Pipe: {0}).", pipeName);

                    pipe = new NamedPipeServerStream(pipeName, PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

                    await pipe.WaitForConnectionAsync(cancellationToken);

                    NamedPipeServerStream connectedPipe = pipe;

                    pipe = null;

                    _ = Task.Run(() => HandleClientAsync(connectedPipe, cancellationToken), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Could not listen for messages from other Sirstrap instances, retrying in 5 seconds (Pipe: {0}).", pipeName);

                    await DelayUnlessCancelledAsync(cancellationToken);
                }
                finally
                {
                    if (pipe != null)
                    {
                        await pipe.DisposeAsync();
                    }
                }
            }
        }

        private static async Task<string?> ReadMessageAsync(NamedPipeServerStream pipe, CancellationToken cancellationToken)
        {
            byte[] lengthPrefix = new byte[LengthPrefixBytes];

            if (await pipe.ReadAsync(lengthPrefix.AsMemory(0, LengthPrefixBytes), cancellationToken) != LengthPrefixBytes)
            {
                return null;
            }

            int messageLength = BitConverter.ToInt32(lengthPrefix, 0);

            if (messageLength is <= 0 or > MaximumMessageBytes)
            {
                return null;
            }

            byte[] messageBytes = new byte[messageLength];
            int totalBytesRead = 0;

            while (totalBytesRead < messageLength)
            {
                int bytesRead = await pipe.ReadAsync(messageBytes.AsMemory(totalBytesRead, messageLength - totalBytesRead), cancellationToken);

                if (bytesRead == 0)
                {
                    return null;
                }

                totalBytesRead += bytesRead;
            }

            return Encoding.UTF8.GetString(messageBytes);
        }

        public Task StartAsync(string pipeName, CancellationToken cancellationToken = default)
        {
            Log.Debug("Starting the message listener (Pipe: {0}).", pipeName);

            if (IsRunning)
            {
                return Task.CompletedTask;
            }

            _listeningCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            CancellationToken listeningToken = _listeningCancellation.Token;

            _listeningTask = Task.Run(() => ListenAsync(pipeName, listeningToken), listeningToken);

            return Task.CompletedTask;
        }

        public async Task StopAsync()
        {
            Log.Debug("Stopping the message listener.");

            if (!IsRunning)
            {
                return;
            }

            try
            {
                await _listeningCancellation.CancelAsync();
                await _listeningTask;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not stop the message listener.");

                return;
            }

            _listeningCancellation.Dispose();

            _listeningCancellation = null;
            _listeningTask = null;
        }

        [MemberNotNullWhen(true, nameof(_listeningTask), nameof(_listeningCancellation))]
        public bool IsRunning => _listeningTask != null && _listeningCancellation != null;
    }
}