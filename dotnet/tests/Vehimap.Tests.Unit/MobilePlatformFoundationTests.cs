// SPDX-License-Identifier: GPL-3.0-or-later
using System.Globalization;
using System.Xml.Linq;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Services;
using Vehimap.Domain.Models;
using Vehimap.Mobile.Services;
using Vehimap.Mobile.ViewModels;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class MobilePlatformFoundationTests
{
    [Fact]
    public void Android_project_targets_net10_and_separate_nightly_identity()
    {
        var project = XDocument.Load(RepositoryPath("src", "Vehimap.Android", "Vehimap.Android.csproj"));
        var properties = project.Descendants("PropertyGroup").Elements().ToArray();

        Assert.Contains(properties, element => element.Name.LocalName == "TargetFramework" && element.Value == "net10.0-android");
        Assert.Contains(properties, element => element.Name.LocalName == "SupportedOSPlatformVersion" && element.Value == "31");
        Assert.Contains(properties, element =>
            element.Name.LocalName == "RuntimeIdentifiers"
            && element.Value == "android-arm64;android-x64");
        Assert.Contains(properties, element =>
            element.Name.LocalName == "ApplicationId"
            && element.Attribute("Condition")?.Value.Contains("nightly", StringComparison.Ordinal) == true
            && element.Value == "cz.vlcekapps.vehimap.nightly");
        Assert.Contains(properties, element => element.Name.LocalName == "AndroidPackageFormat" && element.Value == "apk");

        var activity = File.ReadAllText(RepositoryPath("src", "Vehimap.Android", "MainActivity.cs"));
        Assert.DoesNotContain("Label = \"Vehimap Nightly\"", activity, StringComparison.Ordinal);
    }

    [Fact]
    public void Android_host_packages_required_license_notices()
    {
        var project = XDocument.Load(RepositoryPath("src", "Vehimap.Android", "Vehimap.Android.csproj"));
        var assets = project.Descendants("AndroidAsset")
            .Select(element => element.Attribute("Include")?.Value ?? string.Empty)
            .ToArray();

        Assert.Contains(assets, value => value.EndsWith("LICENSE", StringComparison.Ordinal));
        Assert.Contains(assets, value => value.EndsWith("THIRD-PARTY-NOTICES.md", StringComparison.Ordinal));
        Assert.Contains(assets, value => value.Contains("LICENSES", StringComparison.Ordinal));

        var notices = File.ReadAllText(Path.Combine(Directory.GetParent(RepositoryPath())!.FullName, "THIRD-PARTY-NOTICES.md"));
        Assert.Contains("Avalonia.Android", notices, StringComparison.Ordinal);
        Assert.Contains("Xamarin.AndroidX binding packages", notices, StringComparison.Ordinal);
        Assert.Contains("Xamarin.Kotlin and KotlinX binding packages", notices, StringComparison.Ordinal);
    }

    [Fact]
    public void Mobile_vehicle_projection_localizes_known_values_but_preserves_user_data()
    {
        var localizer = new ResourceAppLocalizer(CultureInfo.GetCultureInfo(AppCultureService.EnglishLanguage));
        var vehicle = new Vehicle(
            "veh-1",
            "Rodinný veterán",
            "Osobní vozidla",
            "Uživatelská poznámka",
            "Škoda 100",
            "ABC 12-34",
            "1972",
            "35",
            "",
            "09/2027",
            "",
            "10/2027");
        var meta = new VehicleMeta("veh-1", "Veterán", "", "Benzín", "", "", "");

        var item = new MobileVehicleListItemViewModel(vehicle, meta, localizer);

        Assert.Equal("Rodinný veterán", item.Name);
        Assert.Equal("Škoda 100", item.MakeModel);
        Assert.Equal("Uživatelská poznámka", item.Note);
        Assert.Equal("Passenger vehicles", item.Category);
        Assert.Equal("Veteran", item.State);
        Assert.Contains("license plate ABC 12-34", item.AccessibleLabel, StringComparison.Ordinal);
    }

    [Fact]
    public void Mobile_resources_exist_in_both_supported_languages()
    {
        var english = ReadResourceKeys(RepositoryPath("src", "Vehimap.Application", "Resources", "Strings.resx"));
        var czech = ReadResourceKeys(RepositoryPath("src", "Vehimap.Application", "Resources", "Strings.cs-CZ.resx"));
        var mobileKeys = english.Where(key => key.StartsWith("Mobile.", StringComparison.Ordinal)).ToArray();

        Assert.NotEmpty(mobileKeys);
        Assert.All(mobileKeys, key => Assert.Contains(key, czech));
    }

    [Fact]
    public void Mobile_shell_uses_four_accessible_primary_destinations()
    {
        var view = File.ReadAllText(RepositoryPath("src", "Vehimap.Mobile", "Views", "MobileMainView.axaml"));

        Assert.Equal(4, XDocument.Parse(view).Descendants().Count(element => element.Name.LocalName == "RadioButton"));
        Assert.Contains("AutomationProperties.AutomationId=\"MobilePrimaryNavigation\"", view, StringComparison.Ordinal);
        Assert.Contains("MobileHomeNavigationButton", view, StringComparison.Ordinal);
        Assert.Contains("MobileVehiclesNavigationButton", view, StringComparison.Ordinal);
        Assert.Contains("MobileAlertsNavigationButton", view, StringComparison.Ordinal);
        Assert.Contains("MobileMoreNavigationButton", view, StringComparison.Ordinal);
        Assert.Equal(4, XDocument.Parse(view).Descendants()
            .Where(element => element.Name.LocalName == "RadioButton")
            .Count(element => (string?)element.Attribute("MinHeight") == "56"));
    }

    [Fact]
    public void Primary_surfaces_do_not_embed_redundant_tutorial_copy()
    {
        var mobileViews = Directory.GetFiles(RepositoryPath("src", "Vehimap.Mobile", "Views"), "*.axaml")
            .Select(File.ReadAllText)
            .ToArray();
        var mobileContent = string.Join(Environment.NewLine, mobileViews);
        var desktopShell = File.ReadAllText(RepositoryPath("src", "Vehimap.Desktop", "Views", "MainWindow.axaml"));
        var trayActions = File.ReadAllText(RepositoryPath("src", "Vehimap.Desktop", "Views", "TrayActionsWindow.axaml"));
        var english = ReadResourceValues(RepositoryPath("src", "Vehimap.Application", "Resources", "Strings.resx"));
        var czech = ReadResourceValues(RepositoryPath("src", "Vehimap.Application", "Resources", "Strings.cs-CZ.resx"));
        var removedResourceKeys = new[]
        {
            "Mobile.Navigation.Help",
            "Mobile.Home.Intro",
            "Mobile.Vehicles.Intro",
            "Mobile.Vehicles.HubIntro",
            "Mobile.Alerts.Intro",
            "Mobile.Alerts.ListHelp",
            "Mobile.More.Intro",
            "Mobile.Shell.Intro",
            "Mobile.Shell.ReadOnly",
            "Mobile.VehicleList.Help",
            "Shell.Footer",
            "VehicleList.ListHelp",
            "TrayActions.Description",
            "TrayActions.HelpText"
        };

        Assert.DoesNotContain("{Binding Intro}", mobileContent, StringComparison.Ordinal);
        Assert.DoesNotContain("{Binding NavigationHelp}", mobileContent, StringComparison.Ordinal);
        Assert.DoesNotContain("{Binding VehicleHubIntro}", mobileContent, StringComparison.Ordinal);
        Assert.DoesNotContain("{Binding ReadOnlyText}", mobileContent, StringComparison.Ordinal);
        Assert.DoesNotContain("Shell.Footer", desktopShell, StringComparison.Ordinal);
        Assert.DoesNotContain("VehicleList.ListHelp", desktopShell, StringComparison.Ordinal);
        Assert.DoesNotContain("TrayActions.HelpText", trayActions, StringComparison.Ordinal);
        Assert.DoesNotContain("{Binding Description}", trayActions, StringComparison.Ordinal);
        Assert.All(removedResourceKeys, key =>
        {
            Assert.DoesNotContain(key, english.Keys);
            Assert.DoesNotContain(key, czech.Keys);
        });

        foreach (var key in new[]
                 {
                     "GlobalSearch.Detail.EmptySelection",
                     "FuelWorkspace.Detail.Empty",
                     "HistoryWorkspace.Detail.Empty",
                     "MaintenanceWorkspace.Detail.Empty",
                     "RecordWorkspace.Detail.Empty",
                     "ReminderWorkspace.Detail.Empty",
                     "CostWorkspace.Detail.Empty",
                     "TimelineWorkspace.Detail.Empty",
                     "SmartAdvisor.Detail.Empty"
                 })
        {
            Assert.False(english[key].StartsWith("Select ", StringComparison.OrdinalIgnoreCase));
            Assert.False(czech[key].StartsWith("Vyber", StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public void Mobile_vehicle_hub_is_separate_from_the_list_and_contains_no_inline_editor()
    {
        var view = File.ReadAllText(RepositoryPath("src", "Vehimap.Mobile", "Views", "MobileVehiclesView.axaml"));
        var mobileViews = Directory.GetFiles(RepositoryPath("src", "Vehimap.Mobile", "Views"), "*.axaml")
            .Select(File.ReadAllText)
            .ToArray();

        Assert.Contains("IsVehicleListVisible", view, StringComparison.Ordinal);
        Assert.Contains("IsVehicleHubVisible", view, StringComparison.Ordinal);
        Assert.Contains("MobileVehicleHubBackButton", view, StringComparison.Ordinal);
        Assert.All(mobileViews, content => Assert.DoesNotContain("EditorHost", content, StringComparison.Ordinal));
        Assert.All(mobileViews, content => Assert.DoesNotContain("IsEditing", content, StringComparison.Ordinal));
    }

    [Fact]
    public void Android_system_back_delegates_to_mobile_navigation_before_exiting()
    {
        var activity = File.ReadAllText(RepositoryPath("src", "Vehimap.Android", "MainActivity.cs"));
        var mainView = File.ReadAllText(RepositoryPath("src", "Vehimap.Mobile", "Views", "MobileMainView.axaml.cs"));
        var manifest = File.ReadAllText(RepositoryPath("src", "Vehimap.Android", "Properties", "AndroidManifest.xml"));

        Assert.Contains("_topLevel.BackRequested += OnBackRequested", mainView, StringComparison.Ordinal);
        Assert.Contains("viewModel.TryNavigateBack()", mainView, StringComparison.Ordinal);
        Assert.Contains("e.Handled = true", mainView, StringComparison.Ordinal);
        Assert.DoesNotContain("OnBackPressed", activity, StringComparison.Ordinal);
        Assert.DoesNotContain("OnBackInvokedDispatcher", activity, StringComparison.Ordinal);
        Assert.DoesNotContain("enableOnBackInvokedCallback", manifest, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Mobile_navigation_and_vehicle_hub_preserve_expected_back_stack()
    {
        var originalDefaultCulture = CultureInfo.DefaultThreadCurrentCulture;
        var originalDefaultUiCulture = CultureInfo.DefaultThreadCurrentUICulture;
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;
        var dataSet = new VehimapDataSet
        {
            Vehicles =
            [
                new Vehicle(
                    "veh-1",
                    "Test vehicle",
                    "Osobní vozidla",
                    "User note",
                    "Test model",
                    "",
                    "2024",
                    "80",
                    "",
                    "",
                    "",
                    "")
            ]
        };
        dataSet.Settings.SetValue("app", "language", AppCultureService.EnglishLanguage);
        var root = new VehimapDataRoot(Path.GetTempPath(), Path.Combine(Path.GetTempPath(), $"vehimap-mobile-{Guid.NewGuid():N}"), false);
        var cultureService = new NonMutatingCultureService();
        var session = new MobileSessionController(
            new StubDataStore(dataSet),
            new StubMobileDataRootProvider(root),
            cultureService,
            new DesktopSupportedSettingsService(),
            new ResourceAppLocalizer(CultureInfo.GetCultureInfo(AppCultureService.EnglishLanguage)));
        try
        {
            var viewModel = new MobileMainViewModel(session);

            await viewModel.InitializeAsync();

            Assert.True(viewModel.IsHomeSelected);
            Assert.Single(viewModel.Vehicles.Vehicles);
            Assert.NotEmpty(viewModel.Alerts.Alerts);

            viewModel.SelectVehiclesCommand.Execute(null);
            viewModel.Vehicles.OpenSelectedVehicleCommand.Execute(null);

            Assert.True(viewModel.IsVehiclesSelected);
            Assert.True(viewModel.Vehicles.IsVehicleHubVisible);
            Assert.True(viewModel.TryNavigateBack());
            Assert.True(viewModel.Vehicles.IsVehicleListVisible);
            Assert.True(viewModel.TryNavigateBack());
            Assert.True(viewModel.IsHomeSelected);
            Assert.False(viewModel.TryNavigateBack());
        }
        finally
        {
            CultureInfo.DefaultThreadCurrentCulture = originalDefaultCulture;
            CultureInfo.DefaultThreadCurrentUICulture = originalDefaultUiCulture;
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    [Fact]
    public void Android_talkback_role_limitation_is_reviewed_for_the_pinned_Avalonia_version()
    {
        var projectPaths = new[]
        {
            RepositoryPath("src", "Vehimap.Desktop", "Vehimap.Desktop.csproj"),
            RepositoryPath("src", "Vehimap.Mobile", "Vehimap.Mobile.csproj"),
            RepositoryPath("src", "Vehimap.Android", "Vehimap.Android.csproj")
        };
        var versions = projectPaths
            .SelectMany(path => XDocument.Load(path).Descendants("PackageReference"))
            .Where(element => (element.Attribute("Include")?.Value ?? string.Empty)
                .StartsWith("Avalonia", StringComparison.Ordinal))
            .Select(element => element.Attribute("Version")?.Value ?? string.Empty)
            .Where(version => !string.IsNullOrWhiteSpace(version))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var version = Assert.Single(versions);
        var accessibilityGuide = File.ReadAllText(RepositoryPath("docs", "ACCESSIBILITY.md"));
        var evidence = File.ReadAllText(RepositoryPath(
            "docs", "accessibility-evidence", "2026-07-13-android-talkback-baseline.md"));

        Assert.Contains($"Last verified package: `Avalonia {version}`", accessibilityGuide, StringComparison.Ordinal);
        Assert.Contains("TalkBack", accessibilityGuide, StringComparison.Ordinal);
        Assert.Contains("must not add application-specific automation peers", accessibilityGuide, StringComparison.Ordinal);
        Assert.Contains("peer.GetAutomationControlType()", evidence, StringComparison.Ordinal);
    }

    [Fact]
    public void Android_nightly_script_validates_package_metadata_and_payload()
    {
        var script = File.ReadAllText(RepositoryPath("build", "Test-DotnetAndroidNightlyReadiness.ps1"));

        Assert.Contains("dotnet workload list", script, StringComparison.Ordinal);
        Assert.Contains("platforms\\android-$targetApi\\android.jar", script, StringComparison.Ordinal);
        Assert.Contains("-p:VehimapReleaseChannel=nightly", script, StringComparison.Ordinal);
        Assert.Contains("*-Signed.apk", script, StringComparison.Ordinal);
        Assert.Contains("aapt2 dump badging", script, StringComparison.Ordinal);
        Assert.Contains("assets/legal/LICENSE", script, StringComparison.Ordinal);
        Assert.Contains("arm64-v8a", script, StringComparison.Ordinal);
        Assert.Contains("x86_64", script, StringComparison.Ordinal);
        Assert.Contains("artifacts\\nightly\\android", script, StringComparison.Ordinal);
        Assert.Contains("Vehimap.apk", script, StringComparison.Ordinal);
    }

    [Fact]
    public void Combined_local_nightly_script_uses_one_version_for_desktop_and_android()
    {
        var script = File.ReadAllText(RepositoryPath("build", "Build-DotnetLocalNightlies.ps1"));

        Assert.Contains("Test-DotnetNightlyMatrix.ps1", script, StringComparison.Ordinal);
        Assert.Contains("Test-DotnetAndroidNightlyReadiness.ps1", script, StringComparison.Ordinal);
        Assert.Contains("EffectiveVersion = $EffectiveVersion", script, StringComparison.Ordinal);
        Assert.Contains("android\\app\\Vehimap.apk", script, StringComparison.Ordinal);
    }

    private static HashSet<string> ReadResourceKeys(string path) =>
        XDocument.Load(path)
            .Descendants("data")
            .Select(element => element.Attribute("name")?.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .ToHashSet(StringComparer.Ordinal);

    private static Dictionary<string, string> ReadResourceValues(string path) =>
        XDocument.Load(path)
            .Descendants("data")
            .Where(element => !string.IsNullOrWhiteSpace(element.Attribute("name")?.Value))
            .ToDictionary(
                element => element.Attribute("name")!.Value,
                element => element.Element("value")?.Value ?? string.Empty,
                StringComparer.Ordinal);

    private static string RepositoryPath(params string[] segments)
    {
        var path = AppContext.BaseDirectory;
        while (!File.Exists(Path.Combine(path, "Vehimap.sln")))
        {
            path = Directory.GetParent(path)?.FullName
                ?? throw new DirectoryNotFoundException("Could not locate the dotnet repository root.");
        }

        return Path.Combine([path, .. segments]);
    }

    private sealed class StubDataStore(VehimapDataSet dataSet) : IVehimapDataStore
    {
        public Task<VehimapDataSet> LoadAsync(VehimapDataRoot dataRoot, CancellationToken cancellationToken = default) =>
            Task.FromResult(dataSet);

        public Task SaveAsync(VehimapDataRoot dataRoot, VehimapDataSet dataSet, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class StubMobileDataRootProvider(VehimapDataRoot dataRoot) : IMobileDataRootProvider
    {
        public VehimapDataRoot GetDataRoot() => dataRoot;
    }

    private sealed class NonMutatingCultureService : IAppCultureService
    {
        private readonly AppCultureService _inner = new();

        public CultureInfo ResolveCulture(string language) => _inner.ResolveCulture(language);

        public Vehimap.Application.Models.AppCulturePreferences Normalize(
            Vehimap.Application.Models.AppCulturePreferences preferences) => _inner.Normalize(preferences);

        public void ApplyThreadCulture(Vehimap.Application.Models.AppCulturePreferences preferences)
        {
        }
    }
}
