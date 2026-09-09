// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Services;
using Vehimap.Domain.Models;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class CalendarArithmeticSafetyTests
{
    [Theory]
    [InlineData(9999, 12, 1)]
    [InlineData(1, 1, -1)]
    [InlineData(2026, 1, int.MaxValue)]
    [InlineData(2026, 1, int.MinValue)]
    public void Unsupported_month_arithmetic_returns_false_without_overflow(int year, int month, int interval) =>
        Assert.False(CalendarArithmetic.TryAddMonths(new DateOnly(year, month, 1), interval, out _));

    [Fact]
    public void Valid_month_arithmetic_keeps_calendar_end_of_month_behavior()
    {
        Assert.True(CalendarArithmetic.TryAddMonths(new DateOnly(2024, 1, 31), 1, out var date));
        Assert.Equal(new DateOnly(2024, 2, 29), date);
    }

    [Fact]
    public void Extreme_service_interval_does_not_crash_timeline_or_service_book()
    {
        var data = new VehimapDataSet
        {
            Vehicles = [new("v", "Test", "", "", "", "", "", "", "", "", "", "")],
            MaintenancePlans = [new("m", "v", "Service", "", "2147483647", "2026-01-01", "", true, "")]
        };
        Assert.Empty(new LegacyTimelineService().BuildVehicleTimeline(data, "v", new DateOnly(2026, 1, 1)));
        var book = new LegacyServiceBookService().BuildVehicleServiceBook(data, "v", new DateOnly(2026, 1, 1));
        Assert.NotNull(book);
    }
}
