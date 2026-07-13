// SPDX-License-Identifier: GPL-3.0-or-later
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Vehimap.Mobile.Services;

namespace Vehimap.Mobile.ViewModels;

public sealed class MobileAlertsViewModel : ObservableObject
{
    private readonly MobileSessionController _session;
    private MobileAlertItemViewModel? _selectedAlert;

    public MobileAlertsViewModel(MobileSessionController session)
    {
        _session = session;
    }

    public ObservableCollection<MobileAlertItemViewModel> Alerts { get; } = [];

    public MobileAlertItemViewModel? SelectedAlert
    {
        get => _selectedAlert;
        set => SetProperty(ref _selectedAlert, value);
    }

    public bool HasAlerts => Alerts.Count > 0;

    public string Heading => L("Mobile.Alerts.Heading");

    public string Intro => L("Mobile.Alerts.Intro");

    public string Summary => string.IsNullOrWhiteSpace(_session.AdvisorSummary.Status)
        ? L("Mobile.Alerts.Empty")
        : _session.AdvisorSummary.Status;

    public string ListName => L("Mobile.Alerts.ListName");

    public string ListHelp => L("Mobile.Alerts.ListHelp");

    public string EmptyText => L("Mobile.Alerts.Empty");

    internal void Refresh()
    {
        var selectedId = SelectedAlert?.Id;
        Alerts.Clear();
        foreach (var item in _session.AdvisorSummary.Items)
        {
            Alerts.Add(new MobileAlertItemViewModel(item, _session.Localizer));
        }

        SelectedAlert = Alerts.FirstOrDefault(item => string.Equals(item.Id, selectedId, StringComparison.Ordinal))
            ?? Alerts.FirstOrDefault();
        OnPropertyChanged(nameof(HasAlerts));
        OnPropertyChanged(nameof(Heading));
        OnPropertyChanged(nameof(Intro));
        OnPropertyChanged(nameof(Summary));
        OnPropertyChanged(nameof(ListName));
        OnPropertyChanged(nameof(ListHelp));
        OnPropertyChanged(nameof(EmptyText));
    }

    private string L(string key) => _session.Localizer.GetString(key);
}
