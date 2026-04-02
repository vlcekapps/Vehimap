using System.Diagnostics;
using Vehimap.Application.Models;

namespace Vehimap.Desktop.Services;

internal sealed class ProcessUpdateInstallLauncher : IUpdateInstallLauncher
{
    public void Launch(UpdateInstallPlan plan)
    {
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

        Process.Start(startInfo);
    }
}
