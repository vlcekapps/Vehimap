using Vehimap.Desktop.Services;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class DesktopSingleInstanceCoordinatorTests
{
    [Fact]
    public void Build_names_are_channel_specific_and_safe()
    {
        var stable = DesktopSingleInstanceCoordinator.BuildNames("stable");
        var beta = DesktopSingleInstanceCoordinator.BuildNames("beta");
        var nightly = DesktopSingleInstanceCoordinator.BuildNames("nightly");
        var custom = DesktopSingleInstanceCoordinator.BuildNames("Unit Test/Channel");

        Assert.NotEqual(stable.MutexName, beta.MutexName);
        Assert.NotEqual(beta.MutexName, nightly.MutexName);
        Assert.Contains("stable", stable.MutexName, StringComparison.Ordinal);
        Assert.Contains("beta", beta.PipeName, StringComparison.Ordinal);
        Assert.Contains("nightly", nightly.PipeName, StringComparison.Ordinal);
        Assert.Contains("unit-test-channel", custom.MutexName, StringComparison.Ordinal);
        Assert.DoesNotContain("\\", stable.MutexName, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Secondary_instance_signals_primary_activation()
    {
        var channel = $"unit-{Guid.NewGuid():N}";
        using var primary = DesktopSingleInstanceCoordinator.Acquire(channel);
        var activated = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        primary.SetActivationHandler(() =>
        {
            activated.TrySetResult();
            return Task.CompletedTask;
        });

        using var secondary = DesktopSingleInstanceCoordinator.Acquire(channel);

        Assert.True(primary.IsPrimary);
        Assert.False(secondary.IsPrimary);
        Assert.True(await secondary.TrySignalExistingInstanceAsync(TimeSpan.FromSeconds(3)));
        await activated.Task.WaitAsync(TimeSpan.FromSeconds(3));
    }

    [Fact]
    public async Task Activation_request_is_kept_until_handler_is_registered()
    {
        var channel = $"unit-{Guid.NewGuid():N}";
        using var primary = DesktopSingleInstanceCoordinator.Acquire(channel);
        using var secondary = DesktopSingleInstanceCoordinator.Acquire(channel);

        Assert.True(await secondary.TrySignalExistingInstanceAsync(TimeSpan.FromSeconds(3)));
        await Task.Delay(150);

        var activated = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        primary.SetActivationHandler(() =>
        {
            activated.TrySetResult();
            return Task.CompletedTask;
        });

        await activated.Task.WaitAsync(TimeSpan.FromSeconds(3));
    }
}
