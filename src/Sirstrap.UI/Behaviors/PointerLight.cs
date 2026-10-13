using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.VisualTree;
using System.Runtime.CompilerServices;

namespace Sirstrap.UI.Behaviors
{
    public static class PointerLight
    {
        private static readonly ConditionalWeakTable<Control, LightState> States = [];

        public static readonly AttachedProperty<bool> IsEnabledProperty = AvaloniaProperty.RegisterAttached<Control, bool>("IsEnabled", typeof(PointerLight));

        static PointerLight()
        {
            _ = IsEnabledProperty.Changed.AddClassHandler<Control>(OnIsEnabledChanged);
        }

        private static LightState? GetOrCreate(Control control)
        {
            if (States.TryGetValue(control, out LightState? existing))
            {
                return existing;
            }

            ContentPresenter? presenter = control.FindDescendantOfType<ContentPresenter>();

            if (presenter is null)
            {
                return null;
            }

            LightState state = new(presenter);

            States.Add(control, state);

            return state;
        }

        private static void OnIsEnabledChanged(Control control, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.GetNewValue<bool>())
            {
                control.PointerEntered += OnPointerEntered;
                control.PointerMoved += OnPointerMoved;
                control.PointerExited += OnPointerExited;
            }
            else
            {
                control.PointerEntered -= OnPointerEntered;
                control.PointerMoved -= OnPointerMoved;
                control.PointerExited -= OnPointerExited;

                if (States.TryGetValue(control, out LightState? state))
                {
                    state.Clear();

                    _ = States.Remove(control);
                }
            }
        }

        private static void OnPointerEntered(object? sender, PointerEventArgs e)
        {
            if (sender is Control control)
            {
                GetOrCreate(control)?.Apply(e);
            }
        }

        private static void OnPointerExited(object? sender, PointerEventArgs e)
        {
            if (sender is Control control && States.TryGetValue(control, out LightState? state))
            {
                state.Clear();
            }
        }

        private static void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (sender is Control control && States.TryGetValue(control, out LightState? state))
            {
                state.Update(e);
            }
        }

        public static bool GetIsEnabled(Control control)
        {
            return control.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(Control control, bool value)
        {
            _ = control.SetValue(IsEnabledProperty, value);
        }

        private sealed class LightState
        {
            private readonly RadialGradientBrush _brush;

            private readonly ContentPresenter _presenter;

            public LightState(ContentPresenter presenter)
            {
                _presenter = presenter;
                _brush = new RadialGradientBrush
                {
                    GradientStops =
                    {
                        new GradientStop(Color.Parse("#b3ffffff"), 0),
                        new GradientStop(Color.Parse("#59ffffff"), 0.35),
                        new GradientStop(Color.Parse("#1fffffff"), 1)
                    }
                };
            }

            private void MoveTo(PointerEventArgs e)
            {
                Rect bounds = _presenter.Bounds;
                Point position = e.GetPosition(_presenter);

                double x = bounds.Width > 0 ? position.X / bounds.Width : 0.5;
                double y = bounds.Height > 0 ? position.Y / bounds.Height : 0.5;

                RelativePoint point = new(x, y, RelativeUnit.Relative);

                _brush.Center = point;
                _brush.GradientOrigin = point;

                double radius = bounds.Height * 0.70;

                _brush.RadiusX = new RelativeScalar(radius, RelativeUnit.Absolute);
                _brush.RadiusY = new RelativeScalar(radius, RelativeUnit.Absolute);
            }

            public void Apply(PointerEventArgs e)
            {
                MoveTo(e);

                _presenter.Background = _brush;
            }

            public void Clear()
            {
                _presenter.ClearValue(ContentPresenter.BackgroundProperty);
            }

            public void Update(PointerEventArgs e)
            {
                if (ReferenceEquals(_presenter.Background, _brush))
                {
                    MoveTo(e);
                }
            }
        }
    }
}