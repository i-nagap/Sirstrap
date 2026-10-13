namespace Sirstrap.UI.Models
{
    public partial class FastFlagEntry : ObservableObject
    {
        public static ObservableCollection<FastFlagEntry> FromDictionary(IReadOnlyDictionary<string, string> flags)
        {
            return [.. flags.Select(flag => new FastFlagEntry
            {
                Name = flag.Key,
                Value = flag.Value
            })];
        }

        public static Dictionary<string, string> ToDictionary(IEnumerable<FastFlagEntry> entries)
        {
            Dictionary<string, string> flags = new(StringComparer.Ordinal);

            foreach (FastFlagEntry entry in entries.Where(entry => !string.IsNullOrWhiteSpace(entry.Name)))
            {
                flags[entry.Name.Trim()] = entry.Value;
            }

            return flags;
        }

        [ObservableProperty]
        public partial string Name { get; set; } = string.Empty;

        [ObservableProperty]
        public partial double Opacity { get; set; } = 1;

        [ObservableProperty]
        public partial string Value { get; set; } = string.Empty;
    }
}