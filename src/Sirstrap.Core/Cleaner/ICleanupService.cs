namespace Sirstrap.Core.Cleaner
{
    public interface ICleanupService
    {
        void Run(string trigger, bool cleanTempFolders, bool cleanProtectedFiles);
    }
}
