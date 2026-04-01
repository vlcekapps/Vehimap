using Vehimap.Application;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Services;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class LegacyAuditServiceTests
{
    [Fact]
    public void BuildAudit_flags_missing_vehicle_fields_and_attachment_problems()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "vehimap-audit-" + Guid.NewGuid());
        var dataRoot = new VehimapDataRoot(tempRoot, Path.Combine(tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        try
        {
            var service = new LegacyAuditService(new TestAttachmentService());
            var dataSet = new VehimapDataSet
            {
                Vehicles =
                [
                    new Vehicle("veh_1", "Octavia", "Osobní vozidla", "", "Škoda Octavia", "", "2020", "110", "", "", "06/2026", "05/2026")
                ],
                Records =
                [
                    new VehicleRecord("rec_1", "veh_1", "Doklad", "Bez cesty", "", "01/2026", "02/2026", "1000", VehicleRecordAttachmentMode.External, "", ""),
                    new VehicleRecord("rec_2", "veh_1", "Doklad", "Managed", "", "03/2026", "04/2026", "bad", VehicleRecordAttachmentMode.Managed, "attachments/veh_1/chybi.pdf", ""),
                    new VehicleRecord("rec_3", "veh_1", "Doklad", "Rozsah", "", "05/2026", "04/2026", "500", VehicleRecordAttachmentMode.External, @"C:\missing.pdf", "")
                ]
            };

            var audit = service.BuildAudit(dataRoot, dataSet);
            var titles = audit.Select(item => item.Title).ToList();

            Assert.Contains("Chybí SPZ", titles);
            Assert.Contains("Chybí příští TK", titles);
            Assert.Contains("Neplatný rozsah zelené karty", titles);
            Assert.Contains("Doklad bez cesty", titles);
            Assert.Contains("Chybí spravovaná příloha", titles);
            Assert.Contains("Neplatná částka", titles);
            Assert.Contains("Neplatný rozsah platnosti", titles);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, true);
            }
        }
    }

    [Fact]
    public void BuildAudit_flags_odometer_regression_and_missing_maintenance_odometer()
    {
        var service = new LegacyAuditService(new TestAttachmentService());
        var dataSet = new VehimapDataSet
        {
            Vehicles =
            [
                new Vehicle("veh_1", "Passat", "Osobní vozidla", "", "Volkswagen Passat", "1AB2345", "2021", "110", "05/2025", "05/2027", "", ""),
                new Vehicle("veh_2", "Transit", "Nákladní vozidla", "", "Ford Transit", "2AB2345", "2020", "125", "05/2025", "05/2027", "", "")
            ],
            HistoryEntries =
            [
                new VehicleHistoryEntry("hist_1", "veh_1", "10.01.2026", "Servis", "12000", "", ""),
                new VehicleHistoryEntry("hist_2", "veh_1", "20.01.2026", "Servis", "11800", "", "")
            ],
            MaintenancePlans =
            [
                new MaintenancePlan("plan_1", "veh_2", "Olej", "15000", "", "", "", true, "")
            ]
        };

        var audit = service.BuildAudit(new VehimapDataRoot("C:\\vehimap", "C:\\vehimap\\data", true), dataSet);

        Assert.Contains(audit, item => item.Title == "Klesající tachometr" && item.VehicleId == "veh_1" && item.Severity == AuditSeverity.Error);
        Assert.Contains(audit, item => item.Title == "Chybí použitelný tachometr" && item.VehicleId == "veh_2");
    }

    private sealed class TestAttachmentService : IFileAttachmentService
    {
        public string ResolveManagedAttachmentPath(VehimapDataRoot dataRoot, string relativePath)
        {
            return Path.Combine(dataRoot.DataPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
