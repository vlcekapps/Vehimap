using Microsoft.Win32;
using System.Runtime.Versioning;
using Vehimap.Application.Abstractions;

namespace Vehimap.Platform;

public sealed class SystemEventsResumeService : ISystemResumeService
{
    private bool subscribed;

    public event EventHandler? Resumed;

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (subscribed || !OperatingSystem.IsWindows())
        {
            return Task.CompletedTask;
        }

        try
        {
            SubscribeWindows();
        }
        catch (InvalidOperationException)
        {
            subscribed = false;
        }
        catch (PlatformNotSupportedException)
        {
            subscribed = false;
        }

        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        if (subscribed)
        {
            if (OperatingSystem.IsWindows())
            {
                UnsubscribeWindows();
            }

            subscribed = false;
        }

        return ValueTask.CompletedTask;
    }

    [SupportedOSPlatform("windows")]
    private void SubscribeWindows()
    {
        SystemEvents.PowerModeChanged += OnPowerModeChanged;
        subscribed = true;
    }

    [SupportedOSPlatform("windows")]
    private void UnsubscribeWindows()
    {
        SystemEvents.PowerModeChanged -= OnPowerModeChanged;
    }

    [SupportedOSPlatform("windows")]
    private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
        if (e.Mode == PowerModes.Resume)
        {
            Resumed?.Invoke(this, EventArgs.Empty);
        }
    }
}
