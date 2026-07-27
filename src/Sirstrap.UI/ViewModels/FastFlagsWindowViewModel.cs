namespace Sirstrap.UI.ViewModels
{
    public partial class FastFlagsWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _currentFullVersion;

        [ObservableProperty]
        private ObservableCollection<FastFlagEntry> _fastFlags = [];

        [ObservableProperty]
        private string _newFastFlagName = string.Empty;

        [ObservableProperty]
        private string _newFastFlagValue = string.Empty;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private Settings _settings;

        public FastFlagsWindowViewModel(Settings settings, ISirstrapVersion sirstrapVersion)
        {
            _settings = settings;
            _currentFullVersion = sirstrapVersion.GetFullVersion();

            ApplySearch();
        }

        partial void OnSearchTextChanged(string value) => ApplySearch();

        private void ApplySearch()
        {
            var term = SearchText.Trim();

            FastFlags = new ObservableCollection<FastFlagEntry>(string.IsNullOrEmpty(term)
                ? Settings.RobloxFastFlags
                : Settings.RobloxFastFlags.Where(entry => entry.Name.Contains(term, StringComparison.OrdinalIgnoreCase)));
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
