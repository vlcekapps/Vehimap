// SPDX-License-Identifier: GPL-3.0-or-later
using Avalonia.Controls;
using Vehimap.Mobile.ViewModels;

namespace Vehimap.Mobile.Views;

public partial class MobileMainView : UserControl
{
    private bool _initialized;

    public MobileMainView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_initialized || DataContext is not MobileMainViewModel viewModel)
        {
            return;
        }

        _initialized = true;
        await viewModel.InitializeAsync();
        if (viewModel.HasVehicles)
        {
            MobileVehicleList.Focus();
        }
        else
        {
            MobileReloadButton.Focus();
        }
    }
}
