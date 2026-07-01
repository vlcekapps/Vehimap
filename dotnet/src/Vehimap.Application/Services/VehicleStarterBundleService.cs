// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Models;
using Vehimap.Domain.Models;

namespace Vehimap.Application.Services;

public sealed class VehicleStarterBundleService
{
    private static readonly VehicleStarterBundleTemplate[] MaintenanceTemplates =
    [
        Maintenance("Servis", "Souhrn", "Pravidelný servis", "15000", "12", "Souhrnný servisní úkon: výměna motorového oleje a olejového filtru, kontrola nebo výměna vzduchového, kabinového a podle pohonu také palivového filtru."),
        Maintenance("Motor", "Olej a filtry", "Motorový olej a filtr", "15000", "12", "Pravidelná výměna oleje a olejového filtru."),
        Maintenance("Motor", "Olej a filtry", "Palivový filtr", "30000", "24", "Výměna palivového filtru podle provozu a doporučení výrobce."),
        Maintenance("Motor", "Olej a filtry", "Vzduchový filtr", "30000", "24", "Zkontrolovat nebo vyměnit vzduchový filtr."),
        Maintenance("Motor", "Olej a filtry", "Kabinový filtr", "15000", "12", "Pravidelná výměna pylového nebo kabinového filtru."),
        Maintenance("Podvozek", "Brzdy", "Brzdová kapalina", string.Empty, "24", "Pravidelná výměna brzdové kapaliny."),
        Maintenance("Motor", "Kapaliny", "Chladicí kapalina", string.Empty, "60", "Kontrola a obnova chladicí kapaliny."),
        Maintenance("Motor", "Rozvody", "Rozvody", "90000", "60", "Rozvodový řemen nebo řetěz podle doporučení výrobce."),
        Maintenance("Motor", "Převody", "Převodový olej", "60000", "48", "Kontrola nebo výměna převodového oleje."),
        Maintenance("Elektronika", "Komfort", "Klimatizace a dezinfekce", string.Empty, "12", "Servis klimatizace a dezinfekce okruhu."),
        Maintenance("Motor", "Zapalování a žhavení", "Svíčky / žhaviče", "60000", "48", "Kontrola nebo výměna zapalovacích svíček u benzinu, případně žhavičů u naftového motoru."),
        Maintenance("Motor", "Snímače", "Snímače motoru", "60000", "48", "Kontrola nebo výměna motorových snímačů a senzorů podle diagnostiky a chování vozidla."),
        Maintenance("Motor", "Sání a přeplňování", "Sání motoru", "60000", "48", "Kontrola, čištění nebo servis sání motoru podle provozu a zanesení."),
        Maintenance("Motor", "Sání a přeplňování", "Turbo", "90000", "60", "Kontrola nebo servis turbodmychadla, hadic a regulace přeplňování."),
        Maintenance("Podvozek", "Brzdy", "Brzdové kotouče / bubny", "60000", "48", "Kontrola nebo výměna brzdových kotoučů, bubnů a souvisejících dílů."),
        Maintenance("Podvozek", "Brzdy", "Brzdové destičky / obložení", "30000", "24", "Kontrola nebo výměna brzdových destiček, čelistí nebo obložení podle opotřebení."),
        Maintenance("Podvozek", "Uložení a ramena", "Silentbloky", "60000", "48", "Kontrola nebo výměna silentbloků náprav, ramen nebo stabilizátoru."),
        Maintenance("Podvozek", "Uložení a ramena", "Ramena náprav", "60000", "48", "Kontrola nebo výměna ramen náprav a jejich uložení."),
        Maintenance("Podvozek", "Stabilizátor", "Kosti stabilizátoru", "40000", "36", "Kontrola nebo výměna táhel stabilizátoru a souvisejícího uložení."),
        Maintenance("Podvozek", "Řízení a čepy", "Čepy řízení a náprav", "60000", "48", "Kontrola nebo výměna čepů řízení, kulových čepů a souvisejících dílů náprav."),
        Maintenance("Podvozek", "Tlumení", "Tlumiče", "80000", "60", "Kontrola nebo výměna tlumičů podle stavu, úniku kapaliny a jízdního projevu."),
        Maintenance("Podvozek", "Tlumení", "Pružiny", "80000", "60", "Kontrola nebo výměna pružin a jejich uložení."),
        Maintenance("Výfukové potrubí", "Emise", "Katalyzátor", "120000", "72", "Kontrola nebo výměna katalyzátoru podle emisí, diagnostiky a stavu výfuku."),
        Maintenance("Výfukové potrubí", "Emise", "Lambda sonda", "90000", "60", "Kontrola nebo výměna lambda sondy podle diagnostiky a spotřeby."),
        Maintenance("Výfukové potrubí", "Koncové díly", "Koncovka výfuku", "90000", "60", "Kontrola nebo výměna koncovky výfuku."),
        Maintenance("Výfukové potrubí", "Tlumení", "Tlumič výfuku", "90000", "60", "Kontrola nebo výměna tlumiče výfuku."),
        Maintenance("Výfukové potrubí", "Potrubí", "Výfukové trubky", "90000", "60", "Kontrola nebo výměna výfukových trubek, spojů a závěsů."),
        Maintenance("Elektronika", "Osvětlení", "Žárovky a osvětlení", string.Empty, "12", "Pravidelná kontrola a výměna žárovek, světelných zdrojů a osvětlení vozidla."),
        Maintenance("Elektronika", "Napájení", "Baterie", string.Empty, "48", "Kontrola nebo výměna startovací baterie, svorek a dobíjení."),
        Maintenance("Elektronika", "Jištění", "Pojistky", string.Empty, "24", "Kontrola pojistek, pojistkové skříně a souvisejících elektrických závad."),
        Maintenance("Elektronika", "Snímače", "Parkovací senzory", string.Empty, "24", "Kontrola nebo výměna parkovacích senzorů a jejich kabeláže."),
        Maintenance("Elektronika", "Snímače", "Ostatní snímače a senzory", string.Empty, "24", "Kontrola nebo výměna ostatních snímačů a senzorů, které nesouvisí přímo s motorem.")
    ];

