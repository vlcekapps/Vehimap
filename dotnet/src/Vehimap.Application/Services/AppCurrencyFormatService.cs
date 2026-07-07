// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Application.Services;

public static class AppCurrencyFormatService
{
    public const string CzechCrowns = "CZK";
    public const string UsDollars = "USD";
    public const string Euros = "EUR";
    public const string BritishPounds = "GBP";
    public const string DefaultCurrency = CzechCrowns;

    private const string CzechCrownsSymbol = "K\u010D";
    private const string UsDollarsSymbol = "$";
    private const string EurosSymbol = "€";
    private const string BritishPoundsSymbol = "£";

    private static readonly string[] SupportedCurrencies = [CzechCrowns, UsDollars, Euros, BritishPounds];

    public static string NormalizeCurrency(string? currency)
    {
        var normalized = string.IsNullOrWhiteSpace(currency)
            ? DefaultCurrency
            : currency.Trim().ToUpperInvariant();
        return SupportedCurrencies.Contains(normalized, StringComparer.Ordinal)
            ? normalized
            : DefaultCurrency;
    }

    public static string GetCurrencySymbol(string? currency) =>
        NormalizeCurrency(currency) switch
        {
            UsDollars => UsDollarsSymbol,
            Euros => EurosSymbol,
            BritishPounds => BritishPoundsSymbol,
            _ => CzechCrownsSymbol
        };
}
