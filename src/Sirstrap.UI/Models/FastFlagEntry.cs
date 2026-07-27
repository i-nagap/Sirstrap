namespace Sirstrap.UI.Models
{
    public partial class FastFlagEntry : ModelBase
    {
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private double _opacity = 1;

        [ObservableProperty]
        private string _value = string.Empty;
    }
}
