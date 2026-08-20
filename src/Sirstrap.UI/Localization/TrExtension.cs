using Avalonia.Data;
using Avalonia.Data.Core;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;

namespace Sirstrap.UI.Localization
{
    public sealed class TrExtension(string key) : MarkupExtension
    {
        private static readonly CompiledBindingPath _valuePath = new CompiledBindingPathBuilder()
            .Property(new ClrPropertyInfo(nameof(LocalizedString.Value), instance => ((LocalizedString)instance).Value, null, typeof(string)), PropertyInfoAccessorFactory.CreateInpcPropertyAccessor)
            .Build();

        public string Key { get; set; } = key;

        public override object ProvideValue(IServiceProvider serviceProvider) => new CompiledBindingExtension(_valuePath)
        {
            Mode = BindingMode.OneWay,
            Source = Strings.Instance.Get(Key)
        };
    }
}
