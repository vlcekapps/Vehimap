using System.Diagnostics;
using System.Text;
using Avalonia.Threading;
using Vehimap.Application.Abstractions;
using Vehimap.Desktop.Views;

namespace Vehimap.Desktop.Services;

internal sealed class DesktopNotificationService : INotificationService
{
    private readonly IAppBuildInfoProvider _appBuildInfoProvider;

    internal Func<ProcessStartInfo, Process?> ProcessStarter { get; set; } = static startInfo => Process.Start(startInfo);

    public DesktopNotificationService(IAppBuildInfoProvider appBuildInfoProvider)
    {
        _appBuildInfoProvider = appBuildInfoProvider;
    }

    public async Task ShowAsync(string title, string message, CancellationToken cancellationToken = default)
    {
        if (OperatingSystem.IsWindows())
        {
            var startInfo = BuildWindowsNotificationProcessStartInfo(title, message, _appBuildInfoProvider.GetCurrent().ApplicationPath);
            if (TryStartWindowsNotificationProcess(startInfo))
            {
                return;
            }
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var notification = new NotificationWindow(title, message);
            notification.Show();
        }).GetTask().ConfigureAwait(false);
    }

    internal static ProcessStartInfo BuildWindowsNotificationProcessStartInfo(string title, string message, string applicationPath)
    {
        var normalizedTitle = NormalizeBalloonText(title, 63);
        var normalizedMessage = NormalizeBalloonText(message, 255);
        var normalizedPath = applicationPath ?? string.Empty;
        var powershellPath = ResolvePowerShellPath();
        var encodedCommand = EncodePowerShellScript(BuildWindowsNotificationScript(normalizedTitle, normalizedMessage, normalizedPath));

        return new ProcessStartInfo
        {
            FileName = powershellPath,
            Arguments = $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -WindowStyle Hidden -EncodedCommand {encodedCommand}",
            UseShellExecute = false,
            CreateNoWindow = true
        };
    }

    internal static string BuildWindowsNotificationScript(string title, string message, string applicationPath)
    {
        var titlePayload = Convert.ToBase64String(Encoding.UTF8.GetBytes(title));
        var messagePayload = Convert.ToBase64String(Encoding.UTF8.GetBytes(message));
        var pathPayload = Convert.ToBase64String(Encoding.UTF8.GetBytes(applicationPath));

        return $$"""
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
$title = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String('{{titlePayload}}'))
$message = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String('{{messagePayload}}'))
$applicationPath = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String('{{pathPayload}}'))
$notify = New-Object System.Windows.Forms.NotifyIcon
try {
    $icon = $null
    if ($applicationPath -and (Test-Path $applicationPath)) {
        try {
            $icon = [System.Drawing.Icon]::ExtractAssociatedIcon($applicationPath)
        } catch {
            $icon = $null
        }
    }
    if (-not $icon) {
        $icon = [System.Drawing.SystemIcons]::Information
    }
    $notify.Icon = $icon
    $notify.Visible = $true
    $notify.BalloonTipIcon = [System.Windows.Forms.ToolTipIcon]::Info
    $notify.BalloonTipTitle = $title
    $notify.BalloonTipText = $message
    $notify.ShowBalloonTip(6000)
    Start-Sleep -Milliseconds 6500
} finally {
    $notify.Dispose()
}
""";
    }

    internal static string NormalizeBalloonText(string value, int maxLength)
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

    private bool TryStartWindowsNotificationProcess(ProcessStartInfo startInfo)
    {
        try
        {
            return ProcessStarter(startInfo) is not null;
        }
        catch
        {
            return false;
        }
    }

    private static string ResolvePowerShellPath()
    {
        var systemPowerShell = Path.Combine(Environment.SystemDirectory, "WindowsPowerShell", "v1.0", "powershell.exe");
        return File.Exists(systemPowerShell)
            ? systemPowerShell
            : "powershell.exe";
    }

    private static string EncodePowerShellScript(string script)
    {
        var bytes = Encoding.Unicode.GetBytes(script);
        return Convert.ToBase64String(bytes);
    }
}
