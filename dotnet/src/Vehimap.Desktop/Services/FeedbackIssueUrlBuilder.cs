using System.Text;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;

namespace Vehimap.Desktop.Services;

internal static class FeedbackIssueUrlBuilder
{
    public const string IssueBaseUrl = "https://github.com/vlcekapps/Vehimap/issues/new";

    public static string Build(
        AppBuildInfo appInfo,
        string dataMode,
        int vehicleCount,
        int auditCount,
        IAppLocalizer? localizer = null)
    {
        var effectiveLocalizer = localizer ?? new ResourceAppLocalizer();
        var channel = string.IsNullOrWhiteSpace(appInfo.ReleaseChannel)
            ? "stable"
            : appInfo.ReleaseChannel.Trim().ToLowerInvariant();
        var title = effectiveLocalizer.Format("FeedbackIssue.Title", appInfo.ApplicationName, channel, appInfo.AppVersion);
        var body = BuildBody(appInfo, dataMode, vehicleCount, auditCount, effectiveLocalizer);

        return $"{IssueBaseUrl}?title={Uri.EscapeDataString(title)}&body={Uri.EscapeDataString(body)}";
    }

    private static string BuildBody(
        AppBuildInfo appInfo,
        string dataMode,
        int vehicleCount,
        int auditCount,
        IAppLocalizer localizer)
    {
        var builder = new StringBuilder();
        AppendHeading(builder, localizer.GetString("FeedbackIssue.ReportHeading"));
        builder.AppendLine("- ");
        builder.AppendLine();
        AppendHeading(builder, localizer.GetString("FeedbackIssue.StepsHeading"));
        builder.AppendLine("1. ");
        builder.AppendLine("2. ");
        builder.AppendLine("3. ");
        builder.AppendLine();
        AppendHeading(builder, localizer.GetString("FeedbackIssue.ExpectedHeading"));
        builder.AppendLine("- ");
        builder.AppendLine();
        AppendHeading(builder, localizer.GetString("FeedbackIssue.ActualHeading"));
        builder.AppendLine("- ");
        builder.AppendLine();
        AppendHeading(builder, localizer.GetString("FeedbackIssue.AccessibilityHeading"));
        AppendEmptyListLine(builder, localizer.GetString("FeedbackIssue.ScreenReaderLabel"));
        AppendEmptyListLine(builder, localizer.GetString("FeedbackIssue.KeyboardLabel"));
        AppendEmptyListLine(builder, localizer.GetString("FeedbackIssue.LargeTextLabel"));
        builder.AppendLine();
        AppendHeading(builder, localizer.GetString("FeedbackIssue.SystemHeading"));
        AppendValueListLine(builder, localizer.GetString("FeedbackIssue.AppLabel"), appInfo.ApplicationName);
        AppendValueListLine(builder, localizer.GetString("FeedbackIssue.VersionLabel"), appInfo.AppVersion);
        AppendValueListLine(builder, localizer.GetString("FeedbackIssue.ChannelLabel"), appInfo.ReleaseChannel);
        AppendValueListLine(builder, localizer.GetString("FeedbackIssue.PlatformLabel"), appInfo.PlatformDescription);
        AppendValueListLine(builder, localizer.GetString("FeedbackIssue.FrameworkLabel"), appInfo.FrameworkDescription);
        AppendValueListLine(builder, localizer.GetString("FeedbackIssue.RuntimeModeLabel"), appInfo.RuntimeMode);
        AppendValueListLine(builder, localizer.GetString("FeedbackIssue.DataModeLabel"), NormalizeOptionalValue(dataMode, localizer));
        AppendValueListLine(builder, localizer.GetString("FeedbackIssue.VehicleCountLabel"), vehicleCount.ToString());
        AppendValueListLine(builder, localizer.GetString("FeedbackIssue.AuditCountLabel"), auditCount.ToString());
        builder.AppendLine();
        builder.AppendLine(localizer.GetString("FeedbackIssue.Note"));

        return builder.ToString();
    }

    private static void AppendHeading(StringBuilder builder, string text) =>
        builder.AppendLine("## " + text);

    private static void AppendEmptyListLine(StringBuilder builder, string label) =>
        builder.AppendLine($"- {label}:");

    private static void AppendValueListLine(StringBuilder builder, string label, string value) =>
        builder.AppendLine($"- {label}: {value}");

    private static string NormalizeOptionalValue(string value, IAppLocalizer localizer) =>
        string.IsNullOrWhiteSpace(value)
            ? localizer.GetString("FeedbackIssue.UnknownValue")
            : value.Trim();
}
