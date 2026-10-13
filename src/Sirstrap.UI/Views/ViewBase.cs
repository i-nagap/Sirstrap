namespace Sirstrap.UI.Views
{
    public class ViewBase : Window
    {
        private const int CloseAnimationMilliseconds = 150;

        private const double DimmedOwnerOpacity = 0.25;

        private const int FrameDelayMilliseconds = 10;

        private const double HiddenScale = 0.1;

        private const int OpenAnimationMilliseconds = 300;

        private const int OwnerGapPixels = 8;

        public ViewBase()
        {
            Opacity = 0;
            RenderTransformOrigin = RelativePoint.Center;
            RenderTransform = new ScaleTransform(HiddenScale, HiddenScale);

            Loaded += OnLoaded;
            Closing += OnClosing;
        }

        public event EventHandler? OpenAnimationCompleted;

        private async Task AnimateAsync(int durationMilliseconds, Func<double, double> easing, double fromScale, double toScale, double fromOpacity, double toOpacity)
        {
            int frameCount = durationMilliseconds / FrameDelayMilliseconds;

            for (int frame = 0; frame <= frameCount; frame++)
            {
                double progress = (double)frame / frameCount;
                double scale = Interpolate(fromScale, toScale, easing(progress));
                double opacity = Interpolate(fromOpacity, toOpacity, progress);

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (RenderTransform is ScaleTransform scaleTransform)
                    {
                        scaleTransform.ScaleX = scale;
                        scaleTransform.ScaleY = scale;
                    }

                    Opacity = opacity;
                });

                await Task.Delay(FrameDelayMilliseconds);
            }
        }

        private static double EaseInExpo(double progress)
        {
            return progress <= 0 ? 0 : Math.Pow(2, 10 * (progress - 1));
        }

        private static double EaseOutExpo(double progress)
        {
            return progress >= 1 ? 1 : 1 - Math.Pow(2, -10 * progress);
        }

        private static double Interpolate(double from, double to, double progress)
        {
            return from + ((to - from) * progress);
        }

        private async void OnClosing(object? sender, WindowClosingEventArgs e)
        {
            e.Cancel = true;

            if (TryHideInsteadOfClosing())
            {
                return;
            }

            SetOwnerOpacity(1);

            await AnimateAsync(CloseAnimationMilliseconds, EaseInExpo, 1, HiddenScale, 1, 0);

            Closing -= OnClosing;

            Close();
        }

        private async void OnLoaded(object? sender, RoutedEventArgs e)
        {
            PositionNextToOwner();
            SetOwnerOpacity(DimmedOwnerOpacity);

            await AnimateAsync(OpenAnimationMilliseconds, EaseOutExpo, HiddenScale, 1, 0, 1);

            OpenAnimationFinished = true;

            OpenAnimationCompleted?.Invoke(this, EventArgs.Empty);
        }

        private void PositionNextToOwner()
        {
            if (Owner is not Window owner || Screens.ScreenFromWindow(owner) is not
                {
                } screen)
            {
                return;
            }

            int ownerWidth = (int)(owner.Bounds.Width * owner.RenderScaling);
            int ownerHeight = (int)(owner.Bounds.Height * owner.RenderScaling);
            int width = (int)(Bounds.Width * RenderScaling);
            int height = (int)(Bounds.Height * RenderScaling);

            PixelRect besideOwner = new(owner.Position.X + ownerWidth + OwnerGapPixels, owner.Position.Y, width, height);

            Position = screen.WorkingArea.Contains(besideOwner) ? besideOwner.Position : new PixelPoint(owner.Position.X + ((ownerWidth - width) / 2), owner.Position.Y + ((ownerHeight - height) / 2));
        }

        private void SetOwnerOpacity(double opacity)
        {
            if (Owner is Window owner)
            {
                owner.Opacity = opacity;
            }
        }

        protected virtual bool TryHideInsteadOfClosing()
        {
            return false;
        }

        public bool OpenAnimationFinished { get; private set; }
    }
}