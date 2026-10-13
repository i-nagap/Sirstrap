namespace Sirstrap.Core.Bootstrap
{
    public static class LaunchArgumentsParser
    {
        private const string OptionPrefix = "--";

        private static bool IsOption(string argument)
        {
            return argument.StartsWith(OptionPrefix);
        }

        private static string NormalizeBlobDirectory(string blobDirectory)
        {
            string normalizedBlobDirectory = blobDirectory.StartsWith('/') ? blobDirectory : $"/{blobDirectory}";

            return normalizedBlobDirectory.EndsWith('/') ? normalizedBlobDirectory : $"{normalizedBlobDirectory}/";
        }

        private static RobloxBinaryType ParseBinaryType(string value)
        {
            return Enum.GetNames<RobloxBinaryType>().Contains(value, StringComparer.OrdinalIgnoreCase) ? Enum.Parse<RobloxBinaryType>(value, ignoreCase: true) : throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The binary type {0} is not supported.", value));
        }

        private static Dictionary<string, string> ParseOptions(string[] arguments)
        {
            Dictionary<string, string> options = new(StringComparer.OrdinalIgnoreCase);

            int index = 0;

            while (index < arguments.Length)
            {
                bool hasValue = IsOption(arguments[index]) && index + 1 < arguments.Length && !IsOption(arguments[index + 1]) && arguments[index].Length > OptionPrefix.Length && !string.IsNullOrEmpty(arguments[index + 1]);

                if (!hasValue)
                {
                    index++;

                    continue;
                }

                options[arguments[index][OptionPrefix.Length..]] = arguments[index + 1];

                index += 2;
            }

            if (arguments.Length > 0 && !IsOption(arguments[0]))
            {
                options["launch-uri"] = arguments[0];
            }

            return options;
        }

        public static LaunchRequest Parse(string[] arguments)
        {
            ArgumentNullException.ThrowIfNull(arguments);

            Dictionary<string, string> options = ParseOptions(arguments);
            RobloxBinaryType binaryType = ParseBinaryType(options.GetValueOrDefault("binary-type", nameof(RobloxBinaryType.WindowsPlayer)));

            string? blobDirectory = options.GetValueOrDefault("blob-directory");

            RobloxDeploymentTarget target = new(binaryType, options.GetValueOrDefault("channel-name", "LIVE"), string.IsNullOrEmpty(blobDirectory) ? binaryType.GetDefaultBlobDirectory() : NormalizeBlobDirectory(blobDirectory), options.GetValueOrDefault("version-hash", string.Empty));

            return new LaunchRequest(target, options.GetValueOrDefault("launch-uri", string.Empty));
        }
    }
}