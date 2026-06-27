using System.Diagnostics;
using Vehimap.Application.Models;

namespace Vehimap.Desktop.Services;

internal sealed class ProcessUpdateInstallLauncher : IUpdateInstallLauncher
{
    public void Launch(UpdateInstallPlan plan)
    {
        var startInfo = BuildStartInfo(plan);
        Process.Start(startInfo);
    }

    internal static ProcessStartInfo BuildStartInfo(UpdateInstallPlan plan)
    {
        if (string.Equals(plan.InstallKind, "installer", StringComparison.OrdinalIgnoreCase))
        {
            var installerPath = string.IsNullOrWhiteSpace(plan.InstallerPath)
                ? plan.UpdaterPath
                : plan.InstallerPath;
            var installerStartInfo = new ProcessStartInfo
            {
                FileName = installerPath,
                UseShellExecute = true,
                WorkingDirectory = Path.GetDirectoryName(installerPath) ?? AppContext.BaseDirectory
            };

            installerStartInfo.ArgumentList.Add("/CLOSEAPPLICATIONS");
            installerStartInfo.ArgumentList.Add("/NORESTARTAPPLICATIONS");
            return installerStartInfo;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = plan.UpdaterPath,
            UseShellExecute = true,
            WorkingDirectory = Path.GetDirectoryName(plan.UpdaterPath) ?? AppContext.BaseDirectory
        };

        startInfo.ArgumentList.Add("--source");
        startInfo.ArgumentList.Add(plan.SourceDirectory);
        startInfo.ArgumentList.Add("--target");
        startInfo.ArgumentList.Add(plan.TargetDirectory);
        startInfo.ArgumentList.Add("--pid");
        startInfo.ArgumentList.Add(plan.ProcessId.ToString());
        startInfo.ArgumentList.Add("--entry");
        startInfo.ArgumentList.Add(plan.EntryPath);

        return startInfo;
    }
}
