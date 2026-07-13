// SPDX-License-Identifier: GPL-3.0-or-later
using System.Globalization;
using System.Xml.Linq;
using Vehimap.Application.Services;
using Vehimap.Domain.Models;
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
    public void Mobile_startup_focus_uses_a_meaningful_control_for_empty_data()
    {
        var view = File.ReadAllText(RepositoryPath("src", "Vehimap.Mobile", "Views", "MobileMainView.axaml"));
        var codeBehind = File.ReadAllText(RepositoryPath("src", "Vehimap.Mobile", "Views", "MobileMainView.axaml.cs"));

        Assert.Contains("Name=\"MobileReloadButton\"", view, StringComparison.Ordinal);
        Assert.Contains("if (viewModel.HasVehicles)", codeBehind, StringComparison.Ordinal);
        Assert.Contains("MobileVehicleList.Focus();", codeBehind, StringComparison.Ordinal);
        Assert.Contains("MobileReloadButton.Focus();", codeBehind, StringComparison.Ordinal);
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
}
