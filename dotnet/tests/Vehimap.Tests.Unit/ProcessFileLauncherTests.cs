// SPDX-License-Identifier: GPL-3.0-or-later
using System.Diagnostics;
using System.Globalization;
using Vehimap.Application.Services;
using Vehimap.Platform;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class ProcessFileLauncherTests
{
    [Fact]
    public void Build_start_info_uses_shell_execute_on_windows()
    {
        var startInfo = ProcessFileLauncher.BuildStartInfo(@"C:\vehimap\data\doklad.pdf", FileLaunchPlatform.Windows);

        Assert.Equal(@"C:\vehimap\data\doklad.pdf", startInfo.FileName);
        Assert.True(startInfo.UseShellExecute);
        Assert.Empty(startInfo.ArgumentList);
    }

    [Fact]
    public void Build_start_info_uses_open_on_macos()
    {
        var startInfo = ProcessFileLauncher.BuildStartInfo("/Users/test/Vehimap/doklad.pdf", FileLaunchPlatform.MacOS);

        Assert.Equal("open", startInfo.FileName);
        Assert.False(startInfo.UseShellExecute);
        Assert.Equal(["/Users/test/Vehimap/doklad.pdf"], startInfo.ArgumentList);
    }

    [Fact]
    public void Build_start_info_uses_xdg_open_on_linux()
    {
        var startInfo = ProcessFileLauncher.BuildStartInfo("/home/test/Vehimap/doklad.pdf", FileLaunchPlatform.Linux);

        Assert.Equal("xdg-open", startInfo.FileName);
        Assert.False(startInfo.UseShellExecute);
        Assert.Equal(["/home/test/Vehimap/doklad.pdf"], startInfo.ArgumentList);
    }

    [Fact]
    public void Build_folder_start_info_uses_explorer_on_windows()
    {
        var startInfo = ProcessFileLauncher.BuildFolderStartInfo(@"C:\vehimap\data\attachments", FileLaunchPlatform.Windows);

        Assert.Equal("explorer.exe", startInfo.FileName);
        Assert.False(startInfo.UseShellExecute);
        Assert.Equal([@"C:\vehimap\data\attachments"], startInfo.ArgumentList);
    }

    [Fact]
    public void Build_folder_start_info_uses_open_on_macos()
    {
        var startInfo = ProcessFileLauncher.BuildFolderStartInfo("/Users/test/Vehimap/data", FileLaunchPlatform.MacOS);

        Assert.Equal("open", startInfo.FileName);
        Assert.False(startInfo.UseShellExecute);
        Assert.Equal(["/Users/test/Vehimap/data"], startInfo.ArgumentList);
    }

    [Fact]
    public void Build_folder_start_info_uses_xdg_open_on_linux()
    {
        var startInfo = ProcessFileLauncher.BuildFolderStartInfo("/home/test/Vehimap/data", FileLaunchPlatform.Linux);

        Assert.Equal("xdg-open", startInfo.FileName);
        Assert.False(startInfo.UseShellExecute);
        Assert.Equal(["/home/test/Vehimap/data"], startInfo.ArgumentList);
    }

    [Fact]
    public async Task Open_folder_async_uses_injected_process_starter_without_launching_process()
    {
        ProcessStartInfo? capturedStartInfo = null;
        var launcher = new ProcessFileLauncher(
            startInfo => capturedStartInfo = startInfo,
            () => FileLaunchPlatform.Windows);

        await launcher.OpenFolderAsync(@"C:\vehimap\data\attachments\veh_1");

        Assert.NotNull(capturedStartInfo);
        Assert.Equal("explorer.exe", capturedStartInfo!.FileName);
        Assert.Equal([@"C:\vehimap\data\attachments\veh_1"], capturedStartInfo.ArgumentList);
    }

    [Fact]
    public void Build_start_info_rejects_empty_path()
    {
        var error = Assert.Throws<ArgumentException>(() =>
            ProcessFileLauncher.BuildStartInfo(" ", FileLaunchPlatform.Windows));

        Assert.Equal("path", error.ParamName);
    }

    [Fact]
    public void Build_start_info_uses_configured_localizer_for_empty_path()
    {
        var english = new ResourceAppLocalizer(CultureInfo.GetCultureInfo(AppCultureService.EnglishLanguage));

        var error = Assert.Throws<ArgumentException>(() =>
            ProcessFileLauncher.BuildStartInfo(" ", FileLaunchPlatform.Windows, english));

        Assert.Contains("Path to open must not be empty.", error.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("Cesta k otevření", error.Message, StringComparison.Ordinal);
        Assert.Equal("path", error.ParamName);
    }

    [Fact]
    public void Build_start_info_uses_configured_localizer_for_unsupported_platform()
    {
        var english = new ResourceAppLocalizer(CultureInfo.GetCultureInfo(AppCultureService.EnglishLanguage));

        var error = Assert.Throws<PlatformNotSupportedException>(() =>
            ProcessFileLauncher.BuildStartInfo("document.pdf", (FileLaunchPlatform)999, english));

        Assert.Equal("Opening files is not supported on this platform.", error.Message);
    }

    [Fact]
    public void Build_folder_start_info_rejects_empty_path()
    {
        var error = Assert.Throws<ArgumentException>(() =>
            ProcessFileLauncher.BuildFolderStartInfo(" ", FileLaunchPlatform.Windows));

        Assert.Equal("path", error.ParamName);
    }

    [Fact]
    public void Build_folder_start_info_uses_configured_localizer_for_unsupported_platform()
    {
        var english = new ResourceAppLocalizer(CultureInfo.GetCultureInfo(AppCultureService.EnglishLanguage));

        var error = Assert.Throws<PlatformNotSupportedException>(() =>
            ProcessFileLauncher.BuildFolderStartInfo("documents", (FileLaunchPlatform)999, english));

        Assert.Equal("Opening folders is not supported on this platform.", error.Message);
    }
}
