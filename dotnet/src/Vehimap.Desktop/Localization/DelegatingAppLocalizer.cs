// SPDX-License-Identifier: GPL-3.0-or-later
using System.Globalization;
using Vehimap.Application.Abstractions;

namespace Vehimap.Desktop.Localization;

internal sealed class DelegatingAppLocalizer(Func<IAppLocalizer> localizerProvider) : IAppLocalizer
{
    public CultureInfo Culture => localizerProvider().Culture;

    public string GetString(string key) =>
        localizerProvider().GetString(key);

    public string Format(string key, params object?[] args) =>
        localizerProvider().Format(key, args);
}
