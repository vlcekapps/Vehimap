using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Domain.Models;
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
        Assert.InRange(service.ConvertDistanceFromKilometers(100m, new AppUnitPreferences("mi", "l")), 62.137m, 62.138m);
        Assert.InRange(service.ConvertDistanceToKilometers(62.137119m, new AppUnitPreferences("mi", "l")), 99.999m, 100.001m);
        Assert.InRange(service.ConvertVolumeToLiters(2.64172m, new AppUnitPreferences("km", "us_gal")), 9.999m, 10.001m);
    }

    [Theory]
    [InlineData("cs-CZ", "none", "comma", "km", "l")]
    [InlineData("en-US", "comma", "dot", "mi", "us_gal")]
    public void Locale_defaults_match_installer_language_policy(
        string language,
        string thousandsSeparator,
        string decimalSeparator,
        string distanceUnit,
        string volumeUnit)
    {
        var defaults = new AppLocaleDefaultsService().GetDefaultsForLanguage(language);

        Assert.Equal(language, defaults.Language);
        Assert.Equal(thousandsSeparator, defaults.ThousandsSeparator);
        Assert.Equal(decimalSeparator, defaults.DecimalSeparator);
        Assert.Equal(distanceUnit, defaults.DistanceUnit);
        Assert.Equal(volumeUnit, defaults.VolumeUnit);
    }

    [Fact]
    public void Supported_settings_service_uses_language_defaults_for_missing_formatting_and_units()
    {
        var settings = new VehimapSettings();
        settings.SetValue("app", "language", "en-US");

        var snapshot = new DesktopSupportedSettingsService().Read(settings);

        Assert.Equal("en-US", snapshot.Language);
        Assert.Equal("comma", snapshot.ThousandsSeparator);
        Assert.Equal("dot", snapshot.DecimalSeparator);
        Assert.Equal("mi", snapshot.DistanceUnit);
        Assert.Equal("us_gal", snapshot.VolumeUnit);
    }

    [Fact]
    public async Task Installer_locale_seed_applies_only_missing_settings_and_is_removed_after_completion()
    {
        var tempRoot = CreateTempDirectory();
        try
        {
            var dataRoot = new VehimapDataRoot(tempRoot, tempRoot, false);
            var seedPath = InstallerLocaleSeedService.GetSeedPath(dataRoot);
            await File.WriteAllTextAsync(seedPath, """{"language":"en-US"}""");
            var settings = new VehimapSettings();
            settings.SetValue("app", "language", "cs-CZ");
            settings.SetValue("app", "decimal_separator", "comma");

            var service = new InstallerLocaleSeedService();
            var result = await service.ApplyIfPresentAsync(dataRoot, settings);
            service.CompleteSeed(result);

            Assert.True(result.SeedFound);
            Assert.True(result.SeedValid);
            Assert.True(result.SettingsChanged);
            Assert.Equal("cs-CZ", settings.GetValue("app", "language"));
            Assert.Equal("none", settings.GetValue("app", "thousands_separator"));
            Assert.Equal("comma", settings.GetValue("app", "decimal_separator"));
            Assert.Equal("km", settings.GetValue("app", "distance_unit"));
            Assert.Equal("l", settings.GetValue("app", "volume_unit"));
            Assert.False(File.Exists(seedPath));
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, true);
            }
        }
    }

    [Fact]
    public async Task Installer_locale_seed_sets_english_defaults_for_fresh_data_set()
    {
        var tempRoot = CreateTempDirectory();
        try
        {
            var dataRoot = new VehimapDataRoot(tempRoot, tempRoot, false);
            await File.WriteAllTextAsync(InstallerLocaleSeedService.GetSeedPath(dataRoot), """{"language":"en-US"}""");
            var settings = new VehimapSettings();

            var result = await new InstallerLocaleSeedService().ApplyIfPresentAsync(dataRoot, settings);

            Assert.True(result.SettingsChanged);
            Assert.Equal("en-US", settings.GetValue("app", "language"));
            Assert.Equal("comma", settings.GetValue("app", "thousands_separator"));
            Assert.Equal("dot", settings.GetValue("app", "decimal_separator"));
            Assert.Equal("mi", settings.GetValue("app", "distance_unit"));
            Assert.Equal("us_gal", settings.GetValue("app", "volume_unit"));
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, true);
            }
        }
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
        Assert.Contains("AutomationProperties.Name=\"{i18n:Loc MainMenu.Name}\"", mainWindow);
        Assert.Contains("Header=\"{i18n:Loc MainMenu.File.PrintableReport}\"", mainWindow);
        Assert.Contains("AutomationProperties.Name=\"{i18n:Loc MainMenu.Vehicle.ServiceBookName}\"", mainWindow);
        Assert.Contains("Text=\"{i18n:Loc VehicleList.SearchHeading}\"", mainWindow);
        Assert.Contains("Content=\"{i18n:Loc WorkspaceTabs.OpenInWindow}\"", mainWindow);
        Assert.Contains("AutomationProperties.Name=\"{i18n:Loc WorkspaceTabs.ContentName}\"", mainWindow);
        Assert.Contains("Title=\"{i18n:Loc Settings.Title}\"", settingsWindow);
        Assert.Contains("Settings.LocaleFormattingHeading", settingsWindow);
        Assert.Contains("AutomationProperties.Name=\"{i18n:Loc About.Title}\"", aboutWindow);
        Assert.Contains("VehicleEditor.HelpText", vehicleEditorWindow);
        Assert.Contains("VehicleEditor.CancelName", vehicleEditorWindow);
    }

    [Fact]
    public void Pilot_editor_dialogs_use_resource_localization_for_static_text()
    {
        var root = FindRepositoryRoot();
        var historyEditor = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "HistoryEditorWindow.axaml"));
        var fuelEditor = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "FuelEditorWindow.axaml"));
        var maintenanceEditor = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "MaintenanceEditorWindow.axaml"));

        Assert.Contains("xmlns:i18n=\"using:Vehimap.Desktop.Localization\"", historyEditor);
        Assert.Contains("HistoryEditor.HelpText", historyEditor);
        Assert.Contains("HistoryEditor.DateLabel", historyEditor);
        Assert.Contains("HistoryEditor.CancelName", historyEditor);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), historyEditor);

        Assert.Contains("xmlns:i18n=\"using:Vehimap.Desktop.Localization\"", fuelEditor);
        Assert.Contains("FuelEditor.HelpText", fuelEditor);
        Assert.Contains("FuelEditor.FuelDetailName", fuelEditor);
        Assert.Contains("FuelEditor.FullTank", fuelEditor);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), fuelEditor);

        Assert.Contains("xmlns:i18n=\"using:Vehimap.Desktop.Localization\"", maintenanceEditor);
        Assert.Contains("MaintenanceEditor.HelpText", maintenanceEditor);
        Assert.Contains("MaintenanceEditor.TemplateName", maintenanceEditor);
        Assert.Contains("MaintenanceEditor.IsActive", maintenanceEditor);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), maintenanceEditor);
    }

    [Fact]
    public void Pilot_shell_surfaces_do_not_keep_czech_hardcoded_ui_text()
    {
        var root = FindRepositoryRoot();
        var mainWindow = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "MainWindow.axaml"));
        var aboutDialogViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "AboutDialogViewModel.cs"));

        Assert.DoesNotMatch(CzechDiacriticsRegex(), mainWindow);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), aboutDialogViewModel);
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

    private static Regex CzechDiacriticsRegex() =>
        new("[ÁČĎÉĚÍŇÓŘŠŤÚŮÝŽáčďéěíňóřšťúůýž]", RegexOptions.Compiled);

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "vehimap-i18n-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
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
