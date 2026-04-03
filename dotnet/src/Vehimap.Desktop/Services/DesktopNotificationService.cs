using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using Vehimap.Application.Abstractions;
using Vehimap.Desktop.Views;

namespace Vehimap.Desktop.Services;

internal sealed class DesktopNotificationService : INotificationService
{
    private const uint NifMessage = 0x00000001;
    private const uint NifIcon = 0x00000002;
    private const uint NifTip = 0x00000004;
    private const uint NifInfo = 0x00000010;

    private const uint NimAdd = 0x00000000;
    private const uint NimModify = 0x00000001;
    private const uint NimDelete = 0x00000002;
    private const uint NimSetVersion = 0x00000004;
    private const uint NotifyIconVersion4 = 4;
    private const uint NiifInfo = 0x00000001;

    private readonly IAppBuildInfoProvider _appBuildInfoProvider;
    private Func<nint>? _hostWindowHandleProvider;
    private uint _notificationId = 1001;

    internal Func<string, string, CancellationToken, Task<bool>> WindowsNotificationPresenter { get; set; }
    internal Func<string, string, CancellationToken, Task> InlineNotificationPresenter { get; set; } =
        ShowVehimapNotificationWindowAsync;

    public DesktopNotificationService(IAppBuildInfoProvider appBuildInfoProvider)
    {
        _appBuildInfoProvider = appBuildInfoProvider;
        WindowsNotificationPresenter = ShowWindowsBalloonAsync;
    }

    public void BindHostWindow(Window window)
    {
        _hostWindowHandleProvider = () => window.TryGetPlatformHandle()?.Handle ?? nint.Zero;
    }

    public async Task ShowAsync(string title, string message, CancellationToken cancellationToken = default)
    {
        if (OperatingSystem.IsWindows())
        {
            var shown = await WindowsNotificationPresenter(
                    NormalizeNotificationText(title, 63),
                    NormalizeNotificationText(message, 255),
                    cancellationToken)
                .ConfigureAwait(false);
            if (shown)
            {
                return;
            }
        }

        await InlineNotificationPresenter(
                NormalizeNotificationText(title, 96),
                NormalizeNotificationText(message, 512),
                cancellationToken)
            .ConfigureAwait(false);
    }

    internal static string NormalizeNotificationText(string value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value
            .Replace("\r\n", " ", StringComparison.Ordinal)
            .Replace('\r', ' ')
            .Replace('\n', ' ')
            .Trim();

        if (normalized.Length <= maxLength)
        {
            return normalized;
        }

        return maxLength <= 1
            ? normalized[..maxLength]
            : normalized[..(maxLength - 1)].TrimEnd() + "…";
    }

    private async Task<bool> ShowWindowsBalloonAsync(string title, string message, CancellationToken cancellationToken)
    {
        var hostHandle = await Dispatcher.UIThread.InvokeAsync(() => _hostWindowHandleProvider?.Invoke() ?? nint.Zero).GetTask();
        if (hostHandle == nint.Zero)
        {
            return false;
        }

        return await Task.Run(async () =>
        {
            var notifyIcon = CreateNotifyIconData(hostHandle, title, message);
            if (!Shell_NotifyIcon(NimAdd, ref notifyIcon))
            {
                return false;
            }

            try
            {
                notifyIcon.uVersionOrTimeout = NotifyIconVersion4;
                Shell_NotifyIcon(NimSetVersion, ref notifyIcon);

                notifyIcon.uFlags = NifInfo;
                if (!Shell_NotifyIcon(NimModify, ref notifyIcon))
                {
                    return false;
                }

                try
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(6500), cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                }

                return true;
            }
            finally
            {
                Shell_NotifyIcon(NimDelete, ref notifyIcon);
            }
        }, cancellationToken).ConfigureAwait(false);
    }

    private NotifyIconData CreateNotifyIconData(nint hostHandle, string title, string message)
    {
        var toolTipText = NormalizeNotificationText(_appBuildInfoProvider.GetCurrent().ApplicationName, 63);
        if (string.IsNullOrWhiteSpace(toolTipText))
        {
            toolTipText = "Vehimap";
        }

        return new NotifyIconData
        {
            cbSize = (uint)Marshal.SizeOf<NotifyIconData>(),
            hWnd = hostHandle,
            uID = _notificationId++,
            uFlags = NifMessage | NifIcon | NifTip,
            hIcon = LoadIcon(nint.Zero, new nint(32512)),
            szTip = toolTipText,
            szInfo = message,
            szInfoTitle = title,
            dwInfoFlags = NiifInfo,
            uVersionOrTimeout = 6500
        };
    }

    private static Task ShowVehimapNotificationWindowAsync(string title, string message, CancellationToken cancellationToken)
    {
        return Dispatcher.UIThread.InvokeAsync(() =>
        {
            var notification = new NotificationWindow(title, message);
            notification.Show();
        }).GetTask();
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern bool Shell_NotifyIcon(uint dwMessage, ref NotifyIconData pnid);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern nint LoadIcon(nint hInstance, nint lpIconName);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NotifyIconData
    {
        public uint cbSize;
        public nint hWnd;
        public uint uID;
        public uint uFlags;
        public uint uCallbackMessage;
        public nint hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szTip;
        public uint dwState;
        public uint dwStateMask;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szInfo;
        public uint uVersionOrTimeout;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string szInfoTitle;
        public uint dwInfoFlags;
        public Guid guidItem;
        public nint hBalloonIcon;
    }
}
