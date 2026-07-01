// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Abstractions;
using Vehimap.Platform;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class DesktopSystemResumeServiceTests
{
    [Theory]
    [MemberData(nameof(CreateServices))]
    public async Task Resume_services_initialize_and_dispose_safely(ISystemResumeService service)
    {
        await using (service.ConfigureAwait(false))
        {
            await service.InitializeAsync();
            await service.InitializeAsync();
        }
    }

    [Fact]
    public void Runtime_controller_schedules_background_refresh_after_system_resume()
    {
        var source = ReadDesktopServiceFile("DesktopAppRuntimeController.cs");

        Assert.Contains("ResumeBackgroundDelay = TimeSpan.FromMilliseconds(1500)", source);
        Assert.Contains("_systemResumeService.Resumed += OnSystemResumed", source);
        Assert.Contains("_systemResumeService.Resumed -= OnSystemResumed", source);
        Assert.Contains("RefreshBackgroundStateAsync(notifyWhenHidden: !_mainWindow.IsVisible, runAutomaticBackup: true)", source);
        Assert.Contains("_resumeRefreshScheduled", source);
    }

    public static IEnumerable<object[]> CreateServices()
    {
        yield return [new NoOpSystemResumeService()];
        yield return [new SystemEventsResumeService()];
    }

    private static string ReadDesktopServiceFile(string fileName)
    {
        var servicesRoot = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "src",
            "Vehimap.Desktop",
            "Services"));

        return File.ReadAllText(Path.Combine(servicesRoot, fileName));
    }
}
