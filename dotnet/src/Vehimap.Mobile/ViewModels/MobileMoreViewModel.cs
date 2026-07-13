// SPDX-License-Identifier: GPL-3.0-or-later
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vehimap.Mobile.Services;

namespace Vehimap.Mobile.ViewModels;

public sealed class MobileMoreViewModel : ObservableObject
{
    private readonly MobileSessionController _session;

    public MobileMoreViewModel(MobileSessionController session, IAsyncRelayCommand reloadCommand)
    {
        _session = session;
        ReloadCommand = reloadCommand;
    }

    public IAsyncRelayCommand ReloadCommand { get; }

    public string Heading => L("Mobile.More.Heading");

    public string Intro => L("Mobile.More.Intro");

    public string ReadOnlyText => L("Mobile.Shell.ReadOnly");

    public string DataSetHeading => L("Mobile.More.DataSetHeading");

    public string DataPathLabel => L("Mobile.More.DataPathLabel");

    public string DataPath => _session.DataRoot.DataPath;

    public string DataModeLabel => L("Mobile.More.DataModeLabel");

    public string DataMode => L("Mobile.More.DataModeSystem");

    public string ReloadText => L("Mobile.Action.Reload");

    public string ReloadName => L("Mobile.Action.ReloadName");

    internal void Refresh()
    {
        OnPropertyChanged(nameof(Heading));
        OnPropertyChanged(nameof(Intro));
        OnPropertyChanged(nameof(ReadOnlyText));
        OnPropertyChanged(nameof(DataSetHeading));
        OnPropertyChanged(nameof(DataPathLabel));
        OnPropertyChanged(nameof(DataPath));
        OnPropertyChanged(nameof(DataModeLabel));
        OnPropertyChanged(nameof(DataMode));
        OnPropertyChanged(nameof(ReloadText));
        OnPropertyChanged(nameof(ReloadName));
    }

    private string L(string key) => _session.Localizer.GetString(key);
}
