using System.Globalization;
using Vehimap.Application.Models;

namespace Vehimap.Application.Abstractions;

public interface IAppCultureService
{
    CultureInfo ResolveCulture(string language);

    AppCulturePreferences Normalize(AppCulturePreferences preferences);

    void ApplyThreadCulture(AppCulturePreferences preferences);
}
