namespace Sirstrap.Core.Deployment
{
    public interface IRobloxVersionService
    {
        bool HasVersionOverride { get; }

        Task<string> GetLatestVersionAsync();

        Task<string> GetSourceVersionAsync();
    }
}
