namespace Sirstrap.UI.Views
{
    public partial class MainWindow : ViewBase
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override bool TryHideInsteadOfClosing()
        {
            if (SystemTray is not
                {
                    IsEnabled: true
                } systemTray)
            {
                return false;
            }

            systemTray.HideToTray(this);

            return true;
        }

        public SystemTray? SystemTray { get; init; }
    }
}