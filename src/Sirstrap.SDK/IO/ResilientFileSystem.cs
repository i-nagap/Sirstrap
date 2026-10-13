namespace Sirstrap.SDK.IO
{
    public static class ResilientFileSystem
    {
        private const int MaximumAttempts = 5;

        private static void RunWithRetries(Action operation, string operationDescription)
        {
            try
            {
                for (int attempt = 1; attempt <= MaximumAttempts; attempt++)
                {
                    try
                    {
                        operation();

                        return;
                    }
                    catch (IOException) when (attempt < MaximumAttempts)
                    {
                        Thread.Sleep(100 * attempt);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Could not {0}.", operationDescription), ex);
            }
        }

        public static void CreateDirectory(string directoryPath)
        {
            RunWithRetries(() => Directory.CreateDirectory(directoryPath), string.Format(CultureInfo.InvariantCulture, "create the folder {0}", directoryPath));
        }

        public static void DeleteDirectory(string directoryPath)
        {
            RunWithRetries(() =>
            {
                if (Directory.Exists(directoryPath))
                {
                    Directory.Delete(directoryPath, true);
                }
            }, string.Format(CultureInfo.InvariantCulture, "delete the folder {0}", directoryPath));
        }

        public static void DeleteFile(string filePath)
        {
            RunWithRetries(() =>
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }, string.Format(CultureInfo.InvariantCulture, "delete the file {0}", filePath));
        }

        public static void MoveDirectory(string sourcePath, string destinationPath)
        {
            RunWithRetries(() => Directory.Move(sourcePath, destinationPath), string.Format(CultureInfo.InvariantCulture, "move the folder {0} to {1}", sourcePath, destinationPath));
        }
    }
}