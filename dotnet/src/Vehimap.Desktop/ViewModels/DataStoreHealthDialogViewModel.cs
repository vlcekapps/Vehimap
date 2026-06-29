using CommunityToolkit.Mvvm.ComponentModel;
using Vehimap.Application.Models;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class DataStoreHealthDialogViewModel : ObservableObject
{
    public DataStoreHealthDialogViewModel(DataStoreHealthReport report)
    {
        Report = report;
        Heading = report.Status switch
        {
            DataStoreHealthStatus.Healthy => "Datová sada 2.0 je v pořádku",
            DataStoreHealthStatus.Warning => "Datová sada 2.0 vyžaduje pozornost",
            _ => "Datová sada 2.0 má problém"
        };
        Summary = report.Summary;
        Details = string.Join(Environment.NewLine, report.Details);
        ClipboardText = report.DiagnosticText;
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
    private string statusMessage = "Výsledek kontroly datové sady je zobrazený. Ctrl+Shift+C zkopíruje diagnostiku.";
}
