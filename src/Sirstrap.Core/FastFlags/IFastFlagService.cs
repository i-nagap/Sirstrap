namespace Sirstrap.Core.FastFlags
{
    public interface IFastFlagService
    {
        void Apply(string versionDirectory, string? fastFlagsFilePath = null);

        IReadOnlyDictionary<string, string>? DeserializeFlags(string json);

        IReadOnlyDictionary<string, string> GetFlags(string? fastFlagsFilePath = null);

        string SerializeFlags(IReadOnlyDictionary<string, string> flags);

        void SetFlags(IReadOnlyDictionary<string, string> flags, string? fastFlagsFilePath = null);
    }
}
