using Avalonia.VisualTree;

namespace Sirstrap.UI.Behaviors
{
    public static class StaggeredReveal
    {
        private const int Steps = 25;

        private const double TravelDistance = 12d;

        public static readonly AttachedProperty<bool> IsEnabledProperty = AvaloniaProperty.RegisterAttached<Panel, bool>("IsEnabled", typeof(StaggeredReveal));

        static StaggeredReveal()
        {
            _ = IsEnabledProperty.Changed.AddClassHandler<Panel>(OnIsEnabledChanged);
        }

        private static async Task AnimateChild(Control child, int delayMs)
        {
            TranslateTransform transform = new(0, TravelDistance);

            child.Opacity = 0;
            child.RenderTransform = transform;

            if (delayMs > 0)
            {
                await Task.Delay(delayMs);
            }

            int stepDelay = 250 / Steps;

            for (int i = 0; i <= Steps; i++)
            {
                double progress = (double)i / Steps;
                double easedProgress = EaseOutExpo(progress);

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    child.Opacity = easedProgress;

                    transform.Y = TravelDistance * (1 - easedProgress);
                });

                await Task.Delay(stepDelay);
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                child.ClearValue(Visual.OpacityProperty);
                child.ClearValue(Visual.RenderTransformProperty);
            });
        }

        private static double EaseOutExpo(double x)
        {
            return x >= 1 ? 1 : 1 - Math.Pow(2, -10 * x);
        }

        private static void Hide(Panel panel)
        {
            foreach (Control child in panel.Children)
            {
                if (!child.IsVisible)
                {
                    continue;
                }

                child.Opacity = 0;
                child.RenderTransform = new TranslateTransform(0, TravelDistance);
            }
        }

        private static void OnIsEnabledChanged(Panel panel, AvaloniaPropertyChangedEventArgs e)
        {
            if (!e.GetNewValue<bool>())
            {
                return;
            }

            if (panel.IsLoaded)
            {
                Schedule(panel);
            }
            else
            {
                panel.Loaded += OnPanelLoaded;
            }
        }

        private static void OnPanelLoaded(object? sender, RoutedEventArgs e)
        {
            if (sender is not Panel panel)
            {
                return;
            }

            panel.Loaded -= OnPanelLoaded;

            Schedule(panel);
        }

        private static void Reveal(Panel panel)
        {
            int index = 0;

            foreach (Control child in panel.Children)
            {
                if (!child.IsVisible)
                {
                    continue;
                }

                _ = AnimateChild(child, index * 50);

                index++;
            }
        }

        private static void Schedule(Panel panel)
        {
            ViewBase? view = panel.FindAncestorOfType<ViewBase>();

            if (view == null || view.OpenAnimationFinished)
            {
                Reveal(panel);

                return;
            }

            Hide(panel);

            void OnOpenAnimationCompleted(object? sender, EventArgs e)
            {
                view.OpenAnimationCompleted -= OnOpenAnimationCompleted;

                Reveal(panel);
            }

            view.OpenAnimationCompleted += OnOpenAnimationCompleted;
        }

        public static bool GetIsEnabled(Panel panel)
        {
            return panel.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(Panel panel, bool value)
        {
            _ = panel.SetValue(IsEnabledProperty, value);
        }
    }
}