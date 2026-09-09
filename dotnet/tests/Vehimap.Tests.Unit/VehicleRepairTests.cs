// SPDX-License-Identifier: GPL-3.0-or-later
using System.Globalization;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Desktop.Localization;
using Vehimap.Desktop.ViewModels;
using Vehimap.Domain.Models;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class VehicleRepairTests
{
    private static VehimapDataSet Data() => new()
    {
        Vehicles = [new Vehicle("v", "Moje auto", "Osobní vozidla", "", "Škoda", "TEST", "", "", "", "", "", "")]
    };
    private static RepairChange Change(RepairAction action = RepairAction.Create, string? id = null) =>
        new(action, "v", id, "Nefunguje klimatizace", "Vlastní poznámka\nřidiče", "09.09.2026", "20.09.2026", 7,
            "Výměna kompresoru", "19.09.2026", "123456", "2500.5");
    private static VehicleRepairService Service(string language = "en-US") => new(new ResourceAppLocalizer(CultureInfo.GetCultureInfo(language)));
    private static readonly DateTimeOffset Now = new(2026, 9, 9, 12, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData("en-US", "Planned repair is approaching", "Planned repair date has passed")]
    [InlineData("cs-CZ", "Blíží se plánovaná oprava", "Termín plánované opravy uplynul")]
    public void Audit_uses_exact_boundaries_and_preserves_user_text(string language, string upcoming, string overdue)
    {
        var data = Data(); var service = Service(language);
        service.Apply(data, Change(), Now);
        Assert.Empty(service.BuildAudit(data, new DateOnly(2026, 9, 12)));
        var item = Assert.Single(service.BuildAudit(data, new DateOnly(2026, 9, 13)));
        Assert.Equal(upcoming, item.Title);
        Assert.Equal("Nefunguje klimatizace", item.Message);
        Assert.Equal(ApplicationEntityKinds.Repair, item.EntityKind);
        Assert.Equal(upcoming, Assert.Single(service.BuildAudit(data, new DateOnly(2026, 9, 20))).Title);
        Assert.Equal(overdue, Assert.Single(service.BuildAudit(data, new DateOnly(2026, 9, 21))).Title);
    }

    [Fact]
    public void Reschedule_retains_dates_and_completion_before_due_creates_exactly_one_history()
    {
        var data = Data(); var service = Service();
        var repair = service.Apply(data, Change(), Now);
        repair = service.Apply(data, Change(RepairAction.Reschedule, repair.Id) with { PlannedDate = "30.09.2026", Reason = "Čekáme na díl" }, Now);
        var move = Assert.Single(repair.ScheduleChanges);
        Assert.Equal("20.09.2026", move.PreviousDate); Assert.Equal("30.09.2026", move.NewDate);
        Assert.Equal("Čekáme na díl", move.Reason);
        Assert.Empty(service.BuildAudit(data, new DateOnly(2026, 9, 21)));
        repair = service.Apply(data, Change(RepairAction.Complete, repair.Id), Now);
        Assert.Equal(VehicleRepair.Repaired, repair.State);
        var history = Assert.Single(data.HistoryEntries);
        Assert.Equal(repair.HistoryEntryId, history.Id);
        Assert.Equal("2500.5", history.Cost);
        Assert.Equal("Výměna kompresoru", history.EventType);
        Assert.Empty(data.MaintenancePlans);
        Assert.Empty(service.BuildAudit(data, new DateOnly(2027, 1, 1)));
        Assert.Throws<RepairValidationException>(() => service.Apply(data, Change(RepairAction.Complete, repair.Id), Now));
        Assert.Single(data.HistoryEntries);
    }

    [Fact]
    public void Existing_history_is_linked_without_duplicating_cost_and_cannot_be_used_twice()
    {
        var data = Data(); var service = Service();
        data.HistoryEntries.Add(new VehicleHistoryEntry("h", "v", "15.09.2026", "Oprava", "", "123.4", ""));
        var repair = service.Apply(data, Change(), Now);
        var second = service.Apply(data, Change(), Now);
        service.Apply(data, Change(RepairAction.Complete, repair.Id) with { ExistingHistoryId = "h" }, Now);
        Assert.Single(data.HistoryEntries);
        Assert.Throws<RepairValidationException>(() => service.Apply(data, Change(RepairAction.Complete, second.Id) with { ExistingHistoryId = "h" }, Now));
        Assert.Equal(VehicleRepair.Waiting, data.Repairs.Single(r => r.Id == second.Id).State);
        VehicleRepairService.ValidateReferences(data);
        data.HistoryEntries.Clear();
        Assert.Throws<InvalidDataException>(() => VehicleRepairService.ValidateReferences(data));
    }

    [Fact]
    public void Unrepairable_fault_remains_visible_without_overdue_or_history()
    {
        var data = Data(); var service = Service();
        var repair = service.Apply(data, Change() with { PlannedDate = "" }, Now);
        Assert.Contains("scheduling", Assert.Single(service.BuildAudit(data, new DateOnly(2026, 9, 10))).Title);
        repair = service.Apply(data, Change(RepairAction.CannotRepair, repair.Id) with { Reason = "Díl se nevyrábí" }, Now);
        Assert.Equal("Díl se nevyrábí", repair.Resolution);
        Assert.Contains("cannot be repaired", Assert.Single(service.BuildAudit(data, new DateOnly(2027, 1, 1))).Title);
        Assert.Empty(data.HistoryEntries);
    }

    [Theory]
    [InlineData("29.02.2027")]
    [InlineData("30.02.2028")]
    [InlineData("31.04.2027")]
    [InlineData("08.09.2026")]
    public void Invalid_or_reversed_date_does_not_mutate_data(string date)
    {
        var data = Data();
        Assert.Throws<RepairValidationException>(() => Service().Apply(data, Change() with { PlannedDate = date }, Now));
        Assert.Empty(data.Repairs); Assert.Empty(data.HistoryEntries);
    }

    [Theory]
    [InlineData("cs-CZ", "none", "comma", "km", "9.9.2026", "2500,50", "100")]
    [InlineData("en-US", "comma", "dot", "mi", "9/9/2026", "2,500.50", "161")]
    public async Task Dialog_converts_input_and_keeps_failed_save_open(string language, string thousands, string decimals, string unit, string date, string cost, string km)
    {
        var culture = new AppCulturePreferences(language, thousands, decimals);
        var previous = DesktopLocalization.CurrentCulture.Name;
        try
        {
            DesktopLocalization.Configure(culture);
            var data = Data(); var repair = Service().Apply(data, Change(), Now);
            RepairChange? saved = null;
            var fail = true;
            var model = new RepairEditorViewModel(RepairAction.Complete, "v", repair, [], culture, new AppUnitPreferences(unit, "l"),
                c => { saved = c; return Task.FromResult<string?>(fail ? "Test failure" : null); });
            model.CompletedDate = date; model.Odometer = "100"; model.Cost = cost; model.Reason = "Moje oprava";
            await model.SaveCommand.ExecuteAsync(null);
            Assert.True(model.IsEditing); Assert.Equal("Test failure", model.Status); Assert.Equal(km, saved!.Odometer);
            Assert.Equal("2500.5", saved.Cost); Assert.Equal("Moje oprava", saved.Reason);
            fail = false; await model.SaveCommand.ExecuteAsync(null);
            Assert.False(model.IsEditing);
        }
        finally { DesktopLocalization.Configure(new AppCulturePreferences(previous)); }
    }
}
