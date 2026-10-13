namespace Sirstrap.UI
{
    internal static class Program
    {
        private static void UseBundledNativeLibraries()
        {
            string nativeLibrariesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");

            if (Directory.Exists(nativeLibrariesDirectory))
            {
                _ = NativeMethods.SetDllDirectory(nativeLibrariesDirectory);
            }
        }

        internal static ServiceProvider CreateServices(string[] args)
        {
            return new ServiceCollection().AddSingleton(new LaunchArguments(args)).AddSingleton<IStatusLine, NullStatusLine>().AddSingleton<IUserPrompt, UnattendedUserPrompt>().AddSirstrapCore().AddSingleton<AppAppearance>().AddSingleton<SystemTray>().AddSingleton<VersionSourceCatalog>().AddTransient<EditableSettings>().AddTransient<MainWindowViewModel>().AddTransient<SettingsWindowViewModel>().AddSingleton<Func<SettingsWindowViewModel>>(services => services.GetRequiredService<SettingsWindowViewModel>).BuildServiceProvider();
        }

        public static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<App>().UsePlatformDetect().WithInterFont().LogToTrace();
        }

        [STAThread]
        public static void Main(string[] args)
        {
            UseBundledNativeLibraries();

            if (SirstrapVersionInfo.IsDebugBuild)
            {
                _ = NativeMethods.AllocConsole();
            }

            _ = BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
    }
}