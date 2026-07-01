// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Models;
using Vehimap.Desktop.Services;
using Vehimap.Desktop.ViewModels;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class DesktopServiceBookExportServiceTests
{
    [Fact]
    public void Build_file_name_should_be_stable_and_html()
    {
        var service = new DesktopServiceBookExportService();
        var summary = CreateSummary();

        var fileName = service.BuildFileName(summary, new DateTime(2026, 4, 3, 9, 15, 0));

        Assert.Equal("Milena-servisni-knizka-2026-04-03.html", fileName);
    }

    [Fact]
    public void Build_html_should_include_sections_and_escape_values()
    {
        var service = new DesktopServiceBookExportService();
        var summary = CreateSummary() with
        {
            VehicleName = "Milena <test>"
        };
        var history = new[]
        {
            new ServiceBookItemViewModel("veh_1", "Historie", "hist_1", "Historie a servis", "01.04.2026", "Servis <olej>", "Tachometr 120000 km.", "Cena 2500 Kč")
        };
        var maintenance = new[]
        {
            new ServiceBookItemViewModel("veh_1", "Údržba", "mnt_1", "Servisní plán", "Motorový olej", "15000 km", "Poslední servis 01.04.2026.", "Za 12000 km")
        };
        var records = new[]
        {
            new ServiceBookItemViewModel("veh_1", "Doklad", "rec_1", "Servisní doklad", "Faktura", "Servisní dokument", "Poskytovatel Autoservis.", "Příloha je dostupná")
        };

        var html = service.BuildHtml(summary, history, maintenance, records, new DateTime(2026, 4, 3, 9, 15, 0));

        Assert.Contains("<title>Vehimap - Servisní knížka</title>", html);
        Assert.Contains("historie a servisu", html);
        Assert.Contains("pl&#225;ny", html);
        Assert.Contains("doklady", html);
        Assert.Contains("Milena &lt;test&gt;", html);
        Assert.Contains("Servis &lt;olej&gt;", html);
        Assert.DoesNotContain("Milena <test>", html);
    }

    [Fact]
    public void Build_html_should_format_summary_money_with_selected_currency()
    {
        var service = new DesktopServiceBookExportService();
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

        var html = service.BuildHtml(CreateSummary(), [], [], [], new DateTime(2026, 4, 3, 9, 15, 0));

        Assert.Contains("$2,500.00", html);
        Assert.DoesNotContain("Kč", html);
    }

    private static ServiceBookSummary CreateSummary() => new(
        "veh_1",
        "Milena",
        "Osobní vozidla",
        "Škoda 120L",
        "1AB2345",
        "120000 km",
        2500m,
        "Záznamy historie: 1. Servisní plány: 1. Servisní doklady: 1.",
        [],
        [],
        []);
}
