namespace Sirstrap.UI.Views.Common
{
    public class SettingsRow : ContentControl
    {
        public static readonly StyledProperty<string?> CategoryProperty = AvaloniaProperty.Register<SettingsRow, string?>(nameof(Category));

        public static readonly StyledProperty<string?> LabelProperty = AvaloniaProperty.Register<SettingsRow, string?>(nameof(Label));

        public string? Category
        {
            get => GetValue(CategoryProperty);
            set => SetValue(CategoryProperty, value);
        }

        public string? Label
        {
            get => GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }
    }
}
