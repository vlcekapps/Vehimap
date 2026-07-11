// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Models;

namespace Vehimap.Application.Abstractions;

public interface IAppDateFormatService
{
    string FormatDate(DateOnly value, AppCulturePreferences preferences);

    string FormatDateTime(DateTime value, AppCulturePreferences preferences);

    bool TryParseDate(string? text, AppCulturePreferences preferences, out DateOnly value);
}
