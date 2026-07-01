// SPDX-License-Identifier: GPL-3.0-or-later
using Avalonia;
using Vehimap.Desktop.Services;
using Vehimap.Platform;

namespace Vehimap.Desktop;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var singleInstance = DesktopSingleInstanceCoordinator.Acquire(AssemblyAppBuildInfoProvider.ResolveCurrentReleaseChannel());
        if (!singleInstance.IsPrimary)
        {
            try
            {
                _ = singleInstance.TrySignalExistingInstanceAsync().GetAwaiter().GetResult();
            }
            finally
            {
                singleInstance.Dispose();
            }

            return;
        }

        App.SetSingleInstanceCoordinator(singleInstance);
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
