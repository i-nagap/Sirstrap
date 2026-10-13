namespace SirHurt.Cleaner.CLI
{
    internal static class Program
    {
        private const string ConsoleOutputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";

        private const string StartupBanner = @"
   ▄████████  ▄█     ▄████████    ▄█    █▄    ███    █▄     ▄████████     ███      ▄████████  ▄█          ▄████████    ▄████████ ███▄▄▄▄      ▄████████    ▄████████
  ███    ███ ███    ███    ███   ███    ███   ███    ███   ███    ███ ▀█████████▄ ███    ███ ███         ███    ███   ███    ███ ███▀▀▀██▄   ███    ███   ███    ███
  ███    █▀  ███▌   ███    ███   ███    ███   ███    ███   ███    ███    ▀███▀▀██ ███    █▀  ███         ███    █▀    ███    ███ ███   ███   ███    █▀    ███    ███
  ███        ███▌  ▄███▄▄▄▄██▀  ▄███▄▄▄▄███▄▄ ███    ███  ▄███▄▄▄▄██▀     ███   ▀ ███        ███        ▄███▄▄▄       ███    ███ ███   ███  ▄███▄▄▄      ▄███▄▄▄▄██▀
▀███████████ ███▌ ▀▀███▀▀▀▀▀   ▀▀███▀▀▀▀███▀  ███    ███ ▀▀███▀▀▀▀▀       ███     ███        ███       ▀▀███▀▀▀     ▀███████████ ███   ███ ▀▀███▀▀▀     ▀▀███▀▀▀▀▀
         ███ ███  ▀███████████   ███    ███   ███    ███ ▀███████████     ███     ███    █▄  ███         ███    █▄    ███    ███ ███   ███   ███    █▄  ▀███████████
   ▄█    ███ ███    ███    ███   ███    ███   ███    ███   ███    ███     ███     ███    ███ ███▌    ▄   ███    ███   ███    ███ ███   ███   ███    ███   ███    ███
 ▄████████▀  █▀     ███    ███   ███    █▀    ████████▀    ███    ███    ▄████▀   ████████▀  █████▄▄██   ██████████   ███    █▀   ▀█   █▀    ██████████   ███    ███
                    ███    ███                             ███    ███                        ▀                                                            ███    ███";

        private static Logger CreateLogger(IStatusLine statusLine)
        {
            Logger consoleLogger = new LoggerConfiguration().MinimumLevel.Information().WriteTo.Console(outputTemplate: ConsoleOutputTemplate, theme: AnsiConsoleTheme.Code).CreateLogger();

            return new LoggerConfiguration().MinimumLevel.Information().WriteTo.Sink(new StatusLinePreservingSink(consoleLogger, statusLine)).CreateLogger();
        }

        private static async Task Main()
        {
            ConsoleStatusLine statusLine = new();
            Log.Logger = CreateLogger(statusLine);

            TryEnableUtf8Output();

            try
            {
                Log.Information(StartupBanner);
                Log.Information("Starting SirHurt Cleaner.");

                RunCleanup(statusLine);
                WaitForKeyPressWhenInteractive();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "SirHurt Cleaner stopped because of an unexpected error. The cleanup was not completed.");

                Environment.ExitCode = 1;
            }
            finally
            {
                statusLine.Clear();

                await Log.CloseAndFlushAsync();
            }
        }

        private static void RunCleanup(IStatusLine statusLine)
        {
            using ServiceProvider services = new ServiceCollection().AddSingleton(statusLine).AddSirHurtCleaner().BuildServiceProvider();
            bool cleanTemporaryFolders = services.GetRequiredService<IUserPrompt>().Confirm("SirHurt Cleaner can also empty the temporary folders of every user.", defaultAnswer: true);

            services.GetRequiredService<CleanerOptions>().CleanTempFolders = cleanTemporaryFolders;

            services.GetRequiredService<CleanupRunner>().Run();
        }

        private static void TryEnableUtf8Output()
        {
            try
            {
                Console.OutputEncoding = Encoding.UTF8;
            }
            catch (Exception ex)
            {
                Log.Debug(ex, "Could not switch the console to UTF-8, some symbols may not display correctly.");
            }
        }

        private static void WaitForKeyPressWhenInteractive()
        {
            if (Console.IsInputRedirected)
            {
                return;
            }

            Log.Information("Press any key to exit.");

            _ = Console.ReadKey(intercept: true);
        }
    }
}