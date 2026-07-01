// SPDX-License-Identifier: GPL-3.0-or-later
using CommunityToolkit.Mvvm.ComponentModel;
using Vehimap.Application.Models;
using Vehimap.Desktop.Localization;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class DataStoreHealthDialogViewModel : ObservableObject
{
    public DataStoreHealthDialogViewModel(DataStoreHealthReport report)
    {
        Report = report;
        Heading = report.Status switch
        {
            DataStoreHealthStatus.Healthy => L("DataStoreHealth.HeadingHealthy"),
            DataStoreHealthStatus.Warning => L("DataStoreHealth.HeadingWarning"),
            _ => L("DataStoreHealth.HeadingError")
        };
        Summary = report.Summary;
        Details = string.Join(Environment.NewLine, report.Details);
        ClipboardText = BuildDiagnosticText(report);
        CanOpenDataFolder = Directory.Exists(report.DataPath);
        CanOpenPreMigrationBackupFolder = !string.IsNullOrWhiteSpace(report.PreMigrationBackupPath)
            && Directory.Exists(report.PreMigrationBackupPath);
    }

    public DataStoreHealthReport Report { get; }

    public string Heading { get; }

    public string Summary { get; }

    public string Details { get; }

    public string ClipboardText { get; }

    public bool CanOpenDataFolder { get; }

    public bool CanOpenPreMigrationBackupFolder { get; }

    [ObservableProperty]
    private string statusMessage = L("DataStoreHealth.InitialStatus");

    private static string L(string key) => DesktopLocalization.Localizer.GetString(key);

    private static string LF(string key, params object?[] args) => DesktopLocalization.Localizer.Format(key, args);

    private static string BuildDiagnosticText(DataStoreHealthReport report) =>
        string.Join(
            Environment.NewLine,
            new[]
            {
                L("DataStoreHealth.Diagnostics.Title"),
                LF("DataStoreHealth.Diagnostics.Status", report.Status),
                LF("DataStoreHealth.Diagnostics.Summary", report.Summary),
                LF("DataStoreHealth.Diagnostics.DatabasePath", report.DatabasePath),
                LF("DataStoreHealth.Diagnostics.DataPath", report.DataPath),
                LF("DataStoreHealth.Diagnostics.PreMigrationBackupPath", report.PreMigrationBackupPath ?? string.Empty),
                L("DataStoreHealth.Diagnostics.Details")
            }.Concat(report.Details.Select(item => LF("DataStoreHealth.Diagnostics.DetailItem", item))));
}
