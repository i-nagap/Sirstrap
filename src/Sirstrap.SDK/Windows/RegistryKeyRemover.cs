namespace Sirstrap.SDK.Windows
{
    public static class RegistryKeyRemover
    {
        private static bool IsUserHive(string sid)
        {
            return !string.Equals(sid, ".DEFAULT", StringComparison.OrdinalIgnoreCase) && !sid.EndsWith("_Classes", StringComparison.OrdinalIgnoreCase);
        }

        private static void Remove(RegistryKey hive, string keyPath)
        {
            string hiveName = hive.Name;

            try
            {
                Log.Debug("Checking registry key (Key: {0}\\{1}).", hiveName, keyPath);

                using (RegistryKey? existingKey = hive.OpenSubKey(keyPath))
                {
                    if (existingKey == null)
                    {
                        Log.Debug("Registry key not found, nothing to remove (Key: {0}\\{1}).", hiveName, keyPath);

                        return;
                    }
                }

                hive.DeleteSubKeyTree(keyPath);

                Log.Information("Removed registry key (Key: {0}\\{1}).", hiveName, keyPath);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Warning(ex, "Could not remove registry key because access was denied, administrator rights may be needed (Key: {0}\\{1}).", hiveName, keyPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not remove registry key (Key: {0}\\{1}).", hiveName, keyPath);
            }
        }

        private static void RemoveAll(RegistryKey hive, IEnumerable<string> keyPaths)
        {
            foreach (string keyPath in keyPaths)
            {
                Remove(hive, keyPath);
            }
        }

        public static void RemoveFromAllUsers(IEnumerable<string> keyPaths)
        {
            string[] keyPathList = [.. keyPaths];

            try
            {
                string[] userHives = Registry.Users.GetSubKeyNames();

                Log.Debug("Found the registry of every user (Count: {0}).", userHives.Length);

                foreach (string sid in userHives.Where(IsUserHive))
                {
                    try
                    {
                        Log.Debug("Checking the registry of a user (SID: {0}).", sid);

                        foreach (string keyPath in keyPathList)
                        {
                            Remove(Registry.Users, $@"{sid}\{keyPath}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Could not access the registry of a user (SID: {0}).", sid);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not access the registry of the other users.");
            }
        }

        public static void RemoveFromCurrentUser(IEnumerable<string> keyPaths)
        {
            RemoveAll(Registry.CurrentUser, keyPaths);
        }

        public static void RemoveFromLocalMachine(IEnumerable<string> keyPaths)
        {
            RemoveAll(Registry.LocalMachine, keyPaths);
        }
    }
}