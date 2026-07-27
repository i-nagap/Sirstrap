namespace Sirstrap.UI.ViewModels
{
    public partial class FastFlagsWindowViewModel : ViewModelBase
    {
        private readonly IFastFlagService _fastFlagService;

        private bool _isRefreshingRawText;

        [ObservableProperty]
        private string _currentFullVersion;

        [ObservableProperty]
        private ObservableCollection<FastFlagEntry> _fastFlags = [];

        [ObservableProperty]
        private string _newFastFlagName = string.Empty;

        [ObservableProperty]
        private string _newFastFlagValue = string.Empty;

        [ObservableProperty]
        private string _rawText = string.Empty;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsRawTabSelected))]
        private FastFlagsTab _selectedFastFlagsTab = FastFlagsTab.List;

        [ObservableProperty]
        private Settings _settings;

        public FastFlagsWindowViewModel(Settings settings, IFastFlagService fastFlagService, ISirstrapVersion sirstrapVersion)
        {
            _settings = settings;
            _fastFlagService = fastFlagService;
            _currentFullVersion = sirstrapVersion.GetFullVersion();

            ApplySearch();
        }

        public bool IsRawTabSelected => SelectedFastFlagsTab == FastFlagsTab.Raw;

        public IReadOnlyList<FastFlagsTab> FastFlagsTabs { get; } = Enum.GetValues<FastFlagsTab>();

        partial void OnRawTextChanged(string value)
        {
            if (_isRefreshingRawText)
                return;

            var flags = string.IsNullOrWhiteSpace(value) ? new Dictionary<string, string>() : _fastFlagService.DeserializeFlags(value);

            if (flags == null)
                return;

            Settings.RobloxFastFlags = [.. flags.Select(flag => new FastFlagEntry { Name = flag.Key, Value = flag.Value })];

            ApplySearch();
        }

        partial void OnSearchTextChanged(string value) => ApplySearch();

        partial void OnSelectedFastFlagsTabChanged(FastFlagsTab value)
        {
            if (value != FastFlagsTab.Raw)
                return;

            _isRefreshingRawText = true;

            RawText = _fastFlagService.SerializeFlags(GetFlags());

            _isRefreshingRawText = false;
        }

        private void ApplySearch()
        {
            var term = SearchText.Trim();
            var hasTerm = !string.IsNullOrEmpty(term);

            foreach (var entry in Settings.RobloxFastFlags)
                entry.Opacity = !hasTerm || Matches(entry, term) ? 1 : 0.4;

            IEnumerable<FastFlagEntry> entries = Settings.RobloxFastFlags;

            if (hasTerm)
                entries = entries.OrderByDescending(entry => Matches(entry, term));

            FastFlags = [.. entries];
        }

        private static bool Matches(FastFlagEntry entry, string term) => entry.Name.Contains(term, StringComparison.OrdinalIgnoreCase);

        private Dictionary<string, string> GetFlags()
        {
            Dictionary<string, string> flags = new(StringComparer.Ordinal);

            foreach (var entry in Settings.RobloxFastFlags.Where(entry => !string.IsNullOrWhiteSpace(entry.Name)))
                flags[entry.Name.Trim()] = entry.Value;

            return flags;
        }

        [RelayCommand]
        private void AddFastFlag()
        {
            if (string.IsNullOrWhiteSpace(NewFastFlagName))
                return;

            FastFlagEntry entry = new() { Name = NewFastFlagName.Trim(), Value = NewFastFlagValue };

            var index = 0;

            while (index < Settings.RobloxFastFlags.Count
                && string.Compare(Settings.RobloxFastFlags[index].Name, entry.Name, StringComparison.Ordinal) < 0)
                index++;

            Settings.RobloxFastFlags.Insert(index, entry);

            NewFastFlagName = string.Empty;
            NewFastFlagValue = string.Empty;
            SearchText = string.Empty;

            ApplySearch();
        }

        [RelayCommand]
        private void RemoveFastFlag(FastFlagEntry entry)
        {
            Settings.RobloxFastFlags.Remove(entry);

            ApplySearch();
        }
    }
}
