// SPDX-License-Identifier: GPL-3.0-or-later
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class DeveloperEnvironmentDocumentationTests
{
    [Fact]
    public void User_readmes_document_self_contained_requirements_for_all_desktop_targets()
    {
        var root = FindRepositoryRoot();
        var czech = File.ReadAllText(Path.Combine(root, "README.md"));
        var english = File.ReadAllText(Path.Combine(root, "README.en-US.md"));

        Assert.Contains("self-contained", czech, StringComparison.Ordinal);
        Assert.Contains("Běžný uživatel proto neinstaluje .NET SDK", czech, StringComparison.Ordinal);
        Assert.Contains("Windows 10 22H2 x64", czech, StringComparison.Ordinal);
        Assert.Contains("macOS 14 Sonoma", czech, StringComparison.Ordinal);
        Assert.Contains("libx11-6 libice6 libsm6 libfontconfig1 xdg-utils", czech, StringComparison.Ordinal);
        Assert.Contains("libX11 libICE libSM fontconfig xdg-utils", czech, StringComparison.Ordinal);
        Assert.Contains("libx11 libice libsm fontconfig xdg-utils", czech, StringComparison.Ordinal);

        Assert.Contains("self-contained", english, StringComparison.Ordinal);
        Assert.Contains("Regular users do not need to install the .NET SDK", english, StringComparison.Ordinal);
        Assert.Contains("Windows 10 22H2 x64", english, StringComparison.Ordinal);
        Assert.Contains("macOS 14 Sonoma", english, StringComparison.Ordinal);
        Assert.Contains("linux-x64", english, StringComparison.Ordinal);
        Assert.Contains("dotnet/docs/DEVELOPMENT.md", english, StringComparison.Ordinal);
    }

    [Fact]
    public void Development_guide_matches_current_sdk_and_release_targets()
    {
        var root = FindRepositoryRoot();
        var guide = File.ReadAllText(Path.Combine(root, "dotnet", "docs", "DEVELOPMENT.md"));
        var globalJson = File.ReadAllText(Path.Combine(root, "dotnet", "global.json"));
        var buildProps = File.ReadAllText(Path.Combine(root, "dotnet", "Directory.Build.props"));

        Assert.Contains(".NET 10 SDK", guide, StringComparison.Ordinal);
        Assert.Contains("PowerShell 7", guide, StringComparison.Ordinal);
        Assert.Contains("win-x64", guide, StringComparison.Ordinal);
        Assert.Contains("linux-x64", guide, StringComparison.Ordinal);
        Assert.Contains("osx-x64", guide, StringComparison.Ordinal);
        Assert.Contains("osx-arm64", guide, StringComparison.Ordinal);
        Assert.Contains("Inno Setup 7", guide, StringComparison.Ordinal);
        Assert.Contains("WinAppDriver 1.2.1", guide, StringComparison.Ordinal);
        Assert.Contains("dotnet workload install android", guide, StringComparison.Ordinal);
        Assert.Contains("Vehimap.Android.sln", guide, StringComparison.Ordinal);
        Assert.Contains("API 36", guide, StringComparison.Ordinal);
        Assert.Contains("JDK 21", guide, StringComparison.Ordinal);
        Assert.Contains("Build-DotnetLocalNightlies.ps1", guide, StringComparison.Ordinal);
        Assert.Contains("dotnet workload install ios", guide, StringComparison.Ordinal);
        Assert.Contains("\"version\": \"10.0.100\"", globalJson, StringComparison.Ordinal);
        Assert.Contains("<TargetFramework>net10.0</TargetFramework>", buildProps, StringComparison.Ordinal);
    }

    [Fact]
    public void Developer_environment_checker_covers_required_and_optional_platform_tools()
    {
        var root = FindRepositoryRoot();
        var script = File.ReadAllText(Path.Combine(root, "dotnet", "build", "Test-DotnetDeveloperEnvironment.ps1"));

        Assert.Contains("PowerShell 7 or later is required", script, StringComparison.Ordinal);
        Assert.Contains("dotnet --list-sdks", script, StringComparison.Ordinal);
        Assert.Contains("libX11.so.6", script, StringComparison.Ordinal);
        Assert.Contains("libICE.so.6", script, StringComparison.Ordinal);
        Assert.Contains("libSM.so.6", script, StringComparison.Ordinal);
        Assert.Contains("libfontconfig.so.1", script, StringComparison.Ordinal);
        Assert.Contains("Find-InnoSetupCompiler", script, StringComparison.Ordinal);
        Assert.Contains("Find-WinAppDriver", script, StringComparison.Ordinal);
        Assert.Contains("xcode-select", script, StringComparison.Ordinal);
        Assert.Contains("IncludeReleaseTools", script, StringComparison.Ordinal);
        Assert.Contains("IncludeWindowsUiTools", script, StringComparison.Ordinal);
        Assert.Contains("IncludeAndroidTools", script, StringComparison.Ordinal);
        Assert.Contains("Android SDK with platform API 36", script, StringComparison.Ordinal);
        Assert.Contains("JDK 21", script, StringComparison.Ordinal);
        Assert.Contains("dotnet workload install android", script, StringComparison.Ordinal);
        Assert.Contains("${Description}: $path", script, StringComparison.Ordinal);
        Assert.DoesNotContain("$Description: $path", script, StringComparison.Ordinal);
        Assert.DoesNotContain("$isWindows =", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("$isLinux =", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("$isMacOS =", script, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Contributing_guide_links_to_the_platform_setup_and_project_quality_rules()
    {
        var root = FindRepositoryRoot();
        var guide = File.ReadAllText(Path.Combine(root, "CONTRIBUTING.md"));

        Assert.Contains("dotnet/docs/DEVELOPMENT.md", guide, StringComparison.Ordinal);
        Assert.Contains("Commit messages are written in English", guide, StringComparison.Ordinal);
        Assert.Contains("English and Czech `.resx`", guide, StringComparison.Ordinal);
        Assert.Contains("ACCESSIBILITY.md", guide, StringComparison.Ordinal);
        Assert.Contains("GPL-3.0-or-later", guide, StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "README.md")) &&
                File.Exists(Path.Combine(current.FullName, "dotnet", "Vehimap.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Repository root was not found.");
    }
}
