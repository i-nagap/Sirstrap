namespace Sirstrap.SDK.Windows
{
    public static partial class ProtocolHandlerRegistrar
    {
        private const string ClassesKeyPath = @"Software\Classes";

        private const string DefaultIconSubKey = "DefaultIcon";

        private const string OpenCommandSubKey = @"shell\open\command";

        private const string UrlProtocolValueName = "URL Protocol";

        private static RegistryKey CreateSubKey(RegistryKey parentKey, string subKeyPath)
        {
            return parentKey.CreateSubKey(subKeyPath) ?? throw new UnauthorizedAccessException(string.Format(CultureInfo.InvariantCulture, "Could not create the registry key {0}\\{1}.", parentKey.Name, subKeyPath));
        }

        private static bool EnsureRegistered(string protocol)
        {
            try
            {
                using RegistryKey? commandKey = Registry.CurrentUser.OpenSubKey($@"{GetProtocolKeyPath(protocol)}\{OpenCommandSubKey}");

                if (commandKey == null)
                {
                    Log.Debug("No program is set to open Roblox links yet (Protocol: {0}).", protocol);
                }
                else
                {
                    string? currentCommand = commandKey.GetValue(string.Empty)?.ToString();

                    if (string.Equals(currentCommand, ExpectedCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        Log.Debug("Sirstrap already opens Roblox links (Protocol: {0}).", protocol);

                        return true;
                    }

                    Log.Information("Roblox links are currently opened by another program (Protocol: {0}, Command: {1}).", protocol, currentCommand ?? "none");
                }

                return Register(protocol);
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not check which program opens Roblox links (Protocol: {0}).", protocol);

                return false;
            }
        }

        private static string GetProtocolKeyPath(string protocol)
        {
            return $@"{ClassesKeyPath}\{protocol}";
        }

        [GeneratedRegex(@"^[a-zA-Z0-9\-]+$")]
        private static partial Regex ProtocolNamePattern();

        private static bool Register(string protocol)
        {
            try
            {
                using RegistryKey protocolKey = CreateSubKey(Registry.CurrentUser, GetProtocolKeyPath(protocol));
                using RegistryKey defaultIconKey = CreateSubKey(protocolKey, DefaultIconSubKey);
                using RegistryKey commandKey = CreateSubKey(protocolKey, OpenCommandSubKey);

                if (protocolKey.GetValue(string.Empty) is null)
                {
                    protocolKey.SetValue(string.Empty, $"URL: {protocol} Protocol");
                    protocolKey.SetValue(UrlProtocolValueName, string.Empty);
                }

                SetValueIfDifferent(defaultIconKey, ExecutablePath);
                SetValueIfDifferent(commandKey, ExpectedCommand);

                Log.Information("Sirstrap now opens Roblox links (Protocol: {0}, Command: {1}).", protocol, ExpectedCommand);

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not set Sirstrap as the program that opens Roblox links (Protocol: {0}).", protocol);

                return false;
            }
        }

        private static void SetValueIfDifferent(RegistryKey key, string value)
        {
            if (!string.Equals(key.GetValue(string.Empty)?.ToString(), value, StringComparison.OrdinalIgnoreCase))
            {
                key.SetValue(string.Empty, value);
            }
        }

        private static void ThrowIfInvalid(string protocol)
        {
            if (string.IsNullOrWhiteSpace(protocol))
            {
                throw new ArgumentException("Protocol name cannot be null, empty, or whitespace.", nameof(protocol));
            }

            if (!ProtocolNamePattern().IsMatch(protocol))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The protocol name {0} is not valid. Only letters, digits and hyphens are allowed.", protocol), nameof(protocol));
            }
        }

        private static string ExecutablePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.FriendlyName);

        private static string ExpectedCommand => $"cmd /c start \"\" \"{ExecutablePath}\" \"%1\"";

        public static bool EnsureRegistered(string protocol, string[] restartArguments)
        {
            ThrowIfInvalid(protocol);

            try
            {
                return EnsureRegistered(protocol);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Warning(ex, "Windows denied access to the registry, asking for administrator rights (Protocol: {0}).", protocol);

                return AdministratorElevation.RunOrRestartElevated(() => EnsureRegistered(protocol), restartArguments, string.Format(CultureInfo.InvariantCulture, "Set Sirstrap as the program that opens {0} links", protocol));
            }
        }

        public static void Unregister(string protocol)
        {
            ThrowIfInvalid(protocol);

            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(GetProtocolKeyPath(protocol), throwOnMissingSubKey: false);
                Log.Information("Sirstrap no longer opens Roblox links (Protocol: {0}).", protocol);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not stop Sirstrap from opening Roblox links (Protocol: {0}).", protocol);
            }
        }
    }
}