// SPDX-License-Identifier: GPL-3.0-or-later
using System.Globalization;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Domain.Models;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class LegacyFuelAnalysisServiceTests
{
    [Fact]
    public void BuildVehicleFuelAnalysis_calculates_consumption_between_full_tanks()
    {
        var service = new LegacyFuelAnalysisService();
        var dataSet = new VehimapDataSet
        {
            FuelEntries =
            [
                new FuelEntry("fuel_1", "veh_1", "01.01.2026", "10000", "50", "2000", true, "Natural 95", "", "FuelSave", "Shell"),
                new FuelEntry("fuel_2", "veh_1", "10.01.2026", "10200", "20", "900", false, "Natural 95", "", "FuelSave", "Shell"),
                new FuelEntry("fuel_3", "veh_1", "20.01.2026", "10500", "30", "1500", true, "Natural 95", "", "FuelSave", "Shell")
            ]
        };

        var summary = service.BuildVehicleFuelAnalysis(dataSet, "veh_1");

        var segment = Assert.Single(summary.ConsumptionSegments);
        Assert.Equal("fuel_1", segment.StartFuelEntryId);
        Assert.Equal("fuel_3", segment.EndFuelEntryId);
        Assert.Equal(500, segment.DistanceKm);
        Assert.Equal(50m, segment.Liters);
        Assert.Equal(10m, segment.ConsumptionLitersPer100Km);
        Assert.Equal(48m, segment.PricePerLiter);
        Assert.Equal(10m, summary.AverageConsumptionLitersPer100Km);
        Assert.Equal(100m, summary.TotalLiters);
        Assert.Equal(4400m, summary.TotalCost);
        Assert.Contains("1 použitelného úseku", summary.Status, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildVehicleFuelAnalysis_preserves_decimal_liters_in_consumption_and_price()
    {
        var service = new LegacyFuelAnalysisService();
        var dataSet = new VehimapDataSet
        {
            FuelEntries =
            [
                new FuelEntry("fuel_1", "veh_1", "01.01.2026", "10000", "3.12", "156", true, "Natural 95", "", "", "Test"),
                new FuelEntry("fuel_2", "veh_1", "10.01.2026", "10100", "3.1", "155", true, "Natural 95", "", "", "Test")
            ]
        };

        var summary = service.BuildVehicleFuelAnalysis(dataSet, "veh_1");

        var segment = Assert.Single(summary.ConsumptionSegments);
        Assert.Equal(3.1m, segment.Liters);
        Assert.Equal(3.1m, segment.ConsumptionLitersPer100Km);
        Assert.Equal(50m, segment.PricePerLiter);
        Assert.Equal(6.22m, summary.TotalLiters);
        Assert.Equal(50m, summary.AveragePricePerLiter);
    }

    [Fact]
    public void BuildVehicleFuelAnalysis_groups_by_station_fuel_and_detail()
    {
        var service = new LegacyFuelAnalysisService();
        var dataSet = new VehimapDataSet
        {
            FuelEntries =
            [
                new FuelEntry("fuel_1", "veh_1", "01.01.2026", "10000", "40", "1800", true, "Natural 95", "", "FuelSave", "Shell Brno"),
                new FuelEntry("fuel_2", "veh_1", "10.01.2026", "10400", "35", "1575", true, "Natural 95", "", "FuelSave", "Shell Brno"),
                new FuelEntry("fuel_3", "veh_1", "20.01.2026", "10800", "30", "1350", true, "Natural 98", "", "V-Power", "Shell Praha")
            ]
        };

        var summary = service.BuildVehicleFuelAnalysis(dataSet, "veh_1");

        Assert.Equal(2, summary.GroupSummaries.Count);
        var shellBrno = summary.GroupSummaries.Single(item => item.Station == "Shell Brno");
        Assert.Equal("Natural 95", shellBrno.FuelType);
        Assert.Equal("FuelSave", shellBrno.FuelDetail);
        Assert.Equal(2, shellBrno.EntryCount);
        Assert.Equal(75m, shellBrno.Liters);
        Assert.Equal(3375m, shellBrno.TotalCost);
        Assert.Equal(45m, shellBrno.AveragePricePerLiter);
        Assert.Equal("fuel_2", shellBrno.LatestFuelEntryId);
    }

    [Fact]
    public void BuildVehicleFuelAnalysis_explains_unavailable_consumption_without_full_tanks()
    {
        var service = new LegacyFuelAnalysisService();
        var dataSet = new VehimapDataSet
        {
            FuelEntries =
            [
                new FuelEntry("fuel_1", "veh_1", "01.01.2026", "10000", "40", "1800", false, "Nafta", "")
            ]
        };

        var summary = service.BuildVehicleFuelAnalysis(dataSet, "veh_1");

        Assert.Empty(summary.ConsumptionSegments);
        Assert.Null(summary.AverageConsumptionLitersPer100Km);
        Assert.Contains("Spotřebu zatím nejde spočítat", summary.Status, StringComparison.Ordinal);
        Assert.Contains(summary.Warnings, item =>
            item.Severity == FuelAnalysisWarningSeverity.Info
            && item.Title == "Spotřeba zatím není dostupná");
    }

    [Fact]
    public void BuildVehicleFuelAnalysis_uses_localized_domain_messages()
    {
        var service = new LegacyFuelAnalysisService(new ResourceAppLocalizer(CultureInfo.GetCultureInfo("en-US")));
        var dataSet = new VehimapDataSet
        {
            FuelEntries =
            [
                new FuelEntry("fuel_1", "veh_1", "01.01.2026", "bad", "many", "expensive", false, "Diesel", "")
            ]
        };

        var summary = service.BuildVehicleFuelAnalysis(dataSet, "veh_1");

        Assert.Contains("Consumption cannot be calculated", summary.Status, StringComparison.Ordinal);
        Assert.Contains(summary.Warnings, item =>
            item.FuelEntryId == "fuel_1"
            && item.Title == "Odometer cannot be used"
            && item.Description.Contains("non-numeric", StringComparison.Ordinal));
        Assert.Contains(summary.Warnings, item =>
            item.Severity == FuelAnalysisWarningSeverity.Info
            && item.Title == "Consumption is not available yet");
    }

    [Fact]
    public void BuildVehicleFuelAnalysis_formats_regression_odometer_with_selected_distance_unit()
    {
        var service = new LegacyFuelAnalysisService(new ResourceAppLocalizer(CultureInfo.GetCultureInfo("en-US")));
        service.ApplySupportedSettings(new DesktopSupportedSettingsSnapshot(
            30,
            30,
            31,
            1000,
            false,
            false,
            false,
            false,
            1,
            30,
            "en-US",
            "comma",
            "dot",
            "mi",
            "us_gal",
            "USD"));
        var dataSet = new VehimapDataSet
        {
            FuelEntries =
            [
                new FuelEntry("fuel_1", "veh_1", "01.01.2026", "1609", "30", "1200", true, "Diesel", ""),
                new FuelEntry("fuel_2", "veh_1", "02.01.2026", "805", "20", "1000", true, "Diesel", "")
            ]
        };

        var summary = service.BuildVehicleFuelAnalysis(dataSet, "veh_1");

        var warning = Assert.Single(summary.Warnings.Where(item => item.Id == "fuel-analysis-odometer-regression-fuel_2"));
        Assert.Contains("500 mi", warning.Description, StringComparison.Ordinal);
        Assert.Contains("1,000 mi", warning.Description, StringComparison.Ordinal);
        Assert.DoesNotContain("805 km", warning.Description, StringComparison.Ordinal);
        Assert.DoesNotContain("1609 km", warning.Description, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildVehicleFuelAnalysis_warns_about_invalid_values_and_odometer_regression()
    {
        var service = new LegacyFuelAnalysisService();
        var dataSet = new VehimapDataSet
        {
            FuelEntries =
            [
                new FuelEntry("fuel_1", "veh_1", "01.01.2026", "1000", "30", "1200", true, "Nafta", ""),
                new FuelEntry("fuel_2", "veh_1", "02.01.2026", "abc", "hodně", "drahé", true, "Nafta", ""),
                new FuelEntry("fuel_3", "veh_1", "03.01.2026", "900", "20", "1000", true, "Nafta", "")
            ]
        };

        var summary = service.BuildVehicleFuelAnalysis(dataSet, "veh_1");

        Assert.Contains(summary.Warnings, item => item.FuelEntryId == "fuel_2" && item.Title == "Tachometr nejde použít");
        Assert.Contains(summary.Warnings, item => item.FuelEntryId == "fuel_2" && item.Title == "Množství paliva nejde použít");
        Assert.Contains(summary.Warnings, item => item.FuelEntryId == "fuel_2" && item.Title == "Cena tankování nejde použít");
        Assert.Contains(summary.Warnings, item => item.FuelEntryId == "fuel_3" && item.Title == "Tachometr v čase klesá");
    }

    [Fact]
    public void BuildVehicleFuelAnalysis_reports_consumption_outliers_only_after_enough_segments()
    {
        var service = new LegacyFuelAnalysisService();
        var dataSet = new VehimapDataSet
        {
            FuelEntries =
            [
                new FuelEntry("fuel_1", "veh_1", "01.01.2026", "1000", "40", "1600", true, "Natural 95", ""),
                new FuelEntry("fuel_2", "veh_1", "05.01.2026", "1500", "40", "1600", true, "Natural 95", ""),
                new FuelEntry("fuel_3", "veh_1", "10.01.2026", "2000", "40", "1600", true, "Natural 95", ""),
                new FuelEntry("fuel_4", "veh_1", "15.01.2026", "2500", "80", "3200", true, "Natural 95", "")
            ]
        };

        var summary = service.BuildVehicleFuelAnalysis(dataSet, "veh_1");

        Assert.Equal(3, summary.ConsumptionSegments.Count);
        Assert.Contains(summary.Warnings, item =>
            item.FuelEntryId == "fuel_4"
            && item.Title == "Nezvykle vysoká spotřeba");
    }

    [Fact]
    public void BuildVehicleFuelAnalysis_reports_price_outliers_only_after_enough_samples()
    {
        var service = new LegacyFuelAnalysisService();
        var dataSet = new VehimapDataSet
        {
            FuelEntries =
            [
                new FuelEntry("fuel_1", "veh_1", "01.01.2026", "1000", "10", "400", true, "Natural 95", ""),
                new FuelEntry("fuel_2", "veh_1", "02.01.2026", "1100", "10", "400", false, "Natural 95", ""),
                new FuelEntry("fuel_3", "veh_1", "03.01.2026", "1200", "10", "400", false, "Natural 95", ""),
                new FuelEntry("fuel_4", "veh_1", "04.01.2026", "1300", "10", "400", false, "Natural 95", ""),
                new FuelEntry("fuel_5", "veh_1", "05.01.2026", "1400", "10", "800", true, "Natural 95", "")
            ]
        };

        var summary = service.BuildVehicleFuelAnalysis(dataSet, "veh_1");

        Assert.Contains(summary.Warnings, item =>
            item.FuelEntryId == "fuel_5"
            && item.Title == "Nezvykle vysoká cena za jednotku paliva");
    }
}
