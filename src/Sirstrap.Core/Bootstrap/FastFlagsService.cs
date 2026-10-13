namespace Sirstrap.Core.Bootstrap
{
    public sealed class FastFlagsService(SirstrapSettings settings, FastFlagsFile fastFlagsFile)
    {
        public void ApplyTo(string robloxVersionDirectory)
        {
            try
            {
                if (settings.RobloxFastFlagsEnabled)
                {
                    fastFlagsFile.ApplyTo(robloxVersionDirectory);
                }
                else
                {
                    FastFlagsFile.RemoveFrom(robloxVersionDirectory);
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not apply the FastFlags, Roblox will start without them.");
            }
        }

        public IReadOnlyDictionary<string, string> GetFlags()
        {
            return fastFlagsFile.Read();
        }

        public void SaveFlags(IReadOnlyDictionary<string, string> flags)
        {
            fastFlagsFile.Write(flags);
        }
    }
}