    private static readonly VehicleStarterBundleTemplate[] RoadRecordTemplates =
    [
        new(VehicleStarterBundleSection.Record, "Doklad", "Povinné ručení", string.Empty, string.Empty, "Povinné ručení", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "Doplňte číslo smlouvy, platnost a případnou přílohu."),
        new(VehicleStarterBundleSection.Record, "Doklad", "Havarijní pojištění", string.Empty, string.Empty, "Havarijní pojištění", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "Doplňte rozsah pojištění, platnost a případnou přílohu."),
        new(VehicleStarterBundleSection.Record, "Doklad", "Asistence", string.Empty, string.Empty, "Asistence", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "Doplňte poskytovatele asistence a důležité kontakty.")
    ];

    private static readonly VehicleStarterBundleTemplate[] GenericRecordTemplates =
    [
        new(VehicleStarterBundleSection.Record, "Doklad", "Obecný doklad", string.Empty, string.Empty, "Doklad", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "Doplňte název dokladu, platnost a případnou přílohu.")
    ];

    public static IReadOnlyList<VehicleStarterBundleTemplate> GetMaintenanceTemplateCatalog() =>
        MaintenanceTemplates;

    public static string BuildMaintenanceTemplateDisplayName(VehicleStarterBundleTemplate template)
    {
        var category = template.Category.Trim();
        var subcategory = template.Subcategory.Trim();
        return (category, subcategory) switch
        {
            ("", "") => template.Title,
            (_, "") => $"{category} - {template.Title}",
            _ => $"{category} / {subcategory} - {template.Title}"
        };
    }

    public static VehicleStarterBundleTemplate? FindMaintenanceTemplateByDisplayName(string value) =>
        MaintenanceTemplates.FirstOrDefault(item =>
            string.Equals(item.Title, value, StringComparison.Ordinal)
            || string.Equals(BuildMaintenanceTemplateDisplayName(item), value, StringComparison.Ordinal));

