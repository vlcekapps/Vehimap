// SPDX-License-Identifier: GPL-3.0-or-later
using System.Globalization;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Domain.Models;

namespace Vehimap.Application.Services;

public sealed class VehicleStarterBundleService
{
    private static readonly IAppLocalizer EnglishTemplateLocalizer = new ResourceAppLocalizer(CultureInfo.GetCultureInfo(AppCultureService.EnglishLanguage));
    private static readonly IAppLocalizer CzechTemplateLocalizer = new ResourceAppLocalizer(CultureInfo.GetCultureInfo(AppCultureService.CzechLanguage));

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

    private static readonly IReadOnlyDictionary<string, string> MaintenanceTemplateKeys = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["Pravidelný servis"] = "RegularService",
        ["Motorový olej a filtr"] = "EngineOilFilter",
        ["Palivový filtr"] = "FuelFilter",
        ["Vzduchový filtr"] = "AirFilter",
        ["Kabinový filtr"] = "CabinFilter",
        ["Brzdová kapalina"] = "BrakeFluid",
        ["Chladicí kapalina"] = "Coolant",
        ["Rozvody"] = "TimingDrive",
        ["Převodový olej"] = "TransmissionOil",
        ["Klimatizace a dezinfekce"] = "AirConditioning",
        ["Svíčky / žhaviče"] = "SparkGlowPlugs",
        ["Snímače motoru"] = "EngineSensors",
        ["Sání motoru"] = "Intake",
        ["Turbo"] = "Turbo",
        ["Brzdové kotouče / bubny"] = "BrakeDiscsDrums",
        ["Brzdové destičky / obložení"] = "BrakePadsShoes",
        ["Silentbloky"] = "Bushings",
        ["Ramena náprav"] = "SuspensionArms",
        ["Kosti stabilizátoru"] = "StabilizerLinks",
        ["Čepy řízení a náprav"] = "SteeringJoints",
        ["Tlumiče"] = "ShockAbsorbers",
        ["Pružiny"] = "Springs",
        ["Katalyzátor"] = "CatalyticConverter",
        ["Lambda sonda"] = "LambdaSensor",
        ["Koncovka výfuku"] = "ExhaustTip",
        ["Tlumič výfuku"] = "ExhaustMuffler",
        ["Výfukové trubky"] = "ExhaustPipes",
        ["Žárovky a osvětlení"] = "BulbsLighting",
        ["Baterie"] = "Battery",
        ["Pojistky"] = "Fuses",
        ["Parkovací senzory"] = "ParkingSensors",
        ["Ostatní snímače a senzory"] = "OtherSensors"
    };

    private static readonly IReadOnlyDictionary<string, string> RecordTemplateKeys = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["Povinné ručení"] = "LiabilityInsurance",
        ["Havarijní pojištění"] = "ComprehensiveInsurance",
        ["Asistence"] = "Assistance",
        ["Obecný doklad"] = "GenericDocument"
    };

    private static readonly IReadOnlyDictionary<string, string> ReminderTemplateKeys = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["Předsezónní kontrola motocyklu"] = "MotorcyclePreSeasonCheck",
        ["Pravidelná provozní kontrola"] = "OperationalCheck",
        ["Pravidelná kontrola stavu"] = "ConditionCheck",
        ["Pravidelná kontrola stavu vozidla"] = "VehicleConditionCheck"
    };

    private readonly IAppLocalizer _localizer;

    public VehicleStarterBundleService()
        : this(new ResourceAppLocalizer(CultureInfo.GetCultureInfo(AppCultureService.CzechLanguage)))
    {
    }

    public VehicleStarterBundleService(IAppLocalizer localizer)
    {
        _localizer = localizer;
    }

    public static IReadOnlyList<VehicleStarterBundleTemplate> GetMaintenanceTemplateCatalog() =>
        MaintenanceTemplates;

    public static IReadOnlyList<VehicleStarterBundleTemplate> GetMaintenanceTemplateCatalog(IAppLocalizer localizer) =>
        MaintenanceTemplates.Select(template => LocalizeTemplate(template, localizer)).ToList();

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

    public static VehicleStarterBundleTemplate? FindMaintenanceTemplateByDisplayName(string value, IAppLocalizer localizer) =>
        GetMaintenanceTemplateCatalog(localizer).FirstOrDefault(item =>
            string.Equals(item.Title, value, StringComparison.Ordinal)
            || string.Equals(BuildMaintenanceTemplateDisplayName(item), value, StringComparison.Ordinal));

    public static IReadOnlyList<string> GetKnownTemplateTitleVariants(string title)
    {
        var key = ResolveTemplateKeyByAnyLocalizedTitle(title);
        if (key is null)
        {
            return [title];
        }

        return
        [
            title,
            EnglishTemplateLocalizer.GetString(key),
            CzechTemplateLocalizer.GetString(key)
        ];
    }

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

        return new VehicleStarterBundlePreview(
            vehicle.Id,
            vehicle.Name,
            BuildProfileLabel(vehicle, meta, _localizer),
            items.Select(item => LocalizeTemplate(item, _localizer)).ToList());
    }

    private static IReadOnlyList<VehicleStarterBundleTemplate> GetMissingMaintenanceTemplates(VehimapDataSet dataSet, Vehicle vehicle, VehicleMeta? meta)
    {
        var existingTitles = dataSet.MaintenancePlans
            .Where(item => string.Equals(item.VehicleId, vehicle.Id, StringComparison.Ordinal))
            .Select(item => NormalizeKey(item.Title))
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .ToHashSet(StringComparer.Ordinal);

        return GetRecommendedMaintenanceTemplates(vehicle, meta)
            .Where(template => !GetKnownTemplateTitleVariants(template.Title).Any(title => existingTitles.Contains(NormalizeKey(title))))
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
            .Where(template => !GetKnownTemplateTitleVariants(template.Title).Any(title => existingKeys.Contains(BuildRecordKey(template.RecordType, title))))
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
            .Where(template => !GetKnownTemplateTitleVariants(template.Title).Any(title => existingKeys.Contains(BuildReminderKey(title, template.RepeatMode))))
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
        var dueDate = today.AddDays(30).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
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

    private static VehicleStarterBundleTemplate LocalizeTemplate(VehicleStarterBundleTemplate template, IAppLocalizer localizer)
    {
        var sectionLabel = template.Section switch
        {
            VehicleStarterBundleSection.Maintenance => localizer.GetString("VehicleStarterBundle.Catalog.Section.Maintenance"),
            VehicleStarterBundleSection.Record => localizer.GetString("VehicleStarterBundle.Catalog.Section.Record"),
            VehicleStarterBundleSection.Reminder => localizer.GetString("VehicleStarterBundle.Catalog.Section.Reminder"),
            _ => template.SectionLabel
        };

        var titleKey = ResolveTemplateKeyByLegacyTitle(template.Title);
        var title = titleKey is null ? template.Title : localizer.GetString(titleKey);
        var noteKey = titleKey is null ? null : $"{titleKey}.Note";
        var note = noteKey is null ? template.Note : localizer.GetString(noteKey);
        var category = template.Section == VehicleStarterBundleSection.Maintenance
            ? LocalizeMaintenanceCategory(template.Category, localizer)
            : template.Category;
        var subcategory = template.Section == VehicleStarterBundleSection.Maintenance
            ? LocalizeMaintenanceSubcategory(template.Subcategory, localizer)
            : template.Subcategory;

        return template with
        {
            SectionLabel = sectionLabel,
            Title = title,
            Note = note,
            Category = category,
            Subcategory = subcategory
        };
    }

    private static string? ResolveTemplateKeyByLegacyTitle(string title)
    {
        if (MaintenanceTemplateKeys.TryGetValue(title, out var maintenanceKey))
        {
            return $"VehicleStarterBundle.Catalog.Maintenance.{maintenanceKey}.Title";
        }

        if (RecordTemplateKeys.TryGetValue(title, out var recordKey))
        {
            return $"VehicleStarterBundle.Catalog.Record.{recordKey}.Title";
        }

        return ReminderTemplateKeys.TryGetValue(title, out var reminderKey)
            ? $"VehicleStarterBundle.Catalog.Reminder.{reminderKey}.Title"
            : null;
    }

    private static string? ResolveTemplateKeyByAnyLocalizedTitle(string title)
    {
        var normalizedTitle = NormalizeKey(title);
        foreach (var legacyTitle in MaintenanceTemplateKeys.Keys.Concat(RecordTemplateKeys.Keys).Concat(ReminderTemplateKeys.Keys))
        {
            var key = ResolveTemplateKeyByLegacyTitle(legacyTitle);
            if (key is null)
            {
                continue;
            }

            if (NormalizeKey(legacyTitle) == normalizedTitle
                || NormalizeKey(EnglishTemplateLocalizer.GetString(key)) == normalizedTitle
                || NormalizeKey(CzechTemplateLocalizer.GetString(key)) == normalizedTitle)
            {
                return key;
            }
        }

        return null;
    }

    private static string LocalizeMaintenanceCategory(string value, IAppLocalizer localizer) =>
        value switch
        {
            "Servis" => localizer.GetString("VehicleStarterBundle.Catalog.Maintenance.Category.Service"),
            "Motor" => localizer.GetString("VehicleStarterBundle.Catalog.Maintenance.Category.Engine"),
            "Podvozek" => localizer.GetString("VehicleStarterBundle.Catalog.Maintenance.Category.Chassis"),
            "Výfukové potrubí" => localizer.GetString("VehicleStarterBundle.Catalog.Maintenance.Category.Exhaust"),
            "Elektronika" => localizer.GetString("VehicleStarterBundle.Catalog.Maintenance.Category.Electronics"),
            _ => value
        };

    private static string LocalizeMaintenanceSubcategory(string value, IAppLocalizer localizer) =>
        value switch
        {
            "Souhrn" => localizer.GetString("VehicleStarterBundle.Catalog.Maintenance.Subcategory.Summary"),
            "Olej a filtry" => localizer.GetString("VehicleStarterBundle.Catalog.Maintenance.Subcategory.OilFilters"),
            "Brzdy" => localizer.GetString("VehicleStarterBundle.Catalog.Maintenance.Subcategory.Brakes"),
            "Kapaliny" => localizer.GetString("VehicleStarterBundle.Catalog.Maintenance.Subcategory.Fluids"),
            "Rozvody" => localizer.GetString("VehicleStarterBundle.Catalog.Maintenance.Subcategory.Timing"),
            "Převody" => localizer.GetString("VehicleStarterBundle.Catalog.Maintenance.Subcategory.Transmission"),
            "Komfort" => localizer.GetString("VehicleStarterBundle.Catalog.Maintenance.Subcategory.Comfort"),
            "Zapalování a žhavení" => localizer.GetString("VehicleStarterBundle.Catalog.Maintenance.Subcategory.IgnitionGlow"),
            "Snímače" => localizer.GetString("VehicleStarterBundle.Catalog.Maintenance.Subcategory.Sensors"),
            "Sání a přeplňování" => localizer.GetString("VehicleStarterBundle.Catalog.Maintenance.Subcategory.IntakeBoost"),
            "Uložení a ramena" => localizer.GetString("VehicleStarterBundle.Catalog.Maintenance.Subcategory.MountsArms"),
            "Stabilizátor" => localizer.GetString("VehicleStarterBundle.Catalog.Maintenance.Subcategory.Stabilizer"),
            "Řízení a čepy" => localizer.GetString("VehicleStarterBundle.Catalog.Maintenance.Subcategory.SteeringJoints"),
            "Tlumení" => localizer.GetString("VehicleStarterBundle.Catalog.Maintenance.Subcategory.Damping"),
            "Emise" => localizer.GetString("VehicleStarterBundle.Catalog.Maintenance.Subcategory.Emissions"),
            "Koncové díly" => localizer.GetString("VehicleStarterBundle.Catalog.Maintenance.Subcategory.EndParts"),
            "Potrubí" => localizer.GetString("VehicleStarterBundle.Catalog.Maintenance.Subcategory.Pipes"),
            "Osvětlení" => localizer.GetString("VehicleStarterBundle.Catalog.Maintenance.Subcategory.Lighting"),
            "Napájení" => localizer.GetString("VehicleStarterBundle.Catalog.Maintenance.Subcategory.Power"),
            "Jištění" => localizer.GetString("VehicleStarterBundle.Catalog.Maintenance.Subcategory.Protection"),
            _ => value
        };

    private static string BuildProfileLabel(Vehicle vehicle, VehicleMeta? meta, IAppLocalizer localizer)
    {
        var parts = new List<string> { LocalizeVehicleCategory(NormalizeCategory(vehicle.Category), localizer) };
        var powertrain = ResolvePowertrain(vehicle, meta);
        if (!string.IsNullOrWhiteSpace(powertrain))
        {
            parts.Add(powertrain switch
            {
                "Benzín" => localizer.GetString("VehicleStarterBundle.Profile.Powertrain.Gasoline"),
                "Nafta" => localizer.GetString("VehicleStarterBundle.Profile.Powertrain.Diesel"),
                "Hybrid" => localizer.GetString("VehicleStarterBundle.Profile.Powertrain.Hybrid"),
                "Plug-in hybrid" => localizer.GetString("VehicleStarterBundle.Profile.Powertrain.PlugInHybrid"),
                "Elektro" => localizer.GetString("VehicleStarterBundle.Profile.Powertrain.Electric"),
                "LPG / CNG" => localizer.GetString("VehicleStarterBundle.Profile.Powertrain.LpgCng"),
                "Jiné" => localizer.GetString("VehicleStarterBundle.Profile.Powertrain.Other"),
                _ => powertrain.ToLowerInvariant()
            });
        }

        if (!string.IsNullOrWhiteSpace(meta?.ClimateProfile))
        {
            parts.Add(meta.ClimateProfile switch
            {
                "Má klimatizaci" => localizer.GetString("VehicleStarterBundle.Profile.Climate.Has"),
                "Bez klimatizace" => localizer.GetString("VehicleStarterBundle.Profile.Climate.None"),
                _ => meta.ClimateProfile.ToLowerInvariant()
            });
        }

        if (!string.IsNullOrWhiteSpace(meta?.TimingDrive))
        {
            parts.Add(meta.TimingDrive switch
            {
                "Řemen" => localizer.GetString("VehicleStarterBundle.Profile.Timing.Belt"),
                "Řetěz" => localizer.GetString("VehicleStarterBundle.Profile.Timing.Chain"),
                "Není relevantní" => localizer.GetString("VehicleStarterBundle.Profile.Timing.NotRelevant"),
                _ => localizer.Format("VehicleStarterBundle.Profile.Timing.Other", meta.TimingDrive.ToLowerInvariant())
            });
        }

        if (!string.IsNullOrWhiteSpace(meta?.Transmission))
        {
            parts.Add(meta.Transmission switch
            {
                "Manuální" => localizer.GetString("VehicleStarterBundle.Profile.Transmission.Manual"),
                "Automatická" => localizer.GetString("VehicleStarterBundle.Profile.Transmission.Automatic"),
                "Není relevantní" => localizer.GetString("VehicleStarterBundle.Profile.Transmission.NotRelevant"),
                _ => localizer.Format("VehicleStarterBundle.Profile.Transmission.Other", meta.Transmission.ToLowerInvariant())
            });
        }

        return string.Join(", ", parts.Where(static item => !string.IsNullOrWhiteSpace(item)));
    }

    private static string LocalizeVehicleCategory(string value, IAppLocalizer localizer) =>
        value switch
        {
            "Osobní vozidla" => localizer.GetString("VehicleStarterBundle.Profile.Category.PassengerVehicles"),
            "Motocykly" => localizer.GetString("VehicleStarterBundle.Profile.Category.Motorcycles"),
            "Nákladní vozidla" => localizer.GetString("VehicleStarterBundle.Profile.Category.Trucks"),
            "Autobusy" => localizer.GetString("VehicleStarterBundle.Profile.Category.Buses"),
            "Ostatní" => localizer.GetString("VehicleStarterBundle.Profile.Category.Other"),
            _ => value
        };

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
