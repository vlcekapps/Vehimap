// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;

namespace Vehimap.Application.Services;

public sealed class AppFileSizeFormatService : IAppFileSizeFormatService
{
    private const decimal BytesPerUnit = 1024m;
    private const string BytesLabel = "B";
    private const string KilobytesLabel = "KB";
    private const string MegabytesLabel = "MB";
    private const string GigabytesLabel = "GB";

    private readonly IAppNumberFormatService _numberFormatService;

    public AppFileSizeFormatService(IAppNumberFormatService? numberFormatService = null)
    {
        _numberFormatService = numberFormatService ?? new AppNumberFormatService();
    }

    public string FormatBytes(long sizeBytes, AppCulturePreferences preferences)
    {
        var bytes = Math.Max(0m, sizeBytes);
        if (bytes < BytesPerUnit)
        {
            return Format(bytes, BytesLabel, preferences, 0);
        }

        var kilobytes = bytes / BytesPerUnit;
        if (kilobytes < BytesPerUnit)
        {
            return Format(kilobytes, KilobytesLabel, preferences, 1);
        }

        var megabytes = kilobytes / BytesPerUnit;
        if (megabytes < BytesPerUnit)
        {
            return Format(megabytes, MegabytesLabel, preferences, 1);
        }

        return Format(megabytes / BytesPerUnit, GigabytesLabel, preferences, 2);
    }

    private string Format(decimal value, string unit, AppCulturePreferences preferences, int decimalPlaces) =>
        $"{_numberFormatService.FormatDecimal(value, preferences, decimalPlaces)} {unit}";
}
