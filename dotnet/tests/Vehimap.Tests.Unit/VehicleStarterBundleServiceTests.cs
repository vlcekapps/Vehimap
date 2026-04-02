using Vehimap.Application.Services;
using Vehimap.Domain.Models;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class VehicleStarterBundleServiceTests
{
    private readonly VehicleStarterBundleService _service = new();

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
}
