using System.Diagnostics;
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
    public async Task Open_folder_async_uses_injected_process_starter_without_launching_process()
    {
        ProcessStartInfo? capturedStartInfo = null;
        var launcher = new ProcessFileLauncher(
            startInfo => capturedStartInfo = startInfo,
            () => FileLaunchPlatform.Linux);

        await launcher.OpenFolderAsync("/home/test/Vehimap/data/attachments/veh_1");

        Assert.NotNull(capturedStartInfo);
        Assert.Equal("xdg-open", capturedStartInfo!.FileName);
        Assert.Equal(["/home/test/Vehimap/data/attachments/veh_1"], capturedStartInfo.ArgumentList);
    }

    [Fact]
    public void Build_start_info_rejects_empty_path()
    {
        var error = Assert.Throws<ArgumentException>(() =>
            ProcessFileLauncher.BuildStartInfo(" ", FileLaunchPlatform.Windows));

        Assert.Equal("path", error.ParamName);
    }
}
