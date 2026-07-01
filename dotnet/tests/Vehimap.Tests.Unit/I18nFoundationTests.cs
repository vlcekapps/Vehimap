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
        var czechKeys = ReadResourceKeys(Path.Combine(root, "dotnet", "src", "Vehimap.Application", "Resources", "Strings.cs-CZ.resx"));

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
        Assert.Equal("Service book", english.GetString("ServiceBook.Window.Title"));
        Assert.Equal("Servisní knížka", czech.GetString("ServiceBook.Window.Title"));
        Assert.Equal("Vehimap - Service book", english.GetString("ServiceBook.Export.Title"));
        Assert.Equal("Vehimap - Servisní knížka", czech.GetString("ServiceBook.Export.Title"));
        Assert.Equal("Year to date", english.GetString("CostPeriod.YearToDate"));
        Assert.Equal("Od začátku roku", czech.GetString("CostPeriod.YearToDate"));
        Assert.Equal("OK", english.GetString("CostAnalysis.Status.Ok"));
        Assert.Equal("V pořádku", czech.GetString("CostAnalysis.Status.Ok"));
        Assert.Equal("Fill in the reminder and save it.", english.GetString("ReminderEditor.Status.CreatePrompt"));
        Assert.Equal("Vyplňte připomínku a uložte ji.", czech.GetString("ReminderEditor.Status.CreatePrompt"));
        Assert.Equal("Fill in the document and choose an attachment if needed.", english.GetString("RecordEditor.Status.CreatePrompt"));
        Assert.Equal("Vyplňte doklad a podle potřeby vyberte přílohu.", czech.GetString("RecordEditor.Status.CreatePrompt"));
        Assert.Equal("Document attachment has been opened: invoice.pdf.", english.Format("RecordAttachmentAction.FileOpened", "invoice.pdf"));
        Assert.Equal("Příloha dokladu byla otevřena: faktura.pdf.", czech.Format("RecordAttachmentAction.FileOpened", "faktura.pdf"));
        Assert.Equal("Vehicle bundle", english.GetString("VehicleStarterBundle.Title"));
        Assert.Equal("Balíček pro vozidlo", czech.GetString("VehicleStarterBundle.Title"));
        Assert.Equal("Selected: 3 items | Service 1 | Documents 1 | Reminders 1", english.Format("VehicleStarterBundle.Summary.SectionCounts", 3, 1, 1, 1));
        Assert.Equal("Vybráno: 3 položek | Servis 1 | Doklady 1 | Připomínky 1", czech.Format("VehicleStarterBundle.Summary.SectionCounts", 3, 1, 1, 1));
        Assert.Equal("Press Up or Down Arrow to open the list and choose a value.", english.GetString("App.ComboBox.HelpText"));
        Assert.Equal("Šipkami nahoru nebo dolů otevřete seznam a vyberte hodnotu.", czech.GetString("App.ComboBox.HelpText"));
        Assert.Equal("Vehimap Nightly: feedback for nightly 2.0.0", english.Format("FeedbackIssue.Title", "Vehimap Nightly", "nightly", "2.0.0"));
        Assert.Equal("Vehimap Nightly: zpětná vazba k nightly 2.0.0", czech.Format("FeedbackIssue.Title", "Vehimap Nightly", "nightly", "2.0.0"));
        Assert.Equal("Discard changes", english.GetString("PendingEdits.Confirmation.Confirm"));
        Assert.Equal("Zahodit změny", czech.GetString("PendingEdits.Confirmation.Confirm"));
        Assert.Equal("open data audit", english.GetString("PendingEdits.Action.OpenAuditWindow"));
        Assert.Equal("otevřít audit dat", czech.GetString("PendingEdits.Action.OpenAuditWindow"));
        Assert.Equal("exit the application", english.GetString("PendingEdits.Action.ExitApplication"));
        Assert.Equal("ukončit aplikaci", czech.GetString("PendingEdits.Action.ExitApplication"));
        Assert.Equal("New vehicle was saved.", english.GetString("VehicleDetail.Status.NewVehicleSaved"));
        Assert.Equal("Nové vozidlo bylo uloženo.", czech.GetString("VehicleDetail.Status.NewVehicleSaved"));
        Assert.Equal("Downloading update package.", english.GetString("UpdateService.Install.DownloadProgress"));
        Assert.Equal("Stahuji aktualizační balíček.", czech.GetString("UpdateService.Install.DownloadProgress"));
        Assert.Equal("The desktop update channel for the .NET branch has not been published yet.", english.GetString("UpdateService.Check.DotnetManifestUnavailable"));
        Assert.Equal("Desktopový update kanál pro .NET větev zatím není publikovaný.", czech.GetString("UpdateService.Check.DotnetManifestUnavailable"));
        Assert.Equal("development Avalonia shell", english.GetString("AppBuildInfo.RuntimeMode.Development"));
        Assert.Equal("vývojový Avalonia shell", czech.GetString("AppBuildInfo.RuntimeMode.Development"));
        Assert.Equal("Currency", english.GetString("Settings.Currency"));
        Assert.Equal("Měna", czech.GetString("Settings.Currency"));
        Assert.Equal("Installer language preferences were added to the 2.0 data set.", english.GetString("InstallerLocaleSeed.Applied"));
        Assert.Equal("Instalační jazykové předvolby byly doplněny do datové sady 2.0.", czech.GetString("InstallerLocaleSeed.Applied"));
        Assert.Equal("Restore data from backup", english.GetString("AppShell.ImportBackup.ConfirmTitle"));
        Assert.Equal("Obnovit data ze zálohy", czech.GetString("AppShell.ImportBackup.ConfirmTitle"));
        Assert.Equal("close the fuel editor", english.GetString("WorkspaceWindow.CloseAction.Fuel"));
        Assert.Equal("zavřít editor tankování", czech.GetString("WorkspaceWindow.CloseAction.Fuel"));
        Assert.Equal("Search “oil” found 2 history entries.", english.Format("HistoryWorkspace.SearchSummary.Filtered", "oil", 2));
        Assert.Equal("Hledání „olej“ našlo 2 historických záznamů.", czech.Format("HistoryWorkspace.SearchSummary.Filtered", "olej", 2));
        Assert.Equal("2026-06, Service, odometer 12345, cost 2500, note no note", english.Format("HistoryItem.AccessibleLabel", "2026-06", "Service", "12345", "2500", "no note"));
        Assert.Equal("2026-06, Servis, tachometr 12345, cena 2500, poznámka bez poznámky", czech.Format("HistoryItem.AccessibleLabel", "2026-06", "Servis", "12345", "2500", "bez poznámky"));
        Assert.Equal("Missing.Key.For.Test", english.GetString("Missing.Key.For.Test"));
    }

    [Fact]
    public void Pilot_pending_edit_action_descriptions_use_resource_localization()
    {
        var root = FindRepositoryRoot();
        var runtimeController = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Services", "DesktopAppRuntimeController.cs"));
        var mainWindow = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "MainWindow.axaml.cs"));
        var shellViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "MainWindowViewModel.cs"));
        var overviewViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "MainWindowViewModel.Overviews.cs"));
        var workspaceStateViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "MainWindowViewModel.WorkspaceState.cs"));
        var vehicleEditingViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "MainWindowViewModel.VehicleEditing.cs"));

        Assert.Contains("PendingEdits.Action.ExitApplication", runtimeController);
        Assert.Contains("PendingEdits.Action.SwitchVehicle", mainWindow);
        Assert.Contains("PendingEdits.Action.OpenDashboardWindow", mainWindow);
        Assert.Contains("PendingEdits.Action.OpenSelectedTimelineItem", shellViewModel);
        Assert.Contains("PendingEdits.Action.OpenAuditItem", shellViewModel);
        Assert.Contains("PendingEdits.Action.OpenUpcomingOverviewItem", overviewViewModel);
        Assert.Contains("PendingEdits.Action.OpenSmartAdvisorItem", workspaceStateViewModel);
        Assert.Contains("VehicleDetail.Status.NewVehicleSaved", vehicleEditingViewModel);
        Assert.Contains("VehicleDetail.Status.NewVehicleBundleOpenFailed", mainWindow);

        var combined = string.Join(
            Environment.NewLine,
            runtimeController,
            mainWindow,
            shellViewModel,
            overviewViewModel,
            workspaceStateViewModel,
            vehicleEditingViewModel);

        Assert.DoesNotContain("\"ukončit aplikaci\"", combined);
        Assert.DoesNotContain("\"přejít na jiné vozidlo\"", combined);
        Assert.DoesNotContain("\"otevřít audit dat\"", combined);
        Assert.DoesNotContain("\"otevřít chytrého poradce\"", combined);
        Assert.DoesNotContain("\"otevřít doporučení chytrého poradce\"", combined);
        Assert.DoesNotContain("\"Nové vozidlo bylo uloženo.", combined);
        Assert.DoesNotContain("\"Vozidlo bylo upraveno.", combined);
    }

    [Fact]
    public void Platform_update_service_uses_resource_localization_for_user_messages()
    {
        var root = FindRepositoryRoot();
        var updateService = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Platform", "LegacyUpdateService.cs"));
        var buildInfoProvider = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Platform", "AssemblyAppBuildInfoProvider.cs"));
        var mainWindowViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "MainWindowViewModel.cs"));

        Assert.Contains("UpdateService.Check.UpdateAvailable", updateService);
        Assert.Contains("UpdateService.Install.DownloadProgress", updateService);
        Assert.Contains("UpdateService.Install.VerifyProgress", updateService);
        Assert.Contains("UpdateService.Download.HashMismatch", updateService);
        Assert.Contains("AppBuildInfo.RuntimeMode.Development", buildInfoProvider);
        Assert.Contains("AppBuildInfo.RuntimeMode.Published", buildInfoProvider);
        Assert.Contains("localizerProvider: () => DesktopLocalization.Localizer", mainWindowViewModel);

        Assert.DoesNotContain("Stahuji aktualizační balíček.", updateService);
        Assert.DoesNotContain("Automaticka instalace", updateService);
        Assert.DoesNotContain("Pouzivate aktualni verzi", updateService);
        Assert.DoesNotContain("Manifest neobsahuje", updateService);
        Assert.DoesNotContain("samostatná desktopová aplikace", buildInfoProvider);
        Assert.DoesNotContain("vývojový Avalonia shell", buildInfoProvider);
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

    [Theory]
    [InlineData("en-US", "comma", "dot", "USD", "$1,234.50")]
    [InlineData("en-US", "comma", "dot", "EUR", "€1,234.50")]
    [InlineData("cs-CZ", "none", "comma", "CZK", "1234,50 Kč")]
    public void Number_format_service_formats_money_with_selected_currency_without_conversion(
        string language,
        string thousandsSeparator,
        string decimalSeparator,
        string currency,
        string expected)
    {
        var service = new AppNumberFormatService();
        var preferences = new AppCulturePreferences(language, thousandsSeparator, decimalSeparator);

        Assert.Equal(expected, service.FormatMoney(1234.5m, preferences, currency, 2));
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
    [InlineData("cs-CZ", "none", "comma", "km", "l", "CZK")]
    [InlineData("en-US", "comma", "dot", "mi", "us_gal", "USD")]
    public void Locale_defaults_match_installer_language_policy(
        string language,
        string thousandsSeparator,
        string decimalSeparator,
        string distanceUnit,
        string volumeUnit,
        string currency)
    {
        var defaults = new AppLocaleDefaultsService().GetDefaultsForLanguage(language);

        Assert.Equal(language, defaults.Language);
        Assert.Equal(thousandsSeparator, defaults.ThousandsSeparator);
        Assert.Equal(decimalSeparator, defaults.DecimalSeparator);
        Assert.Equal(distanceUnit, defaults.DistanceUnit);
        Assert.Equal(volumeUnit, defaults.VolumeUnit);
        Assert.Equal(currency, defaults.Currency);
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
        Assert.Equal("USD", snapshot.Currency);
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

            var service = new InstallerLocaleSeedService(
                new AppLocaleDefaultsService(),
                new ResourceAppLocalizer(CultureInfo.GetCultureInfo("cs-CZ")));
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
            Assert.Equal("CZK", settings.GetValue("app", "currency"));
            Assert.Equal("Instalační jazykové předvolby byly doplněny do datové sady 2.0.", result.Message);
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

            var result = await new InstallerLocaleSeedService(
                    new AppLocaleDefaultsService(),
                    new ResourceAppLocalizer(CultureInfo.GetCultureInfo("en-US")))
                .ApplyIfPresentAsync(dataRoot, settings);

            Assert.True(result.SettingsChanged);
            Assert.Equal("Installer language preferences were added to the 2.0 data set.", result.Message);
            Assert.Equal("en-US", settings.GetValue("app", "language"));
            Assert.Equal("comma", settings.GetValue("app", "thousands_separator"));
            Assert.Equal("dot", settings.GetValue("app", "decimal_separator"));
            Assert.Equal("mi", settings.GetValue("app", "distance_unit"));
            Assert.Equal("us_gal", settings.GetValue("app", "volume_unit"));
            Assert.Equal("USD", settings.GetValue("app", "currency"));
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
        var serviceBookWindow = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "ServiceBookWindow.axaml"));

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
        Assert.Contains("xmlns:i18n=\"using:Vehimap.Desktop.Localization\"", serviceBookWindow);
        Assert.Contains("AutomationProperties.HelpText=\"{i18n:Loc ServiceBook.Window.HelpText}\"", serviceBookWindow);
        Assert.Contains("AutomationProperties.ItemType=\"{i18n:Loc ServiceBook.Window.ItemType}\"", serviceBookWindow);
        Assert.Contains("Content=\"{i18n:Loc ServiceBook.Window.ExportHtml}\"", serviceBookWindow);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), serviceBookWindow);
    }

    [Fact]
    public void Service_book_uses_resource_localization_for_generated_texts()
    {
        var root = FindRepositoryRoot();
        var service = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Application", "Services", "LegacyServiceBookService.cs"));
        var exportService = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Services", "DesktopServiceBookExportService.cs"));
        var shellServiceBook = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "MainWindowViewModel.ServiceBook.cs"));
        var windowViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "ServiceBookWindowViewModel.cs"));

        Assert.Contains("IAppLocalizer", service);
        Assert.Contains("ServiceBook.Summary.Empty", service);
        Assert.Contains("ServiceBook.Value.Money", service);
        Assert.Contains("ServiceBook.Attachment.Available", shellServiceBook);
        Assert.Contains("ServiceBook.FileDialog.ExportTitle", shellServiceBook);
        Assert.Contains("ServiceBook.Export.Title", exportService);
        Assert.Contains("ServiceBook.Export.Column.Primary", exportService);
        Assert.Contains("ServiceBook.Window.SelectedItemEmpty", windowViewModel);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), exportService);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), shellServiceBook);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), windowViewModel);
    }

    [Fact]
    public void Vehicle_starter_bundle_dialog_uses_resource_localization_for_static_and_runtime_text()
    {
        var root = FindRepositoryRoot();
        var bundleWindow = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "VehicleStarterBundleWindow.axaml"));
        var dialogViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "VehicleStarterBundleDialogViewModel.cs"));
        var itemViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "VehicleStarterBundleItemEditorViewModel.cs"));

        Assert.Contains("xmlns:i18n=\"using:Vehimap.Desktop.Localization\"", bundleWindow);
        Assert.Contains("AutomationProperties.HelpText=\"{i18n:Loc VehicleStarterBundle.ItemsHelpText}\"", bundleWindow);
        Assert.Contains("AutomationProperties.ItemType=\"{i18n:Loc VehicleStarterBundle.ItemType}\"", bundleWindow);
        Assert.Contains("Content=\"{i18n:Loc VehicleStarterBundle.Apply}\"", bundleWindow);
        Assert.Contains("AutomationProperties.Name=\"{i18n:Loc VehicleStarterBundle.CloseName}\"", bundleWindow);
        Assert.Contains("VehicleStarterBundle.Summary.SectionCounts", dialogViewModel);
        Assert.Contains("VehicleStarterBundle.MaintenanceTitle", dialogViewModel);
        Assert.Contains("VehicleStarterBundle.Profile.Empty", dialogViewModel);
        Assert.Contains("VehicleStarterBundle.AccessibleLabel.Full", itemViewModel);
        Assert.Contains("VehicleStarterBundle.AccessibleLabel.Category", itemViewModel);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), bundleWindow);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), dialogViewModel);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), itemViewModel);
    }

    [Fact]
    public void Pilot_editor_dialogs_use_resource_localization_for_static_text()
    {
        var root = FindRepositoryRoot();
        var historyEditor = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "HistoryEditorWindow.axaml"));
        var fuelEditor = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "FuelEditorWindow.axaml"));
        var reminderEditor = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "ReminderEditorWindow.axaml"));
        var maintenanceEditor = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "MaintenanceEditorWindow.axaml"));
        var recordEditor = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "RecordEditorWindow.axaml"));

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

        Assert.Contains("xmlns:i18n=\"using:Vehimap.Desktop.Localization\"", reminderEditor);
        Assert.Contains("ReminderEditor.HelpText", reminderEditor);
        Assert.Contains("ReminderEditor.TitleName", reminderEditor);
        Assert.Contains("ReminderEditor.RepeatName", reminderEditor);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), reminderEditor);

        Assert.Contains("xmlns:i18n=\"using:Vehimap.Desktop.Localization\"", recordEditor);
        Assert.Contains("RecordEditor.HelpText", recordEditor);
        Assert.Contains("RecordEditor.AttachmentModeName", recordEditor);
        Assert.Contains("RecordEditor.BrowseFileName", recordEditor);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), recordEditor);
    }

    [Fact]
    public void Pilot_editor_runtime_statuses_use_resource_localization()
    {
        var root = FindRepositoryRoot();
        var editingViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "MainWindowViewModel.Editing.cs"));
        var shellViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "MainWindowViewModel.cs"));

        Assert.Contains("ReminderEditor.Status.CreatePrompt", editingViewModel);
        Assert.Contains("ReminderEditor.Validation.TitleRequired", editingViewModel);
        Assert.Contains("RecordEditor.Status.CreatePrompt", editingViewModel);
        Assert.Contains("RecordEditor.AttachmentAvailability.ManagedImportPrompt", editingViewModel);
        Assert.Contains("RecordEditor.FileDialog.ManagedTitle", editingViewModel);
        Assert.Contains("RecordAttachmentAction.NoPath", shellViewModel);
        Assert.Contains("RecordAttachmentAction.FileOpened", shellViewModel);
        Assert.DoesNotContain("Vyplňte připomínku a uložte ji.", editingViewModel);
        Assert.DoesNotContain("Vyplňte doklad a podle potřeby vyberte přílohu.", editingViewModel);
        Assert.DoesNotContain("Doklad nemá dostupnou cestu k příloze.", shellViewModel);
    }

    [Fact]
    public void Pilot_workspace_surfaces_use_resource_localization_for_static_text()
    {
        var root = FindRepositoryRoot();
        var historyWorkspace = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "Workspaces", "HistoryWorkspaceView.axaml"));
        var fuelWorkspace = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "Workspaces", "FuelWorkspaceView.axaml"));
        var reminderWorkspace = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "Workspaces", "ReminderWorkspaceView.axaml"));
        var maintenanceWorkspace = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "Workspaces", "MaintenanceWorkspaceView.axaml"));
        var recordWorkspace = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "Workspaces", "RecordWorkspaceView.axaml"));
        var vehicleDetailWorkspace = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "Workspaces", "VehicleDetailWorkspaceView.axaml"));
        var globalSearchWorkspace = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "Workspaces", "GlobalSearchWorkspaceView.axaml"));
        var timelineWorkspace = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "Workspaces", "TimelineWorkspaceView.axaml"));
        var upcomingOverviewWorkspace = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "Workspaces", "UpcomingOverviewWorkspaceView.axaml"));
        var overdueOverviewWorkspace = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "Workspaces", "OverdueOverviewWorkspaceView.axaml"));
        var smartAdvisorWorkspace = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "Workspaces", "SmartAdvisorWorkspaceView.axaml"));
        var auditWorkspace = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "Workspaces", "AuditWorkspaceView.axaml"));
        var costWorkspace = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "Workspaces", "CostWorkspaceView.axaml"));
        var dashboardWorkspace = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "Workspaces", "DashboardWorkspaceView.axaml"));
        var dashboardWorkspaceCodeBehind = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "Workspaces", "DashboardWorkspaceView.axaml.cs"));
        var maintenanceWorkspaceCodeBehind = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "Workspaces", "MaintenanceWorkspaceView.axaml.cs"));
        var vehicleDetailWorkspaceCodeBehind = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "Workspaces", "VehicleDetailWorkspaceView.axaml.cs"));

        Assert.Contains("xmlns:i18n=\"using:Vehimap.Desktop.Localization\"", historyWorkspace);
        Assert.Contains("Text=\"{i18n:Loc HistoryWorkspace.Title}\"", historyWorkspace);
        Assert.Contains("PlaceholderText=\"{i18n:Loc HistoryWorkspace.SearchPlaceholder}\"", historyWorkspace);
        Assert.Contains("AutomationProperties.ItemType=\"{i18n:Loc HistoryWorkspace.ItemType}\"", historyWorkspace);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), historyWorkspace);

        Assert.Contains("xmlns:i18n=\"using:Vehimap.Desktop.Localization\"", fuelWorkspace);
        Assert.Contains("Text=\"{i18n:Loc FuelWorkspace.Title}\"", fuelWorkspace);
        Assert.Contains("PlaceholderText=\"{i18n:Loc FuelWorkspace.SearchPlaceholder}\"", fuelWorkspace);
        Assert.Contains("AutomationProperties.ItemType=\"{i18n:Loc FuelWorkspace.ItemType}\"", fuelWorkspace);
        Assert.Contains("FuelWorkspace.AnalysisHeading", fuelWorkspace);
        Assert.Contains("FuelWorkspace.OpenWarningName", fuelWorkspace);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), fuelWorkspace);

        Assert.Contains("xmlns:i18n=\"using:Vehimap.Desktop.Localization\"", maintenanceWorkspace);
        Assert.Contains("Text=\"{i18n:Loc MaintenanceWorkspace.Title}\"", maintenanceWorkspace);
        Assert.Contains("PlaceholderText=\"{i18n:Loc MaintenanceWorkspace.SearchPlaceholder}\"", maintenanceWorkspace);
        Assert.Contains("AutomationProperties.ItemType=\"{i18n:Loc MaintenanceWorkspace.ItemType}\"", maintenanceWorkspace);
        Assert.Contains("MaintenanceWorkspace.CompleteName", maintenanceWorkspace);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), maintenanceWorkspace);

        Assert.Contains("xmlns:i18n=\"using:Vehimap.Desktop.Localization\"", reminderWorkspace);
        Assert.Contains("Text=\"{i18n:Loc ReminderWorkspace.Title}\"", reminderWorkspace);
        Assert.Contains("PlaceholderText=\"{i18n:Loc ReminderWorkspace.SearchPlaceholder}\"", reminderWorkspace);
        Assert.Contains("AutomationProperties.ItemType=\"{i18n:Loc ReminderWorkspace.ItemType}\"", reminderWorkspace);
        Assert.Contains("ReminderWorkspace.AdvanceName", reminderWorkspace);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), reminderWorkspace);

        Assert.Contains("xmlns:i18n=\"using:Vehimap.Desktop.Localization\"", recordWorkspace);
        Assert.Contains("Text=\"{i18n:Loc RecordWorkspace.Title}\"", recordWorkspace);
        Assert.Contains("PlaceholderText=\"{i18n:Loc RecordWorkspace.SearchPlaceholder}\"", recordWorkspace);
        Assert.Contains("AutomationProperties.ItemType=\"{i18n:Loc RecordWorkspace.ItemType}\"", recordWorkspace);
        Assert.Contains("RecordWorkspace.MoveToManagedName", recordWorkspace);
        Assert.Contains("RecordWorkspace.CopyPathName", recordWorkspace);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), recordWorkspace);

        Assert.Contains("xmlns:i18n=\"using:Vehimap.Desktop.Localization\"", vehicleDetailWorkspace);
        Assert.Contains("Content=\"{i18n:Loc VehicleDetail.CreateVehicle}\"", vehicleDetailWorkspace);
        Assert.Contains("AutomationProperties.Name=\"{i18n:Loc VehicleDetail.RelatedActionsPanelName}\"", vehicleDetailWorkspace);
        Assert.Contains("Text=\"{i18n:Loc VehicleDetail.RelatedActionsHeading}\"", vehicleDetailWorkspace);
        Assert.Contains("AutomationProperties.Name=\"{i18n:Loc VehicleDetail.OpenServiceBookName}\"", vehicleDetailWorkspace);
        Assert.Contains("AutomationProperties.ItemType=\"{i18n:Loc VehicleDetail.EvidenceSummaryItemType}\"", vehicleDetailWorkspace);
        Assert.Contains("AutomationProperties.ItemType=\"{i18n:Loc VehicleDetail.RecentHistoryItemType}\"", vehicleDetailWorkspace);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), vehicleDetailWorkspace);

        Assert.Contains("xmlns:i18n=\"using:Vehimap.Desktop.Localization\"", globalSearchWorkspace);
        Assert.Contains("PlaceholderText=\"{i18n:Loc GlobalSearchWorkspace.SearchPlaceholder}\"", globalSearchWorkspace);
        Assert.Contains("AutomationProperties.Name=\"{i18n:Loc GlobalSearchWorkspace.OpenItemName}\"", globalSearchWorkspace);
        Assert.Contains("AutomationProperties.ItemType=\"{i18n:Loc GlobalSearchWorkspace.ItemType}\"", globalSearchWorkspace);
        Assert.Contains("Text=\"{i18n:Loc GlobalSearchWorkspace.DetailHeading}\"", globalSearchWorkspace);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), globalSearchWorkspace);

        Assert.Contains("xmlns:i18n=\"using:Vehimap.Desktop.Localization\"", timelineWorkspace);
        Assert.Contains("Text=\"{i18n:Loc TimelineWorkspace.ShowFilter}\"", timelineWorkspace);
        Assert.Contains("PlaceholderText=\"{i18n:Loc TimelineWorkspace.SearchPlaceholder}\"", timelineWorkspace);
        Assert.Contains("AutomationProperties.Name=\"{i18n:Loc TimelineWorkspace.OpenItemName}\"", timelineWorkspace);
        Assert.Contains("AutomationProperties.ItemType=\"{i18n:Loc TimelineWorkspace.ItemType}\"", timelineWorkspace);
        Assert.Contains("Text=\"{i18n:Loc TimelineWorkspace.DetailHeading}\"", timelineWorkspace);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), timelineWorkspace);

        Assert.Contains("xmlns:i18n=\"using:Vehimap.Desktop.Localization\"", upcomingOverviewWorkspace);
        Assert.Contains("PlaceholderText=\"{i18n:Loc UpcomingOverviewWorkspace.SearchPlaceholder}\"", upcomingOverviewWorkspace);
        Assert.Contains("AutomationProperties.Name=\"{i18n:Loc UpcomingOverviewWorkspace.OpenItemName}\"", upcomingOverviewWorkspace);
        Assert.Contains("Content=\"{i18n:Loc UpcomingOverviewWorkspace.IncludeMissingGreenCards}\"", upcomingOverviewWorkspace);
        Assert.Contains("AutomationProperties.ItemType=\"{i18n:Loc UpcomingOverviewWorkspace.ItemType}\"", upcomingOverviewWorkspace);
        Assert.Contains("Text=\"{i18n:Loc UpcomingOverviewWorkspace.DetailHeading}\"", upcomingOverviewWorkspace);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), upcomingOverviewWorkspace);

        Assert.Contains("xmlns:i18n=\"using:Vehimap.Desktop.Localization\"", overdueOverviewWorkspace);
        Assert.Contains("PlaceholderText=\"{i18n:Loc OverdueOverviewWorkspace.SearchPlaceholder}\"", overdueOverviewWorkspace);
        Assert.Contains("AutomationProperties.Name=\"{i18n:Loc OverdueOverviewWorkspace.OpenItemName}\"", overdueOverviewWorkspace);
        Assert.Contains("AutomationProperties.ItemType=\"{i18n:Loc OverdueOverviewWorkspace.ItemType}\"", overdueOverviewWorkspace);
        Assert.Contains("Text=\"{i18n:Loc OverdueOverviewWorkspace.DetailHeading}\"", overdueOverviewWorkspace);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), overdueOverviewWorkspace);

        Assert.Contains("xmlns:i18n=\"using:Vehimap.Desktop.Localization\"", smartAdvisorWorkspace);
        Assert.Contains("PlaceholderText=\"{i18n:Loc SmartAdvisorWorkspace.SearchPlaceholder}\"", smartAdvisorWorkspace);
        Assert.Contains("AutomationProperties.Name=\"{i18n:Loc SmartAdvisorWorkspace.OpenItemName}\"", smartAdvisorWorkspace);
        Assert.Contains("AutomationProperties.Name=\"{i18n:Loc SmartAdvisorWorkspace.PriorityFilterName}\"", smartAdvisorWorkspace);
        Assert.Contains("AutomationProperties.ItemType=\"{i18n:Loc SmartAdvisorWorkspace.ItemType}\"", smartAdvisorWorkspace);
        Assert.Contains("Text=\"{i18n:Loc SmartAdvisorWorkspace.DetailHeading}\"", smartAdvisorWorkspace);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), smartAdvisorWorkspace);

        Assert.Contains("xmlns:i18n=\"using:Vehimap.Desktop.Localization\"", auditWorkspace);
        Assert.Contains("PlaceholderText=\"{i18n:Loc AuditWorkspace.SearchPlaceholder}\"", auditWorkspace);
        Assert.Contains("AutomationProperties.Name=\"{i18n:Loc AuditWorkspace.OpenItemName}\"", auditWorkspace);
        Assert.Contains("AutomationProperties.Name=\"{i18n:Loc AuditWorkspace.SortName}\"", auditWorkspace);
        Assert.Contains("AutomationProperties.ItemType=\"{i18n:Loc AuditWorkspace.ItemType}\"", auditWorkspace);
        Assert.Contains("Text=\"{i18n:Loc AuditWorkspace.KeyboardHelp}\"", auditWorkspace);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), auditWorkspace);

        Assert.Contains("xmlns:i18n=\"using:Vehimap.Desktop.Localization\"", costWorkspace);
        Assert.Contains("Text=\"{i18n:Loc CostWorkspace.PeriodHeading}\"", costWorkspace);
        Assert.Contains("AutomationProperties.HelpText=\"{i18n:Loc CostWorkspace.PeriodStartHelp}\"", costWorkspace);
        Assert.Contains("Content=\"{i18n:Loc CostWorkspace.ExportFleetSummary}\"", costWorkspace);
        Assert.Contains("AutomationProperties.Name=\"{i18n:Loc CostWorkspace.OpenVehicleName}\"", costWorkspace);
        Assert.Contains("PlaceholderText=\"{i18n:Loc CostWorkspace.SearchPlaceholder}\"", costWorkspace);
        Assert.Contains("AutomationProperties.ItemType=\"{i18n:Loc CostWorkspace.ItemType}\"", costWorkspace);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), costWorkspace);

        Assert.Contains("xmlns:i18n=\"using:Vehimap.Desktop.Localization\"", dashboardWorkspace);
        Assert.Contains("AutomationProperties.Name=\"{i18n:Loc DashboardWorkspace.ScrollName}\"", dashboardWorkspace);
        Assert.Contains("Text=\"{i18n:Loc DashboardWorkspace.KeyboardHelp}\"", dashboardWorkspace);
        Assert.Contains("Content=\"{i18n:Loc DashboardWorkspace.ShowOnLaunch}\"", dashboardWorkspace);
        Assert.Contains("AutomationProperties.Name=\"{i18n:Loc DashboardWorkspace.AuditListName}\"", dashboardWorkspace);
        Assert.Contains("AutomationProperties.ItemType=\"{i18n:Loc AuditWorkspace.ItemType}\"", dashboardWorkspace);
        Assert.Contains("AutomationProperties.ItemType=\"{i18n:Loc CostWorkspace.ItemType}\"", dashboardWorkspace);
        Assert.Contains("AutomationProperties.ItemType=\"{i18n:Loc TimelineWorkspace.ItemType}\"", dashboardWorkspace);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), dashboardWorkspace);

        Assert.Contains("DesktopLocalization.Localizer.GetString(\"DashboardWorkspace.Status.SelectMaintenancePlan\")", dashboardWorkspaceCodeBehind);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), dashboardWorkspaceCodeBehind);
        Assert.Contains("DesktopLocalization.Localizer.GetString(\"MaintenanceWorkspace.Status.NoMissingTemplates\")", maintenanceWorkspaceCodeBehind);
        Assert.Contains("DesktopLocalization.Localizer.GetString(\"MaintenanceWorkspace.Status.SelectMaintenancePlan\")", maintenanceWorkspaceCodeBehind);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), maintenanceWorkspaceCodeBehind);
        Assert.Contains("\"VehicleDetail.Status.NewVehicleBundleNoItems\"", vehicleDetailWorkspaceCodeBehind);
        Assert.Contains("\"VehicleDetail.Status.BundleNoMissingItems\"", vehicleDetailWorkspaceCodeBehind);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), vehicleDetailWorkspaceCodeBehind);
    }

    [Fact]
    public void Evidence_workspace_runtime_texts_use_resource_localization()
    {
        var root = FindRepositoryRoot();
        var historyWorkspace = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "Workspaces", "HistoryWorkspaceViewModel.cs"));
        var fuelWorkspace = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "Workspaces", "FuelWorkspaceViewModel.cs"));
        var reminderWorkspace = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "Workspaces", "ReminderWorkspaceViewModel.cs"));
        var maintenanceWorkspace = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "Workspaces", "MaintenanceWorkspaceViewModel.cs"));
        var recordWorkspace = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "Workspaces", "RecordWorkspaceViewModel.cs"));
        var mainWindowViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "MainWindowViewModel.cs"));
        var historyItem = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "VehicleHistoryItemViewModel.cs"));
        var fuelItem = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "VehicleFuelItemViewModel.cs"));
        var reminderItem = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "VehicleReminderItemViewModel.cs"));
        var maintenanceItem = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "VehicleMaintenanceItemViewModel.cs"));
        var recordItem = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "VehicleRecordItemViewModel.cs"));

        Assert.Contains("HistoryWorkspace.Summary.Initial", historyWorkspace);
        Assert.Contains("HistoryWorkspace.SearchSummary.Filtered", historyWorkspace);
        Assert.Contains("HistoryWorkspace.Detail.Note", historyWorkspace);
        Assert.DoesNotContain("Vyberte historický záznam", historyWorkspace);

        Assert.Contains("FuelWorkspace.Summary.Initial", fuelWorkspace);
        Assert.Contains("FuelWorkspace.AnalysisSummary.Initial", fuelWorkspace);
        Assert.Contains("FuelWorkspace.SearchSummary.Filtered", fuelWorkspace);
        Assert.DoesNotContain("Vyberte tankování", fuelWorkspace);

        Assert.Contains("ReminderWorkspace.Summary.Initial", reminderWorkspace);
        Assert.Contains("ReminderWorkspace.SearchSummary.Filtered", reminderWorkspace);
        Assert.DoesNotContain("Vyberte připomínku", reminderWorkspace);

        Assert.Contains("MaintenanceWorkspace.Summary.Initial", maintenanceWorkspace);
        Assert.Contains("MaintenanceEditor.TemplateApplied", maintenanceWorkspace);
        Assert.Contains("MaintenanceWorkspace.Status.SelectVehicleFirst", maintenanceWorkspace);
        Assert.DoesNotContain("Vyberte servisní úkon", maintenanceWorkspace);

        Assert.Contains("RecordWorkspace.Summary.Initial", recordWorkspace);
        Assert.Contains("RecordWorkspace.SearchSummary.Filtered", recordWorkspace);
        Assert.Contains("RecordEditor.AttachmentAvailability.SelectOrEnterPath", recordWorkspace);
        Assert.DoesNotContain("Vyberte doklad", recordWorkspace);

        Assert.Contains("HistoryWorkspace.Summary.Initial", mainWindowViewModel);
        Assert.Contains("FuelWorkspace.Summary.Initial", mainWindowViewModel);
        Assert.Contains("ReminderWorkspace.Summary.Initial", mainWindowViewModel);
        Assert.Contains("MaintenanceWorkspace.Summary.Initial", mainWindowViewModel);
        Assert.Contains("RecordWorkspace.Summary.Initial", mainWindowViewModel);

        Assert.Contains("HistoryItem.AccessibleLabel", historyItem);
        Assert.Contains("FuelItem.AccessibleLabel", fuelItem);
        Assert.Contains("ReminderItem.AccessibleLabel", reminderItem);
        Assert.Contains("MaintenanceItem.AccessibleLabel", maintenanceItem);
        Assert.Contains("RecordItem.AccessibleLabel", recordItem);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), historyItem);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), fuelItem);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), reminderItem);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), maintenanceItem);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), recordItem);
    }

    [Fact]
    public void Pilot_safety_dialogs_use_resource_localization_for_static_text()
    {
        var root = FindRepositoryRoot();
        var confirmationWindow = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "ConfirmationWindow.axaml"));
        var dataStoreHealthWindow = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "DataStoreHealthWindow.axaml"));
        var dataStoreHealthViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "DataStoreHealthDialogViewModel.cs"));
        var dataStoreHealthCodeBehind = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "DataStoreHealthWindow.axaml.cs"));

        Assert.Contains("xmlns:i18n=\"using:Vehimap.Desktop.Localization\"", confirmationWindow);
        Assert.Contains("AutomationProperties.HelpText=\"{i18n:Loc Confirmation.HelpText}\"", confirmationWindow);
        Assert.Contains("AutomationProperties.Name=\"{i18n:Loc Confirmation.MessageName}\"", confirmationWindow);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), confirmationWindow);

        Assert.Contains("xmlns:i18n=\"using:Vehimap.Desktop.Localization\"", dataStoreHealthWindow);
        Assert.Contains("Title=\"{i18n:Loc DataStoreHealth.Title}\"", dataStoreHealthWindow);
        Assert.Contains("AutomationProperties.HelpText=\"{i18n:Loc DataStoreHealth.HelpText}\"", dataStoreHealthWindow);
        Assert.Contains("Content=\"{i18n:Loc DataStoreHealth.CopyDiagnostics}\"", dataStoreHealthWindow);
        Assert.Contains("Content=\"{i18n:Loc Common.Close}\"", dataStoreHealthWindow);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), dataStoreHealthWindow);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), dataStoreHealthViewModel);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), dataStoreHealthCodeBehind);
    }

    [Fact]
    public void Pilot_workspace_window_chrome_uses_resource_localization_for_static_text()
    {
        var root = FindRepositoryRoot();
        var workspaceWindows = new[]
        {
            "AuditWindow.axaml",
            "CostWindow.axaml",
            "DashboardWindow.axaml",
            "FuelWindow.axaml",
            "GlobalSearchWindow.axaml",
            "HistoryWindow.axaml",
            "MaintenanceWindow.axaml",
            "OverdueOverviewWindow.axaml",
            "RecordsWindow.axaml",
            "RemindersWindow.axaml",
            "SmartAdvisorWindow.axaml",
            "TimelineWindow.axaml",
            "UpcomingOverviewWindow.axaml",
            "VehicleDetailWindow.axaml"
        };

        foreach (var fileName in workspaceWindows)
        {
            var xaml = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", fileName));
            Assert.Contains("xmlns:i18n=\"using:Vehimap.Desktop.Localization\"", xaml);
            Assert.Contains("AutomationProperties.Name=\"{i18n:Loc WorkspaceWindow.", xaml);
            Assert.Contains("AutomationProperties.HelpText=\"{i18n:Loc WorkspaceWindow.", xaml);
            Assert.Contains("Content=\"{i18n:Loc Common.Close}\"", xaml);
            Assert.DoesNotMatch(CzechDiacriticsRegex(), xaml);
        }
    }

    [Fact]
    public void Pilot_tray_actions_dialog_uses_resource_localization_for_static_text()
    {
        var root = FindRepositoryRoot();
        var trayActionsWindow = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "TrayActionsWindow.axaml"));
        var trayActionsViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "TrayActionsDialogViewModel.cs"));

        Assert.Contains("xmlns:i18n=\"using:Vehimap.Desktop.Localization\"", trayActionsWindow);
        Assert.Contains("AutomationProperties.Name=\"{i18n:Loc TrayActions.WindowName}\"", trayActionsWindow);
        Assert.Contains("AutomationProperties.HelpText=\"{i18n:Loc TrayActions.HelpText}\"", trayActionsWindow);
        Assert.Contains("Text=\"{i18n:Loc TrayActions.Section.ApplicationAndOverviews}\"", trayActionsWindow);
        Assert.Contains("Text=\"{i18n:Loc TrayActions.Section.FileAndSettings}\"", trayActionsWindow);
        Assert.Contains("AutomationProperties.Name=\"{i18n:Loc TrayActions.ExportBackupName}\"", trayActionsWindow);
        Assert.Contains("AutomationProperties.HelpText=\"{i18n:Loc TrayActions.ImportBackupHelpText}\"", trayActionsWindow);
        Assert.Contains("effectiveLocalizer.GetString(\"TrayActions.Title\")", trayActionsViewModel);
        Assert.Contains("effectiveLocalizer.GetString(\"TrayActions.CheckForUpdatesLabel\")", trayActionsViewModel);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), trayActionsWindow);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), trayActionsViewModel);
    }

    [Fact]
    public void Pilot_maintenance_completion_dialog_uses_resource_localization_for_static_text()
    {
        var root = FindRepositoryRoot();
        var maintenanceCompletionWindow = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "MaintenanceCompletionWindow.axaml"));
        var maintenanceCompletionViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "MaintenanceCompletionDialogViewModel.cs"));
        var aboutCodeBehind = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "AboutWindow.axaml.cs"));

        Assert.Contains("xmlns:i18n=\"using:Vehimap.Desktop.Localization\"", maintenanceCompletionWindow);
        Assert.Contains("Title=\"{i18n:Loc MaintenanceCompletion.Title}\"", maintenanceCompletionWindow);
        Assert.Contains("AutomationProperties.HelpText=\"{i18n:Loc MaintenanceCompletion.HelpText}\"", maintenanceCompletionWindow);
        Assert.Contains("Text=\"{i18n:Loc MaintenanceCompletion.CompletedDate}\"", maintenanceCompletionWindow);
        Assert.Contains("AutomationProperties.Name=\"{i18n:Loc MaintenanceCompletion.HistoryNoteName}\"", maintenanceCompletionWindow);
        Assert.Contains("Content=\"{i18n:Loc Common.Save}\"", maintenanceCompletionWindow);
        Assert.Contains("_localizer.Format(\"MaintenanceCompletion.CompletedOdometerLabel\"", maintenanceCompletionViewModel);
        Assert.Contains("_localizer.GetString(\"MaintenanceCompletion.Validation.CompletedDate\")", maintenanceCompletionViewModel);
        Assert.Contains("DesktopLocalization.Localizer.GetString(\"About.Status.DiagnosticsCopied\")", aboutCodeBehind);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), maintenanceCompletionWindow);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), maintenanceCompletionViewModel);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), aboutCodeBehind);
    }

    [Fact]
    public void Pilot_update_dialogs_use_resource_localization_for_static_text()
    {
        var root = FindRepositoryRoot();
        var updateCheckWindow = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "UpdateCheckWindow.axaml"));
        var updateCheckCodeBehind = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "UpdateCheckWindow.axaml.cs"));
        var updateDialogViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "UpdateDialogViewModel.cs"));
        var updateInstallWindow = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "UpdateInstallProgressWindow.axaml"));
        var updateInstallCodeBehind = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "UpdateInstallProgressWindow.axaml.cs"));
        var updateInstallViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "UpdateInstallProgressDialogViewModel.cs"));

        Assert.Contains("xmlns:i18n=\"using:Vehimap.Desktop.Localization\"", updateCheckWindow);
        Assert.Contains("Title=\"{i18n:Loc UpdateCheck.Title}\"", updateCheckWindow);
        Assert.Contains("AutomationProperties.HelpText=\"{i18n:Loc UpdateCheck.HelpText}\"", updateCheckWindow);
        Assert.Contains("AutomationProperties.Name=\"{i18n:Loc UpdateCheck.DetailsName}\"", updateCheckWindow);
        Assert.Contains("Content=\"{i18n:Loc UpdateCheck.CopyDetails}\"", updateCheckWindow);
        Assert.Contains("Content=\"{i18n:Loc Common.Close}\"", updateCheckWindow);
        Assert.Contains("DesktopLocalization.Localizer.GetString(\"UpdateCheck.Status.DetailsCopied\")", updateCheckCodeBehind);
        Assert.Contains("_localizer.GetString(\"UpdateCheck.Heading.Default\")", updateDialogViewModel);
        Assert.Contains("_localizer.Format(\"UpdateCheck.Details.AssetUrl\"", updateDialogViewModel);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), updateCheckWindow);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), updateCheckCodeBehind);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), updateDialogViewModel);

        Assert.Contains("xmlns:i18n=\"using:Vehimap.Desktop.Localization\"", updateInstallWindow);
        Assert.Contains("Title=\"{i18n:Loc UpdateInstall.Title}\"", updateInstallWindow);
        Assert.Contains("AutomationProperties.Name=\"{i18n:Loc UpdateInstall.Title}\"", updateInstallWindow);
        Assert.Contains("AutomationProperties.HelpText=\"{i18n:Loc UpdateInstall.ProgressHelpText}\"", updateInstallWindow);
        Assert.Contains("DesktopLocalization.Localizer.GetString(\"UpdateInstall.CancelledResult\")", updateInstallCodeBehind);
        Assert.Contains("_localizer.GetString(\"UpdateInstall.InitialStatus\")", updateInstallViewModel);
        Assert.Contains("UpdateInstall.ProgressWithBytes", updateInstallViewModel);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), updateInstallWindow);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), updateInstallCodeBehind);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), updateInstallViewModel);
    }

    [Fact]
    public void Pilot_notification_window_uses_resource_localization_for_static_text()
    {
        var root = FindRepositoryRoot();
        var notificationWindow = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "NotificationWindow.axaml"));
        var notificationCodeBehind = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "NotificationWindow.axaml.cs"));
        var runtimeController = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Services", "DesktopAppRuntimeController.cs"));

        Assert.Contains("xmlns:i18n=\"using:Vehimap.Desktop.Localization\"", notificationWindow);
        Assert.Contains("Title=\"{i18n:Loc Notification.Title}\"", notificationWindow);
        Assert.Contains("AutomationProperties.Name=\"{i18n:Loc Notification.WindowName}\"", notificationWindow);
        Assert.Contains("AutomationProperties.HelpText=\"{i18n:Loc Notification.HelpText}\"", notificationWindow);
        Assert.Contains("AutomationProperties.Name=\"{i18n:Loc Notification.HeadingName}\"", notificationWindow);
        Assert.Contains("AutomationProperties.SetName(titleBlock, notificationTitle)", notificationCodeBehind);
        Assert.Contains("DesktopLocalization.Localizer.GetString(\"Notification.AutoBackupTitle\")", runtimeController);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), notificationWindow);
    }

    [Fact]
    public void Domain_fuel_analysis_uses_resource_localization_for_generated_messages()
    {
        var root = FindRepositoryRoot();
        var service = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Application", "Services", "LegacyFuelAnalysisService.cs"));
        var itemViewModels = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "FuelAnalysisItemViewModels.cs"));

        Assert.Contains("FuelAnalysis.Warning.OdometerInvalid.Title", service);
        Assert.Contains("FuelAnalysis.Status.ManySegments", service);
        Assert.Contains("FuelAnalysis.Group.UnknownStation", service);
        Assert.Contains("string AccessibleLabel", itemViewModels);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), service);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), itemViewModels);
    }

    [Fact]
    public void Domain_audit_uses_resource_localization_for_generated_messages()
    {
        var root = FindRepositoryRoot();
        var service = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Application", "Services", "LegacyAuditService.cs"));
        var projectionService = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Services", "DesktopProjectionService.cs"));

        Assert.Contains("Audit.Title.MissingPlate", service);
        Assert.Contains("Audit.Message.OdometerRegression", service);
        Assert.Contains("Audit.Category.Attachment", service);
        Assert.Contains("Audit.Severity.Warning", projectionService);
        Assert.Contains("Audit.Summary.WithItems", projectionService);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), service);
    }

    [Fact]
    public void Domain_smart_advisor_uses_resource_localization_for_generated_messages()
    {
        var root = FindRepositoryRoot();
        var service = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Application", "Services", "LegacySmartAdvisorService.cs"));
        var projectionService = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Services", "DesktopProjectionService.cs"));

        Assert.Contains("SmartAdvisor.Status.Empty", service);
        Assert.Contains("SmartAdvisor.Detail.FuelAnalysis", service);
        Assert.Contains("SmartAdvisor.Title.CostPerKmUnavailable", service);
        Assert.Contains("SmartAdvisor.Action.OpenVehicleCosts", service);
        Assert.Contains("SmartAdvisor.Priority.Critical", projectionService);
        Assert.Contains("SmartAdvisor.Category.Attachments", projectionService);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), service);
    }

    [Fact]
    public void Domain_timeline_uses_resource_localization_for_generated_messages()
    {
        var root = FindRepositoryRoot();
        var service = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Application", "Services", "LegacyTimelineService.cs"));
        var mainWindowViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "MainWindowViewModel.cs"));
        var timelineWorkspaceViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "Workspaces", "TimelineWorkspaceViewModel.cs"));
        var projectionService = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Services", "DesktopProjectionService.cs"));

        Assert.Contains("Timeline.Kind.TechnicalInspection", service);
        Assert.Contains("Timeline.Status.Overdue", service);
        Assert.Contains("Timeline.Value.Cost", service);
        Assert.Contains("Timeline.Value.ServiceTask", service);
        Assert.Contains("new LegacyTimelineService(DesktopLocalization.Localizer)", mainWindowViewModel);
        Assert.Contains("TimelineWorkspace.Detail.Selected", timelineWorkspaceViewModel);
        Assert.Contains("TimelineWorkspace.Summary.Filtered", projectionService);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), service);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), timelineWorkspaceViewModel);
    }

    [Fact]
    public void Domain_calendar_export_uses_resource_localization_for_generated_messages()
    {
        var root = FindRepositoryRoot();
        var service = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Application", "Services", "LegacyCalendarExportService.cs"));
        var mainWindowViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "MainWindowViewModel.cs"));
        var preferencesViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "MainWindowViewModel.TimelinePreferences.cs"));

        Assert.Contains("CalendarExport.Summary", service);
        Assert.Contains("CalendarExport.Description.Vehicle", service);
        Assert.Contains("AppShell.CalendarExport.SavedWithSkippedMaintenance", mainWindowViewModel);
        Assert.Contains("AppShell.FileDialog.CalendarExportTitle", mainWindowViewModel);
        Assert.Contains("TimelineFilterFutureKey", preferencesViewModel);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), service);
    }

    [Fact]
    public void Domain_global_search_uses_resource_localization_for_generated_messages()
    {
        var root = FindRepositoryRoot();
        var service = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Application", "Services", "LegacyGlobalSearchService.cs"));
        var mainWindowViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "MainWindowViewModel.cs"));
        var workspaceViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "Workspaces", "GlobalSearchWorkspaceViewModel.cs"));
        var itemViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "GlobalSearchResultItemViewModel.cs"));

        Assert.Contains("GlobalSearch.Entity.Vehicle", service);
        Assert.Contains("GlobalSearch.Value.Money", service);
        Assert.Contains("GlobalSearch.Attachment.Managed", service);
        Assert.Contains("GlobalSearch.Summary.WithResults", mainWindowViewModel);
        Assert.Contains("GlobalSearch.Detail.Selected", workspaceViewModel);
        Assert.Contains("VehicleLabel", itemViewModel);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), service);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), workspaceViewModel);
    }

    [Fact]
    public void Domain_due_overviews_use_resource_localization_for_generated_messages()
    {
        var root = FindRepositoryRoot();
        var upcomingWorkspaceViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "Workspaces", "UpcomingOverviewWorkspaceViewModel.cs"));
        var overdueWorkspaceViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "Workspaces", "OverdueOverviewWorkspaceViewModel.cs"));
        var dashboardWorkspaceViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "Workspaces", "DashboardWorkspaceViewModel.cs"));
        var overviewsViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "MainWindowViewModel.Overviews.cs"));
        var projectionService = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Services", "DesktopProjectionService.cs"));

        Assert.Contains("Overview.Filter.All", upcomingWorkspaceViewModel);
        Assert.Contains("Overview.Detail.Selected", upcomingWorkspaceViewModel);
        Assert.Contains("Overview.Filter.DataIssues", upcomingWorkspaceViewModel);
        Assert.Contains("Overview.Detail.EmptyOverdue", overdueWorkspaceViewModel);
        Assert.Contains("DashboardTimeline.Detail.Selected", dashboardWorkspaceViewModel);
        Assert.Contains("Overview.Summary.UpcomingWithItems", overviewsViewModel);
        Assert.Contains("Overview.MissingGreen.Title", overviewsViewModel);
        Assert.Contains("Overview.DataIssue.KindLabel", overviewsViewModel);
        Assert.Contains("Overview.Summary.DashboardWithItems", projectionService);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), upcomingWorkspaceViewModel);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), overdueWorkspaceViewModel);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), dashboardWorkspaceViewModel);
    }

    [Fact]
    public void Domain_quick_actions_use_resource_localization_for_generated_messages()
    {
        var root = FindRepositoryRoot();
        var quickActionsViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "MainWindowViewModel.QuickActions.cs"));
        var workspaceStateViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "MainWindowViewModel.WorkspaceState.cs"));
        var appShellViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "MainWindowViewModel.AppShell.cs"));

        Assert.Contains("QuickActions.Status.NearestTechnical", quickActionsViewModel);
        Assert.Contains("QuickActions.Status.ReviewRecordOpened", quickActionsViewModel);
        Assert.Contains("QuickActions.Status.OpenedBackgroundTimeline", quickActionsViewModel);
        Assert.Contains("Timeline.Status.NoAlert", quickActionsViewModel);
        Assert.Contains("Overview.Filter.GreenCards", quickActionsViewModel);
        Assert.Contains("Overview.MissingGreen.Title", quickActionsViewModel);
        Assert.Contains("IsTimelineStatusAttention(item.Status)", appShellViewModel);
        Assert.Contains("WorkspaceStatus.TimelineRefreshed", workspaceStateViewModel);
        Assert.Contains("WorkspaceStatus.SmartAdvisorOpenedEntity", workspaceStateViewModel);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), quickActionsViewModel);
    }

    [Fact]
    public void Domain_app_shell_workflows_use_resource_localization_for_generated_messages()
    {
        var root = FindRepositoryRoot();
        var appShellViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "MainWindowViewModel.AppShell.cs"));
        var appShellController = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Services", "DesktopAppShellController.cs"));

        Assert.Contains("AppShell.ExportBackup.Success", appShellViewModel);
        Assert.Contains("AppShell.ImportBackup.Success", appShellViewModel);
        Assert.Contains("AppShell.VehiclePackage.ExportSuccess", appShellViewModel);
        Assert.Contains("AppShell.PrintableReport.SavedAndOpened", appShellViewModel);
        Assert.Contains("AppShell.Background.NotificationTimelineTitle", appShellViewModel);
        Assert.Contains("AppShell.External.Failed", appShellViewModel);
        Assert.Contains("AppShell.DataStoreHealth.HealthyManual", appShellViewModel);
        Assert.Contains("AppShell.Update.PrepareInstallFailed", appShellViewModel);
        Assert.Contains("AppShell.Controller.ExportBackupCancelled", appShellController);
        Assert.Contains("AppShell.Controller.ImportBackupAction", appShellController);
        Assert.Contains("AppShell.Controller.UpdateInstallerLaunched", appShellController);
        Assert.Contains("AppShell.Controller.UpdateCheckFailed", appShellController);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), appShellViewModel);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), appShellController);
    }

    [Fact]
    public void Pilot_feedback_combo_and_pending_edit_text_use_resource_localization()
    {
        var root = FindRepositoryRoot();
        var appXaml = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "App.axaml"));
        var feedbackIssueBuilder = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Services", "FeedbackIssueUrlBuilder.cs"));
        var pendingEdits = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "MainWindowViewModel.PendingEdits.cs"));
        var dialogService = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Services", "AvaloniaAppShellDialogService.cs"));

        Assert.Contains("xmlns:i18n=\"using:Vehimap.Desktop.Localization\"", appXaml);
        Assert.Contains("Value=\"{i18n:Loc App.ComboBox.HelpText}\"", appXaml);
        Assert.Contains("FeedbackIssue.Title", feedbackIssueBuilder);
        Assert.Contains("FeedbackIssue.ReportHeading", feedbackIssueBuilder);
        Assert.Contains("PendingEdits.VehicleListLockStatus", pendingEdits);
        Assert.Contains("PendingEdits.BlockDataAction", pendingEdits);
        Assert.Contains("PendingEdits.Confirmation.Title", dialogService);
        Assert.Contains("PendingEdits.Confirmation.MessageDiscard", dialogService);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), appXaml);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), feedbackIssueBuilder);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), pendingEdits);
    }

    [Fact]
    public void Pilot_app_edge_text_uses_resource_localization()
    {
        var root = FindRepositoryRoot();
        var installerSeedService = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Application", "Services", "InstallerLocaleSeedService.cs"));
        var trayService = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Services", "AvaloniaTrayService.cs"));
        var dialogService = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Services", "AvaloniaAppShellDialogService.cs"));
        var historyWindow = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "HistoryWindow.axaml.cs"));
        var fuelWindow = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "FuelWindow.axaml.cs"));
        var remindersWindow = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "RemindersWindow.axaml.cs"));
        var maintenanceWindow = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "MaintenanceWindow.axaml.cs"));
        var recordsWindow = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "RecordsWindow.axaml.cs"));

        Assert.Contains("InstallerLocaleSeed.InvalidRead", installerSeedService);
        Assert.Contains("InstallerLocaleSeed.Applied", installerSeedService);
        Assert.Contains("TrayActions.ShowMainWindowLabel", trayService);
        Assert.Contains("TrayActions.ShowDashboardLabel", trayService);
        Assert.Contains("TrayActions.ExitName", trayService);
        Assert.Contains("AppShell.ImportBackup.ConfirmTitle", dialogService);
        Assert.Contains("AppShell.ImportBackup.ConfirmMessage", dialogService);
        Assert.Contains("WorkspaceWindow.CloseAction.History", historyWindow);
        Assert.Contains("WorkspaceWindow.CloseAction.Fuel", fuelWindow);
        Assert.Contains("WorkspaceWindow.CloseAction.Reminder", remindersWindow);
        Assert.Contains("WorkspaceWindow.CloseAction.Maintenance", maintenanceWindow);
        Assert.Contains("WorkspaceWindow.CloseAction.Record", recordsWindow);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), installerSeedService);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), trayService);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), historyWindow);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), fuelWindow);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), remindersWindow);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), maintenanceWindow);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), recordsWindow);
    }

    [Fact]
    public void Pilot_sqlite_health_and_migration_messages_use_resource_localization()
    {
        var root = FindRepositoryRoot();
        var migrationService = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Storage.Sqlite", "SqliteDataMigrationService.cs"));
        var healthService = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Storage.Sqlite", "SqliteDataStoreHealthService.cs"));
        var migrationResult = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Application", "Models", "DataMigrationResult.cs"));
        var healthReport = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Application", "Models", "DataStoreHealthReport.cs"));
        var healthDialogViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "DataStoreHealthDialogViewModel.cs"));

        Assert.Contains("DataMigration.LegacyMigrationCompleted", migrationService);
        Assert.Contains("DataMigration.LegacyCleanupCompleted", migrationService);
        Assert.Contains("DataMigration.NotNeeded", migrationService);
        Assert.Contains("DataStoreHealth.Report.SummaryHealthy", healthService);
        Assert.Contains("DataStoreHealth.Report.DatabaseCheckFailed", healthService);
        Assert.Contains("DataStoreHealth.Report.QuickCheckOk", healthService);
        Assert.Contains("DataStoreHealth.Diagnostics.Title", healthDialogViewModel);
        Assert.Contains("DataStoreHealth.Diagnostics.DetailItem", healthDialogViewModel);
        Assert.DoesNotContain("DiagnosticText", healthReport);
        Assert.DoesNotContain("NotNeeded", migrationResult);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), migrationService);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), healthService);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), migrationResult);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), healthReport);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), healthDialogViewModel);
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