    public VehicleStarterBundlePreview BuildPreview(VehimapDataSet dataSet, string vehicleId, DateOnly today)
    {
        var vehicle = dataSet.Vehicles.FirstOrDefault(item => string.Equals(item.Id, vehicleId, StringComparison.Ordinal));
        if (vehicle is null)
        {
            return new VehicleStarterBundlePreview(vehicleId, string.Empty, string.Empty, []);
        }

        var meta = dataSet.VehicleMetaEntries.FirstOrDefault(item => string.Equals(item.VehicleId, vehicleId, StringComparison.Ordinal));
        var items = new List<VehicleStarterBundleTemplate>();
        items.AddRange(GetMissingMaintenanceTemplates(dataSet, vehicle, meta));
        items.AddRange(GetMissingRecordTemplates(dataSet, vehicle));
        items.AddRange(GetMissingReminderTemplates(dataSet, vehicle, today));

        return new VehicleStarterBundlePreview(vehicle.Id, vehicle.Name, BuildProfileLabel(vehicle, meta), items);
    }

    private static IReadOnlyList<VehicleStarterBundleTemplate> GetMissingMaintenanceTemplates(VehimapDataSet dataSet, Vehicle vehicle, VehicleMeta? meta)
    {
        var existingTitles = dataSet.MaintenancePlans
            .Where(item => string.Equals(item.VehicleId, vehicle.Id, StringComparison.Ordinal))
            .Select(item => NormalizeKey(item.Title))
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .ToHashSet(StringComparer.Ordinal);

        return GetRecommendedMaintenanceTemplates(vehicle, meta)
            .Where(template => !existingTitles.Contains(NormalizeKey(template.Title)))
            .ToList();
    }

    private static IReadOnlyList<VehicleStarterBundleTemplate> GetMissingRecordTemplates(VehimapDataSet dataSet, Vehicle vehicle)
    {
        var existingKeys = dataSet.Records
            .Where(item => string.Equals(item.VehicleId, vehicle.Id, StringComparison.Ordinal))
            .Select(item => BuildRecordKey(item.RecordType, item.Title))
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .ToHashSet(StringComparer.Ordinal);

        var templates = IsRoadVehicleCategory(vehicle.Category) ? RoadRecordTemplates : GenericRecordTemplates;
        return templates
            .Where(template => !existingKeys.Contains(BuildRecordKey(template.RecordType, template.Title)))
            .ToList();
    }

    private static IReadOnlyList<VehicleStarterBundleTemplate> GetMissingReminderTemplates(VehimapDataSet dataSet, Vehicle vehicle, DateOnly today)
    {
        var existingKeys = dataSet.Reminders
            .Where(item => string.Equals(item.VehicleId, vehicle.Id, StringComparison.Ordinal))
            .Select(item => BuildReminderKey(item.Title, item.RepeatMode))
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .ToHashSet(StringComparer.Ordinal);

        return GetReminderTemplates(vehicle, today)
            .Where(template => !existingKeys.Contains(BuildReminderKey(template.Title, template.RepeatMode)))
            .ToList();
    }

    private static IReadOnlyList<VehicleStarterBundleTemplate> GetRecommendedMaintenanceTemplates(Vehicle vehicle, VehicleMeta? meta)
    {
        var labels = new List<string>();
        var category = NormalizeCategory(vehicle.Category);
        var powertrain = ResolvePowertrain(vehicle, meta);
        var isElectric = string.Equals(powertrain, "Elektro", StringComparison.Ordinal);
        var isDiesel = string.Equals(powertrain, "Nafta", StringComparison.Ordinal);
        var recommendClimate = ShouldRecommendClimateMaintenance(category, meta);
        var recommendTiming = ShouldRecommendTimingMaintenance(powertrain, meta);
        var recommendTransmission = ShouldRecommendTransmissionMaintenance(powertrain, meta);

        switch (category)
        {
            case "Osobní vozidla":
                if (isElectric)
                {
                    labels.AddRange(["Kabinový filtr", "Brzdová kapalina", "Chladicí kapalina"]);
                }
                else
                {
                    labels.AddRange(["Motorový olej a filtr", "Vzduchový filtr", "Kabinový filtr", "Brzdová kapalina", "Chladicí kapalina"]);
                    if (recommendTiming)
                    {
                        labels.Add("Rozvody");
                    }

                    if (recommendTransmission)
                    {
                        labels.Add("Převodový olej");
                    }
                }
                break;

            case "Motocykly":
                labels.AddRange(isElectric
                    ? ["Brzdová kapalina"]
                    : ["Motorový olej a filtr", "Vzduchový filtr", "Brzdová kapalina", "Chladicí kapalina"]);
                break;

            case "Nákladní vozidla":
            case "Autobusy":
                if (isElectric)
                {
                    labels.AddRange(["Brzdová kapalina", "Chladicí kapalina"]);
                }
                else
                {
                    labels.AddRange(["Motorový olej a filtr", "Vzduchový filtr", "Brzdová kapalina", "Chladicí kapalina"]);
                    if (recommendTransmission)
                    {
                        labels.Add("Převodový olej");
                    }
                }
                break;

            default:
                labels.AddRange(isElectric
                    ? ["Brzdová kapalina"]
                    : ["Brzdová kapalina", "Chladicí kapalina"]);
                break;
        }

        if (recommendClimate)
        {
            labels.Add("Klimatizace a dezinfekce");
        }

        if (isDiesel && !isElectric)
        {
            labels.Add("Palivový filtr");
        }

        return labels
            .Distinct(StringComparer.Ordinal)
            .Select(GetMaintenanceTemplateByTitle)
            .Where(static template => template is not null)
            .Cast<VehicleStarterBundleTemplate>()
            .ToList();
    }

