// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Application.Models;

public sealed record AppBuildInfo(
    string ApplicationName,
    string AppVersion,
    string FileVersion,
    string RuntimeMode,
    string ApplicationPath,
    string PlatformDescription,
    string FrameworkDescription,
    string UpdateManifestUrl,
    string ReleaseNotesUrl,
    string UpdaterPath,
    bool IsPublishedBuild,
    string ReleaseChannel = "stable");
