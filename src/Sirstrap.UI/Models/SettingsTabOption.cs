namespace Sirstrap.UI.Models
{
    public sealed record SettingsTabOption(LocalizedString Display, string? Category, bool IsPreview = false);
}