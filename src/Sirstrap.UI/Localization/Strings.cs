using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace Sirstrap.UI.Localization
{
    public sealed class Strings
    {
        private static readonly ResourceManager _resourceManager = new("Sirstrap.UI.Resources.Strings", typeof(Strings).Assembly);

        private CultureInfo _culture = CultureInfo.CurrentUICulture;

        private readonly ConcurrentDictionary<string, LocalizedString> _entries = new(StringComparer.Ordinal);

        public string this[string key] => _resourceManager.GetString(key, _culture) ?? key;

        private static CultureInfo ResolveCulture(string language)
        {
            if (string.IsNullOrWhiteSpace(language))
            {
                return CultureInfo.InstalledUICulture;
            }

            try
            {
                return CultureInfo.GetCultureInfo(language);
            }
            catch (CultureNotFoundException ex)
            {
                Log.Warning(ex, "The selected language is not available, using the system language instead (Language: {0}).", language);

                return CultureInfo.InstalledUICulture;
            }
        }

        public LocalizedString Get(string key)
        {
            return _entries.GetOrAdd(key, static k => new LocalizedString(k));
        }

        public void SetLanguage(string language)
        {
            CultureInfo culture = ResolveCulture(language);

            if (culture.Name.Equals(_culture.Name, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _culture = culture;

            CultureInfo.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            foreach (LocalizedString entry in _entries.Values)
            {
                entry.Refresh();
            }
        }

        public static Strings Instance { get; } = new();

        public static IReadOnlyList<LanguageOption> Languages { get; } = [new(string.Empty, "System"), new("en", "English"), new("de", "Deutsch"), new("es", "Español"), new("fr", "Français"), new("it", "Italiano"), new("vec", "Vèneto")];
    }

    public sealed class LocalizedString(string key) : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public void Refresh()
        {
            Dispatcher.UIThread.Post(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value))));
        }

        public string Key { get; } = key;

        public string Value => Strings.Instance[Key];
    }

    public sealed record LanguageOption(string Value, string Display);
}