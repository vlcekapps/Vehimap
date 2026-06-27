using System.Text;
using Vehimap.Application.Models;

namespace Vehimap.Desktop.Services;

internal static class FeedbackIssueUrlBuilder
{
    public const string IssueBaseUrl = "https://github.com/vlcekapps/Vehimap/issues/new";

    public static string Build(
        AppBuildInfo appInfo,
        string dataMode,
        int vehicleCount,
        int auditCount)
    {
        var channel = string.IsNullOrWhiteSpace(appInfo.ReleaseChannel)
            ? "stable"
            : appInfo.ReleaseChannel.Trim().ToLowerInvariant();
        var title = $"{appInfo.ApplicationName}: zpětná vazba k {channel} {appInfo.AppVersion}";
        var body = BuildBody(appInfo, dataMode, vehicleCount, auditCount);

        return $"{IssueBaseUrl}?title={Uri.EscapeDataString(title)}&body={Uri.EscapeDataString(body)}";
    }

    private static string BuildBody(
        AppBuildInfo appInfo,
        string dataMode,
        int vehicleCount,
        int auditCount)
    {
        var builder = new StringBuilder();
        builder.AppendLine("## Co chcete nahlásit nebo navrhnout");
        builder.AppendLine("- ");
        builder.AppendLine();
        builder.AppendLine("## Kroky k reprodukci nebo použití");
        builder.AppendLine("1. ");
        builder.AppendLine("2. ");
        builder.AppendLine("3. ");
        builder.AppendLine();
        builder.AppendLine("## Očekávané chování");
        builder.AppendLine("- ");
        builder.AppendLine();
        builder.AppendLine("## Skutečné chování / proč je to nepohodlné");
        builder.AppendLine("- ");
        builder.AppendLine();
        builder.AppendLine("## Přístupnost");
        builder.AppendLine("- Čtečka obrazovky:");
        builder.AppendLine("- Klávesnice:");
        builder.AppendLine("- Větší písmo nebo DPI:");
        builder.AppendLine();
        builder.AppendLine("## Prostředí");
        builder.AppendLine($"- Aplikace: {appInfo.ApplicationName}");
        builder.AppendLine($"- Verze: {appInfo.AppVersion}");
        builder.AppendLine($"- Kanál: {appInfo.ReleaseChannel}");
        builder.AppendLine($"- Platforma: {appInfo.PlatformDescription}");
        builder.AppendLine($"- Runtime: {appInfo.FrameworkDescription}");
        builder.AppendLine($"- Režim spuštění: {appInfo.RuntimeMode}");
        builder.AppendLine($"- Režim dat: {NormalizeOptionalValue(dataMode)}");
        builder.AppendLine($"- Počet vozidel: {vehicleCount}");
        builder.AppendLine($"- Položky auditu: {auditCount}");
        builder.AppendLine();
        builder.AppendLine("Poznámka: veřejný issue nepředvyplňuje datovou složku ani názvy vozidel. Pokud jsou potřeba, doplňte je prosím ručně jen v rozsahu, který chcete zveřejnit.");

        return builder.ToString();
    }

    private static string NormalizeOptionalValue(string value) =>
        string.IsNullOrWhiteSpace(value) ? "nezjištěno" : value.Trim();
}
