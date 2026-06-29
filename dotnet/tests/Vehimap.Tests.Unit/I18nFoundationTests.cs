using System.Globalization;
using System.Xml.Linq;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class I18nFoundationTests
{
    [Fact]
    public void English_and_czech_resource_files_have_the_same_keys()
    {
        var root = FindRepositoryRoot();
        var englishKeys = ReadResourceKeys(Path.Combine(root, "dotnet", "src", "Vehimap.Application", "Resources", "Strings.resx"));
        var czechKeys = ReadResourceKeys(Path.Combine(root, "dotnet", "src", "Vehimap.Application", "Resources", "Strings.cs.resx"));

        Assert.Empty(englishKeys.Except(czechKeys).OrderBy(key => key, StringComparer.Ordinal));
        Assert.Empty(czechKeys.Except(englishKeys).OrderBy(key => key, StringComparer.Ordinal));
    }

    [Fact]
    public void Resource_localizer_returns_expected_language_values()
    {
        var english = new ResourceAppLocalizer(CultureInfo.GetCultureInfo("en-US"));
        var czech = new ResourceAppLocalizer(CultureInfo.GetCultureInfo("cs-CZ"));

        Assert.Equal("Vehimap settings", english.GetString("Settings.Title"));
        Assert.Equal("Nastavení Vehimapu", czech.GetString("Settings.Title"));
        Assert.Equal("Missing.Key.For.Test", english.GetString("Missing.Key.For.Test"));
    }

    [Theory]
    [InlineData("en-US", "comma", "dot", "1,234.50")]
    [InlineData("cs-CZ", "space", "comma", "1 234,50")]
    [InlineData("en-US", "none", "dot", "1234.50")]
    public void Number_format_service_respects_separator_preferences(
        string language,
        string thousandsSeparator,
        string decimalSeparator,
        string expected)
    {
        var service = new AppNumberFormatService();
        var preferences = new AppCulturePreferences(language, thousandsSeparator, decimalSeparator);

        Assert.Equal(expected, service.FormatDecimal(1234.5m, preferences, 2));
        Assert.True(service.TryParseDecimal(expected, preferences, out var parsed));
        Assert.Equal(1234.5m, parsed);
    }

    [Fact]
    public void Unit_format_service_keeps_storage_in_metric_and_formats_display_units()
    {
        var service = new AppUnitFormatService();
        var culturePreferences = new AppCulturePreferences("en-US", "comma", "dot");

        Assert.Equal("62.1 mi", service.FormatDistanceFromKilometers(100m, culturePreferences, new AppUnitPreferences("mi", "us_gal"), 1));
        Assert.Equal("2.64 US gal", service.FormatVolumeFromLiters(10m, culturePreferences, new AppUnitPreferences("mi", "us_gal"), 2));
        Assert.InRange(service.ConvertDistanceToKilometers(62.137119m, new AppUnitPreferences("mi", "l")), 99.999m, 100.001m);
        Assert.InRange(service.ConvertVolumeToLiters(2.64172m, new AppUnitPreferences("km", "us_gal")), 9.999m, 10.001m);
    }

    [Fact]
    public void Pilot_xaml_uses_resource_localization_for_main_pilot_surfaces()
    {
        var root = FindRepositoryRoot();
        var mainWindow = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "MainWindow.axaml"));
        var settingsWindow = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "SettingsWindow.axaml"));
        var aboutWindow = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "AboutWindow.axaml"));
        var vehicleEditorWindow = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "VehicleEditorWindow.axaml"));

        Assert.Contains("xmlns:i18n=\"using:Vehimap.Desktop.Localization\"", mainWindow);
        Assert.Contains("Header=\"{i18n:Loc MainMenu.App}\"", mainWindow);
        Assert.Contains("Header=\"{i18n:Loc MainMenu.App.Settings}\"", mainWindow);
        Assert.Contains("Title=\"{i18n:Loc Settings.Title}\"", settingsWindow);
        Assert.Contains("Settings.LocaleFormattingHeading", settingsWindow);
        Assert.Contains("AutomationProperties.Name=\"{i18n:Loc About.Title}\"", aboutWindow);
        Assert.Contains("VehicleEditor.HelpText", vehicleEditorWindow);
        Assert.Contains("VehicleEditor.CancelName", vehicleEditorWindow);
    }

    private static SortedSet<string> ReadResourceKeys(string path)
    {
        var document = XDocument.Load(path);
        return new SortedSet<string>(
            document.Root!
            .Elements("data")
            .Select(element => element.Attribute("name")?.Value)
            .Where(name => !string.IsNullOrWhiteSpace(name))!,
            StringComparer.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var versionFile = Path.Combine(current.FullName, "src", "VERSION");
            var dotnetFolder = Path.Combine(current.FullName, "dotnet");
            if (File.Exists(versionFile) && Directory.Exists(dotnetFolder))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not find the Vehimap repository root.");
    }
}
