namespace Sirstrap.UI.ViewModels
{
    public partial class FastFlagsEditor : ObservableObject
    {
        private const double DimmedOpacity = 0.4;

        private bool _isRefreshingRawJson;

        private string _searchTerm = string.Empty;

        private readonly EditableSettings _settings;

        public FastFlagsEditor(EditableSettings settings)
        {
            _settings = settings;

            Filter(string.Empty);
        }

        private bool MatchesSearch(FastFlagEntry entry)
        {
            return entry.Name.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase);
        }

        partial void OnRawJsonChanged(string value)
        {
            if (_isRefreshingRawJson)
            {
                return;
            }

            IReadOnlyDictionary<string, string>? flags = string.IsNullOrWhiteSpace(value) ? new Dictionary<string, string>() : FastFlagsJson.TryParse(value);

            if (flags is null)
            {
                return;
            }

            _settings.RobloxFastFlags = FastFlagEntry.FromDictionary(flags);

            Filter(_searchTerm);
        }

        [RelayCommand]
        private void Remove(FastFlagEntry entry)
        {
            _ = _settings.RobloxFastFlags.Remove(entry);

            Filter(_searchTerm);
        }

        public bool AddNewFlag()
        {
            if (string.IsNullOrWhiteSpace(NewFlagName))
            {
                return false;
            }

            FastFlagEntry entry = new()
            {
                Name = NewFlagName.Trim(),
                Value = NewFlagValue
            };
            ObservableCollection<FastFlagEntry> flags = _settings.RobloxFastFlags;

            int insertionIndex = 0;

            while (insertionIndex < flags.Count && string.Compare(flags[insertionIndex].Name, entry.Name, StringComparison.Ordinal) < 0)
            {
                insertionIndex++;
            }

            flags.Insert(insertionIndex, entry);

            NewFlagName = string.Empty;
            NewFlagValue = string.Empty;

            Filter(_searchTerm);

            return true;
        }

        public void Filter(string searchTerm)
        {
            _searchTerm = searchTerm.Trim();

            bool hasSearchTerm = _searchTerm.Length > 0;

            foreach (FastFlagEntry entry in _settings.RobloxFastFlags)
            {
                entry.Opacity = !hasSearchTerm || MatchesSearch(entry) ? 1 : DimmedOpacity;
            }

            VisibleFlags = hasSearchTerm ? [.. _settings.RobloxFastFlags.OrderByDescending(MatchesSearch)] : [.. _settings.RobloxFastFlags];
        }

        public void RefreshRawJson()
        {
            _isRefreshingRawJson = true;

            RawJson = FastFlagsJson.Serialize(FastFlagEntry.ToDictionary(_settings.RobloxFastFlags));

            _isRefreshingRawJson = false;
        }

        [ObservableProperty]
        public partial string NewFlagName { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string NewFlagValue { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string RawJson { get; set; } = string.Empty;

        [ObservableProperty]
        public partial ObservableCollection<FastFlagEntry> VisibleFlags { get; set; } = [];
    }
}