using Avalonia.Media.Transformation;

namespace Sirstrap.UI.Behaviors
{
    public static class LiquidGlass
    {
        private const double ContentDrift = 6d;

        private const double ContentDurationMs = 220d;

        private const double ContentScale = 0.98;

        private const double FrameMs = 16d;

        private const double IndicatorDurationMs = 340d;

        private const double SquashFactor = 0.12d;

        private const double TrailLag = 0.22d;

        private static readonly Dictionary<Control, DispatcherTimer> _contentTimers = [];

        private static readonly Dictionary<ListBox, IndicatorState> _indicators = [];

        public static readonly AttachedProperty<bool> IsContentTransitionEnabledProperty = AvaloniaProperty.RegisterAttached<Control, bool>("IsContentTransitionEnabled", typeof(LiquidGlass));

        public static readonly AttachedProperty<bool> IsTabIndicatorEnabledProperty = AvaloniaProperty.RegisterAttached<ListBox, bool>("IsTabIndicatorEnabled", typeof(LiquidGlass));

        static LiquidGlass()
        {
            _ = IsTabIndicatorEnabledProperty.Changed.AddClassHandler<ListBox>(OnIsTabIndicatorEnabledChanged);
            _ = IsContentTransitionEnabledProperty.Changed.AddClassHandler<Control>(OnIsContentTransitionEnabledChanged);
        }

        private static void AnimateContent(Control control)
        {
            if (_contentTimers.TryGetValue(control, out DispatcherTimer? running))
            {
                running.Stop();
            }

            Stopwatch clock = Stopwatch.StartNew();
            DispatcherTimer timer = new(DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(FrameMs)
            };

            timer.Tick += (_, _) =>
            {
                double progress = Math.Clamp(clock.Elapsed.TotalMilliseconds / ContentDurationMs, 0, 1);
                double eased = EaseOutCubic(progress);

                if (progress >= 1)
                {
                    timer.Stop();

                    control.ClearValue(Visual.OpacityProperty);
                    control.ClearValue(Visual.RenderTransformProperty);

                    return;
                }

                TransformOperations.Builder builder = TransformOperations.CreateBuilder(2);

                double scale = ContentScale + ((1 - ContentScale) * eased);

                builder.AppendTranslate(0, ContentDrift * (1 - eased));
                builder.AppendScale(scale, scale);

                control.Opacity = eased;
                control.RenderTransform = builder.Build();
            };

            _contentTimers[control] = timer;

            timer.Start();
        }

        private static double EaseOutCubic(double x)
        {
            return 1 - Math.Pow(1 - x, 3);
        }

        private static double EaseOutQuint(double x)
        {
            return 1 - Math.Pow(1 - x, 5);
        }

        private static IndicatorState? GetState(ListBox listBox)
        {
            if (_indicators.TryGetValue(listBox, out IndicatorState? existing))
            {
                return existing;
            }

            if (listBox.Parent is not Panel panel)
            {
                return null;
            }

            Border indicator = new()
            {
                CornerRadius = new CornerRadius(999),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Color.Parse("#2effffff")),
                Background = new LinearGradientBrush
                {
                    StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                    EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
                    GradientStops = [new GradientStop(Color.Parse("#3dffffff"), 0), new GradientStop(Color.Parse("#14ffffff"), 1)]
                }
            };
            Canvas layer = new()
            {
                IsHitTestVisible = false,
                Children =
                {
                    indicator
                }
            };

            panel.Children.Insert(0, layer);

            DispatcherTimer timer = new(TimeSpan.FromMilliseconds(FrameMs), DispatcherPriority.Render, (s, _) => Tick(listBox));
            IndicatorState state = new()
            {
                Indicator = indicator,
                Layer = layer,
                Timer = timer
            };

            _indicators[listBox] = state;

            return state;
        }

        private static void Move(ListBox listBox, bool animate)
        {
            IndicatorState? state = GetState(listBox);

            if (state == null || !TryGetSelectedBounds(listBox, state, out Rect target))
            {
                return;
            }

            if (state.Timer.IsEnabled && state.Target == target)
            {
                return;
            }

            if (!animate || !state.IsPlaced)
            {
                if (state.Target == target && state.IsPlaced)
                {
                    return;
                }

                state.Timer.Stop();

                state.Target = target;
                state.IsPlaced = true;

                Place(state, target.X, target.Right, target);

                return;
            }

            state.FromLeft = Canvas.GetLeft(state.Indicator);
            state.FromRight = state.FromLeft + state.Indicator.Width;
            state.Target = target;

            state.Clock.Restart();
            state.Timer.Start();
        }

