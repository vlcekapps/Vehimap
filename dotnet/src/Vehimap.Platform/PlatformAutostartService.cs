// SPDX-License-Identifier: GPL-3.0-or-later
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Services;

namespace Vehimap.Platform;

public sealed class PlatformAutostartService : IAutostartService
{
    private const string WindowsShortcutName = "Vehimap Desktop.lnk";
    private const string LinuxDesktopFileName = "vehimap-desktop.desktop";
    private const string MacLaunchAgentFileName = "cz.vlcekapps.vehimap.desktop.plist";

    private readonly IAppLocalizer _localizer;

    public PlatformAutostartService(IAppLocalizer? localizer = null)
    {
        _localizer = localizer ?? new ResourceAppLocalizer();
    }

    public Task<bool> IsEnabledAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(File.Exists(GetAutostartEntryPath()));
    }

    public Task SetEnabledAsync(bool enabled, CancellationToken cancellationToken = default)
    {
        if (enabled)
        {
            EnableAutostart();
        }
        else
        {
            DisableAutostart();
        }

        return Task.CompletedTask;
    }

    private void EnableAutostart()
    {
        var command = ResolveLaunchCommand();
        if (OperatingSystem.IsWindows())
        {
            CreateWindowsShortcut(GetAutostartEntryPath(), command, _localizer);
            return;
        }

        var entryPath = GetAutostartEntryPath();
        Directory.CreateDirectory(Path.GetDirectoryName(entryPath)!);
        var content = OperatingSystem.IsMacOS()
            ? BuildMacLaunchAgentContent(command)
            : BuildLinuxDesktopEntryContent(command);
        File.WriteAllText(entryPath, content, new UTF8Encoding(false));
    }

    private static void DisableAutostart()
    {
        var entryPath = GetAutostartEntryPath();
        if (File.Exists(entryPath))
        {
            File.Delete(entryPath);
        }
    }

    private static string GetAutostartEntryPath()
    {
        if (OperatingSystem.IsWindows())
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), WindowsShortcutName);
        }

        if (OperatingSystem.IsMacOS())
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(home, "Library", "LaunchAgents", MacLaunchAgentFileName);
        }

        var configHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        if (string.IsNullOrWhiteSpace(configHome))
        {
            configHome = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");
        }

        return Path.Combine(configHome, "autostart", LinuxDesktopFileName);
    }

    private static LaunchCommand ResolveLaunchCommand()
    {
        var appBasePath = AppContext.BaseDirectory;
        var entryAssemblyLocation = Assembly.GetEntryAssembly()?.Location;
        var processPath = Environment.ProcessPath
            ?? entryAssemblyLocation
            ?? Path.Combine(appBasePath, OperatingSystem.IsWindows() ? "Vehimap.Desktop.exe" : "Vehimap.Desktop");

        if (IsDotnetHost(processPath))
        {
            var dllPath = Path.Combine(appBasePath, "Vehimap.Desktop.dll");
            if (File.Exists(dllPath))
            {
                return new LaunchCommand(processPath, [dllPath]);
            }
        }

        var preferredExecutable = Path.Combine(appBasePath, OperatingSystem.IsWindows() ? "Vehimap.Desktop.exe" : "Vehimap.Desktop");
        if (File.Exists(preferredExecutable))
        {
            return new LaunchCommand(preferredExecutable, []);
        }

        return new LaunchCommand(processPath, []);
    }

    private static bool IsDotnetHost(string? processPath)
    {
        if (string.IsNullOrWhiteSpace(processPath))
        {
            return false;
        }

        var fileName = Path.GetFileName(processPath);
        return string.Equals(fileName, "dotnet", StringComparison.OrdinalIgnoreCase)
            || string.Equals(fileName, "dotnet.exe", StringComparison.OrdinalIgnoreCase);
    }

    [SupportedOSPlatform("windows")]
    private static void CreateWindowsShortcut(string shortcutPath, LaunchCommand command, IAppLocalizer localizer)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(shortcutPath)!);

        var shellType = Type.GetTypeFromProgID("WScript.Shell")
            ?? throw new InvalidOperationException(localizer.GetString("Platform.Autostart.WScriptUnavailable"));
        var shell = Activator.CreateInstance(shellType)
            ?? throw new InvalidOperationException(localizer.GetString("Platform.Autostart.WScriptCreateFailed"));
        object? shortcut = null;

        try
        {
            shortcut = shellType.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod, null, shell, [shortcutPath])
                ?? throw new InvalidOperationException(localizer.GetString("Platform.Autostart.ShortcutCreateFailed"));
            var shortcutType = shortcut.GetType();
            shortcutType.InvokeMember("TargetPath", BindingFlags.SetProperty, null, shortcut, [command.ExecutablePath]);
            shortcutType.InvokeMember("Arguments", BindingFlags.SetProperty, null, shortcut, [string.Join(' ', command.Arguments.Select(QuoteCommandArgument))]);
            shortcutType.InvokeMember("WorkingDirectory", BindingFlags.SetProperty, null, shortcut, [Path.GetDirectoryName(command.ExecutablePath) ?? AppContext.BaseDirectory]);
            shortcutType.InvokeMember("Description", BindingFlags.SetProperty, null, shortcut, ["Vehimap Desktop"]);
            shortcutType.InvokeMember("Save", BindingFlags.InvokeMethod, null, shortcut, null);
        }
        finally
        {
            if (shortcut is not null && Marshal.IsComObject(shortcut))
            {
                Marshal.FinalReleaseComObject(shortcut);
            }

            if (Marshal.IsComObject(shell))
            {
                Marshal.FinalReleaseComObject(shell);
            }
        }
    }

    internal static string BuildLinuxDesktopEntryContent(LaunchCommand command)
    {
        var exec = string.Join(' ', new[] { QuoteCommandArgument(command.ExecutablePath) }.Concat(command.Arguments.Select(QuoteCommandArgument)));
        return $$"""
[Desktop Entry]
Type=Application
Version=1.0
Name=Vehimap Desktop
Comment=Vehimap desktop
Exec={{exec}}
Terminal=false
X-GNOME-Autostart-enabled=true
""";
    }

    internal static string BuildMacLaunchAgentContent(LaunchCommand command)
    {
        var builder = new StringBuilder();
        builder.AppendLine("""<?xml version="1.0" encoding="UTF-8"?>""");
        builder.AppendLine("""<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "https://www.apple.com/DTDs/PropertyList-1.0.dtd">""");
        builder.AppendLine("""<plist version="1.0">""");
        builder.AppendLine("<dict>");
        builder.AppendLine("  <key>Label</key>");
        builder.AppendLine("  <string>cz.vlcekapps.vehimap.desktop</string>");
        builder.AppendLine("  <key>ProgramArguments</key>");
        builder.AppendLine("  <array>");
        builder.AppendLine($"    <string>{System.Security.SecurityElement.Escape(command.ExecutablePath)}</string>");
        foreach (var argument in command.Arguments)
        {
            builder.AppendLine($"    <string>{System.Security.SecurityElement.Escape(argument)}</string>");
        }

        builder.AppendLine("  </array>");
        builder.AppendLine("  <key>RunAtLoad</key>");
        builder.AppendLine("  <true/>");
        builder.AppendLine("  <key>WorkingDirectory</key>");
        builder.AppendLine($"  <string>{System.Security.SecurityElement.Escape(GetUnixWorkingDirectory(command.ExecutablePath))}</string>");
        builder.AppendLine("</dict>");
        builder.AppendLine("</plist>");
        return builder.ToString();
    }

    private static string GetUnixWorkingDirectory(string executablePath)
    {
        if (string.IsNullOrWhiteSpace(executablePath))
        {
            return AppContext.BaseDirectory;
        }

        var normalized = executablePath.Replace('\\', '/');
        var lastSeparator = normalized.LastIndexOf('/');
        return lastSeparator switch
        {
            < 0 => AppContext.BaseDirectory,
            0 => "/",
            _ => normalized[..lastSeparator]
        };
    }

    internal static string QuoteCommandArgument(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "\"\"";
        }

        return value.Contains(' ') || value.Contains('"')
            ? $"\"{value.Replace("\"", "\\\"", StringComparison.Ordinal)}\""
            : value;
    }

    internal sealed record LaunchCommand(
        string ExecutablePath,
        IReadOnlyList<string> Arguments);
}
