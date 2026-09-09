// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Application.Services;

public static class CalendarArithmetic
{
    public static bool TryAddMonths(DateOnly date, int months, out DateOnly result)
    {
        result = default;
        var targetMonth = (long)date.Year * 12 + date.Month - 1 + months;
        if (targetMonth < 12 || targetMonth > 9999 * 12 + 11) return false;
        result = date.AddMonths(months);
        return true;
    }
}
