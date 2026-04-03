using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.Input;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;

namespace Vehimap.Desktop.Services;

internal sealed class AvaloniaTrayService : ITrayService
{
    private readonly IAppBuildInfoProvider _appBuildInfoProvider;
    private TrayIcon? _trayIcon;
    private TrayIcons? _trayIcons;

    public AvaloniaTrayService(IAppBuildInfoProvider appBuildInfoProvider)
    {
        _appBuildInfoProvider = appBuildInfoProvider;
    }

    public bool IsSupported { get; private set; }

    public Task InitializeAsync(TrayServiceConfiguration configuration, CancellationToken cancellationToken = default)
    {
        if (Avalonia.Application.Current is null)
        {
            IsSupported = false;
            return Task.CompletedTask;
        }

        if (_trayIcon is not null)
        {
            _trayIcon.ToolTipText = configuration.ToolTipText;
            _trayIcon.Menu = BuildTrayMenu(configuration);
            IsSupported = true;
            return Task.CompletedTask;
        }

        _trayIcon = new TrayIcon
        {
            ToolTipText = configuration.ToolTipText,
            IsVisible = true,
            Command = new AsyncRelayCommand(configuration.OpenTrayActionsAsync),
            Menu = BuildTrayMenu(configuration)
        };

        var icon = TryLoadIcon();
        if (icon is not null)
        {
            _trayIcon.Icon = icon;
        }

        _trayIcons = [];
        _trayIcons.Add(_trayIcon);
        Avalonia.Application.Current.SetValue(TrayIcon.IconsProperty, _trayIcons);
        IsSupported = true;
        return Task.CompletedTask;
    }

    public Task UpdateToolTipAsync(string toolTipText, CancellationToken cancellationToken = default)
    {
        if (_trayIcon is not null)
        {
            _trayIcon.ToolTipText = toolTipText;
        }

        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        if (_trayIcon is not null)
        {
            _trayIcon.IsVisible = false;
            _trayIcon.Menu = null;
        }

        if (Avalonia.Application.Current is not null)
        {
            Avalonia.Application.Current.SetValue(TrayIcon.IconsProperty, new TrayIcons());
        }

        _trayIcon = null;
        _trayIcons = null;
        IsSupported = false;
        return ValueTask.CompletedTask;
    }

    private static NativeMenu BuildTrayMenu(TrayServiceConfiguration configuration)
    {
        var menu = new NativeMenu();
        menu.Add(new NativeMenuItem("Akce Vehimapu…")
        {
            Command = new AsyncRelayCommand(configuration.OpenTrayActionsAsync)
        });
        menu.Add(new NativeMenuItemSeparator());
        menu.Add(new NativeMenuItem("Zobrazit Vehimap")
        {
            Command = new AsyncRelayCommand(configuration.ShowMainWindowAsync)
        });
        menu.Add(new NativeMenuItem("Otevřít dashboard")
        {
            Command = new AsyncRelayCommand(configuration.ShowDashboardAsync)
        });
        menu.Add(new NativeMenuItemSeparator());
        menu.Add(new NativeMenuItem("Ukončit Vehimap")
        {
            Command = new AsyncRelayCommand(configuration.ExitApplicationAsync)
        });
        return menu;
    }

    private WindowIcon? TryLoadIcon()
    {
        try
        {
            using var stream = AssetLoader.Open(new Uri("avares://Vehimap.Desktop/Assets/vehimap-tray.png"));
            return new WindowIcon(stream);
        }
        catch
        {
            var appPath = _appBuildInfoProvider.GetCurrent().ApplicationPath;
            if (!string.IsNullOrWhiteSpace(appPath) && File.Exists(appPath))
            {
                try
                {
                    return new WindowIcon(appPath);
                }
                catch
                {
                }
            }

            return null;
        }
    }
}
