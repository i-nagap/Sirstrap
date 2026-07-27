namespace Sirstrap.UI.Views
{
    public class ViewBase : Window
    {
        public ViewBase()
        {
            Opacity = 0;
            RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
            RenderTransform = new ScaleTransform(0.1, 0.1);

            Loaded += OnLoaded;
            Closing += OnClosing;
        }

        public event EventHandler? OpenAnimationCompleted;

        public bool OpenAnimationFinished { get; private set; }

        private async Task AnimateWindowClose()
        {
            var animationDuration = 150;
            var steps = 15;
            var stepDelay = animationDuration / steps;

            for (int i = 0; i <= steps; i++)
            {
                var progress = (double)i / steps;
                var easedProgress = EaseInExpo(progress);

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (RenderTransform is ScaleTransform scaleTransform)
                    {
                        scaleTransform.ScaleX = 1.0 - (0.9 * easedProgress);
                        scaleTransform.ScaleY = 1.0 - (0.9 * easedProgress);
                    }

                    Opacity = 1 * (1 - progress);
                });

                await Task.Delay(stepDelay);
            }
        }

        private async Task AnimateWindowOpen()
        {
            var animationDuration = 300;
            var steps = 30;
            var stepDelay = animationDuration / steps;

            for (int i = 0; i <= steps; i++)
            {
                var progress = (double)i / steps;
                var easedProgress = EaseOutExpo(progress);

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (RenderTransform is ScaleTransform scaleTransform)
                    {
                        scaleTransform.ScaleX = 0.1 + (0.9 * easedProgress);
                        scaleTransform.ScaleY = 0.1 + (0.9 * easedProgress);
                    }

                    Opacity = 1 * progress;
                });

                await Task.Delay(stepDelay);
            }
        }

        private void DimOwnerOpacity()
        {
            if (Owner is Window owner)
                owner.Opacity = 0.25;
        }

        private static double EaseInExpo(double x) => x <= 0 ? 0 : Math.Pow(2, 10 * (x - 1));

        private static double EaseOutExpo(double x) => x >= 1 ? 1 : 1 - Math.Pow(2, -10 * x);

        protected async void OnClosing(object? sender, WindowClosingEventArgs e)
        {
            e.Cancel = true;

            if (Program.Services.GetRequiredService<SirstrapConfiguration>().SirstrapTrayMode != TrayMode.None
                && Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                && ReferenceEquals(desktop.MainWindow, this))
            {
                Hide();

                App.SetTrayIconVisible(true);

                return;
            }

            RestoreOwnerOpacity();

            await AnimateWindowClose();

            Closing -= OnClosing;

            Close();
        }

        protected async void OnLoaded(object? sender, RoutedEventArgs e)
        {
            PositionRelativeToOwner();
            DimOwnerOpacity();

            await AnimateWindowOpen();

            OpenAnimationFinished = true;

            OpenAnimationCompleted?.Invoke(this, EventArgs.Empty);
        }

        private void PositionRelativeToOwner()
        {
            if (Owner is not Window window)
                return;

            var screen = Screens.ScreenFromWindow(window);

            if (screen == null)
                return;

            var ownerWidthPx = (int)(window.Bounds.Width * window.RenderScaling);
            var ownerHeightPx = (int)(window.Bounds.Height * window.RenderScaling);
            var thisWidthPx = (int)(Bounds.Width * RenderScaling);
            var thisHeightPx = (int)(Bounds.Height * RenderScaling);
            var gap = 8;
            var candidateX = window.Position.X + ownerWidthPx + gap;
            var candidateY = window.Position.Y;
            var workingArea = screen.WorkingArea;

            if (candidateX >= workingArea.X
                && candidateX + thisWidthPx <= workingArea.Right
                && candidateY >= workingArea.Y
                && candidateY + thisHeightPx <= workingArea.Bottom)
            {
                Position = new PixelPoint(candidateX, candidateY);

                return;
            }

            Position = new PixelPoint(window.Position.X + ((ownerWidthPx - thisWidthPx) / 2), window.Position.Y + ((ownerHeightPx - thisHeightPx) / 2));
        }

        private void RestoreOwnerOpacity()
        {
            if (Owner is Window owner)
                owner.Opacity = 1;
        }
    }
}
