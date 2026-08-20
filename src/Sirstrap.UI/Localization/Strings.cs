using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace Sirstrap.UI.Localization
{
    public sealed class Strings
    {
        private static readonly ResourceManager _resourceManager = new("Sirstrap.UI.Resources.Strings", typeof(Strings).Assembly);

        private readonly ConcurrentDictionary<string, LocalizedString> _entries = new(StringComparer.Ordinal);

        private CultureInfo _culture = CultureInfo.CurrentUICulture;

        public static Strings Instance { get; } = new();

        public static IReadOnlyList<LanguageOption> Languages { get; } =
        [
            new(string.Empty, "System"),
            new("en", "English"),
            new("it", "Italiano")
        ];

        public string this[string key] => _resourceManager.GetString(key, _culture) ?? key;

        public LocalizedString Get(string key) => _entries.GetOrAdd(key, static k => new LocalizedString(k));

        public void SetLanguage(string? language)
        {
            var culture = ResolveCulture(language);

            if (culture.Name.Equals(_culture.Name, StringComparison.OrdinalIgnoreCase))
                return;

            _culture = culture;

            CultureInfo.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            foreach (var entry in _entries.Values)
                entry.Refresh();
        }

        private static CultureInfo ResolveCulture(string? language)
        {
            if (string.IsNullOrWhiteSpace(language))
                return CultureInfo.InstalledUICulture;

            try
            {
                return CultureInfo.GetCultureInfo(language);
            }
            catch (CultureNotFoundException)
            {
                Log.Warning("[!] The language {Language} is not available, falling back to the system one.", language);

                return CultureInfo.InstalledUICulture;
            }
        }
    }

    public sealed class LocalizedString(string key) : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string Key { get; } = key;

        public string Value => Strings.Instance[Key];

        public void Refresh() => Dispatcher.UIThread.Post(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value))));
    }

    public sealed record LanguageOption(string Value, string Display);
}
