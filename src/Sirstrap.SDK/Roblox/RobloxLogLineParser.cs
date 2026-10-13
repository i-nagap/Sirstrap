namespace Sirstrap.SDK.Roblox
{
    public static partial class RobloxLogLineParser
    {
        [GeneratedRegex(@"\b(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})\b")]
        private static partial Regex IpAddressPattern();

        private static bool IsPrivate172Address(string address)
        {
            if (!address.StartsWith("172."))
            {
                return false;
            }

            string[] octets = address.Split('.');

            return octets.Length >= 2 && int.TryParse(octets[1], out int secondOctet) && secondOctet is >= 16 and <= 31;
        }

        private static bool IsPrivateAddress(string address)
        {
            return address.StartsWith("127.") || address.StartsWith("192.168.") || address.StartsWith("10.") || IsPrivate172Address(address);
        }

        public static string? TryExtractServerAddress(string logLine)
        {
            try
            {
                if (logLine.Contains("UDMUX") || logLine.Contains("GameHost"))
                {
                    Match match = IpAddressPattern().Match(logLine);

                    if (match.Success)
                    {
                        return match.Groups[1].Value;
                    }
                }

                if (logLine.Contains("server", StringComparison.OrdinalIgnoreCase) || logLine.Contains("connect", StringComparison.OrdinalIgnoreCase))
                {
                    Match match = IpAddressPattern().Match(logLine);

                    if (match.Success && !IsPrivateAddress(match.Groups[1].Value))
                    {
                        return match.Groups[1].Value;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex, "Could not read the server address from a Roblox log line.");
            }

            return null;
        }
    }
}