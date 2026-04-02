using System.Net.Http;

namespace Vehimap.Tests.UI;

internal sealed record DesktopUiTestConfiguration(Uri ServerUri, string AppPath, TimeSpan CommandTimeout)
{
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
            reason = "Chybí publish build Vehimap.Desktop.exe pro UI test.";
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

        configuration = new DesktopUiTestConfiguration(serverUri, appPath, TimeSpan.FromSeconds(30));
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

        return Path.Combine(repositoryRoot, "dotnet", "artifacts", "desktop-preview", "Vehimap.Desktop.exe");
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
