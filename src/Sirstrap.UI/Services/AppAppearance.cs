namespace Sirstrap.UI.Services
{
    public sealed class AppAppearance(SirstrapSettings settings)
    {
        public const string BundledFontFamilyName = "JetBrains Mono";

        private static readonly (string ResourceKey, double Amount)[] _accentDarkShades = [("SystemAccentColorDark1", 0.15), ("SystemAccentColorDark2", 0.30), ("SystemAccentColorDark3", 0.45)];

        private static readonly (string ResourceKey, double Amount)[] _accentLightShades = [("SystemAccentColorLight1", 0.15), ("SystemAccentColorLight2", 0.30), ("SystemAccentColorLight3", 0.45)];

        private void ApplyAccentColor()
        {
            if (Application.Current is not
                {
                } application || !Color.TryParse(settings.SirstrapAccentColor, out Color accentColor))
            {
                return;
            }

            application.Resources["SystemAccentColor"] = accentColor;

            foreach ((string resourceKey, double amount) in _accentLightShades)
            {
                application.Resources[resourceKey] = Lighten(accentColor, amount);
            }

            foreach ((string resourceKey, double amount) in _accentDarkShades)
            {
                application.Resources[resourceKey] = Darken(accentColor, amount);
            }
        }

        private void ApplyFontFamily()
        {
            if (Application.Current is not
                {
                } application)
            {
                return;
            }

            string fontFamilyName = settings.SirstrapFontFamily;

            application.Resources["AppFontFamily"] = fontFamilyName == BundledFontFamilyName ? new FontFamily($"avares://Sirstrap/Assets#{BundledFontFamilyName}") : new FontFamily(fontFamilyName);
        }

        private void ApplyLanguage()
        {
            Strings.Instance.SetLanguage(settings.SirstrapLanguage);
        }

        private static Color Darken(Color color, double amount)
        {
            return new(color.A, (byte)(color.R * (1 - amount)), (byte)(color.G * (1 - amount)), (byte)(color.B * (1 - amount)));
        }

        private static Color Lighten(Color color, double amount)
        {
            return new(color.A, LightenChannel(color.R, amount), LightenChannel(color.G, amount), LightenChannel(color.B, amount));
        }

        private static byte LightenChannel(byte channel, double amount)
        {
            return (byte)Math.Min(255, channel + ((255 - channel) * amount));
        }

        private static void TryApply(Action apply, string aspect)
        {
            try
            {
                apply();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not apply an appearance setting (Setting: {0}).", aspect);
            }
        }

        public void Apply()
        {
            TryApply(ApplyLanguage, "language");
            TryApply(ApplyFontFamily, "font family");
            TryApply(ApplyAccentColor, "accent color");
        }
    }
}