    private static IReadOnlyList<VehicleStarterBundleTemplate> GetReminderTemplates(Vehicle vehicle, DateOnly today)
    {
        var dueDate = today.AddDays(30).ToString("dd.MM.yyyy");
        var category = NormalizeCategory(vehicle.Category);
        var title = category switch
        {
            "Motocykly" => "Předsezónní kontrola motocyklu",
            "Nákladní vozidla" or "Autobusy" => "Pravidelná provozní kontrola",
            "Ostatní" => "Pravidelná kontrola stavu",
            _ => "Pravidelná kontrola stavu vozidla"
        };

        var note = category switch
        {
            "Motocykly" => "Zkontrolujte brzdy, řetěz, pneumatiky, kapaliny a baterii.",
            "Nákladní vozidla" or "Autobusy" => "Zkontrolujte kapaliny, osvětlení, pneumatiky a povinnou výbavu.",
            "Ostatní" => "Doplňte vlastní kontrolní kroky podle typu zařízení.",
            _ => "Zkontrolujte výbavu, kapaliny, osvětlení a stav pneumatik."
        };

        return
        [
            new VehicleStarterBundleTemplate(
                VehicleStarterBundleSection.Reminder,
                "Připomínka",
                title,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                dueDate,
                "14",
                "Každý rok",
                note)
        ];
    }

    private static VehicleStarterBundleTemplate? GetMaintenanceTemplateByTitle(string title) =>
        MaintenanceTemplates.FirstOrDefault(item => string.Equals(item.Title, title, StringComparison.Ordinal));

    private static VehicleStarterBundleTemplate Maintenance(
        string category,
        string subcategory,
        string title,
        string intervalKm,
        string intervalMonths,
        string note) =>
        new(
            VehicleStarterBundleSection.Maintenance,
            "Servis",
            title,
            intervalKm,
            intervalMonths,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            note,
            category,
            subcategory);

    private static string BuildProfileLabel(Vehicle vehicle, VehicleMeta? meta)
    {
        var parts = new List<string> { NormalizeCategory(vehicle.Category) };
        var powertrain = ResolvePowertrain(vehicle, meta);
        if (!string.IsNullOrWhiteSpace(powertrain))
        {
            parts.Add(powertrain switch
            {
                "Benzín" => "benzínový pohon",
                "Nafta" => "naftový pohon",
                "Hybrid" => "hybridní pohon",
                "Plug-in hybrid" => "plug-in hybrid",
                "Elektro" => "elektrický pohon",
                "LPG / CNG" => "pohon LPG / CNG",
                "Jiné" => "jiný pohon",
                _ => powertrain.ToLowerInvariant()
            });
        }

        if (!string.IsNullOrWhiteSpace(meta?.ClimateProfile))
        {
            parts.Add(meta.ClimateProfile.ToLowerInvariant());
        }

        if (!string.IsNullOrWhiteSpace(meta?.TimingDrive))
        {
            parts.Add(meta.TimingDrive switch
            {
                "Řemen" => "rozvody řemenem",
                "Řetěz" => "rozvody řetězem",
                "Není relevantní" => "bez pravidelných rozvodů",
                _ => $"rozvody: {meta.TimingDrive.ToLowerInvariant()}"
            });
        }

        if (!string.IsNullOrWhiteSpace(meta?.Transmission))
        {
            parts.Add(meta.Transmission switch
            {
                "Manuální" => "manuální převodovka",
                "Automatická" => "automatická převodovka",
                "Není relevantní" => "bez klasické převodovky",
                _ => $"převodovka: {meta.Transmission.ToLowerInvariant()}"
            });
        }

        return string.Join(", ", parts.Where(static item => !string.IsNullOrWhiteSpace(item)));
    }