        private static void OnControlPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (sender is not Control control || e.Property != Visual.IsVisibleProperty || !e.GetNewValue<bool>())
            {
                return;
            }

            AnimateContent(control);
        }

        private static void OnIsContentTransitionEnabledChanged(Control control, AvaloniaPropertyChangedEventArgs e)
        {
            if (!e.GetNewValue<bool>())
            {
                return;
            }

            control.PropertyChanged += OnControlPropertyChanged;
        }

        private static void OnIsTabIndicatorEnabledChanged(ListBox listBox, AvaloniaPropertyChangedEventArgs e)
        {
            if (!e.GetNewValue<bool>())
            {
                return;
            }

            listBox.SelectionChanged += (_, _) => Move(listBox, animate: true);
            listBox.LayoutUpdated += (_, _) => Move(listBox, animate: false);
        }

        private static void Place(IndicatorState state, double left, double right, Rect target)
        {
            Place(state, left, right, target.Y, target.Height);
        }

        private static void Place(IndicatorState state, double left, double right, double top, double height)
        {
            Canvas.SetLeft(state.Indicator, left);
            Canvas.SetTop(state.Indicator, top);

            state.Indicator.Width = Math.Max(0, right - left);
            state.Indicator.Height = Math.Max(0, height);
        }

        private static void Tick(ListBox listBox)
        {
            if (!_indicators.TryGetValue(listBox, out IndicatorState? state))
            {
                return;
            }

            double progress = Math.Clamp(state.Clock.Elapsed.TotalMilliseconds / IndicatorDurationMs, 0, 1);

            Rect target = state.Target;

            bool movingRight = target.X >= state.FromLeft;
            double lead = EaseOutQuint(progress);
            double trail = EaseOutQuint(Math.Clamp((progress - TrailLag) / (1 - TrailLag), 0, 1));
            double leftEase = movingRight ? trail : lead;
            double rightEase = movingRight ? lead : trail;
            double left = state.FromLeft + ((target.X - state.FromLeft) * leftEase);
            double right = state.FromRight + ((target.Right - state.FromRight) * rightEase);
            double squash = Math.Sin(progress * Math.PI) * SquashFactor;
            double height = target.Height * (1 - squash);

            Place(state, left, right, target.Y + ((target.Height - height) / 2), height);

            if (progress < 1)
            {
                return;
            }

            state.Timer.Stop();

            Place(state, target.X, target.Right, target);
        }

        private static bool TryGetSelectedBounds(ListBox listBox, IndicatorState state, out Rect bounds)
        {
            bounds = default;

            if (listBox.SelectedIndex < 0 || listBox.ContainerFromIndex(listBox.SelectedIndex) is not Control container || container.Bounds.Width <= 0 || container.Bounds.Height <= 0)
            {
                return false;
            }

            Point? origin = container.TranslatePoint(default, state.Layer);

            if (origin == null)
            {
                return false;
            }

            bounds = new Rect(origin.Value, container.Bounds.Size);

            return true;
        }

        public static bool GetIsContentTransitionEnabled(Control control)
        {
            return control.GetValue(IsContentTransitionEnabledProperty);
        }

        public static bool GetIsTabIndicatorEnabled(ListBox listBox)
        {
            return listBox.GetValue(IsTabIndicatorEnabledProperty);
        }

        public static void SetIsContentTransitionEnabled(Control control, bool value)
        {
            _ = control.SetValue(IsContentTransitionEnabledProperty, value);
        }

        public static void SetIsTabIndicatorEnabled(ListBox listBox, bool value)
        {
            _ = listBox.SetValue(IsTabIndicatorEnabledProperty, value);
        }

        private sealed class IndicatorState
        {
            public Stopwatch Clock { get; } = new();

            public double FromLeft { get; set; }

            public double FromRight { get; set; }

            public required Border Indicator { get; init; }

            public bool IsPlaced { get; set; }

            public required Canvas Layer { get; init; }

            public Rect Target { get; set; }

            public required DispatcherTimer Timer { get; init; }
        }
    }
}