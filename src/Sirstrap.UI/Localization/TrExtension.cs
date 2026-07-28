using Avalonia.Data;

namespace Sirstrap.UI.Localization
{
    public sealed class TrExtension(string key) : MarkupExtension
    {
        public string Key { get; set; } = key;

        public override object ProvideValue(IServiceProvider serviceProvider) => new Binding(nameof(LocalizedString.Value))
        {
            Mode = BindingMode.OneWay,
            Source = Strings.Instance.Get(Key)
        };
    }
}
