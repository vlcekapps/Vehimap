// SPDX-License-Identifier: GPL-3.0-or-later
using System.Security;
using System.Text.Json;
using Vehimap.Application.Abstractions;

namespace Vehimap.Application.Services;

public static class UserFacingExceptionMessageService
{
    public static string Describe(Exception exception, IAppLocalizer? localizer = null)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var resolvedLocalizer = localizer ?? new ResourceAppLocalizer();
        var effectiveException = exception.GetBaseException();
        var resourceKey = effectiveException switch
        {
            UnauthorizedAccessException or SecurityException => "Error.Detail.AccessDenied",
            FileNotFoundException or DirectoryNotFoundException or DriveNotFoundException => "Error.Detail.FileOrFolderUnavailable",
            HttpRequestException => "Error.Detail.NetworkFailed",
            InvalidDataException or FormatException or JsonException => "Error.Detail.InvalidData",
            IOException => "Error.Detail.FileOperationFailed",
            InvalidOperationException or NotSupportedException => "Error.Detail.OperationFailed",
            _ => "Error.Detail.Unexpected"
        };

        return resolvedLocalizer.GetString(resourceKey);
    }
}
