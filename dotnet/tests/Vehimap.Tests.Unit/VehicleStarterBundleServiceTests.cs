using Vehimap.Application.Services;
using Vehimap.Domain.Models;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class VehicleStarterBundleServiceTests
{
    private readonly VehicleStarterBundleService _service = new();

    [Fact]
    public void Maintenance_catalog_contains_regular_service_template()
    {
        var template = Assert.Single(
            VehicleStarterBundleService.GetMaintenanceTemplateCatalog(),
            item => item.Title == "Pravidelný servis");

        Assert.Equal("15000", template.IntervalKm);
        Assert.Equal("12", template.IntervalMonths);
        Assert.Equal("Servis", template.Category);
        Assert.Equal("Souhrn", template.Subcategory);
        Assert.Equal("Servis / Souhrn - Pravidelný servis", VehicleStarterBundleService.BuildMaintenanceTemplateDisplayName(template));
        Assert.Contains("motorového oleje", template.Note, StringComparison.CurrentCultureIgnoreCase);
        Assert.Contains("olejového filtru", template.Note, StringComparison.CurrentCultureIgnoreCase);
        Assert.Contains("vzduchového", template.Note, StringComparison.CurrentCultureIgnoreCase);
        Assert.Contains("kabinového", template.Note, StringComparison.CurrentCultureIgnoreCase);
    }

    [Fact]
    public void Maintenance_catalog_contains_tester_requested_groups_and_templates()
    {
        var catalog = VehicleStarterBundleService.GetMaintenanceTemplateCatalog();

        AssertMaintenanceTemplate(catalog, "Motor", "Zapalování a žhavení", "Svíčky / žhaviče");
        AssertMaintenanceTemplate(catalog, "Motor", "Snímače", "Snímače motoru");
        AssertMaintenanceTemplate(catalog, "Motor", "Sání a přeplňování", "Sání motoru");
        AssertMaintenanceTemplate(catalog, "Motor", "Sání a přeplňování", "Turbo");

        AssertMaintenanceTemplate(catalog, "Podvozek", "Brzdy", "Brzdové kotouče / bubny");
        AssertMaintenanceTemplate(catalog, "Podvozek", "Brzdy", "Brzdové destičky / obložení");
        AssertMaintenanceTemplate(catalog, "Podvozek", "Uložení a ramena", "Silentbloky");
        AssertMaintenanceTemplate(catalog, "Podvozek", "Uložení a ramena", "Ramena náprav");
        AssertMaintenanceTemplate(catalog, "Podvozek", "Stabilizátor", "Kosti stabilizátoru");
        AssertMaintenanceTemplate(catalog, "Podvozek", "Řízení a čepy", "Čepy řízení a náprav");
        AssertMaintenanceTemplate(catalog, "Podvozek", "Tlumení", "Tlumiče");
        AssertMaintenanceTemplate(catalog, "Podvozek", "Tlumení", "Pružiny");

        AssertMaintenanceTemplate(catalog, "Výfukové potrubí", "Emise", "Katalyzátor");
        AssertMaintenanceTemplate(catalog, "Výfukové potrubí", "Emise", "Lambda sonda");
        AssertMaintenanceTemplate(catalog, "Výfukové potrubí", "Koncové díly", "Koncovka výfuku");
        AssertMaintenanceTemplate(catalog, "Výfukové potrubí", "Tlumení", "Tlumič výfuku");
        AssertMaintenanceTemplate(catalog, "Výfukové potrubí", "Potrubí", "Výfukové trubky");

        AssertMaintenanceTemplate(catalog, "Elektronika", "Osvětlení", "Žárovky a osvětlení");
        AssertMaintenanceTemplate(catalog, "Elektronika", "Napájení", "Baterie");
        AssertMaintenanceTemplate(catalog, "Elektronika", "Jištění", "Pojistky");
        AssertMaintenanceTemplate(catalog, "Elektronika", "Snímače", "Parkovací senzory");
        AssertMaintenanceTemplate(catalog, "Elektronika", "Snímače", "Ostatní snímače a senzory");
    }

    [Fact]
    public void Build_preview_uses_full_service_profile_for_recommendations()
    {
        var dataSet = new VehimapDataSet
        {
            Vehicles =
            [
                new Vehicle("veh_1", "Milena", "Osobní vozidla", "Rodinné auto", "Škoda Octavia TDI", "1AB2345", "2018", "110", string.Empty, "08/2026", "05/2025", "06/2026")
            ],
            VehicleMetaEntries =
            [
                new VehicleMeta("veh_1", "Běžný provoz", string.Empty, "Nafta", "Má klimatizaci", "Řemen", "Automatická")
            ]
        };

        var preview = _service.BuildPreview(dataSet, "veh_1", new DateOnly(2026, 4, 2));

        Assert.Equal("Milena", preview.VehicleName);
        Assert.Contains("naftový pohon", preview.ProfileLabel, StringComparison.CurrentCultureIgnoreCase);
        Assert.Contains("klimatizaci", preview.ProfileLabel, StringComparison.CurrentCultureIgnoreCase);
        Assert.Contains(preview.Items, item => item.Title == "Palivový filtr");
        Assert.Contains(preview.Items, item => item.Title == "Rozvody");
        Assert.Contains(preview.Items, item => item.Title == "Převodový olej");
        Assert.Contains(preview.Items, item => item.Title == "Klimatizace a dezinfekce");
    }

    [Fact]
    public void Build_preview_skips_existing_bundle_items()
    {
        var dataSet = new VehimapDataSet
        {
            Vehicles =
            [
                new Vehicle("veh_1", "Milena", "Osobní vozidla", "Rodinné auto", "Škoda 120L", "1AB2345", "1988", "43", string.Empty, "08/2026", "05/2025", "06/2026")
            ],
            Records =
            [
                new VehicleRecord("rec_1", "veh_1", "Povinné ručení", "Povinné ručení", string.Empty, string.Empty, string.Empty, string.Empty, Domain.Enums.VehicleRecordAttachmentMode.Managed, string.Empty, string.Empty)
            ],
            Reminders =
            [
                new VehicleReminder("rem_1", "veh_1", "Pravidelná kontrola stavu vozidla", "02.05.2026", "14", "Každý rok", string.Empty)
            ],
            MaintenancePlans =
            [
                new MaintenancePlan("mnt_1", "veh_1", "Motorový olej a filtr", "15000", "12", string.Empty, string.Empty, true, string.Empty)
            ]
        };

        var preview = _service.BuildPreview(dataSet, "veh_1", new DateOnly(2026, 4, 2));

        Assert.DoesNotContain(preview.Items, item => item.Title == "Motorový olej a filtr");
        Assert.DoesNotContain(preview.Items, item => item.SectionLabel == "Doklad" && item.Title == "Povinné ručení");
        Assert.DoesNotContain(preview.Items, item => item.SectionLabel == "Připomínka" && item.Title == "Pravidelná kontrola stavu vozidla");
    }

    private static void AssertMaintenanceTemplate(
        IReadOnlyList<Vehimap.Application.Models.VehicleStarterBundleTemplate> catalog,
        string category,
        string subcategory,
        string title)
    {
        var template = Assert.Single(catalog, item => item.Title == title);
        Assert.Equal(category, template.Category);
        Assert.Equal(subcategory, template.Subcategory);
        Assert.Equal($"{category} / {subcategory} - {title}", VehicleStarterBundleService.BuildMaintenanceTemplateDisplayName(template));
        Assert.True(!string.IsNullOrWhiteSpace(template.IntervalKm) || !string.IsNullOrWhiteSpace(template.IntervalMonths));
    }
}
