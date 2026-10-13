using System.Runtime.CompilerServices;

namespace Sirstrap.UI.Behaviors
{
    public static class ScrollFade
    {
        private static readonly Color OpaqueColor = Color.FromArgb(255, 255, 255, 255);

        private static readonly ConditionalWeakTable<ScrollViewer, FadeState> States = [];

        private static readonly Color TransparentColor = Color.FromArgb(0, 255, 255, 255);

        public static readonly AttachedProperty<bool> IsEnabledProperty = AvaloniaProperty.RegisterAttached<ScrollViewer, bool>("IsEnabled", typeof(ScrollFade));

        static ScrollFade()
        {
            _ = IsEnabledProperty.Changed.AddClassHandler<ScrollViewer>(OnIsEnabledChanged);
        }

        private static FadeState GetOrCreate(ScrollViewer scrollViewer)
        {
            if (States.TryGetValue(scrollViewer, out FadeState? existing))
            {
                return existing;
            }

            FadeState state = new(scrollViewer);

            States.Add(scrollViewer, state);

            return state;
        }

        private static void OnIsEnabledChanged(ScrollViewer scrollViewer, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.GetNewValue<bool>())
            {
                scrollViewer.ScrollChanged += OnScrollChanged;

                GetOrCreate(scrollViewer).Apply();
            }
            else
            {
                scrollViewer.ScrollChanged -= OnScrollChanged;

                if (States.TryGetValue(scrollViewer, out FadeState? state))
                {
                    state.Clear();

                    _ = States.Remove(scrollViewer);
                }
            }
        }

        private static void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer && States.TryGetValue(scrollViewer, out FadeState? state))
            {
                state.Update();
            }
        }

        public static bool GetIsEnabled(ScrollViewer scrollViewer)
        {
            return scrollViewer.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(ScrollViewer scrollViewer, bool value)
        {
            _ = scrollViewer.SetValue(IsEnabledProperty, value);
        }

        private sealed class FadeState
        {
            private readonly LinearGradientBrush _fadeBrush;

            private readonly ScrollViewer _scrollViewer;

            private readonly GradientStop _stopBottomEdge;

            private readonly GradientStop _stopBottomInner;

            private readonly GradientStop _stopTopEdge;

            private readonly GradientStop _stopTopInner;

            public FadeState(ScrollViewer scrollViewer)
            {
                _scrollViewer = scrollViewer;
                _stopTopEdge = new GradientStop(OpaqueColor, 0d);
                _stopTopInner = new GradientStop(OpaqueColor, 0d);
                _stopBottomInner = new GradientStop(OpaqueColor, 1d);
                _stopBottomEdge = new GradientStop(OpaqueColor, 1d);
                _fadeBrush = new LinearGradientBrush
                {
                    StartPoint = new RelativePoint(0.5d, 0d, RelativeUnit.Relative),
                    EndPoint = new RelativePoint(0.5d, 1d, RelativeUnit.Relative),
                    GradientStops =
                    {
                        _stopTopEdge,
                        _stopTopInner,
                        _stopBottomInner,
                        _stopBottomEdge
                    }
                };
            }

            public void Apply()
            {
                _scrollViewer.OpacityMask = _fadeBrush;

                Update();
            }

            public void Clear()
            {
                _scrollViewer.ClearValue(Visual.OpacityMaskProperty);
            }

            public void Update()
            {
                double containerHeight = _scrollViewer.Viewport.Height;
                double overflow = _scrollViewer.Extent.Height - containerHeight;

                if (overflow <= 0 || containerHeight <= 0)
                {
                    _scrollViewer.OpacityMask = null;

                    return;
                }

                double y = _scrollViewer.Offset.Y;
                double topCut = Math.Max(0d, y);
                double bottomCut = Math.Max(0d, overflow - y);
                double fadeHeight = Math.Min(16d, containerHeight / 2d);
                double topFade = Math.Min(fadeHeight, topCut);
                double bottomFade = Math.Min(fadeHeight, bottomCut);

                _stopTopEdge.Color = topCut > 0 ? TransparentColor : OpaqueColor;
                _stopBottomEdge.Color = bottomCut > 0 ? TransparentColor : OpaqueColor;
                _stopTopInner.Offset = topFade / containerHeight;
                _stopBottomInner.Offset = 1d - (bottomFade / containerHeight);
                _scrollViewer.OpacityMask = _fadeBrush;
            }
        }
    }
}