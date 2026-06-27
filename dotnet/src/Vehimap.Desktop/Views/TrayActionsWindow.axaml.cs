using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Vehimap.Desktop.ViewModels;

namespace Vehimap.Desktop.Views;

public partial class TrayActionsWindow : Window
{
    public TrayActionsDialogAction Result { get; private set; }

    public TrayActionsWindow()
    {
        AvaloniaXamlLoader.Load(this);
        AddHandler(InputElement.KeyDownEvent, OnTrayActionsKeyDown, RoutingStrategies.Tunnel);
        Opened += (_, _) => Dispatcher.UIThread.Post(
            () => this.FindControl<Button>("ShowMainWindowTrayActionButton")?.Focus(),
            DispatcherPriority.Input);
    }

    private void OnShowMainWindowClick(object? sender, RoutedEventArgs e)
    {
        Result = TrayActionsDialogAction.ShowMainWindow;
        Close();
    }

    private void OnOpenBackgroundStatusClick(object? sender, RoutedEventArgs e)
    {
        Result = TrayActionsDialogAction.OpenBackgroundStatus;
        Close();
    }

    private void OnShowDashboardClick(object? sender, RoutedEventArgs e)
    {
        Result = TrayActionsDialogAction.ShowDashboard;
        Close();
    }

    private void OnShowUpcomingOverviewClick(object? sender, RoutedEventArgs e)
    {
        Result = TrayActionsDialogAction.ShowUpcomingOverview;
        Close();
    }

    private void OnShowOverdueOverviewClick(object? sender, RoutedEventArgs e)
    {
        Result = TrayActionsDialogAction.ShowOverdueOverview;
        Close();
    }

    private void OnOpenNearestTechnicalClick(object? sender, RoutedEventArgs e)
    {
        Result = TrayActionsDialogAction.OpenNearestTechnical;
        Close();
    }

    private void OnOpenNearestGreenCardClick(object? sender, RoutedEventArgs e)
    {
        Result = TrayActionsDialogAction.OpenNearestGreenCard;
        Close();
    }

    private void OnOpenNearestReminderClick(object? sender, RoutedEventArgs e)
    {
        Result = TrayActionsDialogAction.OpenNearestReminder;
        Close();
    }

    private void OnOpenNearestMaintenanceClick(object? sender, RoutedEventArgs e)
    {
        Result = TrayActionsDialogAction.OpenNearestMaintenance;
        Close();
    }

    private void OnOpenNearestRecordClick(object? sender, RoutedEventArgs e)
    {
        Result = TrayActionsDialogAction.OpenNearestRecord;
        Close();
    }

    private void OnReviewTechnicalClick(object? sender, RoutedEventArgs e)
    {
        Result = TrayActionsDialogAction.ReviewTechnical;
        Close();
    }

    private void OnReviewGreenCardsClick(object? sender, RoutedEventArgs e)
    {
        Result = TrayActionsDialogAction.ReviewGreenCards;
        Close();
    }

    private void OnReviewRemindersClick(object? sender, RoutedEventArgs e)
    {
        Result = TrayActionsDialogAction.ReviewReminders;
        Close();
    }

    private void OnReviewMaintenanceClick(object? sender, RoutedEventArgs e)
    {
        Result = TrayActionsDialogAction.ReviewMaintenance;
        Close();
    }

    private void OnReviewRecordsClick(object? sender, RoutedEventArgs e)
    {
        Result = TrayActionsDialogAction.ReviewRecords;
        Close();
    }

    private void OnOpenPrintableReportClick(object? sender, RoutedEventArgs e)
    {
        Result = TrayActionsDialogAction.OpenPrintableReport;
        Close();
    }

    private void OnExportBackupClick(object? sender, RoutedEventArgs e)
    {
        Result = TrayActionsDialogAction.ExportBackup;
        Close();
    }

    private void OnImportBackupClick(object? sender, RoutedEventArgs e)
    {
        Result = TrayActionsDialogAction.ImportBackup;
        Close();
    }

    private void OnCreateAutomaticBackupNowClick(object? sender, RoutedEventArgs e)
    {
        Result = TrayActionsDialogAction.CreateAutomaticBackupNow;
        Close();
    }

    private void OnOpenAutomaticBackupFolderClick(object? sender, RoutedEventArgs e)
    {
        Result = TrayActionsDialogAction.OpenAutomaticBackupFolder;
        Close();
    }

    private void OnOpenSettingsClick(object? sender, RoutedEventArgs e)
    {
        Result = TrayActionsDialogAction.OpenSettings;
        Close();
    }

    private void OnExportCalendarClick(object? sender, RoutedEventArgs e)
    {
        Result = TrayActionsDialogAction.ExportCalendar;
        Close();
    }

    private void OnReloadDataClick(object? sender, RoutedEventArgs e)
    {
        Result = TrayActionsDialogAction.ReloadData;
        Close();
    }

    private void OnOpenDataFolderClick(object? sender, RoutedEventArgs e)
    {
        Result = TrayActionsDialogAction.OpenDataFolder;
        Close();
    }

    private void OnOpenAboutClick(object? sender, RoutedEventArgs e)
    {
        Result = TrayActionsDialogAction.OpenAbout;
        Close();
    }

    private void OnThankAuthorClick(object? sender, RoutedEventArgs e)
    {
        Result = TrayActionsDialogAction.ThankAuthor;
        Close();
    }

    private void OnCheckForUpdatesClick(object? sender, RoutedEventArgs e)
    {
        Result = TrayActionsDialogAction.CheckForUpdates;
        Close();
    }

    private void OnExitClick(object? sender, RoutedEventArgs e)
    {
        Result = TrayActionsDialogAction.ExitApplication;
        Close();
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        Result = TrayActionsDialogAction.None;
        Close();
    }

    private void OnTrayActionsKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape || e.KeyModifiers != KeyModifiers.None)
        {
            return;
        }

        e.Handled = true;
        Result = TrayActionsDialogAction.None;
        Close();
    }
}