    private static string ResolvePowertrain(Vehicle vehicle, VehicleMeta? meta)
    {
        if (!string.IsNullOrWhiteSpace(meta?.Powertrain))
        {
            return meta.Powertrain;
        }

        var haystack = string.Join(' ', NormalizeCategory(vehicle.Category), vehicle.MakeModel, vehicle.VehicleNote).ToLowerInvariant();

        if (ContainsAny(haystack, "plug-in hybrid", "plug in hybrid", "phev"))
        {
            return "Plug-in hybrid";
        }

        if (ContainsAny(haystack, "hybrid", "hev"))
        {
            return "Hybrid";
        }

        if (ContainsAny(haystack, "elektro", "electric", "bev", "tesla", "ev"))
        {
            return "Elektro";
        }

        if (ContainsAny(haystack, "diesel", "nafta", "tdi", "hdi", "dci", "cdi", "crdi", "multijet", "tdci"))
        {
            return "Nafta";
        }

        if (ContainsAny(haystack, "lpg", "cng", "gpl"))
        {
            return "LPG / CNG";
        }

        if (ContainsAny(haystack, "benzin", "benzín", "gasoline", "tsi", "tfsi", "mpi", "gdi", "fsi", "ecoboost"))
        {
            return "Benzín";
        }

        return string.Empty;
    }

    private static bool ShouldRecommendClimateMaintenance(string category, VehicleMeta? meta)
    {
        if (string.Equals(meta?.ClimateProfile, "Má klimatizaci", StringComparison.Ordinal))
        {
            return true;
        }

        if (string.Equals(meta?.ClimateProfile, "Bez klimatizace", StringComparison.Ordinal))
        {
            return false;
        }

        return category is "Osobní vozidla" or "Nákladní vozidla" or "Autobusy";
    }

    private static bool ShouldRecommendTimingMaintenance(string powertrain, VehicleMeta? meta)
    {
        if (string.Equals(powertrain, "Elektro", StringComparison.Ordinal))
        {
            return false;
        }

        return meta?.TimingDrive switch
        {
            "Řemen" => true,
            "Řetěz" => false,
            "Není relevantní" => false,
            _ => true
        };
    }

    private static bool ShouldRecommendTransmissionMaintenance(string powertrain, VehicleMeta? meta)
    {
        if (string.Equals(powertrain, "Elektro", StringComparison.Ordinal))
        {
            return false;
        }

        return meta?.Transmission switch
        {
            "Automatická" => true,
            "Manuální" => false,
            "Není relevantní" => false,
            _ => true
        };
    }

    private static bool IsRoadVehicleCategory(string? category) =>
        !string.IsNullOrWhiteSpace(category) && !string.Equals(NormalizeCategory(category), "Ostatní", StringComparison.Ordinal);

    private static string NormalizeCategory(string? category)
    {
        var value = (category ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(value) ? "Ostatní" : value;
    }

    private static string NormalizeKey(string? value) =>
        (value ?? string.Empty).Trim().ToLowerInvariant();

    private static string BuildRecordKey(string? recordType, string? title)
    {
        var normalizedType = NormalizeKey(recordType);
        var normalizedTitle = NormalizeKey(title);
        return string.IsNullOrWhiteSpace(normalizedType) || string.IsNullOrWhiteSpace(normalizedTitle)
            ? string.Empty
            : $"{normalizedType}|{normalizedTitle}";
    }

    private static string BuildReminderKey(string? title, string? repeatMode)
    {
        var normalizedTitle = NormalizeKey(title);
        if (string.IsNullOrWhiteSpace(normalizedTitle))
        {
            return string.Empty;
        }

        return $"{normalizedTitle}|{NormalizeKey(repeatMode)}";
    }

    private static bool ContainsAny(string haystack, params string[] values) =>
        values.Any(value => haystack.Contains(value, StringComparison.OrdinalIgnoreCase));
}
