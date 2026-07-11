// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Models;

namespace Vehimap.Application.Abstractions;

public interface IAppPluralizationService
{
    AppPluralForm SelectForm(int count, AppCulturePreferences preferences);

    string Format(
        IAppLocalizer localizer,
        string resourceKeyPrefix,
        int count,
        params object?[] args);

    string Format(
        IAppLocalizer localizer,
        AppCulturePreferences preferences,
        string resourceKeyPrefix,
        int count,
        params object?[] args);
}
