// SPDX-License-Identifier: GPL-3.0-or-later
using Avalonia.Controls;
using Avalonia.Interactivity;
using Vehimap.Mobile.ViewModels;

namespace Vehimap.Mobile.Views;

public partial class MobileMainView : UserControl
{
    private bool _initialized;
    private TopLevel? _topLevel;

    public MobileMainView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        AttachBackNavigation();

        if (_initialized || DataContext is not MobileMainViewModel viewModel)
        {
            return;
        }

        _initialized = true;
        await viewModel.InitializeAsync();
        MobileHomeNavigationButton.Focus();
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        if (_topLevel is not null)
        {
            _topLevel.BackRequested -= OnBackRequested;
            _topLevel = null;
        }
    }

    private void AttachBackNavigation()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (ReferenceEquals(_topLevel, topLevel))
        {
            return;
        }

        if (_topLevel is not null)
        {
            _topLevel.BackRequested -= OnBackRequested;
        }

        _topLevel = topLevel;
        if (_topLevel is not null)
        {
            _topLevel.BackRequested += OnBackRequested;
        }
    }

    private void OnBackRequested(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MobileMainViewModel viewModel && viewModel.TryNavigateBack())
        {
            e.Handled = true;
        }
    }
}
