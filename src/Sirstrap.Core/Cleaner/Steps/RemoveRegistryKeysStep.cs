namespace Sirstrap.Core.Cleaner.Steps
{
    public sealed class RemoveRegistryKeysStep(CleanerOptions options) : ICleanupStep
    {
        public void Execute()
        {
            Log.Information("Removing the registry entries of the current user.");
            RegistryKeyRemover.RemoveFromCurrentUser(options.UserRegistryKeys);
            Log.Information("Removing the registry entries of every user.");
            RegistryKeyRemover.RemoveFromAllUsers(options.UserRegistryKeys);
            Log.Information("Removing the system-wide registry entries.");
            RegistryKeyRemover.RemoveFromLocalMachine(options.LocalMachineRegistryKeys);
        }

        public string Name => "Clean registry";
    }
}