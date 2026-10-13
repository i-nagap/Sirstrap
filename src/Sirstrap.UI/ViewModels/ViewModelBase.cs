namespace Sirstrap.UI.ViewModels
{
    public class ViewModelBase : ObservableObject
    {
        protected static void CloseOwnedWindow<TWindow>() where TWindow : Window
        {
            MainWindow?.OwnedWindows.OfType<TWindow>().FirstOrDefault()?.Close();
        }

        protected static Window? MainWindow => Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: { } mainWindow } ? mainWindow : null;
    }
}