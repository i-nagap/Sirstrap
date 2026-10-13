namespace Sirstrap.SDK.FastFlags
{
    public sealed class FastFlagsFile(string filePath)
    {
        private const string ClientAppSettingsFileName = "ClientAppSettings.json";

        private const string ClientSettingsDirectoryName = "ClientSettings";

        private static string GetClientAppSettingsPath(string robloxVersionDirectory)
        {
            return Path.Combine(robloxVersionDirectory, ClientSettingsDirectoryName, ClientAppSettingsFileName);
        }

        private SortedDictionary<string, object> ReadTyped()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    return new(StringComparer.Ordinal);
                }

                SortedDictionary<string, object>? typedFlags = FastFlagsJson.ParseTyped(File.ReadAllText(FilePath));

                if (typedFlags != null)
                {
                    return typedFlags;
                }

                Log.Warning("The FastFlags file is not valid and was ignored (Path: {0}).", FilePath);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not read the FastFlags file (Path: {0}).", FilePath);
            }

            return new(StringComparer.Ordinal);
        }

        public void ApplyTo(string robloxVersionDirectory)
        {
            SortedDictionary<string, object> typedFlags = ReadTyped();

            if (typedFlags.Count == 0)
            {
                RemoveFrom(robloxVersionDirectory);

                return;
            }

            string clientAppSettingsPath = GetClientAppSettingsPath(robloxVersionDirectory);

            _ = Directory.CreateDirectory(Path.Combine(robloxVersionDirectory, ClientSettingsDirectoryName));

            File.WriteAllText(clientAppSettingsPath, FastFlagsJson.SerializeTyped(typedFlags), Encoding.UTF8);
            Log.Information("Applied the FastFlags to Roblox (Count: {0}, Path: {1}).", typedFlags.Count, clientAppSettingsPath);
        }

        public IReadOnlyDictionary<string, string> Read()
        {
            return FastFlagsJson.ToDisplayFlags(ReadTyped());
        }

        public static void RemoveFrom(string robloxVersionDirectory)
        {
            string clientAppSettingsPath = GetClientAppSettingsPath(robloxVersionDirectory);

            if (!File.Exists(clientAppSettingsPath))
            {
                return;
            }

            File.Delete(clientAppSettingsPath);
            Log.Information("Removed the FastFlags from Roblox (Path: {0}).", clientAppSettingsPath);
        }

        public void Write(IReadOnlyDictionary<string, string> flags)
        {
            try
            {
                SortedDictionary<string, object> typedFlags = FastFlagsJson.ToTypedFlags(flags);

                string? directory = Path.GetDirectoryName(FilePath);

                if (!string.IsNullOrWhiteSpace(directory))
                {
                    _ = Directory.CreateDirectory(directory);
                }

                File.WriteAllText(FilePath, FastFlagsJson.SerializeTyped(typedFlags), Encoding.UTF8);
                Log.Information("Saved the FastFlags (Count: {0}, Path: {1}).", typedFlags.Count, FilePath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not save the FastFlags (Path: {0}).", FilePath);
            }
        }

        public string FilePath { get; } = filePath;
    }
}