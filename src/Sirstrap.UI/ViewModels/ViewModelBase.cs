namespace Sirstrap.UI.ViewModels
{
    public class ViewModelBase : ObservableObject
    {
        public static void CloseSpecificWindow<T>() where T : Window => GetSpecificWindow<T>()?.Close();

        public static Window? GetMainWindow() => Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: { } mainWindow } ? mainWindow : null;

        public static T? GetSpecificWindow<T>() where T : Window => GetMainWindow()?.OwnedWindows.OfType<T>().FirstOrDefault();
    }
}
