// SPDX-License-Identifier: GPL-3.0-or-later
using System.Security.Cryptography;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class BrandingAssetTests
{
    [Fact]
    public async Task Shared_logo_is_embedded_accessibly_and_matches_android_launcher_asset()
    {
        var repositoryRoot = FindRepositoryRoot();
        var logoPath = Path.Combine(repositoryRoot, "vehimap-logo.png");
        var androidIconPath = Path.Combine(
            repositoryRoot,
            "dotnet",
            "src",
            "Vehimap.Android",
            "Resources",
            "drawable",
            "icon.png");
        var desktopProjectPath = Path.Combine(
            repositoryRoot,
            "dotnet",
            "src",
            "Vehimap.Desktop",
            "Vehimap.Desktop.csproj");
        var aboutWindowPath = Path.Combine(
            repositoryRoot,
            "dotnet",
            "src",
            "Vehimap.Desktop",
            "Views",
            "AboutWindow.axaml");
        var gitIgnorePath = Path.Combine(repositoryRoot, ".gitignore");

        Assert.True(File.Exists(logoPath), "The shared Vehimap logo must exist at the repository root.");
        Assert.True(File.Exists(androidIconPath), "The Android launcher asset must exist.");

        var logoHash = await ComputeSha256Async(logoPath);
        var androidIconHash = await ComputeSha256Async(androidIconPath);
        Assert.Equal(logoHash, androidIconHash);

        var desktopProject = await File.ReadAllTextAsync(desktopProjectPath);
        var aboutWindow = await File.ReadAllTextAsync(aboutWindowPath);
        var gitIgnore = await File.ReadAllTextAsync(gitIgnorePath);

        Assert.Contains(
            "<AvaloniaResource Include=\"..\\..\\..\\vehimap-logo.png\" Link=\"Assets\\vehimap-logo.png\" />",
            desktopProject,
            StringComparison.Ordinal);
        Assert.Contains("avares://Vehimap/Assets/vehimap-logo.png", aboutWindow, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.Name=\"{i18n:Loc About.LogoName}\"", aboutWindow, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"AboutLogo\"", aboutWindow, StringComparison.Ordinal);
        Assert.Contains("!vehimap-logo.png", gitIgnore, StringComparison.Ordinal);
    }

    private static async Task<string> ComputeSha256Async(string path)
    {
        await using var stream = File.OpenRead(path);
        var hash = await SHA256.HashDataAsync(stream);
        return Convert.ToHexString(hash);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "README.md")) &&
                Directory.Exists(Path.Combine(directory.FullName, "dotnet")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Repository root could not be located.");
    }
}
