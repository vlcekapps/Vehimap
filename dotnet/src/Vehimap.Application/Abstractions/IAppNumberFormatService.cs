using System.Globalization;
using Vehimap.Application.Models;

namespace Vehimap.Application.Abstractions;

public interface IAppNumberFormatService
{
    NumberFormatInfo CreateNumberFormat(AppCulturePreferences preferences);

    string FormatDecimal(decimal value, AppCulturePreferences preferences, int decimalPlaces = 2);

    string FormatMoney(decimal value, AppCulturePreferences preferences, string currency, int decimalPlaces = 2);

    bool TryParseDecimal(string text, AppCulturePreferences preferences, out decimal value);
}
