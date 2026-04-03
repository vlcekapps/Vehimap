using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Desktop.Services;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class DesktopNotificationServiceTests
{
    [Fact]
    public async Task Show_async_uses_windows_presenter_before_falling_back()
    {
        var provider = new StubBuildInfoProvider();
        var service = new DesktopNotificationService(provider);
        var invoked = false;

        service.WindowsNotificationPresenter = (title, message, _) =>
        {
            invoked = true;
            Assert.Equal("Vehimap upozornění", title);
            Assert.Equal("Propadlá technická kontrola", message);
            return Task.FromResult(true);
        };

        service.InlineNotificationPresenter = (_, _, _) =>
        {
            Assert.Fail("Inline notifikace se nemá použít, pokud se podaří spustit Windows presenter.");
            return Task.CompletedTask;
        };

        await service.ShowAsync("Vehimap upozornění", "Propadlá technická kontrola");

        Assert.True(invoked);
    }

    [Fact]
    public async Task Show_async_falls_back_to_inline_notification_when_windows_presenter_returns_false()
    {
        var provider = new StubBuildInfoProvider();
        var service = new DesktopNotificationService(provider);
        var inlineInvoked = false;

        service.WindowsNotificationPresenter = (_, _, _) => Task.FromResult(false);
        service.InlineNotificationPresenter = (title, message, _) =>
        {
            inlineInvoked = true;
            Assert.Equal("Vehimap upozornění", title);
            Assert.Equal("Propadlá technická kontrola", message);
            return Task.CompletedTask;
        };

        await service.ShowAsync("Vehimap upozornění", "Propadlá technická kontrola");

        Assert.True(inlineInvoked);
    }

    [Fact]
    public void Normalize_notification_text_truncates_and_flattens_multiline_content()
    {
        var value = DesktopNotificationService.NormalizeNotificationText("První řádek\r\nDruhý řádek s delším obsahem", 18);

        Assert.DoesNotContain('\r', value);
        Assert.DoesNotContain('\n', value);
        Assert.True(value.Length <= 18);
    }

    private sealed class StubBuildInfoProvider : IAppBuildInfoProvider
    {
        public AppBuildInfo GetCurrent() => new(
            "Vehimap",
            "1.0.2",
            "1.0.2.0",
            "vývojový Avalonia shell",
            @"C:\vehimap\Vehimap.Desktop.exe",
            "Windows",
            ".NET 10",
            "https://example.com/latest.ini",
            "https://example.com/release",
            @"C:\vehimap\Vehimap.Updater.exe",
            true);
    }
}
