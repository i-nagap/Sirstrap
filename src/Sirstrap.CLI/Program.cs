namespace Sirstrap.CLI
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            using ServiceProvider services = new ServiceCollection().AddSirstrapCore().BuildServiceProvider();

            SirstrapSession session = services.GetRequiredService<SirstrapSession>();

            try
            {
                await session.StartAsync(SirstrapEdition.CLI, args);

                if (!SirstrapVersionInfo.IsDebugBuild)
                {
                    await services.GetRequiredService<RobloxBootstrapper>().RunAsync(args, SirstrapEdition.CLI);
                }

                Environment.ExitCode = 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Sirstrap stopped because of an unexpected error.");

                Environment.ExitCode = 1;
            }
            finally
            {
                if (!SirstrapVersionInfo.IsDebugBuild)
                {
                    await session.StopAsync();

                    Environment.Exit(Environment.ExitCode);
                }
            }
        }
    }
}