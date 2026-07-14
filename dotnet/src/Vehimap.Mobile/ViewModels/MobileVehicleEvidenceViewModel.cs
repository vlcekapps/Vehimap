// SPDX-License-Identifier: GPL-3.0-or-later
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vehimap.Mobile.Services;

namespace Vehimap.Mobile.ViewModels;

public sealed class MobileVehicleEvidenceViewModel : ObservableObject
{
    private readonly MobileSessionController _session;
    private readonly Action _backToVehicle;
    private MobileVehicleEvidenceItemViewModel? _selectedItem;
    private MobileVehicleEvidenceKind _kind;
    private bool _isDetailVisible;
    private string _vehicleName = string.Empty;
    private string _heading = string.Empty;
    private string _summary = string.Empty;
    private string _listName = string.Empty;
    private string _itemType = string.Empty;
    private string _detailHeading = string.Empty;

    public MobileVehicleEvidenceViewModel(MobileSessionController session, Action backToVehicle)
    {
        _session = session;
        _backToVehicle = backToVehicle;
        BackToVehicleCommand = new RelayCommand(BackToVehicle);
        OpenSelectedItemCommand = new RelayCommand(OpenSelectedItem, () => SelectedItem is not null);
        BackToListCommand = new RelayCommand(BackToList);
    }

    public ObservableCollection<MobileVehicleEvidenceItemViewModel> Items { get; } = [];

    public IRelayCommand BackToVehicleCommand { get; }

    public IRelayCommand OpenSelectedItemCommand { get; }

    public IRelayCommand BackToListCommand { get; }

    public MobileVehicleEvidenceKind Kind => _kind;

    public bool IsActive => Kind != MobileVehicleEvidenceKind.None;

    public bool IsListVisible => IsActive && !IsDetailVisible;

    public bool IsDetailVisible
    {
        get => _isDetailVisible;
        private set
        {
            if (SetProperty(ref _isDetailVisible, value))
            {
                OnPropertyChanged(nameof(IsListVisible));
            }
        }
    }

    public bool HasItems => Items.Count > 0;

    public MobileVehicleEvidenceItemViewModel? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (SetProperty(ref _selectedItem, value))
            {
                OnPropertyChanged(nameof(CanOpenSelectedItem));
                OpenSelectedItemCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool CanOpenSelectedItem => SelectedItem is not null;

    public string Heading => _heading;

    public string Summary => _summary;

    public string ListName => _listName;

    public string DetailHeading => _detailHeading;

    public string BackText => L("Mobile.Common.Back");

    public string BackToVehicleName => LF("Mobile.Evidence.BackToVehicleName", _vehicleName);

    public string BackToListName => LF("Mobile.Evidence.BackToListName", Heading);

    public string OpenSelectedText => L("Mobile.Evidence.OpenSelected");

    public string OpenSelectedName => LF("Mobile.Evidence.OpenSelectedName", _itemType);

    internal void Load(
        MobileVehicleEvidenceProjection projection,
        string vehicleName,
        string? selectedItemId = null,
        bool showDetail = false)
    {
        _kind = projection.Kind;
        _vehicleName = vehicleName;
        _heading = projection.Heading;
        _summary = projection.Summary;
        _listName = projection.ListName;
        _itemType = projection.ItemType;
        _detailHeading = projection.DetailHeading;

        Items.Clear();
        foreach (var item in projection.Items)
        {
            Items.Add(item);
        }

        SelectedItem = Items.FirstOrDefault(item => string.Equals(item.Id, selectedItemId, StringComparison.OrdinalIgnoreCase))
            ?? Items.FirstOrDefault();
        IsDetailVisible = showDetail && SelectedItem is not null;
        RaiseAllProperties();
    }

    internal void Clear()
    {
        _kind = MobileVehicleEvidenceKind.None;
        _vehicleName = string.Empty;
        _heading = string.Empty;
        _summary = string.Empty;
        _listName = string.Empty;
        _itemType = string.Empty;
        _detailHeading = string.Empty;
        Items.Clear();
        SelectedItem = null;
        IsDetailVisible = false;
        RaiseAllProperties();
    }

    public bool TryNavigateBack()
    {
        if (!IsDetailVisible)
        {
            return false;
        }

        BackToList();
        return true;
    }

    private void OpenSelectedItem()
    {
        if (SelectedItem is not null)
        {
            IsDetailVisible = true;
        }
    }

    private void BackToList() => IsDetailVisible = false;

    private void BackToVehicle() => _backToVehicle();

    private void RaiseAllProperties()
    {
        foreach (var propertyName in new[]
        {
            nameof(Kind),
            nameof(IsActive),
            nameof(IsListVisible),
            nameof(IsDetailVisible),
            nameof(HasItems),
            nameof(Heading),
            nameof(Summary),
            nameof(ListName),
            nameof(DetailHeading),
            nameof(BackText),
            nameof(BackToVehicleName),
            nameof(BackToListName),
            nameof(OpenSelectedText),
            nameof(OpenSelectedName)
        })
        {
            OnPropertyChanged(propertyName);
        }
    }

    private string L(string key) => _session.Localizer.GetString(key);

    private string LF(string key, params object?[] args) => _session.Localizer.Format(key, args);
}
