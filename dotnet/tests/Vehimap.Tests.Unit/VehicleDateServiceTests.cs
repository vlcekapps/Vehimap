// SPDX-License-Identifier: GPL-3.0-or-later
using System.Globalization;
using Vehimap.Application;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class VehicleDateServiceTests
{
    [Theory]
    [InlineData(1900)]
    [InlineData(2000)]
    [InlineData(2027)]
    [InlineData(2028)]
    [InlineData(2100)]
    public void Czech_input_accepts_only_real_days_in_every_month(int year)
    {
        for (var month = 1; month <= 12; month++)
        {
            for (var day = 0; day <= 32; day++)
            {
                foreach (var input in new[] { $"{day}.{month}.{year}", $"{day:00}.{month:00}.{year}" })
                {
                    var normalized = VehicleDateService.NormalizeInput(input, new AppCulturePreferences("cs-CZ"));
                    if (day >= 1 && day <= DateTime.DaysInMonth(year, month))
                        Assert.Equal(VehimapValueParser.FormatCanonicalEventDate(new DateOnly(year, month, day)), normalized);
                    else
                        Assert.Equal(string.Empty, normalized);
                }
            }
        }
    }

    [Fact]
    public void Renamed_insurance_filters_still_accept_historical_english_preferences()
    {
        var localizer = new ResourceAppLocalizer(CultureInfo.GetCultureInfo("en-US"));
        Assert.True(LocalizedResourceValueMatcher.MatchesStableValueOrResource(localizer, "Green cards", "green_cards", "Overview.Filter.GreenCards"));
        Assert.True(LocalizedResourceValueMatcher.MatchesStableValueOrResource(localizer, "Only missing green card", "missing_green_card", "VehicleList.FilterOption.MissingGreenCard"));
    }

    [Fact]
    public void Calendar_export_uses_precise_day_instead_of_month_end()
    {
        var data = new VehimapDataSet { Vehicles = [Vehicle("03.07.2027")] };
        var localizer = new ResourceAppLocalizer(CultureInfo.GetCultureInfo("en-US"));
        var timeline = new LegacyTimelineService(localizer);
        var export = new LegacyCalendarExportService(timeline, localizer).BuildUpcomingCalendar(data, new DateOnly(2027, 7, 1), DateTimeOffset.UtcNow);
        Assert.Equal(2, export.Items.Count);
        Assert.All(export.Items, item => Assert.Equal(new DateOnly(2027, 7, 3), item.Date));
        Assert.Contains("DTSTART;VALUE=DATE:20270703", export.IcsContent);
        Assert.DoesNotContain("DTSTART;VALUE=DATE:20270731", export.IcsContent);
        Assert.Contains("Vehicle insurance", export.IcsContent);
    }

    [Theory]
    [InlineData("cs-CZ", "3.7.2027", "03.07.2027")]
    [InlineData("cs-CZ", "03.07.2027", "03.07.2027")]
    [InlineData("cs-CZ", "2027-07-03", "03.07.2027")]
    [InlineData("en-US", "7/3/2027", "03.07.2027")]
    [InlineData("en-US", "03.07.2027", "03.07.2027")]
    [InlineData("en-US", "2027-07-03", "03.07.2027")]
    [InlineData("cs-CZ", "29.2.2028", "29.02.2028")]
    [InlineData("cs-CZ", "29.02.2000", "29.02.2000")]
    [InlineData("cs-CZ", "29.02.1900", "")]
    [InlineData("cs-CZ", "29.2.2100", "")]
    [InlineData("cs-CZ", "30.2.2028", "")]
    [InlineData("cs-CZ", "31.02.2028", "")]
    [InlineData("cs-CZ", "30.02.2027", "")]
    [InlineData("cs-CZ", "31.2.2027", "")]
    [InlineData("cs-CZ", "31.6.2027", "")]
    [InlineData("cs-CZ", "31.9.2027", "")]
    [InlineData("cs-CZ", "31.11.2027", "")]
    [InlineData("en-US", "2/29/2028", "29.02.2028")]
    [InlineData("en-US", "2/29/2027", "")]
    [InlineData("en-US", "2/30/2028", "")]
    [InlineData("en-US", "4/31/2027", "")]
    [InlineData("cs-CZ", " 7.2027 ", "07/2027")]
    [InlineData("en-US", "7/2027", "07/2027")]
    [InlineData("en-US", "2027-07", "07/2027")]
    [InlineData("cs-CZ", "", "")]
    [InlineData("cs-CZ", "29.2.2027", "")]
    [InlineData("cs-CZ", "31.4.2027", "")]
    [InlineData("cs-CZ", "3.7.", "")]
    [InlineData("cs-CZ", "3.7", "")]
    [InlineData("en-US", "7/3", "")]
    [InlineData("en-US", "7/3/27", "")]
    [InlineData("en-US", "13/2027", "")]
    [InlineData("en-US", "1/1/2201", "")]
    public void Normalization_preserves_day_or_month_precision_without_inventing_components(string language, string input, string expected)
    {
        Assert.Equal(expected, VehicleDateService.NormalizeInput(input, new AppCulturePreferences(language)));
    }

    [Theory]
    [InlineData("03.07.2027", "03.07.2027", false)]
    [InlineData("04.07.2027", "03.07.2027", true)]
    [InlineData("07/2027", "03.07.2027", false)]
    [InlineData("31.07.2027", "07/2027", false)]
    [InlineData("08/2027", "31.07.2027", true)]
    [InlineData("", "03.07.2027", false)]
    public void Insurance_range_rejects_only_definitely_reversed_dates(string from, string to, bool invalid) =>
        Assert.Equal(invalid, VehicleDateService.HasInvalidRange(from, to));

    [Theory]
    [InlineData("cs-CZ", "03.07.2027")]
    [InlineData("en-US", "7/3/2027")]
    public void Timeline_uses_exact_inclusive_expiry_day_and_keeps_month_fallback(string language, string expectedDate)
    {
        var localizer = new ResourceAppLocalizer(CultureInfo.GetCultureInfo(language));
        var service = new LegacyTimelineService(localizer);
        service.ApplySupportedSettings(new DesktopSupportedSettingsSnapshot(30, 30, 31, 1000, false, false, false, false, 1, 30, Language: language));
        var data = new VehimapDataSet { Vehicles = [Vehicle("03.07.2027")] };
        data.Vehicles[0] = data.Vehicles[0] with { NextTk = "07/2027" };
        var date = new DateOnly(2027, 7, 3);
        var timeline = service.BuildVehicleTimeline(data, "v", date);
        var insurance = Assert.Single(timeline, item => item.Kind == "green");
        Assert.Equal(date, insurance.Date);
        Assert.Equal(expectedDate, insurance.DateText);
        Assert.Equal(localizer.GetString("Timeline.Status.Today"), insurance.Status);
        Assert.True(insurance.IsFuture);
        Assert.Equal(new DateOnly(2027, 7, 31), Assert.Single(timeline, item => item.Kind == "technical").Date);
        var expired = Assert.Single(service.BuildVehicleTimeline(data, "v", date.AddDays(1)), item => item.Kind == "green");
        Assert.False(expired.IsFuture);
        Assert.Equal(localizer.GetString("Timeline.Status.Overdue"), expired.Status);
        var approaching = Assert.Single(service.BuildVehicleTimeline(data, "v", date.AddDays(-1)), item => item.Kind == "green");
        Assert.False(string.IsNullOrWhiteSpace(approaching.Status));
    }

    [Theory]
    [InlineData("cs-CZ")]
    [InlineData("en-US")]
    public void Audit_recommends_exact_dates_only_for_populated_month_precision_fields(string language)
    {
        var localizer = new ResourceAppLocalizer(CultureInfo.GetCultureInfo(language));
        var audit = new LegacyAuditService(new NoAttachments(), localizer);
        var root = new VehimapDataRoot("unused", "unused", true);
        var data = new VehimapDataSet { Vehicles = [Vehicle("07/2027")] };
        var original = data.Vehicles[0];
        var warnings = audit.BuildAudit(root, data);
        Assert.Equal(4, warnings.Count);
        foreach (var warning in warnings)
        {
            Assert.Equal(AuditSeverity.Warning, warning.Severity);
            Assert.Equal(ApplicationEntityKinds.Vehicle, warning.EntityKind);
            Assert.Equal("v", warning.EntityId);
            Assert.Contains("07/2027", warning.Message);
        }
        Assert.Equal(original, data.Vehicles[0]);
        data.Vehicles[0] = Vehicle("03.07.2027");
        Assert.Empty(audit.BuildAudit(root, data));
        data.Vehicles[0] = Vehicle("") with { NextTk = "03.07.2027" };
        Assert.Empty(audit.BuildAudit(root, data));
        data.Vehicles[0] = Vehicle("bad input") with { NextTk = "03.07.2027" };
        Assert.DoesNotContain(audit.BuildAudit(root, data), item => item.Title.StartsWith(localizer.Format("Audit.Title.MonthPrecisionVehicleDate", ""), StringComparison.Ordinal));
        data.Vehicles[0] = Vehicle("03.07.2027") with { GreenCardFrom = "04.07.2027" };
        Assert.Equal(localizer.GetString("Audit.Title.InvalidGreenCardRange"), Assert.Single(audit.BuildAudit(root, data)).Title);
    }

    private static Vehicle Vehicle(string date) =>
        new("v", "User car", "Osobní vozidla", "User note", "User model", "TEST", "", "", date, date, date, date);

    private sealed class NoAttachments : IFileAttachmentService
    {
        public string ResolveManagedAttachmentPath(VehimapDataRoot root, string path) => throw new NotSupportedException();
    }
}
