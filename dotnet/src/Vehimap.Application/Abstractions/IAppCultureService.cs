// SPDX-License-Identifier: GPL-3.0-or-later
using System.Globalization;
using Vehimap.Application.Models;

namespace Vehimap.Application.Abstractions;

public interface IAppCultureService
{
    CultureInfo ResolveCulture(string language);

    AppCulturePreferences Normalize(AppCulturePreferences preferences);

    void ApplyThreadCulture(AppCulturePreferences preferences);
}
