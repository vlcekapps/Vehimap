using Vehimap.Application.Models;
using Vehimap.Desktop.ViewModels;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class ServiceBookWindowViewModelTests
{
    [Fact]
    public void Service_book_item_should_expose_human_readable_accessible_label()
    {
        var item = new ServiceBookItemViewModel(
            "veh_1",
            "Historie",
            "hist_1",
            "Historie a servis",
            "01.04.2026",
            "Servis",
            "Tachometr 120000 km.",
            "Cena 2500 Kč");

        Assert.Equal(item.AccessibleLabel, item.ToString());
        Assert.Contains("Historie a servis", item.AccessibleLabel);
        Assert.Contains("Servis", item.AccessibleLabel);
        Assert.DoesNotContain(nameof(ServiceBookItemViewModel), item.AccessibleLabel);
    }

    [Fact]
    public void Open_selected_item_should_call_navigation_and_request_close()
    {
        var openedItemId = string.Empty;
        var closeRequested = false;
        var item = new ServiceBookItemViewModel(
            "veh_1",
            "Historie",
            "hist_1",
            "Historie a servis",
            "01.04.2026",
            "Servis",
            "Tachometr 120000 km.",
            "Cena 2500 Kč");
        var model = new ServiceBookWindowViewModel(
            CreateSummary(),
            [item],
            [],
            [],
            selected =>
            {
                openedItemId = selected?.EntityId ?? string.Empty;
                return true;
            },
            _ => Task.FromResult("Exportováno."));
        model.CloseRequested += () => closeRequested = true;

        model.OpenSelectedServiceBookItemCommand.Execute(null);

        Assert.Equal("hist_1", openedItemId);
        Assert.True(model.DidOpenSelectedItem);
        Assert.True(closeRequested);
        Assert.Contains("otevřena", model.StatusText, StringComparison.CurrentCultureIgnoreCase);
    }

    [Fact]
    public async Task Export_html_command_should_update_status()
    {
        var model = new ServiceBookWindowViewModel(
            CreateSummary(),
            [],
            [],
            [],
            _ => false,
            _ => Task.FromResult("Servisní knížka byla uložena."));

        await model.ExportHtmlCommand.ExecuteAsync(null);

        Assert.Equal("Servisní knížka byla uložena.", model.StatusText);
    }

    private static ServiceBookSummary CreateSummary() => new(
        "veh_1",
        "Milena",
        "Osobní vozidla",
        "Škoda 120L",
        "1AB2345",
        "120000 km",
        2500m,
        "Záznamy historie: 1. Servisní plány: 0. Servisní doklady: 0.",
        [],
        [],
        []);
}
