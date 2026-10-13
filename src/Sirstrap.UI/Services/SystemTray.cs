namespace Sirstrap.UI.Services
{
    public sealed class SystemTray
    {
        private TrayIcon? _trayIcon;

        private TrayIcon CreateTrayIcon(IClassicDesktopStyleApplicationLifetime desktop)
        {
            NativeMenuItem showMenuItem = new("Show");

            showMenuItem.Click += (_, _) =>
            {
                if (desktop.MainWindow is { } mainWindow)
                {
                    RestoreFromTray(mainWindow);
                }
            };

            NativeMenuItem exitMenuItem = new("Exit");

            exitMenuItem.Click += (_, _) =>
            {
                DisposeTrayIcon();

                desktop.Shutdown();
            };

            NativeMenu menu = [showMenuItem, new NativeMenuItemSeparator(), exitMenuItem];

            return new TrayIcon
            {
                Icon = desktop.MainWindow?.Icon,
                IsVisible = false,
                Menu = menu,
                ToolTipText = "Sirstrap"
            };
        }

        private void DisposeTrayIcon()
        {
            _trayIcon?.Dispose();

            _trayIcon = null;
        }

        private void RestoreFromTray(Window window)
        {
            _trayIcon?.IsVisible = false;

            window.Show();

            window.WindowState = WindowState.Normal;
        }

        private static IClassicDesktopStyleApplicationLifetime? Desktop => Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;

        public void HideToTray(Window window)
        {
            window.Hide();

            _trayIcon?.IsVisible = true;
        }

        public void SetEnabled(bool enabled)
        {
            if (Desktop is not
                {
                } desktop || enabled == IsEnabled)
            {
                return;
            }

            if (enabled)
            {
                _trayIcon = CreateTrayIcon(desktop);
                desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            }
            else
            {
                DisposeTrayIcon();

                desktop.ShutdownMode = ShutdownMode.OnLastWindowClose;
            }
        }

        public bool IsEnabled => _trayIcon is not null;
    }
}