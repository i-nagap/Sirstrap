namespace Sirstrap.SDK.Ipc
{
    public static class IpcClient
    {
        private const int ConnectTimeoutMilliseconds = 5000;

        public static async Task<bool> SendAsync(string pipeName, string message, CancellationToken cancellationToken = default)
        {
            try
            {
                Log.Debug("Sending a message to the running Sirstrap (Pipe: {0}, Message: {1}).", pipeName, message);

                await using NamedPipeClientStream pipe = new(".", pipeName, PipeDirection.Out);

                await pipe.ConnectAsync(ConnectTimeoutMilliseconds, cancellationToken);

                if (!pipe.IsConnected)
                {
                    return false;
                }

                byte[] messageBytes = Encoding.UTF8.GetBytes(message);

                await pipe.WriteAsync(BitConverter.GetBytes(messageBytes.Length), cancellationToken);
                await pipe.WriteAsync(messageBytes, cancellationToken);
                await pipe.FlushAsync(cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not send the message to the running Sirstrap (Pipe: {0}).", pipeName);

                return false;
            }
        }
    }
}