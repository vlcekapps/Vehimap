// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Application.Services;

public static class AppCurrencyFormatService
{
    public const string CzechCrowns = "CZK";
    public const string UsDollars = "USD";
    public const string Euros = "EUR";
    public const string BritishPounds = "GBP";

    private static readonly string[] SupportedCurrencies = [CzechCrowns, UsDollars, Euros, BritishPounds];

    public static string NormalizeCurrency(string? currency)
    {
        var normalized = string.IsNullOrWhiteSpace(currency)
            ? CzechCrowns
            : currency.Trim().ToUpperInvariant();
        return SupportedCurrencies.Contains(normalized, StringComparer.Ordinal)
            ? normalized
            : CzechCrowns;
    }

    public static string GetCurrencySymbol(string? currency) =>
        NormalizeCurrency(currency) switch
        {
            UsDollars => "$",
            Euros => "€",
            BritishPounds => "£",
            _ => "Kč"
        };
}
