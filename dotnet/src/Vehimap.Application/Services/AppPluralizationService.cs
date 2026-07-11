// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;

namespace Vehimap.Application.Services;

public sealed class AppPluralizationService : IAppPluralizationService
{
    private readonly IAppCultureService _cultureService;

    public AppPluralizationService(IAppCultureService? cultureService = null)
    {
        _cultureService = cultureService ?? new AppCultureService();
    }

    public AppPluralForm SelectForm(int count, AppCulturePreferences preferences)
    {
        var culture = _cultureService.ResolveCulture(preferences.Language);
        if (!string.Equals(culture.TwoLetterISOLanguageName, "cs", StringComparison.OrdinalIgnoreCase))
        {
            return count == 1 ? AppPluralForm.One : AppPluralForm.Other;
        }

        return count switch
        {
            1 => AppPluralForm.One,
            >= 2 and <= 4 => AppPluralForm.Few,
            _ => AppPluralForm.Other
        };
    }

    public string Format(
        IAppLocalizer localizer,
        string resourceKeyPrefix,
        int count,
        params object?[] args) =>
        Format(localizer, new AppCulturePreferences(localizer.Culture.Name), resourceKeyPrefix, count, args);

    public string Format(
        IAppLocalizer localizer,
        AppCulturePreferences preferences,
        string resourceKeyPrefix,
        int count,
        params object?[] args)
    {
        ArgumentNullException.ThrowIfNull(localizer);
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceKeyPrefix);

        var suffix = SelectForm(count, preferences).ToString();
        return localizer.Format($"{resourceKeyPrefix}.{suffix}", args);
    }
}
