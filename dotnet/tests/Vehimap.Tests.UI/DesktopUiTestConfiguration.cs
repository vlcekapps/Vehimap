// SPDX-License-Identifier: GPL-3.0-or-later
using System.Net.Http;
using System.Diagnostics;

namespace Vehimap.Tests.UI;

internal sealed record DesktopUiTestConfiguration(
    Uri ServerUri,
    string AppPath,
    TimeSpan CommandTimeout,
    string AutomationName = "Windows",
    bool IsolatedLaunchOnly = false,
    int AppLaunchWaitSeconds = 45)
{
    public bool UsesNovaWindows => AutomationName == "NovaWindows";

    public bool AllowRootWindowFallback => !UsesNovaWindows && !IsolatedLaunchOnly;

    public bool ForceQuitIsolatedApplication => !UsesNovaWindows && IsolatedLaunchOnly;

    internal static int ResolveAppLaunchWaitSeconds(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 45;
        }

        if (int.TryParse(value, out var seconds) && seconds is >= 0 and <= 50)
        {
            return seconds;
        }

        throw new ArgumentException("VEHIMAP_UI_LAUNCH_WAIT_SECONDS must be an integer from 0 to 50.", nameof(value));
    }

    internal static string ResolveAutomationName(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        null or "" or "windows" => "Windows",
        "novawindows" => "NovaWindows",
        _ => throw new ArgumentException("VEHIMAP_UI_AUTOMATION_NAME must be Windows or NovaWindows.", nameof(value))
    };

    public static bool RequireAvailability =>
        string.Equals(Environment.GetEnvironmentVariable("VEHIMAP_UI_REQUIRE_APPIUM"), "1", StringComparison.Ordinal);

    public static bool TryCreate(out DesktopUiTestConfiguration configuration, out string reason)
    {
        configuration = null!;

        if (!OperatingSystem.IsWindows())
        {
            reason = "Appium desktop smoke v této etapě běží jen na Windows.";
            return false;
        }

        var appPath = ResolveAppPath();
        if (string.IsNullOrWhiteSpace(appPath) || !File.Exists(appPath))
        {
            reason = "Chybí publish build Vehimap.exe pro UI test.";
            return false;
        }

        var serverUrl = Environment.GetEnvironmentVariable("VEHIMAP_APPIUM_SERVER_URL");
        if (string.IsNullOrWhiteSpace(serverUrl))
        {
            serverUrl = "http://127.0.0.1:4723/";
        }

        if (!Uri.TryCreate(serverUrl, UriKind.Absolute, out var serverUri))
        {
            reason = "Proměnná VEHIMAP_APPIUM_SERVER_URL neobsahuje platnou URL Appium serveru.";
            return false;
        }

        if (!IsServerReachable(serverUri))
        {
            reason = "Appium server není dostupný.";
            return false;
        }

        var automationName = ResolveAutomationName(Environment.GetEnvironmentVariable("VEHIMAP_UI_AUTOMATION_NAME"));
        var isolatedLaunchOnly = string.Equals(
            Environment.GetEnvironmentVariable("VEHIMAP_UI_ISOLATED_LAUNCH_ONLY"), "1", StringComparison.Ordinal);
        if (isolatedLaunchOnly)
        {
            var runningProcesses = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath));
            try
            {
                if (runningProcesses.Length > 0)
                {
                    reason = $"Close existing Vehimap processes before isolated UI tests (PID: {string.Join(", ", runningProcesses.Select(process => process.Id))}). No running application will be reused or terminated by this preflight.";
                    return false;
                }
            }
            finally
            {
                foreach (var process in runningProcesses)
                {
                    process.Dispose();
                }
            }
        }
        var launchWaitSeconds = ResolveAppLaunchWaitSeconds(Environment.GetEnvironmentVariable("VEHIMAP_UI_LAUNCH_WAIT_SECONDS"));
        configuration = new DesktopUiTestConfiguration(serverUri, appPath, TimeSpan.FromSeconds(90), automationName, isolatedLaunchOnly, launchWaitSeconds);
        reason = string.Empty;
        return true;
    }

    private static string? ResolveAppPath()
    {
        var configuredPath = Environment.GetEnvironmentVariable("VEHIMAP_UI_APP");
        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            return configuredPath;
        }

        var repositoryRoot = FindRepositoryRoot();
        if (string.IsNullOrWhiteSpace(repositoryRoot))
        {
            return null;
        }

        foreach (var channel in new[] { "nightly", "beta", "stable" })
        {
            var channelPath = Path.Combine(repositoryRoot, "dotnet", "artifacts", channel, "win-x64", "app", "Vehimap.exe");
            if (File.Exists(channelPath))
            {
                return channelPath;
            }
        }

        return Path.Combine(repositoryRoot, "dotnet", "artifacts", "desktop-release", "Vehimap.exe");
    }

    private static string? FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var versionFile = Path.Combine(current.FullName, "src", "VERSION");
            var dotnetFolder = Path.Combine(current.FullName, "dotnet");
            if (File.Exists(versionFile) && Directory.Exists(dotnetFolder))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return null;
    }

    private static bool IsServerReachable(Uri serverUri)
    {
        try
        {
            using var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(2)
            };
            using var response = client.GetAsync(new Uri(serverUri, "status")).GetAwaiter().GetResult();
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
