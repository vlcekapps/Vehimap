using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using Vehimap.Application.Abstractions;

namespace Vehimap.Platform;

public sealed class PlatformAutostartService : IAutostartService
{
    private const string WindowsShortcutName = "Vehimap Desktop.lnk";
    private const string LinuxDesktopFileName = "vehimap-desktop.desktop";
    private const string MacLaunchAgentFileName = "cz.vlcekapps.vehimap.desktop.plist";

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

    private static void EnableAutostart()
    {
        var command = ResolveLaunchCommand();
        if (OperatingSystem.IsWindows())
        {
            CreateWindowsShortcut(GetAutostartEntryPath(), command);
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
    private static void CreateWindowsShortcut(string shortcutPath, LaunchCommand command)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(shortcutPath)!);

        var shellType = Type.GetTypeFromProgID("WScript.Shell")
            ?? throw new InvalidOperationException("Není dostupný WScript.Shell pro vytvoření zástupce po startu.");
        var shell = Activator.CreateInstance(shellType)
            ?? throw new InvalidOperationException("Nepodařilo se vytvořit WScript.Shell.");
        object? shortcut = null;

        try
        {
            shortcut = shellType.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod, null, shell, [shortcutPath])
                ?? throw new InvalidOperationException("Nepodařilo se vytvořit COM objekt zástupce po startu.");
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

    private static string BuildLinuxDesktopEntryContent(LaunchCommand command)
    {
        var exec = string.Join(' ', new[] { QuoteCommandArgument(command.ExecutablePath) }.Concat(command.Arguments.Select(QuoteCommandArgument)));
        return $$"""
[Desktop Entry]
Type=Application
Version=1.0
Name=Vehimap Desktop
Comment=Vehimap desktop preview
Exec={{exec}}
Terminal=false
X-GNOME-Autostart-enabled=true
""";
    }

    private static string BuildMacLaunchAgentContent(LaunchCommand command)
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
        builder.AppendLine($"  <string>{System.Security.SecurityElement.Escape(Path.GetDirectoryName(command.ExecutablePath) ?? AppContext.BaseDirectory)}</string>");
        builder.AppendLine("</dict>");
        builder.AppendLine("</plist>");
        return builder.ToString();
    }

    private static string QuoteCommandArgument(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "\"\"";
        }

        return value.Contains(' ') || value.Contains('"')
            ? $"\"{value.Replace("\"", "\\\"", StringComparison.Ordinal)}\""
            : value;
    }

    private sealed record LaunchCommand(
        string ExecutablePath,
        IReadOnlyList<string> Arguments);
}
