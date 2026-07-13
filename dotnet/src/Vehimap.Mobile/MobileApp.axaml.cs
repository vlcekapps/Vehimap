// SPDX-License-Identifier: GPL-3.0-or-later
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Vehimap.Mobile.ViewModels;
using Vehimap.Mobile.Views;

namespace Vehimap.Mobile;

public partial class MobileApp : Avalonia.Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IActivityApplicationLifetime activityLifetime)
        {
            activityLifetime.MainViewFactory = CreateMainView;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewLifetime)
        {
            singleViewLifetime.MainView = CreateMainView();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static MobileMainView CreateMainView() =>
        new()
        {
            DataContext = MobileMainViewModel.CreateDefault()
        };
}
