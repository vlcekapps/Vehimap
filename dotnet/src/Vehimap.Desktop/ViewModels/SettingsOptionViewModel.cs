namespace Vehimap.Desktop.ViewModels;

public sealed record SettingsOptionViewModel(string Value, string Label)
{
    public string AccessibleLabel => Label;

    public override string ToString() => Label;
}
