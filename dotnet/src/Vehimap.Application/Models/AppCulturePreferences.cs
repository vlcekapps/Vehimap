namespace Vehimap.Application.Models;

public sealed record AppCulturePreferences(
    string Language = "system",
    string ThousandsSeparator = "culture",
    string DecimalSeparator = "culture");
