// SPDX-License-Identifier: GPL-3.0-or-later
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Desktop.Localization;
using Vehimap.Desktop.ViewModels;
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
    public void Translation_resources_are_non_empty_and_preserve_format_placeholders()
    {
        var root = FindRepositoryRoot();
        var english = ReadResourceValues(Path.Combine(root, "dotnet", "src", "Vehimap.Application", "Resources", "Strings.resx"));
        var czech = ReadResourceValues(Path.Combine(root, "dotnet", "src", "Vehimap.Application", "Resources", "Strings.cs-CZ.resx"));
        var failures = new List<string>();

        foreach (var key in english.Keys.OrderBy(value => value, StringComparer.Ordinal))
        {
            if (string.IsNullOrWhiteSpace(english[key]))
            {
                failures.Add($"{key}: English value is empty.");
            }

            if (string.IsNullOrWhiteSpace(czech[key]))
            {
                failures.Add($"{key}: Czech value is empty.");
            }

            var englishPlaceholders = ExtractFormatPlaceholders(english[key]);
            var czechPlaceholders = ExtractFormatPlaceholders(czech[key]);
            if (!englishPlaceholders.SequenceEqual(czechPlaceholders, StringComparer.Ordinal))
            {
                failures.Add($"{key}: EN placeholders [{string.Join(", ", englishPlaceholders)}], CS placeholders [{string.Join(", ", czechPlaceholders)}].");
            }
        }

        Assert.True(
            failures.Count == 0,
            "Translation resources must be non-empty and preserve the same composite-format placeholders." +
            Environment.NewLine +
            string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public void English_ui_resources_do_not_contain_czech_diacritics_outside_documented_compatibility_data()
    {
        var root = FindRepositoryRoot();
        var english = ReadResourceValues(Path.Combine(root, "dotnet", "src", "Vehimap.Application", "Resources", "Strings.resx"));
        var allowedBilingualDataKeys = new HashSet<string>(StringComparer.Ordinal)
        {
            "ServiceBook.RecordKeywords"
        };
        var failures = english
            .Where(item => CzechDiacriticsRegex().IsMatch(item.Value) && !allowedBilingualDataKeys.Contains(item.Key))
            .Select(item => $"{item.Key}: {item.Value}")
            .ToArray();

        Assert.True(
            failures.Length == 0,
            "English UI resources must not contain Czech diacritics. Explicit bilingual parser/search data needs a documented allowlist entry." +
            Environment.NewLine +
            string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public void Plural_resource_families_are_complete_in_both_languages()
    {
        var root = FindRepositoryRoot();
        var english = ReadResourceKeys(Path.Combine(root, "dotnet", "src", "Vehimap.Application", "Resources", "Strings.resx"));
        var czech = ReadResourceKeys(Path.Combine(root, "dotnet", "src", "Vehimap.Application", "Resources", "Strings.cs-CZ.resx"));
        var prefixes = english
            .Where(key => key.EndsWith(".One", StringComparison.Ordinal) || key.EndsWith(".Few", StringComparison.Ordinal))
            .Select(key => key[..key.LastIndexOf('.')])
            .Distinct(StringComparer.Ordinal)
            .OrderBy(key => key, StringComparer.Ordinal)
            .ToArray();
        var failures = new List<string>();

        foreach (var prefix in prefixes)
        {
            foreach (var suffix in new[] { "One", "Few", "Other" })
            {
                var key = $"{prefix}.{suffix}";
                if (!english.Contains(key))
                {
                    failures.Add($"English: {key}");
                }

                if (!czech.Contains(key))
                {
                    failures.Add($"Czech: {key}");
                }
            }
        }

        Assert.NotEmpty(prefixes);
        Assert.True(
            failures.Count == 0,
            "Every plural resource family must define One, Few and Other variants in both languages." +
            Environment.NewLine +
            string.Join(Environment.NewLine, failures));
    }

    [Theory]
    [InlineData("en-US", 0, AppPluralForm.Other)]
    [InlineData("en-US", 1, AppPluralForm.One)]
    [InlineData("en-US", 2, AppPluralForm.Other)]
    [InlineData("cs-CZ", 0, AppPluralForm.Other)]
    [InlineData("cs-CZ", 1, AppPluralForm.One)]
    [InlineData("cs-CZ", 2, AppPluralForm.Few)]
    [InlineData("cs-CZ", 4, AppPluralForm.Few)]
    [InlineData("cs-CZ", 5, AppPluralForm.Other)]
    [InlineData("cs-CZ", 21, AppPluralForm.Other)]
    public void Pluralization_service_selects_expected_english_and_czech_forms(
        string language,
        int count,
        AppPluralForm expected)
    {
        var service = new AppPluralizationService();

        Assert.Equal(expected, service.SelectForm(count, new AppCulturePreferences(language)));
    }

    [Fact]
    public void Pluralization_service_formats_natural_vehicle_counts_in_both_languages()
    {
        var service = new AppPluralizationService();
        var english = new ResourceAppLocalizer(CultureInfo.GetCultureInfo(AppCultureService.EnglishLanguage));
        var czech = new ResourceAppLocalizer(CultureInfo.GetCultureInfo(AppCultureService.CzechLanguage));
        var englishPreferences = new AppCulturePreferences(AppCultureService.EnglishLanguage);
        var czechPreferences = new AppCulturePreferences(AppCultureService.CzechLanguage);

        Assert.Equal("Vehicle list: 1 vehicle.", service.Format(english, englishPreferences, "VehicleList.Summary.All", 1, 1));
        Assert.Equal("Vehicle list: 2 vehicles.", service.Format(english, englishPreferences, "VehicleList.Summary.All", 2, 2));
        Assert.Equal("Seznam vozidel: 1 vozidlo.", service.Format(czech, czechPreferences, "VehicleList.Summary.All", 1, 1));
        Assert.Equal("Seznam vozidel: 3 vozidla.", service.Format(czech, czechPreferences, "VehicleList.Summary.All", 3, 3));
        Assert.Equal("Seznam vozidel: 5 vozidel.", service.Format(czech, czechPreferences, "VehicleList.Summary.All", 5, 5));
        Assert.Equal("1 entry", service.Format(english, "FuelAnalysis.Group.EntryCount", 1, 1));
        Assert.Equal("3 záznamy", service.Format(czech, "FuelAnalysis.Group.EntryCount", 3, 3));
        Assert.Equal(
            "There are 2 items to resolve: 1 error and 5 warnings.",
            service.Format(
                english,
                "Audit.Summary.WithItems",
                2,
                2,
                service.Format(english, "Audit.Summary.ErrorCount", 1, 1),
                service.Format(english, "Audit.Summary.WarningCount", 5, 5)));
        Assert.Equal(
            "K řešení jsou 2 položky: 1 chyba a 5 upozornění.",
            service.Format(
                czech,
                "Audit.Summary.WithItems",
                2,
                2,
                service.Format(czech, "Audit.Summary.ErrorCount", 1, 1),
                service.Format(czech, "Audit.Summary.WarningCount", 5, 5)));
        Assert.Equal(AppCultureService.EnglishLanguage, english.Culture.Name);
        Assert.Equal(AppCultureService.CzechLanguage, czech.Culture.Name);
    }

    [Fact]
    public void Migrated_count_sentences_use_pluralization_in_production_code()
    {
        var root = FindRepositoryRoot();
        var sourceRoot = Path.Combine(root, "dotnet", "src");
        var migratedPrefixes = new[]
        {
            "GlobalSearch.Summary.WithResults",
            "HistoryWorkspace.SearchSummary.All",
            "HistoryWorkspace.SearchSummary.Filtered",
            "FuelWorkspace.SearchSummary.All",
            "FuelWorkspace.SearchSummary.Filtered",
            "ReminderWorkspace.SearchSummary.All",
            "ReminderWorkspace.SearchSummary.Filtered",
            "MaintenanceWorkspace.SearchSummary.All",
            "MaintenanceWorkspace.SearchSummary.Filtered",
            "RecordWorkspace.SearchSummary.All",
            "RecordWorkspace.SearchSummary.Filtered",
            "CostWorkspace.SearchSummary.Visible",
            "CostWorkspace.SearchSummary.WithResults",
            "History.Projection.Summary.Count",
            "Fuel.Projection.Summary.Count",
            "Reminder.Projection.Summary.Count",
            "Maintenance.Projection.Summary.Count",
            "Record.Projection.Summary.Count",
            "Timeline.Status.DaysLeft",
            "ServiceBook.Value.MonthCount",
            "ServiceBook.Value.OverdueDays",
            "ServiceBook.Value.InDays",
            "Reminder.Status.Overdue",
            "Reminder.Status.InDays",
            "Maintenance.Interval.MonthCount",
            "Maintenance.Status.Overdue",
            "Maintenance.Status.InDays",
            "VehicleList.Summary.All",
            "VehicleList.Status.AttentionCount",
            "Audit.Summary.WithItems",
            "Audit.Summary.ErrorCount",
            "Audit.Summary.WarningCount",
            "Audit.Summary.SearchCount",
            "FuelAnalysis.Group.EntryCount",
            "Cost.Summary",
            "SmartAdvisor.Status.WithItems",
            "SmartAdvisor.Status.Part.Critical",
            "SmartAdvisor.Status.Part.Warning",
            "SmartAdvisor.Status.Part.Recommendation",
            "SmartAdvisor.Summary.FilteredCount",
            "Overview.Summary.DashboardWithItems",
            "Overview.Summary.OverdueWithItems",
            "Overview.Summary.UpcomingWithItems",
            "Overview.Summary.UpcomingVisibleDataIssues",
            "Overview.Summary.UpcomingMissingGreenCardsHidden",
            "Overview.Summary.UpcomingDataIssuesHidden",
            "VehicleStarterBundle.Summary.MaintenanceOnly",
            "VehicleStarterBundle.Summary.SectionCounts",
            "VehicleStarterBundle.Status.Added",
            "VehicleStarterBundle.Status.AddedPart.Maintenance",
            "VehicleStarterBundle.Status.AddedPart.Record",
            "VehicleStarterBundle.Status.AddedPart.Reminder",
            "VehicleDetail.Projection.RecentHistoryCount",
            "VehicleDetail.Projection.Fuel.CountAndOdometer",
            "VehicleDetail.Projection.Record.Count",
            "GlobalSearch.Value.Months",
            "Cost.Comparison",
            "AppShell.Background.NotificationTimelineTitle",
            "AppShell.Background.NotificationAuditTitle"
        };
        var directFormattingRegex = new Regex(
            @"(?:\.Format|\bLF|\bLFO)\(\s*""(?<key>[A-Za-z0-9_.-]+)""",
            RegexOptions.Compiled);
        var failures = new List<string>();

        foreach (var file in Directory.EnumerateFiles(sourceRoot, "*.cs", SearchOption.AllDirectories)
                     .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
                     .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)))
        {
            var content = File.ReadAllText(file);
            foreach (Match match in directFormattingRegex.Matches(content))
            {
                if (migratedPrefixes.Contains(match.Groups["key"].Value, StringComparer.Ordinal))
                {
                    failures.Add($"{Path.GetRelativePath(root, file)}: {match.Groups["key"].Value}");
                }
            }
        }

        Assert.True(
            failures.Count == 0,
            "Pluralized count sentences must be formatted through AppPluralizationService/LP, not directly through LF or IAppLocalizer.Format." +
            Environment.NewLine +
            string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public void English_and_czech_resources_do_not_share_unreviewed_visible_text()
    {
        var root = FindRepositoryRoot();
        var english = ReadResourceValues(Path.Combine(root, "dotnet", "src", "Vehimap.Application", "Resources", "Strings.resx"));
        var czech = ReadResourceValues(Path.Combine(root, "dotnet", "src", "Vehimap.Application", "Resources", "Strings.cs-CZ.resx"));
        var reviewedSharedTextKeys = new HashSet<string>(StringComparer.Ordinal)
        {
            "Audit.Severity.Info",
            "CalendarExport.Summary",
            "FuelAnalysis.Severity.Info",
            "FuelAnalysis.Value.ConsumptionMpg",
            "FuelAnalysis.Value.ConsumptionMpgImperial",
            "KnownValue.FuelType.Cng",
            "KnownValue.FuelType.Gasoline.LegacyAscii",
            "KnownValue.FuelType.Lpg",
            "KnownValue.Powertrain.Gasoline.LegacyAscii",
            "KnownValue.Powertrain.Hybrid",
            "KnownValue.Powertrain.LpgCng",
            "KnownValue.Powertrain.PluginHybrid",
            "MainMenu.Overview.Dashboard",
            "MaintenanceWorkspace.Detail.Interval",
            "Overview.DataIssue.Severity.Info",
            "Platform.Autostart.DisplayName",
            "ServiceBook.RecordKeywords",
            "Settings.Option.CurrencyEur",
            "Shell.AuditCount",
            "SmartAdvisor.Category.Data",
            "UpdateCheck.Details.Sha256",
            "VehicleStarterBundle.Profile.Powertrain.PlugInHybrid",
            "Window.Dashboard.Title",
            "Window.VehicleDetail.Title.Vehicle",
            "WorkspaceSort.Interval",
            "WorkspaceTabs.Audit",
            "WorkspaceTabs.Dashboard",
            "WorkspaceTabs.Detail",
            "WorkspaceWindow.DashboardName"
        };
        var sharedTextKeys = english
            .Where(item => string.Equals(item.Value, czech[item.Key], StringComparison.Ordinal))
            .Where(item => Regex.Replace(item.Value, @"(?<!\{)\{\d+(?:[^}]*)?\}(?!\})", string.Empty).Any(char.IsLetter))
            .Select(item => item.Key)
            .ToHashSet(StringComparer.Ordinal);
        var unexpected = sharedTextKeys.Except(reviewedSharedTextKeys).OrderBy(key => key, StringComparer.Ordinal).ToArray();
        var stale = reviewedSharedTextKeys.Except(sharedTextKeys).OrderBy(key => key, StringComparer.Ordinal).ToArray();

        Assert.True(
            unexpected.Length == 0 && stale.Length == 0,
            "Identical EN/CS text containing words must be reviewed explicitly. " +
            "This catches copied but untranslated resource values while allowing format-only templates." +
            Environment.NewLine +
            $"Unexpected: {string.Join(", ", unexpected)}" +
            Environment.NewLine +
            $"Stale allowlist entries: {string.Join(", ", stale)}");
    }

    [Fact]
    public void User_facing_resources_do_not_expose_retired_development_terminology()
    {
        var root = FindRepositoryRoot();
        var catalogs = new[]
        {
            (Language: "English", Values: ReadResourceValues(Path.Combine(root, "dotnet", "src", "Vehimap.Application", "Resources", "Strings.resx"))),
            (Language: "Czech", Values: ReadResourceValues(Path.Combine(root, "dotnet", "src", "Vehimap.Application", "Resources", "Strings.cs-CZ.resx")))
        };
        var retiredPhrases = new[]
        {
            "desktop branch",
            ".NET branch",
            "original AHK version",
            "portable data",
            "published desktop build",
            "multiplatform desktop for",
            "accessible focus handling",
            "desktopové větvi",
            ".NET větev",
            "původní AHK verze",
            "publikovaném desktopovém buildu",
            "multiplatformní desktop nad",
            "dotaženou přístupností"
        };
        var failures = catalogs
            .SelectMany(catalog => catalog.Values
                .Where(item => retiredPhrases.Any(phrase => item.Value.Contains(phrase, StringComparison.OrdinalIgnoreCase)))
                .Select(item => $"{catalog.Language} {item.Key}: {item.Value}"))
            .ToArray();

        Assert.True(
            failures.Length == 0,
            "User-facing resources must describe Vehimap behavior without retired implementation lineage or development jargon. " +
            "Technical framework and storage names remain allowed in explicit diagnostics." +
            Environment.NewLine +
            string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public void Literal_resource_keys_referenced_by_production_source_exist()
    {
        var root = FindRepositoryRoot();
        var resourceKeys = ReadResourceKeys(Path.Combine(root, "dotnet", "src", "Vehimap.Application", "Resources", "Strings.resx"));
        var sourceRoot = Path.Combine(root, "dotnet", "src");
        var methodReferenceRegex = new Regex(
            @"(?:GetString|Format|L|LF|LO|LFO)\(\s*""(?<key>[A-Za-z0-9_.-]+)""",
            RegexOptions.Compiled);
        var xamlReferenceRegex = new Regex(
            @"\{i18n:Loc\s+(?<key>[A-Za-z0-9_.-]+)\}",
            RegexOptions.Compiled);
        var failures = new List<string>();

        foreach (var file in Directory.EnumerateFiles(sourceRoot, "*.*", SearchOption.AllDirectories)
                     .Where(path => Path.GetExtension(path) is ".cs" or ".axaml")
                     .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
                     .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)))
        {
            var relativePath = Path.GetRelativePath(root, file).Replace('\\', '/');
            var content = File.ReadAllText(file);
            foreach (var match in methodReferenceRegex.Matches(content).Cast<Match>()
                         .Concat(xamlReferenceRegex.Matches(content).Cast<Match>()))
            {
                var key = match.Groups["key"].Value;
                if (!resourceKeys.Contains(key))
                {
                    failures.Add($"{relativePath}: {key}");
                }
            }
        }

        Assert.True(
            failures.Count == 0,
            "Literal localization keys referenced by production source must exist in the resource catalog." +
            Environment.NewLine +
            string.Join(Environment.NewLine, failures.Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal)));
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
        Assert.Contains("invoice", english.GetString("ServiceBook.RecordKeywords"), StringComparison.Ordinal);
        Assert.Contains("faktura", english.GetString("ServiceBook.RecordKeywords"), StringComparison.Ordinal);
        Assert.Contains("receipt", czech.GetString("ServiceBook.RecordKeywords"), StringComparison.Ordinal);
        Assert.Contains("účtenka", czech.GetString("ServiceBook.RecordKeywords"), StringComparison.Ordinal);
        Assert.Equal("Fill in the reminder and save it.", english.GetString("ReminderEditor.Status.CreatePrompt"));
        Assert.Equal("Vyplňte připomínku a uložte ji.", czech.GetString("ReminderEditor.Status.CreatePrompt"));
        Assert.Equal("Fill in the document and choose an attachment if needed.", english.GetString("RecordEditor.Status.CreatePrompt"));
        Assert.Equal("Vyplňte doklad a podle potřeby vyberte přílohu.", czech.GetString("RecordEditor.Status.CreatePrompt"));
        Assert.Equal("Document attachment has been opened: invoice.pdf.", english.Format("RecordAttachmentAction.FileOpened", "invoice.pdf"));
        Assert.Equal("Příloha dokladu byla otevřena: faktura.pdf.", czech.Format("RecordAttachmentAction.FileOpened", "faktura.pdf"));
        Assert.Equal("Document attachment could not be opened.", english.GetString("RecordAttachmentAction.FileOpenFailed"));
        Assert.Equal("Přílohu dokladu se nepodařilo otevřít.", czech.GetString("RecordAttachmentAction.FileOpenFailed"));
        Assert.Equal("Document attachment folder could not be opened.", english.GetString("RecordAttachmentAction.FolderOpenFailed"));
        Assert.Equal("Složku přílohy dokladu se nepodařilo otevřít.", czech.GetString("RecordAttachmentAction.FolderOpenFailed"));
        Assert.Equal("Document path could not be copied.", english.GetString("RecordAttachmentAction.CopyPathFailed"));
        Assert.Equal("Cestu dokladu se nepodařilo zkopírovat.", czech.GetString("RecordAttachmentAction.CopyPathFailed"));
        Assert.Equal("Dashboard startup preference could not be saved.", english.GetString("AppShell.Dashboard.ShowOnLaunchFailed"));
        Assert.Equal("Volbu dashboardu při startu se nepodařilo uložit.", czech.GetString("AppShell.Dashboard.ShowOnLaunchFailed"));
        Assert.Equal("Vehicle bundle", english.GetString("VehicleStarterBundle.Title"));
        Assert.Equal("Balíček pro vozidlo", czech.GetString("VehicleStarterBundle.Title"));
        Assert.Equal("Selected: 1 item | Service 1 | Documents 0 | Reminders 0", english.Format("VehicleStarterBundle.Summary.SectionCounts.One", 1, 1, 0, 0));
        Assert.Equal("Vybrány: 3 položky | Servis 1 | Doklady 1 | Připomínky 1", czech.Format("VehicleStarterBundle.Summary.SectionCounts.Few", 3, 1, 1, 1));
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
        Assert.Equal("The vehicle must have a name.", english.GetString("VehicleEditor.Validation.NameRequired"));
        Assert.Equal("Vozidlo musí mít název.", czech.GetString("VehicleEditor.Validation.NameRequired"));
        Assert.Equal("Edit vehicle", english.GetString("VehicleEditor.Title.Edit"));
        Assert.Equal("Upravit vozidlo", czech.GetString("VehicleEditor.Title.Edit"));
        Assert.Equal("Downloading update package.", english.GetString("UpdateService.Install.DownloadProgress"));
        Assert.Equal("Stahuji aktualizační balíček.", czech.GetString("UpdateService.Install.DownloadProgress"));
        Assert.Equal("The update channel for this Vehimap edition has not been published yet.", english.GetString("UpdateService.Check.DotnetManifestUnavailable"));
        Assert.Equal("Aktualizační kanál pro tuto edici Vehimapu zatím nebyl zveřejněn.", czech.GetString("UpdateService.Check.DotnetManifestUnavailable"));
        Assert.Equal("Portable mode: data stored next to the application", english.GetString("AppShell.About.DataModePortable"));
        Assert.Equal("Přenosný režim: data uložená vedle aplikace", czech.GetString("AppShell.About.DataModePortable"));
        Assert.Equal("Data storage", english.GetString("Shell.DataModeLabel"));
        Assert.Equal("Uložení dat", czech.GetString("Shell.DataModeLabel"));
        Assert.Equal("Vehimap is ready.", english.GetString("Shell.Status.Ready"));
        Assert.Equal("Vehimap je připraven.", czech.GetString("Shell.Status.Ready"));
        Assert.EndsWith("This step replaces the current Vehimap data set.", english.Format("AppShell.ImportBackup.ConfirmMessage", Environment.NewLine, "backup.vehimapbak"), StringComparison.Ordinal);
        Assert.EndsWith("Tento krok nahradí aktuální datovou sadu Vehimapu.", czech.Format("AppShell.ImportBackup.ConfirmMessage", Environment.NewLine, "zaloha.vehimapbak"), StringComparison.Ordinal);
        Assert.Equal("The manifest does not contain release/version.", english.GetString("UpdateManifest.Error.MissingVersion"));
        Assert.Equal("Manifest neobsahuje položku release/version.", czech.GetString("UpdateManifest.Error.MissingVersion"));
        Assert.Equal("The vehicles row 2 must contain 12 fields.", english.Format("LegacySection.Error.InvalidFieldCount", english.GetString("LegacyData.Section.Vehicles"), 2, english.Format("LegacySection.FieldCount.Count", 12)));
        Assert.Equal("Řádek vozidel 2 musí obsahovat 12 polí.", czech.Format("LegacySection.Error.InvalidFieldCount", czech.GetString("LegacyData.Section.Vehicles"), 2, czech.Format("LegacySection.FieldCount.Count", 12)));
        Assert.Equal("The attachments row 2 contains invalid file content.", english.Format("LegacySection.Error.InvalidAttachmentContent", english.GetString("LegacyData.Section.Attachments"), 2));
        Assert.Equal("Řádek příloh 2 obsahuje neplatný obsah souboru.", czech.Format("LegacySection.Error.InvalidAttachmentContent", czech.GetString("LegacyData.Section.Attachments"), 2));
        Assert.Equal("development Avalonia shell", english.GetString("AppBuildInfo.RuntimeMode.Development"));
        Assert.Equal("vývojový Avalonia shell", czech.GetString("AppBuildInfo.RuntimeMode.Development"));
        Assert.Equal("Currency", english.GetString("Settings.Currency"));
        Assert.Equal("Měna", czech.GetString("Settings.Currency"));
        Assert.Equal("Installer language preferences were added to the 2.0 data set.", english.GetString("InstallerLocaleSeed.Applied"));
        Assert.Equal("Instalační jazykové předvolby byly doplněny do datové sady 2.0.", czech.GetString("InstallerLocaleSeed.Applied"));
        Assert.Equal(".NET runtime: 10.0", english.Format("About.Diagnostics.Framework", "10.0"));
        Assert.Equal("Běhové prostředí .NET: 10.0", czech.Format("About.Diagnostics.Framework", "10.0"));
        Assert.Equal("Detail: loaded", english.Format("LegacyData.Detail", "loaded"));
        Assert.Equal("Podrobnosti: načteno", czech.Format("LegacyData.Detail", "načteno"));
        Assert.Equal("Runtime", english.GetString("FeedbackIssue.FrameworkLabel"));
        Assert.Equal("Běhové prostředí", czech.GetString("FeedbackIssue.FrameworkLabel"));
        Assert.Equal("Restore data from backup", english.GetString("AppShell.ImportBackup.ConfirmTitle"));
        Assert.Equal("Obnovit data ze zálohy", czech.GetString("AppShell.ImportBackup.ConfirmTitle"));
        Assert.Equal("close the fuel editor", english.GetString("WorkspaceWindow.CloseAction.Fuel"));
        Assert.Equal("zavřít editor tankování", czech.GetString("WorkspaceWindow.CloseAction.Fuel"));
        Assert.Equal("Search “oil” found 2 history entries.", english.Format("HistoryWorkspace.SearchSummary.Filtered", "oil", 2));
        Assert.Equal("Hledání „olej“ našlo 2 historických záznamů.", czech.Format("HistoryWorkspace.SearchSummary.Filtered", "olej", 2));
        Assert.Equal("2026-06, Service, odometer 12345, cost 2500, note no note", english.Format("HistoryItem.AccessibleLabel", "2026-06", "Service", "12345", "2500", "no note"));
        Assert.Equal("2026-06, Servis, tachometr 12345, cena 2500, poznámka bez poznámky", czech.Format("HistoryItem.AccessibleLabel", "2026-06", "Servis", "12345", "2500", "bez poznámky"));
        Assert.Equal("Passenger vehicles", english.GetString("KnownValue.Category.PassengerVehicles"));
        Assert.Equal("Osobní vozidla", czech.GetString("KnownValue.Category.PassengerVehicles"));
        Assert.Equal("Liability insurance", english.GetString("KnownValue.RecordType.LiabilityInsurance"));
        Assert.Equal("Povinné ručení", czech.GetString("KnownValue.RecordType.LiabilityInsurance"));
        Assert.Equal("Every 2 years", english.GetString("KnownValue.ReminderRepeat.EveryTwoYears"));
        Assert.Equal("Každé 2 roky", czech.GetString("KnownValue.ReminderRepeat.EveryTwoYears"));
        Assert.Equal("Every two years", english.GetString("KnownValue.ReminderRepeat.EveryTwoYears.LegacyWords"));
        Assert.Equal("Každé dva roky", czech.GetString("KnownValue.ReminderRepeat.EveryTwoYears.LegacyWords"));
        Assert.Equal("Rocne", english.GetString("KnownValue.ReminderRepeat.Yearly.LegacyAscii"));
        Assert.Equal("Ročně", czech.GetString("KnownValue.ReminderRepeat.Yearly.LegacyAscii"));
        Assert.Equal("Detail: service", english.Format("CalendarExport.Description.Detail", "service"));
        Assert.Equal("Podrobnosti: servis", czech.Format("CalendarExport.Description.Detail", "servis"));
        Assert.Equal("Detail", english.GetString("ServiceBook.Export.Column.Detail"));
        Assert.Equal("Podrobnosti", czech.GetString("ServiceBook.Export.Column.Detail"));
        Assert.Equal("Missing.Key.For.Test", english.GetString("Missing.Key.For.Test"));
    }

    [Fact]
    public void Localized_resource_value_matcher_accepts_current_english_and_czech_values()
    {
        var english = new ResourceAppLocalizer(CultureInfo.GetCultureInfo(AppCultureService.EnglishLanguage));

        Assert.True(LocalizedResourceValueMatcher.Matches(english, "Maintenance", "Audit.Category.Maintenance"));
        Assert.True(LocalizedResourceValueMatcher.Matches(english, "Údržba", "Audit.Category.Maintenance"));
        Assert.True(LocalizedResourceValueMatcher.MatchesStableValueOrResource(
            english,
            "technical",
            "technical",
            "Overview.Filter.Technical"));
        Assert.True(LocalizedResourceValueMatcher.MatchesStableValueOrResource(
            english,
            "Technické kontroly",
            "technical",
            "Overview.Filter.Technical"));
        Assert.False(LocalizedResourceValueMatcher.Matches(english, "Custom category", "Audit.Category.Maintenance"));
    }

    [Fact]
    public void User_facing_exception_messages_are_localized_and_hide_raw_technical_details()
    {
        var english = new ResourceAppLocalizer(CultureInfo.GetCultureInfo(AppCultureService.EnglishLanguage));
        var czech = new ResourceAppLocalizer(CultureInfo.GetCultureInfo(AppCultureService.CzechLanguage));
        const string rawDetail = @"C:\private\vehimap.db is locked by another process.";

        Assert.Equal(
            "A file operation could not be completed.",
            UserFacingExceptionMessageService.Describe(new IOException(rawDetail), english));
        Assert.Equal(
            "Souborovou operaci se nepodařilo dokončit.",
            UserFacingExceptionMessageService.Describe(new IOException(rawDetail), czech));
        Assert.Equal(
            "Přístup k požadovanému souboru nebo složce byl odepřen.",
            UserFacingExceptionMessageService.Describe(new UnauthorizedAccessException(rawDetail), czech));
        Assert.Equal(
            "Požadovaný soubor nebo složka nejsou dostupné.",
            UserFacingExceptionMessageService.Describe(new FileNotFoundException(rawDetail), czech));
        Assert.Equal(
            "Síťový požadavek se nepodařilo dokončit.",
            UserFacingExceptionMessageService.Describe(new HttpRequestException(rawDetail), czech));
        Assert.Equal(
            "Vybraná data jsou neplatná nebo poškozená.",
            UserFacingExceptionMessageService.Describe(new InvalidDataException(rawDetail), czech));
        Assert.Equal(
            "Požadovanou operaci se nepodařilo dokončit.",
            UserFacingExceptionMessageService.Describe(new InvalidOperationException(rawDetail), czech));
        Assert.Equal(
            "Došlo k neočekávané chybě.",
            UserFacingExceptionMessageService.Describe(new Exception(rawDetail), czech));
        Assert.DoesNotContain(
            rawDetail,
            UserFacingExceptionMessageService.Describe(
                new InvalidOperationException("wrapper", new IOException(rawDetail)),
                czech),
            StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("vehicle", ApplicationEntityKinds.Vehicle)]
    [InlineData("Vozidlo", ApplicationEntityKinds.Vehicle)]
    [InlineData("fuel", ApplicationEntityKinds.Fuel)]
    [InlineData("Tankování", ApplicationEntityKinds.Fuel)]
    [InlineData("Doklad", ApplicationEntityKinds.Record)]
    [InlineData("Údržba", ApplicationEntityKinds.Maintenance)]
    [InlineData("Připomínka", ApplicationEntityKinds.Reminder)]
    [InlineData("Náklady", ApplicationEntityKinds.Costs)]
    public void Application_entity_kinds_accept_legacy_labels_but_return_stable_tokens(string input, string expected)
    {
        Assert.Equal(expected, ApplicationEntityKinds.Normalize(input));
    }

    [Fact]
    public void Legacy_known_value_display_service_localizes_known_values_and_preserves_custom_values()
    {
        var english = new ResourceAppLocalizer(CultureInfo.GetCultureInfo("en-US"));
        var czech = new ResourceAppLocalizer(CultureInfo.GetCultureInfo("cs-CZ"));

        Assert.Equal("Passenger vehicles", LegacyKnownValueDisplayService.FormatCategory("Osobní vozidla", english));
        Assert.Equal("Osobní vozidla", LegacyKnownValueDisplayService.FormatCategory("Osobní vozidla", czech));
        Assert.Equal("Passenger vehicles", LegacyKnownValueDisplayService.FormatCategory("Passenger vehicles", english));
        Assert.Equal("Osobní vozidla", LegacyKnownValueDisplayService.FormatCategory("Passenger vehicles", czech));
        Assert.Equal("Gasoline", LegacyKnownValueDisplayService.FormatFuelType("Benzin", english));
        Assert.Equal("Benzín", LegacyKnownValueDisplayService.FormatFuelType("Benzin", czech));
        Assert.Equal("Gasoline", LegacyKnownValueDisplayService.FormatPowertrain("Benzín", english));
        Assert.Equal("Every year", LegacyKnownValueDisplayService.FormatReminderRepeatMode("Každý rok", english));
        Assert.Equal("Každý rok", LegacyKnownValueDisplayService.FormatReminderRepeatMode("Every year", czech));
        Assert.Equal("Passenger vehicles", LegacyKnownValueDisplayService.FormatCategory("Passenger", english));
        Assert.Equal("Osobní vozidla", LegacyKnownValueDisplayService.FormatCategory("Passenger", czech));
        Assert.Equal("Every year", LegacyKnownValueDisplayService.FormatReminderRepeatMode("Rocne", english));
        Assert.Equal("Každé 2 roky", LegacyKnownValueDisplayService.FormatReminderRepeatMode("Every two years", czech));
        Assert.Equal("Každých 5 let", LegacyKnownValueDisplayService.FormatReminderRepeatMode("Every five years", czech));
        Assert.Equal("Custom category", LegacyKnownValueDisplayService.FormatCategory("Custom category", english));
        Assert.Equal(string.Empty, LegacyKnownValueDisplayService.FormatRecordType("", english));
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
        var vehicleDetailWorkspaceViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "Workspaces", "VehicleDetailWorkspaceViewModel.cs"));

        Assert.Contains("PendingEdits.Action.ExitApplication", runtimeController);
        Assert.Contains("PendingEdits.Action.SwitchVehicle", mainWindow);
        Assert.Contains("PendingEdits.Action.OpenDashboardWindow", mainWindow);
        Assert.Contains("PendingEdits.Action.OpenSelectedTimelineItem", shellViewModel);
        Assert.Contains("PendingEdits.Action.OpenAuditItem", shellViewModel);
        Assert.Contains("PendingEdits.Action.OpenUpcomingOverviewItem", overviewViewModel);
        Assert.Contains("PendingEdits.Action.OpenSmartAdvisorItem", workspaceStateViewModel);
        Assert.Contains("VehicleEditor.Status.CreatePrompt", vehicleEditingViewModel);
        Assert.Contains("VehicleEditor.Status.EditPrompt", vehicleEditingViewModel);
        Assert.Contains("VehicleEditor.Validation.NameRequired", vehicleEditingViewModel);
        Assert.Contains("VehicleEditor.Validation.NextTkRequired", vehicleEditingViewModel);
        Assert.Contains("VehicleDetail.Status.NewVehicleSaved", vehicleEditingViewModel);
        Assert.Contains("VehicleDetail.Status.NewVehicleBundleOpenFailed", mainWindow);
        Assert.Contains("VehicleEditor.Title.New", vehicleDetailWorkspaceViewModel);
        Assert.Contains("VehicleEditor.Title.Edit", vehicleDetailWorkspaceViewModel);

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
        Assert.DoesNotContain("\"Vyplňte základní údaje o vozidle", combined);
        Assert.DoesNotContain("\"Upravte údaje vozidla", combined);
        Assert.DoesNotContain("\"Vozidlo musí mít název", combined);
        Assert.DoesNotContain("\"Pole Příští TK je povinné", combined);
        Assert.DoesNotContain("\"Nové vozidlo\"", vehicleDetailWorkspaceViewModel);
        Assert.DoesNotContain("\"Upravit vozidlo\"", vehicleDetailWorkspaceViewModel);
    }

    [Fact]
    public void Platform_update_service_uses_resource_localization_for_user_messages()
    {
        var root = FindRepositoryRoot();
        var updateService = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Platform", "LegacyUpdateService.cs"));
        var updateManifestParser = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Application", "Services", "LegacyUpdateManifestParser.cs"));
        var buildInfoProvider = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Platform", "AssemblyAppBuildInfoProvider.cs"));
        var mainWindowViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "MainWindowViewModel.cs"));

        Assert.Contains("UpdateService.Check.UpdateAvailable", updateService);
        Assert.Contains("UpdateService.Install.DownloadProgress", updateService);
        Assert.Contains("UpdateService.Install.VerifyProgress", updateService);
        Assert.Contains("UpdateService.Download.HashMismatch", updateService);
        Assert.Contains("UpdateManifest.Error.MissingVersion", updateManifestParser);
        Assert.Contains("UpdateManifest.Error.InvalidVersion", updateManifestParser);
        Assert.Contains("UpdateManifest.Error.UnsupportedAssetKind", updateManifestParser);
        Assert.Contains("AppBuildInfo.RuntimeMode.Development", buildInfoProvider);
        Assert.Contains("AppBuildInfo.RuntimeMode.Published", buildInfoProvider);
        Assert.Contains("localizerProvider: () => DesktopLocalization.Localizer", mainWindowViewModel);

        Assert.DoesNotContain("Stahuji aktualizační balíček.", updateService);
        Assert.DoesNotContain("Automaticka instalace", updateService);
        Assert.DoesNotContain("Pouzivate aktualni verzi", updateService);
        Assert.DoesNotContain("Manifest neobsahuje", updateService);
        Assert.DoesNotContain("Manifest neobsahuje", updateManifestParser);
        Assert.DoesNotContain("Manifest obsahuje", updateManifestParser);
        Assert.DoesNotContain("samostatná desktopová aplikace", buildInfoProvider);
        Assert.DoesNotContain("vývojový Avalonia shell", buildInfoProvider);
    }

    [Fact]
    public void Legacy_section_parser_uses_resource_localization_for_diagnostics()
    {
        var root = FindRepositoryRoot();
        var parser = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Storage.Legacy", "LegacySectionSerialization.cs"));
        var dataStore = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Storage.Legacy", "LegacyVehimapDataStore.cs"));
        var backupService = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Storage.Legacy", "LegacyBackupService.cs"));

        Assert.Contains("LegacySection.Error.InvalidFieldCount", parser);
        Assert.Contains("LegacySection.Error.InvalidAttachmentContent", parser);
        Assert.Contains("LegacySection.Error.UnsupportedHeader", parser);
        Assert.Contains("ParseVehicles(content, _localizer)", dataStore);
        Assert.Contains("ParseAttachmentsSection(payload.AttachmentsContent, _localizer)", backupService);

        Assert.DoesNotContain("Řádek vozidel", parser);
        Assert.DoesNotContain("Řádek tankování", parser);
        Assert.DoesNotContain("Řádek příloh", parser);
        Assert.DoesNotContain("Soubor obsahuje", parser);
        Assert.DoesNotContain("Nepodporovaná hlavička", parser);
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
    [InlineData("en-US", "4/30/2026")]
    [InlineData("cs-CZ", "30.04.2026")]
    public void Date_format_service_formats_full_dates_for_selected_culture(string language, string expected)
    {
        var service = new AppDateFormatService();
        var preferences = new AppCulturePreferences(language, "system", "system");

        Assert.Equal(expected, service.FormatDate(new DateOnly(2026, 4, 30), preferences));
    }

    [Theory]
    [InlineData("en-US", "4/30/2026")]
    [InlineData("cs-CZ", "30.04.2026")]
    [InlineData("en-US", "30.04.2026")]
    public void Date_format_service_parses_current_culture_and_legacy_day_first_dates(string language, string input)
    {
        var service = new AppDateFormatService();
        var preferences = new AppCulturePreferences(language, "system", "system");

        Assert.True(service.TryParseDate(input, preferences, out var parsed));
        Assert.Equal(new DateOnly(2026, 4, 30), parsed);
    }

    [Fact]
    public void English_date_parser_preserves_unambiguous_legacy_day_first_meaning()
    {
        var service = new AppDateFormatService();
        var preferences = new AppCulturePreferences("en-US", "system", "system");

        Assert.True(service.TryParseDate("10.03.2026", preferences, out var parsed));
        Assert.Equal(new DateOnly(2026, 3, 10), parsed);
        Assert.Equal("10.03.2026", VehimapValueParser.FormatCanonicalEventDate(parsed));
    }

    [Theory]
    [InlineData("en-US", "comma", "dot", 512L, "512 B")]
    [InlineData("en-US", "comma", "dot", 1536L, "1.5 KB")]
    [InlineData("cs-CZ", "space", "comma", 1572864L, "1,5 MB")]
    [InlineData("en-US", "comma", "dot", 1610612736L, "1.50 GB")]
    public void File_size_format_service_respects_number_separator_preferences(
        string language,
        string thousandsSeparator,
        string decimalSeparator,
        long sizeBytes,
        string expected)
    {
        var service = new AppFileSizeFormatService();
        var preferences = new AppCulturePreferences(language, thousandsSeparator, decimalSeparator);

        Assert.Equal(expected, service.FormatBytes(sizeBytes, preferences));
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
    public void Currency_format_service_uses_explicit_default_and_named_symbols()
    {
        Assert.Equal(AppCurrencyFormatService.CzechCrowns, AppCurrencyFormatService.DefaultCurrency);
        Assert.Equal(AppCurrencyFormatService.CzechCrowns, AppCurrencyFormatService.NormalizeCurrency(null));
        Assert.Equal(AppCurrencyFormatService.CzechCrowns, AppCurrencyFormatService.NormalizeCurrency("cad"));
        Assert.Equal(AppCurrencyFormatService.UsDollars, AppCurrencyFormatService.NormalizeCurrency(" usd "));
        Assert.Equal("Kč", AppCurrencyFormatService.GetCurrencySymbol(AppCurrencyFormatService.CzechCrowns));
        Assert.Equal("$", AppCurrencyFormatService.GetCurrencySymbol(AppCurrencyFormatService.UsDollars));
        Assert.Equal("€", AppCurrencyFormatService.GetCurrencySymbol(AppCurrencyFormatService.Euros));
        Assert.Equal("£", AppCurrencyFormatService.GetCurrencySymbol(AppCurrencyFormatService.BritishPounds));
    }

    [Theory]
    [InlineData("1 234,50 Kč", 1234.5)]
    [InlineData("1 234,50 CZK", 1234.5)]
    [InlineData("1,234.50 USD", 1234.5)]
    public void Value_parser_accepts_common_money_markers_without_treating_them_as_ui_labels(string text, double expected)
    {
        Assert.True(VehimapValueParser.TryParseMoney(text, out var parsed));
        Assert.Equal((decimal)expected, parsed);
    }

    [Fact]
    public void Unit_format_service_keeps_storage_in_metric_and_formats_display_units()
    {
        var service = new AppUnitFormatService();
        var culturePreferences = new AppCulturePreferences("en-US", "comma", "dot");

        Assert.Equal("62.1 mi", service.FormatDistanceFromKilometers(100m, culturePreferences, new AppUnitPreferences("mi", "us_gal"), 1));
        Assert.Equal("2.64 US gal", service.FormatVolumeFromLiters(10m, culturePreferences, new AppUnitPreferences("mi", "us_gal"), 2));
        Assert.Equal("mi", service.GetDistanceUnitLabel(new AppUnitPreferences("mi", "us_gal")));
        Assert.Equal("km", service.GetDistanceUnitLabel(new AppUnitPreferences("km", "imp_gal")));
        Assert.Equal("US gal", service.GetVolumeUnitLabel(new AppUnitPreferences("mi", "us_gal")));
        Assert.Equal("imp gal", service.GetVolumeUnitLabel(new AppUnitPreferences("mi", "imp_gal")));
        Assert.InRange(service.ConvertDistanceFromKilometers(100m, new AppUnitPreferences("mi", "l")), 62.137m, 62.138m);
        Assert.InRange(service.ConvertDistanceToKilometers(62.137119m, new AppUnitPreferences("mi", "l")), 99.999m, 100.001m);
        Assert.InRange(service.ConvertVolumeToLiters(2.64172m, new AppUnitPreferences("km", "us_gal")), 9.999m, 10.001m);
    }

    [Fact]
    public void Money_distance_volume_and_separator_preferences_change_display_not_canonical_meaning()
    {
        var numberService = new AppNumberFormatService();
        var unitService = new AppUnitFormatService(numberService);
        var czech = new AppCulturePreferences("cs-CZ", "none", "comma");
        var english = new AppCulturePreferences("en-US", "comma", "dot");
        var metric = new AppUnitPreferences("km", "l");
        var imperial = new AppUnitPreferences("mi", "us_gal");

        Assert.Equal("1234,50 Kč", numberService.FormatMoney(1234.5m, czech, "CZK", 2));
        Assert.Equal("$1,234.50", numberService.FormatMoney(1234.5m, english, "USD", 2));
        Assert.Equal("1000 km", unitService.FormatDistanceFromKilometers(1000m, czech, metric, 0));
        Assert.Equal("621.4 mi", unitService.FormatDistanceFromKilometers(1000m, english, imperial, 1));
        Assert.Equal("42,00 l", unitService.FormatVolumeFromLiters(42m, czech, metric, 2));
        Assert.Equal("11.10 US gal", unitService.FormatVolumeFromLiters(42m, english, imperial, 2));
        Assert.Equal(1000, (int)Math.Round(unitService.ConvertDistanceToKilometers(unitService.ConvertDistanceFromKilometers(1000m, imperial), imperial), MidpointRounding.AwayFromZero));
        Assert.Equal(42m, Math.Round(unitService.ConvertVolumeToLiters(unitService.ConvertVolumeFromLiters(42m, imperial), imperial), 2));
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
    public async Task Installer_locale_seed_overrides_language_but_preserves_existing_formatting_units_and_currency()
    {
        var tempRoot = CreateTempDirectory();
        try
        {
            var dataRoot = new VehimapDataRoot(tempRoot, tempRoot, false);
            var seedPath = InstallerLocaleSeedService.GetSeedPath(dataRoot);
            await File.WriteAllTextAsync(seedPath, """{"language":"cs-CZ"}""");
            var settings = new VehimapSettings();
            settings.SetValue("app", "language", "en-US");
            settings.SetValue("app", "thousands_separator", "comma");
            settings.SetValue("app", "decimal_separator", "dot");
            settings.SetValue("app", "distance_unit", "mi");
            settings.SetValue("app", "volume_unit", "us_gal");
            settings.SetValue("app", "currency", "USD");

            var service = new InstallerLocaleSeedService(
                new AppLocaleDefaultsService(),
                new ResourceAppLocalizer(CultureInfo.GetCultureInfo("cs-CZ")));
            var result = await service.ApplyIfPresentAsync(dataRoot, settings);
            service.CompleteSeed(result);

            Assert.True(result.SeedFound);
            Assert.True(result.SeedValid);
            Assert.True(result.SettingsChanged);
            Assert.Equal("cs-CZ", settings.GetValue("app", "language"));
            Assert.Equal("comma", settings.GetValue("app", "thousands_separator"));
            Assert.Equal("dot", settings.GetValue("app", "decimal_separator"));
            Assert.Equal("mi", settings.GetValue("app", "distance_unit"));
            Assert.Equal("us_gal", settings.GetValue("app", "volume_unit"));
            Assert.Equal("USD", settings.GetValue("app", "currency"));
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
        Assert.Contains("Text=\"{Binding MaintenanceReminderDistance}\"", settingsWindow);
        Assert.Contains("AutomationProperties.AutomationId=\"MaintenanceReminderDistanceBox\"", settingsWindow);
        Assert.DoesNotContain("MaintenanceReminderKmBox", settingsWindow);
        Assert.Contains("AutomationProperties.Name=\"{i18n:Loc Settings.OptionsListName}\"", settingsWindow);
        Assert.DoesNotContain("AutomationProperties.Name=\"Volby nastavení\"", settingsWindow);
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
        Assert.DoesNotMatch(CzechDiacriticsRegex(), service);
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
        var englishResources = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Application", "Resources", "Strings.resx"));
        var czechResources = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Application", "Resources", "Strings.cs-CZ.resx"));

        Assert.Contains("xmlns:i18n=\"using:Vehimap.Desktop.Localization\"", bundleWindow);
        Assert.Contains("AutomationProperties.HelpText=\"{i18n:Loc VehicleStarterBundle.ItemsHelpText}\"", bundleWindow);
        Assert.Contains("AutomationProperties.ItemType=\"{i18n:Loc VehicleStarterBundle.ItemType}\"", bundleWindow);
        Assert.Contains("Content=\"{i18n:Loc VehicleStarterBundle.Apply}\"", bundleWindow);
        Assert.Contains("AutomationProperties.Name=\"{i18n:Loc VehicleStarterBundle.CloseName}\"", bundleWindow);
        Assert.Contains("VehicleStarterBundle.Summary.SectionCounts", dialogViewModel);
        Assert.Contains("VehicleStarterBundle.MaintenanceTitle", dialogViewModel);
        Assert.Contains("VehicleStarterBundle.Profile.Empty", dialogViewModel);
        Assert.Contains("VehicleStarterBundle.MaintenanceIntervalDistanceLabel", dialogViewModel);
        Assert.Contains("Text=\"{Binding SelectedItem.IntervalDistance}\"", bundleWindow);
        Assert.Contains("AutomationProperties.AutomationId=\"BundleMaintenanceIntervalDistanceBox\"", bundleWindow);
        Assert.DoesNotContain("BundleMaintenanceIntervalKmBox", bundleWindow);
        Assert.DoesNotContain("VehicleStarterBundle.MaintenanceIntervalKmLabel", englishResources);
        Assert.DoesNotContain("VehicleStarterBundle.MaintenanceIntervalKmName", englishResources);
        Assert.DoesNotContain("VehicleStarterBundle.MaintenanceIntervalKmLabel", czechResources);
        Assert.DoesNotContain("VehicleStarterBundle.MaintenanceIntervalKmName", czechResources);
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
        var appShellViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "MainWindowViewModel.AppShell.cs"));
        var shellViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "MainWindowViewModel.cs"));

        Assert.Contains("ReminderEditor.Status.CreatePrompt", editingViewModel);
        Assert.Contains("ReminderEditor.Validation.TitleRequired", editingViewModel);
        Assert.Contains("RecordEditor.Status.CreatePrompt", editingViewModel);
        Assert.Contains("RecordEditor.AttachmentAvailability.ManagedImportPrompt", editingViewModel);
        Assert.Contains("RecordEditor.FileDialog.ManagedTitle", editingViewModel);
        Assert.Contains("RecordAttachmentAction.NoPath", shellViewModel);
        Assert.Contains("RecordAttachmentAction.FileOpened", shellViewModel);
        Assert.Contains("LO(\"RecordAttachmentAction.FileOpenFailed\")", shellViewModel);
        Assert.Contains("LO(\"RecordAttachmentAction.FolderOpenFailed\")", shellViewModel);
        Assert.Contains("LO(\"RecordAttachmentAction.CopyPathFailed\")", shellViewModel);
        Assert.DoesNotContain("RecordAttachmentAction.FileOpenFailed\", ex.Message", shellViewModel);
        Assert.DoesNotContain("RecordAttachmentAction.FolderOpenFailed\", ex.Message", shellViewModel);
        Assert.DoesNotContain("RecordAttachmentAction.CopyPathFailed\", ex.Message", shellViewModel);
        Assert.Contains("LO(\"AppShell.Dashboard.ShowOnLaunchFailed\")", appShellViewModel);
        Assert.DoesNotContain("AppShell.Dashboard.ShowOnLaunchFailed\", ex.Message", appShellViewModel);
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
        Assert.Contains("AutomationProperties.HelpText=\"{Binding CostPeriodStartHelp}\"", costWorkspace);
        Assert.Contains("AutomationProperties.HelpText=\"{Binding CostPeriodEndHelp}\"", costWorkspace);
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
        Assert.Contains("DesktopLocalization.Localizer.GetString(\"DataStoreHealth.CopyFailedStatus\")", dataStoreHealthCodeBehind);
        Assert.DoesNotContain("DataStoreHealth.CopyFailedStatus\", ex.Message", dataStoreHealthCodeBehind);
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
        Assert.Contains("_localizer.Format(\"MaintenanceCompletion.Validation.CompletedDate\"", maintenanceCompletionViewModel);
        Assert.Contains("PlaceholderText=\"{Binding CompletedDateExample}\"", maintenanceCompletionWindow);
        Assert.Contains("AutomationProperties.HelpText=\"{Binding CompletedDateExample}\"", maintenanceCompletionWindow);
        Assert.Contains("DesktopLocalization.Localizer.GetString(\"About.Status.DiagnosticsCopied\")", aboutCodeBehind);
        Assert.Contains("DesktopLocalization.Localizer.GetString(\"About.Status.DiagnosticsCopyFailed\")", aboutCodeBehind);
        Assert.DoesNotContain("About.Status.DiagnosticsCopyFailed\", ex.Message", aboutCodeBehind);
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
        Assert.Contains("AutomationProperties.HelpText=\"{Binding PrimaryActionHelpText}\"", updateCheckWindow);
        Assert.Contains("AutomationProperties.HelpText=\"{Binding AssetActionHelpText}\"", updateCheckWindow);
        Assert.Contains("Content=\"{i18n:Loc UpdateCheck.CopyDetails}\"", updateCheckWindow);
        Assert.Contains("Content=\"{i18n:Loc Common.Close}\"", updateCheckWindow);
        Assert.Contains("DesktopLocalization.Localizer.GetString(\"UpdateCheck.Status.DetailsCopied\")", updateCheckCodeBehind);
        Assert.Contains("DesktopLocalization.Localizer.GetString(\"UpdateCheck.Status.CopyFailed\")", updateCheckCodeBehind);
        Assert.DoesNotContain("UpdateCheck.Status.CopyFailed\", ex.Message", updateCheckCodeBehind);
        Assert.Contains("_localizer.GetString(\"UpdateCheck.Heading.Default\")", updateDialogViewModel);
        Assert.Contains("_localizer.GetString(\"UpdateCheck.Primary.InstallHelp\")", updateDialogViewModel);
        Assert.Contains("_localizer.Format(\"UpdateCheck.Details.AssetUrl\"", updateDialogViewModel);
        Assert.Contains("_fileSizeFormatService.FormatBytes", updateDialogViewModel);
        Assert.DoesNotContain("private static string FormatBytes", updateDialogViewModel);
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
        Assert.Contains("UpdateInstall.ProgressPercent", updateInstallViewModel);
        Assert.Contains("_fileSizeFormatService.FormatBytes", updateInstallViewModel);
        Assert.DoesNotContain("private static string FormatBytes", updateInstallViewModel);
        Assert.DoesNotContain("ProgressText = \"100 %\"", updateInstallViewModel);
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
        var englishResources = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Application", "Resources", "Strings.resx"));
        var czechResources = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Application", "Resources", "Strings.cs-CZ.resx"));

        Assert.Contains("FuelAnalysis.Warning.OdometerInvalid.Title", service);
        Assert.Contains("FuelAnalysis.Status.ManySegments", service);
        Assert.Contains("FuelAnalysis.Group.UnknownStation", service);
        Assert.Contains("FormatDistanceFromKilometers", service);
        Assert.DoesNotContain("{1} km", service);
        Assert.DoesNotContain("has odometer {1} km", englishResources);
        Assert.DoesNotContain("má tachometr {1} km", czechResources);
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
        Assert.Contains("SmartAdvisor.Title.CostPerDistanceUnavailable", service);
        Assert.Contains("Timeline.Status.OverDistanceLimit", service);
        Assert.DoesNotContain("SmartAdvisor.Title.CostPerKmUnavailable", service);
        Assert.DoesNotContain("Po limitu", service);
        Assert.DoesNotContain("Over distance limit", service);
        Assert.Contains("SmartAdvisor.Action.OpenVehicleCosts", service);
        Assert.Contains("LocalizedResourceValueMatcher.Matches", service);
        Assert.DoesNotContain("CategoryAttachmentCs", service);
        Assert.DoesNotContain("\"Attachment\"", service);
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
        Assert.Contains("FormatDistanceFromKilometers", service);
        Assert.Contains("FormatVolumeFromLiters", service);
        Assert.Contains("FormatFuelVolume", service);
        Assert.DoesNotContain("FormatFuelLiters", service);
        Assert.DoesNotContain("Timeline.Value.OdometerKm", service);
        Assert.DoesNotContain("Timeline.Value.Liters", service);
        Assert.DoesNotContain("Timeline.Status.OverDistanceLimitKm", service);
        Assert.DoesNotContain("Timeline.Status.WithinDistanceKm", service);
        Assert.Contains("new LegacyTimelineService(DesktopLocalization.LiveLocalizer)", mainWindowViewModel);
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
        Assert.Contains("AppShell.FileName.CalendarExport", mainWindowViewModel);
        Assert.DoesNotContain("vehimap-kalendar", mainWindowViewModel, StringComparison.Ordinal);
        Assert.Contains("TimelineFilterFutureKey", preferencesViewModel);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), service);
    }

    [Fact]
    public void Managed_attachment_storage_fallback_is_language_neutral()
    {
        var root = FindRepositoryRoot();
        var editing = File.ReadAllText(Path.Combine(
            root,
            "dotnet",
            "src",
            "Vehimap.Desktop",
            "ViewModels",
            "MainWindowViewModel.Editing.cs"));

        Assert.Contains("ManagedAttachmentFallbackBaseName = \"attachment\"", editing, StringComparison.Ordinal);
        Assert.DoesNotContain("\"priloha\"", editing, StringComparison.Ordinal);

        var vehiclePackageService = File.ReadAllText(Path.Combine(
            root,
            "dotnet",
            "src",
            "Vehimap.Storage.Sqlite",
            "VehiclePackageService.cs"));
        Assert.Contains("AttachmentFallbackFileName = \"attachment.bin\"", vehiclePackageService, StringComparison.Ordinal);
        Assert.DoesNotContain("VehiclePackage.AttachmentFallbackFileName", vehiclePackageService, StringComparison.Ordinal);
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
        Assert.Contains("FormatDistanceFromKilometers", service);
        Assert.Contains("FormatFuelVolume", service);
        Assert.Contains("LocalizedResourceValueMatcher.Matches", service);
        Assert.DoesNotContain("NeutralTimelineStatusCs", service);
        Assert.DoesNotContain("NeutralTimelineStatusEn", service);
        Assert.DoesNotContain("FormatFuelLiters", service);
        Assert.DoesNotContain("GlobalSearch.Value.OdometerKm", service);
        Assert.DoesNotContain("GlobalSearch.Value.Liters", service);
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
        var overviewFilterOptions = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "Workspaces", "OverviewFilterOptions.cs"));
        var overviewsViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "MainWindowViewModel.Overviews.cs"));
        var projectionService = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Services", "DesktopProjectionService.cs"));

        Assert.Contains("Overview.Filter.All", overviewFilterOptions);
        Assert.Contains("Overview.Detail.Selected", upcomingWorkspaceViewModel);
        Assert.Contains("Overview.Filter.DataIssues", overviewFilterOptions);
        Assert.Contains("Overview.Detail.EmptyOverdue", overdueWorkspaceViewModel);
        Assert.Contains("DashboardTimeline.Detail.Selected", dashboardWorkspaceViewModel);
        Assert.Contains("Overview.Summary.UpcomingWithItems", overviewsViewModel);
        Assert.Contains("Overview.MissingGreen.Title", overviewsViewModel);
        Assert.Contains("Overview.DataIssue.KindLabel", overviewsViewModel);
        Assert.Contains("LocalizedCompatibilityAliases.MatchesStableValueOrResource", overviewsViewModel);
        Assert.Contains("ResourceKeyForKey", overviewFilterOptions);
        Assert.DoesNotContain("FilterLegacyCzech", overviewsViewModel);
        Assert.DoesNotContain("FilterLegacyEnglish", overviewsViewModel);
        Assert.Contains("Overview.Summary.DashboardWithItems", projectionService);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), upcomingWorkspaceViewModel);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), overdueWorkspaceViewModel);
        Assert.DoesNotMatch(CzechDiacriticsRegex(), dashboardWorkspaceViewModel);
    }

    [Fact]
    public void Domain_vehicle_projection_summaries_use_resource_localization_for_generated_messages()
    {
        var root = FindRepositoryRoot();
        var projectionService = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Services", "DesktopProjectionService.cs"));

        Assert.Contains("VehicleDetail.Projection.Overview", projectionService);
        Assert.Contains("VehicleDetail.Projection.EvidenceSummary", projectionService);
        Assert.Contains("VehicleDetail.Projection.Record.NearestValidity", projectionService);
        Assert.Contains("VehicleList.Summary.Filtered", projectionService);
        Assert.Contains("VehicleList.Status.MissingGreenCard", projectionService);
        Assert.Contains("Record.Projection.AttachmentState.Available", projectionService);
        Assert.Contains("History.Projection.Summary.Count", projectionService);
        Assert.Contains("Fuel.Projection.Summary.Count", projectionService);
        Assert.Contains("Reminder.Status.Overdue", projectionService);
        Assert.Contains("Maintenance.Status.OverDistanceLimit", projectionService);
        Assert.Contains("FuelAnalysis.Value.SegmentPeriod", projectionService);
    }

    [Fact]
    public void Domain_quick_actions_use_resource_localization_for_generated_messages()
    {
        var root = FindRepositoryRoot();
        var quickActionsViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "MainWindowViewModel.QuickActions.cs"));
        var workspaceStateViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "MainWindowViewModel.WorkspaceState.cs"));
        var appShellViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "MainWindowViewModel.AppShell.cs"));
        var overviewFilterOptions = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "Workspaces", "OverviewFilterOptions.cs"));

        Assert.Contains("QuickActions.Status.NearestTechnical", quickActionsViewModel);
        Assert.Contains("QuickActions.Status.ReviewRecordOpened", quickActionsViewModel);
        Assert.Contains("QuickActions.Status.OpenedBackgroundTimeline", quickActionsViewModel);
        Assert.Contains("Timeline.Status.NoAlert", quickActionsViewModel);
        Assert.Contains("LocalizedCompatibilityAliases.MatchesAnyResource", quickActionsViewModel);
        Assert.DoesNotContain("QuickActionNoAlertLegacy", quickActionsViewModel);
        Assert.Contains("Overview.Filter.GreenCards", overviewFilterOptions);
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
        var mainWindowViewModel = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "MainWindowViewModel.cs"));
        var appShellController = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Services", "DesktopAppShellController.cs"));
        var printableReportService = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Services", "DesktopPrintableVehicleReportService.cs"));

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
        Assert.Contains("new DesktopPrintableVehicleReportService(DesktopLocalization.LiveLocalizer)", mainWindowViewModel);
        Assert.Contains("PrintableReport.Title", printableReportService);
        Assert.Contains("PrintableReport.Column.GreenCardTo", printableReportService);
        Assert.Contains("PrintableReport.Status.Maintenance", printableReportService);
        Assert.DoesNotContain("Tiskový přehled", printableReportService);
        Assert.DoesNotContain("V této kategorii", printableReportService);
        Assert.DoesNotContain("Zelená karta do", printableReportService);
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

    [Fact]
    public void Unit_sensitive_resource_surfaces_do_not_keep_fixed_metric_labels()
    {
        var root = FindRepositoryRoot();
        var englishPath = Path.Combine(root, "dotnet", "src", "Vehimap.Application", "Resources", "Strings.resx");
        var czechPath = Path.Combine(root, "dotnet", "src", "Vehimap.Application", "Resources", "Strings.cs-CZ.resx");
        var englishResources = File.ReadAllText(englishPath);
        var czechResources = File.ReadAllText(czechPath);
        var englishKeys = ReadResourceKeys(englishPath);
        var czechKeys = ReadResourceKeys(czechPath);

        Assert.DoesNotContain("Settings.MaintenanceReminderKm", englishKeys);
        Assert.DoesNotContain("Settings.MaintenanceReminderKmName", englishKeys);
        Assert.DoesNotContain("Settings.MaintenanceReminderKm", czechKeys);
        Assert.DoesNotContain("Settings.MaintenanceReminderKmName", czechKeys);

        Assert.DoesNotContain("ServiceBook.Value.OverDistanceLimitKm", englishKeys);
        Assert.DoesNotContain("ServiceBook.Value.InKm", englishKeys);
        Assert.DoesNotContain("ServiceBook.Value.OdometerKm", englishKeys);
        Assert.DoesNotContain("ServiceBook.Value.OverDistanceLimitKm", czechKeys);
        Assert.DoesNotContain("ServiceBook.Value.InKm", czechKeys);
        Assert.DoesNotContain("ServiceBook.Value.OdometerKm", czechKeys);

        Assert.DoesNotContain("Maintenance reminder (km)", englishResources);
        Assert.DoesNotContain("Distance interval (km)", englishResources);
        Assert.DoesNotContain("Maintenance distance interval in kilometers", englishResources);
        Assert.DoesNotContain("Upozornění na údržbu (km)", czechResources);
        Assert.DoesNotContain("Interval km", czechResources);
        Assert.DoesNotContain("Interval údržby v kilometrech", czechResources);
        Assert.DoesNotContain("cena za km", czechResources);
        Assert.DoesNotContain("Chybí km v období", czechResources);
        Assert.DoesNotContain("Bez km v období", czechResources);
        Assert.DoesNotContain("bez údajů o litrech", czechResources);
        Assert.DoesNotContain("počet litrů", czechResources);
        Assert.DoesNotContain("počtu litrů", czechResources);
    }

    [Fact]
    public void Unit_sensitive_editor_inputs_use_display_unit_names_at_ui_edge()
    {
        var root = FindRepositoryRoot();
        var workflowEditing = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "MainWindowViewModel.WorkflowEditing.cs"));

        Assert.Contains("volumeText", workflowEditing);
        Assert.Contains("intervalDistanceText", workflowEditing);
        Assert.DoesNotContain("litersText", workflowEditing);
        Assert.DoesNotContain("intervalKmText", workflowEditing);
    }

    [Fact]
    public void Production_unit_and_currency_labels_are_limited_to_formatting_boundaries()
    {
        var root = FindRepositoryRoot();
        var sourceRoot = Path.Combine(root, "dotnet", "src");
        var literalRegex = new Regex("(?:\\s(?:km|mi|l)\\b|/(?:km|mi)\\b|US gal|imp gal|\\bmpg\\b|\\b(?:KB|MB|GB|CZK|USD|EUR|GBP)\\b|Kč|\\bliters?\\b|\\blitres?\\b|\\blitr(?:y|ů|u|em)?\\b|\\bgalon(?:y|ů|u|em)?\\b)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        var failures = new List<string>();
        foreach (var file in Directory.EnumerateFiles(sourceRoot, "*.*", SearchOption.AllDirectories)
                     .Where(path => Path.GetExtension(path) is ".cs" or ".axaml")
                     .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
                     .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
                     .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}Resources{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)))
        {
            var relativePath = Path.GetRelativePath(root, file).Replace('\\', '/');
            var lineNumber = 0;
            foreach (var line in File.ReadLines(file))
            {
                lineNumber++;
                var offendingLiterals = ExtractQuotedStringLiterals(line)
                    .Where(literal => literalRegex.IsMatch(literal))
                    .Where(literal => !IsResourceKeyLiteral(literal))
                    .ToList();
                if (offendingLiterals.Count == 0 || IsAllowedProductionUnitCurrencyLine(relativePath, line.Trim()))
                {
                    continue;
                }

                failures.Add($"{relativePath}:{lineNumber}: {line.Trim()} [{string.Join(", ", offendingLiterals)}]");
            }
        }

        Assert.True(
            failures.Count == 0,
            "Fixed unit and currency literals are allowed only in storage defaults and shared formatting services. Offending lines:" +
            Environment.NewLine +
            string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public void Known_value_editor_dropdowns_use_value_label_options()
    {
        var root = FindRepositoryRoot();
        var vehicleWorkspace = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "Workspaces", "VehicleDetailWorkspaceViewModel.cs"));
        var fuelWorkspace = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "Workspaces", "FuelWorkspaceViewModel.cs"));
        var recordWorkspace = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "Workspaces", "RecordWorkspaceViewModel.cs"));
        var reminderWorkspace = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "Workspaces", "ReminderWorkspaceViewModel.cs"));
        var bundleItem = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "ViewModels", "VehicleStarterBundleItemEditorViewModel.cs"));
        var vehicleEditorXaml = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "VehicleEditorWindow.axaml"));
        var fuelEditorXaml = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "FuelEditorWindow.axaml"));
        var recordEditorXaml = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "RecordEditorWindow.axaml"));
        var reminderEditorXaml = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "ReminderEditorWindow.axaml"));
        var bundleXaml = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "VehicleStarterBundleWindow.axaml"));

        Assert.Contains("IReadOnlyList<LocalizedOptionViewModel> VehicleCategoryOptions", vehicleWorkspace);
        Assert.Contains("IReadOnlyList<LocalizedOptionViewModel> FuelTypeOptions", fuelWorkspace);
        Assert.Contains("IReadOnlyList<LocalizedOptionViewModel> RecordTypeOptions", recordWorkspace);
        Assert.Contains("IReadOnlyList<LocalizedOptionViewModel> ReminderRepeatModeOptions", reminderWorkspace);
        Assert.Contains("IReadOnlyList<LocalizedOptionViewModel> RecordTypeOptions", bundleItem);
        Assert.Contains("IReadOnlyList<LocalizedOptionViewModel> ReminderRepeatModeOptions", bundleItem);

        Assert.Contains("SelectedItem=\"{Binding SelectedVehicleCategoryOption}\"", vehicleEditorXaml);
        Assert.Contains("SelectedItem=\"{Binding SelectedFuelTypeOption}\"", fuelEditorXaml);
        Assert.Contains("SelectedItem=\"{Binding SelectedRecordTypeOption}\"", recordEditorXaml);
        Assert.Contains("SelectedItem=\"{Binding SelectedReminderRepeatModeOption}\"", reminderEditorXaml);
        Assert.Contains("SelectedItem=\"{Binding SelectedItem.SelectedRecordTypeOption}\"", bundleXaml);
        Assert.Contains("SelectedItem=\"{Binding SelectedItem.SelectedReminderRepeatModeOption}\"", bundleXaml);

        Assert.DoesNotContain("IReadOnlyList<string> VehicleCategoryOptions => LegacyKnownValues.Categories", vehicleWorkspace);
        Assert.DoesNotContain("IReadOnlyList<string> FuelTypeOptions => LegacyKnownValues.FuelTypes", fuelWorkspace);
        Assert.DoesNotContain("IReadOnlyList<string> RecordTypeOptions => LegacyKnownValues.RecordTypes", recordWorkspace);
        Assert.DoesNotContain("IReadOnlyList<string> ReminderRepeatModeOptions => LegacyKnownValues.ReminderRepeatModes", reminderWorkspace);
    }

    [Fact]
    public void Known_value_options_keep_values_stable_across_language_switches()
    {
        try
        {
            DesktopLocalization.Configure(new AppCulturePreferences("en-US", "comma", "dot"));
            var englishCategory = KnownValueOptions.SelectVehicleCategory("Osobní vozidla");
            var englishShortCategory = KnownValueOptions.SelectVehicleCategory("Osobní");
            var englishFuel = KnownValueOptions.SelectFuelType("Gasoline");
            var englishAccentFuel = KnownValueOptions.SelectFuelType("Benzín");
            var englishYearlyRepeat = KnownValueOptions.SelectReminderRepeatMode("Ročně");
            var englishAsciiYearlyRepeat = KnownValueOptions.SelectReminderRepeatMode("Rocne");
            var englishWordedTwoYearRepeat = KnownValueOptions.SelectReminderRepeatMode("Every two years");
            var englishWordedFiveYearRepeat = KnownValueOptions.SelectReminderRepeatMode("Every five years");
            var customFuel = KnownValueOptions.SelectFuelType("Natural 100");

            Assert.Equal("Osobní vozidla", englishCategory.Value);
            Assert.Equal("Passenger vehicles", englishCategory.Label);
            Assert.Equal("Osobní vozidla", englishShortCategory.Value);
            Assert.Equal("Passenger vehicles", englishShortCategory.Label);
            Assert.Equal("Benzin", englishFuel.Value);
            Assert.Equal("Gasoline", englishFuel.Label);
            Assert.Equal("Benzin", englishAccentFuel.Value);
            Assert.Equal("Gasoline", englishAccentFuel.Label);
            Assert.Equal("Každý rok", englishYearlyRepeat.Value);
            Assert.Equal("Every year", englishYearlyRepeat.Label);
            Assert.Equal("Každý rok", englishAsciiYearlyRepeat.Value);
            Assert.Equal("Every year", englishAsciiYearlyRepeat.Label);
            Assert.Equal("Každé 2 roky", englishWordedTwoYearRepeat.Value);
            Assert.Equal("Every 2 years", englishWordedTwoYearRepeat.Label);
            Assert.Equal("Každých 5 let", englishWordedFiveYearRepeat.Value);
            Assert.Equal("Every 5 years", englishWordedFiveYearRepeat.Label);
            Assert.Equal("Natural 100", customFuel.Value);
            Assert.Equal("Natural 100", customFuel.Label);

            DesktopLocalization.Configure(new AppCulturePreferences("cs-CZ", "none", "comma"));
            var czechCategory = KnownValueOptions.SelectVehicleCategory(englishCategory.Value);
            var czechFuel = KnownValueOptions.SelectFuelType(englishFuel.Value);

            Assert.Equal("Osobní vozidla", czechCategory.Value);
            Assert.Equal("Osobní vozidla", czechCategory.Label);
            Assert.Equal("Benzin", czechFuel.Value);
            Assert.Equal("Benzín", czechFuel.Label);
        }
        finally
        {
            TestCultureInitializer.ResetToCzech();
        }
    }

    [Fact]
    public void Known_value_option_aliases_are_resource_backed()
    {
        var root = FindRepositoryRoot();
        var source = File.ReadAllText(Path.Combine(
            root,
            "dotnet",
            "src",
            "Vehimap.Desktop",
            "ViewModels",
            "KnownValueOptions.cs"));
        var invalidLiterals = source
            .Split('\n')
            .Where(line => line.Contains("Definition(", StringComparison.Ordinal))
            .SelectMany(ExtractQuotedStringLiterals)
            .Where(literal => !literal.StartsWith("KnownValue.", StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(literal => literal, StringComparer.Ordinal)
            .ToArray();

        Assert.Empty(invalidLiterals);
        Assert.Contains("AliasResourceKeys", source);
        Assert.DoesNotContain("ResourceAliasPrefix", source);
        Assert.DoesNotContain("MatchesAlias", source);
    }

    [Fact]
    public void Production_czech_text_is_limited_to_i18n_compatibility_boundaries()
    {
        var root = FindRepositoryRoot();
        var sourceRoot = Path.Combine(root, "dotnet", "src");
        var failures = new List<string>();
        foreach (var file in Directory.EnumerateFiles(sourceRoot, "*.*", SearchOption.AllDirectories)
                     .Where(path => Path.GetExtension(path) is ".cs" or ".axaml")
                     .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
                     .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
                     .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}Resources{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)))
        {
            var relativePath = Path.GetRelativePath(root, file).Replace('\\', '/');
            var lineNumber = 0;
            foreach (var line in File.ReadLines(file))
            {
                lineNumber++;
                if (!CzechDiacriticsRegex().IsMatch(line) || IsAllowedProductionCzechCompatibilityLine(relativePath, line.Trim()))
                {
                    continue;
                }

                failures.Add($"{relativePath}:{lineNumber}: {line.Trim()}");
            }
        }

        Assert.True(
            failures.Count == 0,
            "Hardcoded Czech production text is allowed only for legacy tokens, compatibility aliases and documented catalogs. Offending lines:" +
            Environment.NewLine +
            string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public void Runtime_services_do_not_default_user_visible_localization_to_czech()
    {
        var root = FindRepositoryRoot();
        var sourceRoot = Path.Combine(root, "dotnet", "src");
        var fallbackRegex = new Regex(
            @"(?:localizer\s*\?\?\s*new\s+ResourceAppLocalizer\(\s*CultureInfo\.GetCultureInfo\(AppCultureService\.CzechLanguage\)\s*\)|:\s*this\(\s*new\s+ResourceAppLocalizer\(\s*CultureInfo\.GetCultureInfo\(AppCultureService\.CzechLanguage\)\s*\)\s*\)|=>\s*new\s+ResourceAppLocalizer\(\s*CultureInfo\.GetCultureInfo\(AppCultureService\.CzechLanguage\)\s*\))",
            RegexOptions.Compiled);
        var failures = new List<string>();

        foreach (var file in Directory.EnumerateFiles(sourceRoot, "*.cs", SearchOption.AllDirectories)
                     .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
                     .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)))
        {
            var relativePath = Path.GetRelativePath(root, file).Replace('\\', '/');
            var lineNumber = 0;
            foreach (var line in File.ReadLines(file))
            {
                lineNumber++;
                if (fallbackRegex.IsMatch(line))
                {
                    failures.Add($"{relativePath}:{lineNumber}: {line.Trim()}");
                }
            }
        }

        Assert.True(
            failures.Count == 0,
            "Runtime services must default to CurrentUICulture via ResourceAppLocalizer(), not to Czech. Explicit Czech localizers are allowed only for compatibility alias generation or bilingual template catalogs. Offending lines:" +
            Environment.NewLine +
            string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public void Runtime_services_do_not_default_unit_and_currency_formatting_to_czech()
    {
        var root = FindRepositoryRoot();
        var sourceRoot = Path.Combine(root, "dotnet", "src");
        var forbiddenPatterns = new Regex(
            @"new\s+AppCulturePreferences\(\s*AppCultureService\.CzechLanguage|new\s+AppUnitPreferences\(\s*AppUnitFormatService\.Kilometers\s*,\s*AppUnitFormatService\.Liters\s*\)|=\s*AppCurrencyFormatService\.CzechCrowns",
            RegexOptions.Compiled);
        var failures = new List<string>();

        foreach (var file in Directory.EnumerateFiles(sourceRoot, "*.cs", SearchOption.AllDirectories)
                     .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
                     .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
                     .Where(path => !path.EndsWith(Path.Combine("Services", "AppLocaleDefaultsService.cs"), StringComparison.OrdinalIgnoreCase)))
        {
            var relativePath = Path.GetRelativePath(root, file).Replace('\\', '/');
            var lineNumber = 0;
            foreach (var line in File.ReadLines(file))
            {
                lineNumber++;
                if (forbiddenPatterns.IsMatch(line))
                {
                    failures.Add($"{relativePath}:{lineNumber}: {line.Trim()}");
                }
            }
        }

        Assert.True(
            failures.Count == 0,
            "Runtime services must derive fallback formatting from AppLocaleDefaultsService.GetCurrentCultureDefaults(), not from hardcoded Czech km/l/CZK defaults. Offending lines:" +
            Environment.NewLine +
            string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public void Preference_persistence_statuses_do_not_append_raw_exception_messages()
    {
        var root = FindRepositoryRoot();
        var preferencePersistence = File.ReadAllText(Path.Combine(
            root,
            "dotnet",
            "src",
            "Vehimap.Desktop",
            "ViewModels",
            "MainWindowViewModel.PreferencePersistence.cs"));

        Assert.Contains("ShellStatus = failurePrefix;", preferencePersistence);
        Assert.DoesNotContain("ex.Message", preferencePersistence);
        Assert.DoesNotContain("\": {ex.Message}\"", preferencePersistence);
    }

    [Fact]
    public void Desktop_user_facing_statuses_do_not_append_raw_exception_messages()
    {
        var root = FindRepositoryRoot();
        var desktopRoot = Path.Combine(root, "dotnet", "src", "Vehimap.Desktop");
        var failures = Directory
            .EnumerateFiles(desktopRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => File.ReadLines(path)
                .Select((line, index) => (Path: path, Line: line, Number: index + 1)))
            .Where(item => Regex.IsMatch(item.Line, @"\b(?:ex|exception)\.Message\b"))
            .Select(item => $"{Path.GetRelativePath(root, item.Path).Replace('\\', '/')}:{item.Number}: {item.Line.Trim()}")
            .ToArray();

        Assert.True(
            failures.Length == 0,
            "Desktop user-facing status paths must classify exceptions through UserFacingExceptionMessageService instead of exposing raw exception text." +
            Environment.NewLine +
            string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public void User_visible_full_dates_do_not_use_a_fixed_czech_format()
    {
        var root = FindRepositoryRoot();
        var sourceRoots = new[]
        {
            Path.Combine(root, "dotnet", "src", "Vehimap.Application"),
            Path.Combine(root, "dotnet", "src", "Vehimap.Desktop")
        };
        var failures = sourceRoots
            .SelectMany(path => Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories))
            .Where(path => Path.GetExtension(path) is ".cs" or ".axaml" or ".resx")
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.EndsWith(Path.Combine("Services", "VehimapValueParser.cs"), StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.EndsWith(Path.Combine("Services", "AppDateFormatService.cs"), StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => File.ReadLines(path)
                .Select((line, index) => (Path: path, Line: line, Number: index + 1)))
            .Where(item => item.Line.Contains("dd.MM.yyyy", StringComparison.Ordinal)
                || item.Line.Contains("DD.MM.YYYY", StringComparison.Ordinal)
                || item.Line.Contains("DD.MM.RRRR", StringComparison.Ordinal)
                || item.Line.Contains(".ToString(\"d\"", StringComparison.Ordinal)
                || item.Line.Contains(".ToString(\"g\"", StringComparison.Ordinal))
            .Select(item => $"{Path.GetRelativePath(root, item.Path).Replace('\\', '/')}:{item.Number}: {item.Line.Trim()}")
            .ToArray();

        Assert.True(
            failures.Length == 0,
            "User-visible full dates must use IAppDateFormatService. The fixed format remains allowed only at canonical storage and legacy migration boundaries:" +
            Environment.NewLine +
            string.Join(Environment.NewLine, failures));
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

    private static SortedDictionary<string, string> ReadResourceValues(string path)
    {
        var document = XDocument.Load(path);
        return new SortedDictionary<string, string>(
            document.Root!
                .Elements("data")
                .Where(element => !string.IsNullOrWhiteSpace(element.Attribute("name")?.Value))
                .ToDictionary(
                    element => element.Attribute("name")!.Value,
                    element => element.Element("value")?.Value ?? string.Empty,
                    StringComparer.Ordinal),
            StringComparer.Ordinal);
    }

    private static string[] ExtractFormatPlaceholders(string value) =>
        Regex.Matches(value, @"(?<!\{)\{\d+(?:[^}]*)?\}(?!\})")
            .Select(match => match.Value)
            .OrderBy(placeholder => placeholder, StringComparer.Ordinal)
            .ToArray();

    private static Regex CzechDiacriticsRegex() =>
        new("[ÁČĎÉĚÍŇÓŘŠŤÚŮÝŽáčďéěíňóřšťúůýž]", RegexOptions.Compiled);

    private static bool IsAllowedProductionCzechCompatibilityLine(string relativePath, string line)
    {
        if (relativePath is
            "dotnet/src/Vehimap.Storage.Legacy/LegacyKnownValues.cs")
        {
            return true;
        }

        if (relativePath is "dotnet/src/Vehimap.Application/Services/VehicleStarterBundleService.cs")
        {
            return IsAllowedVehicleStarterBundleCatalogLine(line);
        }

        return false;
    }

    private static bool IsAllowedVehicleStarterBundleCatalogLine(string line) =>
        line.StartsWith("Maintenance(", StringComparison.Ordinal)
        || line.StartsWith("new(VehicleStarterBundleSection.Record,", StringComparison.Ordinal)
        || line.StartsWith("[\"", StringComparison.Ordinal)
        || line.Contains("=> localizer.GetString(\"VehicleStarterBundle.Catalog.", StringComparison.Ordinal);

    private static bool IsAllowedProductionUnitCurrencyLine(string relativePath, string line)
    {
        if (relativePath is
            "dotnet/src/Vehimap.Application/Models/AppLocaleDefaults.cs" or
            "dotnet/src/Vehimap.Application/Models/AppUnitPreferences.cs" or
            "dotnet/src/Vehimap.Application/Models/DesktopSupportedSettingsSnapshot.cs" or
            "dotnet/src/Vehimap.Application/Services/AppCurrencyFormatService.cs" or
            "dotnet/src/Vehimap.Application/Services/AppFileSizeFormatService.cs" or
            "dotnet/src/Vehimap.Application/Services/AppUnitFormatService.cs")
        {
            return true;
        }

        if (relativePath.StartsWith("dotnet/src/Vehimap.Storage.", StringComparison.Ordinal))
        {
            return true;
        }

        if (relativePath is "dotnet/src/Vehimap.Application/Services/VehimapValueParser.cs")
        {
            return line.Contains("\"k\\u010D\"", StringComparison.Ordinal)
                || line.Contains("\"czk\"", StringComparison.Ordinal);
        }

        return false;
    }

    private static bool IsResourceKeyLiteral(string value) =>
        value.Contains('.', StringComparison.Ordinal)
        && !value.Contains(' ', StringComparison.Ordinal)
        && value.All(character => char.IsLetterOrDigit(character) || character is '.' or '_' or '-');

    private static IEnumerable<string> ExtractQuotedStringLiterals(string line)
    {
        var index = 0;
        while (index < line.Length)
        {
            if (line[index] != '"')
            {
                index++;
                continue;
            }

            index++;
            var start = index;
            var builder = new System.Text.StringBuilder();
            while (index < line.Length)
            {
                if (line[index] == '\\' && index + 1 < line.Length)
                {
                    builder.Append(line[index + 1]);
                    index += 2;
                    continue;
                }

                if (line[index] == '"')
                {
                    yield return builder.Length == 0
                        ? line[start..index]
                        : builder.ToString();
                    index++;
                    break;
                }

                builder.Append(line[index]);
                index++;
            }
        }
    }

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
