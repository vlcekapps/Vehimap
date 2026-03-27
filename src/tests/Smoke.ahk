#Requires AutoHotkey v2.0

VehimapTestMode := true
#Include ..\Vehimap.ahk

RunSmokeTests()

RunSmokeTests() {
    failures := []
    tests := [
        "SmokeTestBuildInfo",
        "SmokeTestUpdateManifestFile",
        "SmokeTestUpdateManifestRoundTrip",
        "SmokeTestUpdateCheckAction",
        "SmokeTestUpdateHelperScript",
        "SmokeTestSemVerComparison",
        "SmokeTestGlobalSearch",
        "SmokeTestMaintenancePlans",
        "SmokeTestMaintenanceRecommendations",
        "SmokeTestMaintenanceRecommendationAction",
        "SmokeTestMaintenanceRecommendationOfferAfterCreate",
        "SmokeTestVehicleStarterBundleAction",
        "SmokeTestVehicleStarterBundleOfferAfterCreate",
        "SmokeTestVehicleMetaProfiles",
        "SmokeTestMaintenanceBackupRoundTrip",
        "SmokeTestRecordPathInfo",
        "SmokeTestFleetCostSummary",
        "SmokeTestVehicleCostComparison",
        "SmokeTestDashboardCosts",
        "SmokeTestDashboardProblemHighlights",
        "SmokeTestDashboardMaintenanceActions",
        "SmokeTestDashboardDataSummary",
        "SmokeTestDashboardEntries",
        "SmokeTestOverviewDataIssues",
        "SmokeTestSortSettings"
    ]

    for _, testName in tests {
        try {
            %testName%()
        } catch as err {
            failures.Push(testName ": line " err.Line ": " err.Message)
        }
    }

    if (failures.Length > 0) {
        WriteSmokeOutput("FAIL")
        for failure in failures {
            WriteSmokeOutput(failure)
        }
        ExitApp(1)
    }

    WriteSmokeOutput("PASS " tests.Length " smoke tests passed.")
    ExitApp(0)
}

SmokeTestBuildInfo() {
    global AppVersion, AppFileVersion, DataDir

    AssertEqual(GetAppVersion(), AppVersion, "Verze aplikace se ma cist z generovaneho build info.")
    AssertEqual(GetAppFileVersion(), AppFileVersion, "Souborova verze se ma cist z generovaneho build info.")

    aboutText := BuildAboutProgramText()
    AssertContains(aboutText, "Verze: " AppVersion, "O programu ma zobrazit aktualni verzi.")
    AssertContains(aboutText, "Datová složka: " DataDir, "O programu ma zobrazit datovou slozku.")
    AssertTrue(!InStr(aboutText, "Souborová verze"), "Stabilni build nema zobrazovat redundantni windows file version.")
}

SmokeTestUpdateManifestFile() {
    global AppVersion

    manifestPath := A_ScriptDir "\..\..\update\latest.ini"
    manifest := ReadLatestReleaseManifestFile(manifestPath)
    AssertEqual(manifest.version, AppVersion, "Lokalni update manifest ma odpovidat aktualni verzi aplikace.")
    AssertContains(manifest.assetUrl, "releases/download/", "Manifest ma obsahovat odkaz na release asset.")
    AssertContains(manifest.notesUrl, "releases/tag/", "Manifest ma obsahovat odkaz na release poznamky.")
}

SmokeTestUpdateManifestRoundTrip() {
    tempPath := A_Temp "\vehimap_manifest_roundtrip.ini"
    content := "[release]`n"
        . "version=1.2.3`n"
        . "published_at=2026-03-27T10:00:59Z`n"
        . "asset_url=https://example.com/releases/vehimap-1.2.3.zip`n"
        . "asset_sha256=341890872be1df557a1a65159ac63734c53b4288f8aee98cfe5c7d154613f736`n"
        . "asset_size=691749`n"
        . "notes_url=https://example.com/releases/v1.2.3`n"

    if FileExist(tempPath) {
        FileDelete(tempPath)
    }

    WriteTextFileUtf8NoBom(tempPath, content)
    manifest := ReadLatestReleaseManifestFile(tempPath)
    AssertEqual(manifest.version, "1.2.3", "Manifest zapsany bez BOM musi jit znovu nacist pres IniRead.")
    AssertEqual(manifest.assetSize, "691749", "Round-trip manifest ma zachovat velikost assetu.")
}

SmokeTestUpdateCheckAction() {
    global AppVersion, VehimapTestHooks

    originalVersion := AppVersion
    try {
        VehimapTestHooks := {
            messages: [],
            runs: [],
            msgBoxResults: [],
            updateManifest: {
                version: originalVersion,
                publishedAt: "2026-03-27T10:00:59Z",
                notesUrl: "https://example.com/releases/v" originalVersion,
                assetUrl: "https://example.com/download/vehimap-" originalVersion ".zip",
                assetSha256: "341890872be1df557a1a65159ac63734c53b4288f8aee98cfe5c7d154613f736",
                assetSize: "691749"
            }
        }

        CheckForUpdates()
        AssertEqual(VehimapTestHooks.messages.Length, 1, "Kontrola aktualizaci ma pro aktualni verzi zobrazit jednu hlasku.")
        AssertContains(VehimapTestHooks.messages[1].text, "Vehimap (" originalVersion ").", "Aktualni verze ma byt oznamena v dialogu.")
        AssertEqual(VehimapTestHooks.runs.Length, 0, "Pri aktualni verzi se nema otevirat zadna stranka.")
        AssertTrue(!VehimapTestHooks.HasOwnProp("exitRequested"), "Ve smoke testu se aplikace nema pokouset ukoncit.")

        AppVersion := "1.0.0"
        VehimapTestHooks := {
            messages: [],
            runs: [],
            msgBoxResults: ["Yes"],
            updateManifest: {
                version: originalVersion,
                publishedAt: "2026-03-27T10:00:59Z",
                notesUrl: "https://example.com/releases/v" originalVersion,
                assetUrl: "https://example.com/download/vehimap-" originalVersion ".zip",
                assetSha256: "341890872be1df557a1a65159ac63734c53b4288f8aee98cfe5c7d154613f736",
                assetSize: "2048"
            }
        }

        CheckForUpdates()
        AssertEqual(VehimapTestHooks.messages.Length, 1, "Pri novejsi verzi ma ve skriptovem rezimu probehnout jedno potvrzeni.")
        AssertContains(VehimapTestHooks.messages[1].text, "1.0.0", "Dialog ma obsahovat aktualni lokalni verzi.")
        AssertContains(VehimapTestHooks.messages[1].text, originalVersion, "Dialog ma obsahovat nejnovejsi dostupnou verzi.")
        AssertContains(VehimapTestHooks.messages[1].text, "vehimap.exe", "Dialog ma vysvetlit omezeni automaticke instalace ve skriptovem rezimu.")
        AssertContains(VehimapTestHooks.messages[1].text, "2,0 KB", "Dialog ma ukazat velikost update assetu.")
        AssertEqual(VehimapTestHooks.runs.Length, 1, "Po potvrzeni ma aplikace otevrit stranku vydani.")
        AssertContains(VehimapTestHooks.runs[1].command, "https://example.com/releases/v" originalVersion, "Otevrena ma byt URL release poznamek.")
        AssertTrue(!VehimapTestHooks.HasOwnProp("exitRequested"), "Ve skriptovem rezimu se nema vyzadovat ukonceni aplikace.")

        VehimapTestHooks := {
            messages: [],
            runs: [],
            msgBoxResults: [],
            updateManifestError: "manifest selhal"
        }

        CheckForUpdates()
        AssertEqual(VehimapTestHooks.messages.Length, 1, "Pri chybe manifestu ma byt zobrazena jedna chybova hlaska.")
        AssertContains(VehimapTestHooks.messages[1].text, "manifest selhal", "Chybovy dialog ma obsahovat duvod selhani.")
        AssertEqual(VehimapTestHooks.runs.Length, 0, "Pri chybe manifestu se nema otevirat zadna stranka.")
    } finally {
        AppVersion := originalVersion
        VehimapTestHooks := 0
    }
}

SmokeTestUpdateHelperScript() {
    helperScript := BuildUpdateHelperPowerShellScript()

    AssertContains(helperScript, "Invoke-WebRequest", "Helper musi umet stahnout asset.")
    AssertContains(helperScript, "Get-FileHash", "Helper musi overovat SHA-256 hash.")
    AssertContains(helperScript, "Expand-Archive", "Helper musi rozbalit release zip.")
    AssertContains(helperScript, "Wait-Process", "Helper musi pockat na ukonceni bezici aplikace.")
    AssertContains(helperScript, "Start-Process", "Helper musi po instalaci znovu spustit Vehimap.")
    AssertContains(helperScript, "@('changelog.html', 'readme.html', 'vehimap.exe')", "Helper musi overovat novy obsah assetu.")
    AssertContains(helperScript, "legacyReadmeText", "Helper ma po prechodu uklidit stary readme.txt.")
}

SmokeTestSemVerComparison() {
    AssertEqual(CompareSemVer("1.0.0", "1.0.0"), 0, "Stejne verze se musi vyhodnotit jako shodne.")
    AssertEqual(CompareSemVer("1.0.1", "1.0.0"), 1, "Vyssi patch verze musi byt novejsi.")
    AssertEqual(CompareSemVer("1.1.0", "1.2.0"), -1, "Nizsi minor verze musi byt starsi.")
    AssertEqual(CompareSemVer("1.0.0-beta.1", "1.0.0"), -1, "Prerelease musi byt starsi nez final.")
    AssertEqual(CompareSemVer("1.0.0-beta.2", "1.0.0-beta.1"), 1, "Vyssi prerelease poradi musi byt novejsi.")
}

SmokeTestGlobalSearch() {
    global Vehicles, VehicleHistory, VehicleFuelLog, VehicleRecords, VehicleMetaEntries, VehicleReminders, VehicleMaintenancePlans, SettingsFile

    ResetSmokeData()
    tempSettings := A_Temp "\vehimap_smoke_global_search_settings.ini"
    try FileDelete(tempSettings)
    SettingsFile := tempSettings
    EnsureSettingsDefaults()
    Vehicles := [
        {
            id: "veh_1",
            name: "Alfa test",
            category: "Osobní vozidla",
            vehicleNote: "Hatchback",
            makeModel: "Skoda Scala",
            plate: "1AB2345",
            year: "2021",
            power: "85",
            lastTk: "03/2025",
            nextTk: "03/2027",
            greenCardFrom: "04/2025",
            greenCardTo: "04/2026"
        }
    ]
    VehicleMetaEntries := [{vehicleId: "veh_1", state: "Běžný provoz", tags: "alfa, test"}]
    VehicleHistory := [{id: "hist_1", vehicleId: "veh_1", eventDate: "10.01." FormatTime(A_Now, "yyyy"), eventType: "Alfa servis", odometer: "15000", cost: "1200", note: "kontrola"}]
    VehicleFuelLog := [{id: "fuel_1", vehicleId: "veh_1", entryDate: "11.01." FormatTime(A_Now, "yyyy"), odometer: "15100", liters: "42", totalCost: "1800", fullTank: 1, fuelType: "Benzin", note: "alfa pumpa"}]
    VehicleRecords := [{id: "record_1", vehicleId: "veh_1", recordType: "Doklad", title: "Alfa smlouva", provider: "Kooperativa", validFrom: "01/" FormatTime(A_Now, "yyyy"), validTo: "12/" FormatTime(A_Now, "yyyy"), price: "900", filePath: "C:\\Temp\\alfa.pdf", note: "archiv"}]
    VehicleReminders := [{id: "rem_1", vehicleId: "veh_1", title: "Alfa připomínka", dueDate: "12.01." FormatTime(A_Now, "yyyy"), reminderDays: "20", repeatMode: "Neopakovat", note: "zavolat"}]

    VehicleMaintenancePlans := [{id: "mnt_1", vehicleId: "veh_1", title: "Alfa olej", intervalKm: "15000", intervalMonths: "12", lastServiceDate: "01.01." FormatTime(A_Now, "yyyy"), lastServiceOdometer: "10000", isActive: 1, note: "alfa interval"}]

    results := BuildGlobalSearchResults("alfa")
    AssertTrue(results.Length >= 5, "Globální hledání mělo vrátit alespoň 5 výsledků.")
    AssertSearchKindPresent(results, "vehicle")
    AssertSearchKindPresent(results, "history")
    AssertSearchKindPresent(results, "fuel")
    AssertSearchKindPresent(results, "record")
    AssertSearchKindPresent(results, "reminder")
    AssertSearchKindPresent(results, "maintenance")
}

SmokeTestMaintenancePlans() {
    global Vehicles, VehicleFuelLog, VehicleMaintenancePlans, SettingsFile

    ResetSmokeData()
    tempSettings := A_Temp "\vehimap_smoke_maintenance_settings.ini"
    try FileDelete(tempSettings)
    SettingsFile := tempSettings
    EnsureSettingsDefaults()
    IniWrite("31", SettingsFile, "notifications", "maintenance_reminder_days")
    IniWrite("1000", SettingsFile, "notifications", "maintenance_reminder_km")

    currentYear := FormatTime(A_Now, "yyyy")
    Vehicles := [
        {
            id: "veh_1",
            name: "Servis test",
            category: "Osobní vozidla",
            vehicleNote: "Kombi",
            makeModel: "Skoda Octavia",
            plate: "1AB2345",
            year: "2020",
            power: "110",
            lastTk: "03/2024",
            nextTk: "03/2028",
            greenCardFrom: "04/2025",
            greenCardTo: "04/2027"
        }
    ]
    VehicleFuelLog := [
        {id: "fuel_1", vehicleId: "veh_1", entryDate: "20.03." currentYear, odometer: "14650", liters: "35", totalCost: "1700", fullTank: 1, fuelType: "Benzin", note: ""}
    ]
    VehicleMaintenancePlans := [
        {id: "mnt_1", vehicleId: "veh_1", title: "Motorový olej", intervalKm: "5000", intervalMonths: "12", lastServiceDate: "01.04." currentYear, lastServiceOdometer: "10000", isActive: 1, note: "Pravidelný servis"}
    ]

    snapshot := BuildVehicleMaintenancePlanSnapshot(VehicleMaintenancePlans[1], Vehicles[1])
    AssertTrue((snapshot.nextOdometer + 0) = 15000, "Plán údržby má správně dopočítat příští limit tachometru.")
    AssertTrue((snapshot.remainingKm + 0) = 350, "Plán údržby má správně dopočítat zbývající kilometry.")
    AssertContains(snapshot.nextServiceText, FormatHistoryOdometer("15000"), "Další servis má obsahovat dopočítaný stav tachometru.")
    AssertContains(snapshot.statusText, "350 km", "Stav servisního plánu má upozornit na blížící se limit kilometrů.")

    summaryText := BuildVehicleMaintenanceSummaryText("veh_1")
    AssertContains(summaryText, "Plánů údržby: 1. Aktivních: 1.", "Souhrn detailu vozidla má uvést počet servisních plánů.")
    AssertContains(summaryText, "Motorový olej", "Souhrn detailu vozidla má zmínit nejbližší servisní úkon.")

    upcoming := GetUpcomingVehicleMaintenance("veh_1")
    AssertEqual(upcoming.Length, 1, "Vozidlo s blížícím se servisem má být v seznamu údržby právě jednou.")
    AssertEqual(upcoming[1].kind, "maintenance", "Blížící se servis má být označen jako maintenance.")

    overviewEntries := BuildUpcomingOverviewEntries(true, false)
    maintenanceEntries := FilterUpcomingOverviewEntries(overviewEntries, "maintenance")
    AssertEqual(maintenanceEntries.Length, 1, "Přehled termínů má zobrazit blížící se servisní úkon.")
    AssertDashboardEntryPresent(maintenanceEntries, "maintenance", "veh_1")

    dashboardText := BuildDashboardTermSummaryText()
    AssertContains(dashboardText, "servisních úkonů", "Dashboard má v souhrnu termínů zohlednit údržbu.")
}

SmokeTestMaintenanceRecommendations() {
    global Vehicles, VehicleMetaEntries, VehicleMaintenancePlans

    ResetSmokeData()
    Vehicles := [
        {
            id: "veh_1",
            name: "Doporučený profil",
            category: "Osobní vozidla",
            vehicleNote: "Rodinné kombi",
            makeModel: "Škoda Octavia",
            plate: "1AB2345",
            year: "2021",
            power: "110",
            lastTk: "03/2025",
            nextTk: "03/2027",
            greenCardFrom: "04/2025",
            greenCardTo: "04/2026"
        }
    ]
    VehicleMetaEntries := [
        {vehicleId: "veh_1", state: "Běžný provoz", tags: "rodina", powertrain: "Nafta", climateProfile: "Má klimatizaci", timingDrive: "Řetěz", transmission: "Manuální"}
    ]
    VehicleMaintenancePlans := [
        {id: "mnt_1", vehicleId: "veh_1", title: "Motorový olej a filtr", intervalKm: "15000", intervalMonths: "12", lastServiceDate: "", lastServiceOdometer: "", isActive: 1, note: ""}
    ]

    preview := GetVehicleMaintenanceRecommendationPreview("veh_1")
    AssertContains(preview.profileLabel, "Osobní vozidla", "Profil doporučení má vycházet z kategorie vozidla.")
    AssertContains(preview.profileLabel, "naftový pohon", "Profil doporučení má respektovat explicitně zadaný pohon.")
    AssertContains(preview.profileLabel, "má klimatizaci", "Profil doporučení má ukázat i explicitně zadanou klimatizaci.")
    AssertContains(preview.profileLabel, "rozvody řetězem", "Profil doporučení má ukázat i explicitní typ rozvodů.")
    AssertEqual(preview.existingCount, 1, "Už existující doporučený plán se má odfiltrovat.")
    AssertTrue(HasMaintenanceTemplateWithTitle(preview.missing, "Palivový filtr"), "Nafta má mezi doporučenými šablonami dostat i palivový filtr.")
    AssertTrue(HasMaintenanceTemplateWithTitle(preview.missing, "Klimatizace a dezinfekce"), "Vozidlo s klimatizací má dostat i klimatizační doporučení.")
    AssertTrue(!HasMaintenanceTemplateWithTitle(preview.missing, "Rozvody"), "Řetězové rozvody se nemají znovu doporučovat jako pravidelný řemenový servis.")
    AssertTrue(!HasMaintenanceTemplateWithTitle(preview.missing, "Převodový olej"), "Explicitně manuální převodovka nemá dostat automatické doporučení převodového oleje.")
    AssertTrue(!HasMaintenanceTemplateWithTitle(preview.missing, "Motorový olej a filtr"), "Už existující plán se nesmí znovu nabízet.")

    applied := AddVehicleMaintenanceRecommendedTemplates("veh_1", preview.missing, false)
    AssertTrue(applied.addedPlans.Length >= 1, "Doporučené šablony se mají umět přidat do paměti bez uložení.")
    AssertEqual(CountMaintenancePlansByTitle("veh_1", "Palivový filtr"), 1, "Palivový filtr se má po přidání objevit mezi plány právě jednou.")

    previewAfter := GetVehicleMaintenanceRecommendationPreview("veh_1")
    AssertEqual(previewAfter.missing.Length, 0, "Po doplnění doporučených plánů už nemají zbýt další chybějící šablony.")
}
SmokeTestMaintenanceRecommendationAction() {
    global Vehicles, VehicleMetaEntries, VehicleMaintenancePlans, MaintenanceVehicleId, MaintenancePlansFile, VehimapTestHooks

    ResetSmokeData()
    originalMaintenancePlansFile := MaintenancePlansFile
    tempMaintenanceFile := A_Temp "\vehimap_smoke_maintenance_recommend_action.tsv"
    try FileDelete(tempMaintenanceFile)
    MaintenancePlansFile := tempMaintenanceFile
    Vehicles := [
        {
            id: "veh_1",
            name: "Akční profil",
            category: "Osobní vozidla",
            vehicleNote: "Rodinné auto",
            makeModel: "Volkswagen Passat",
            plate: "2AB3456",
            year: "2022",
            power: "110",
            lastTk: "05/2025",
            nextTk: "05/2027",
            greenCardFrom: "05/2025",
            greenCardTo: "05/2026"
        }
    ]
    VehicleMetaEntries := [
        {vehicleId: "veh_1", state: "Běžný provoz", tags: "test", powertrain: "Nafta", climateProfile: "Má klimatizaci", timingDrive: "Řemen", transmission: "Automatická"}
    ]
    VehicleMaintenancePlans := []
    MaintenanceVehicleId := "veh_1"

    try {
        VehimapTestHooks := {
            messages: [],
            maintenanceRecommendationSelection: {
                selectedTitles: ["Palivový filtr", "Brzdová kapalina"],
                updates: [
                    {matchTitle: "Palivový filtr", title: "Palivový filtr diesel", note: "Upraveno ve výběru"}
                ]
            }
        }

        AddRecommendedVehicleMaintenancePlans()
        AssertTrue(VehimapTestHooks.HasOwnProp("maintenanceRecommendationOpened"), "Akce doporučených šablon má otevřít výběrový dialog.")
        AssertContains(VehimapTestHooks.maintenanceRecommendationOpened.profileLabel, "naftový pohon", "Výběrový dialog má nést profil doporučení.")
        AssertTrue(HasMaintenanceTemplateWithTitleByText(VehimapTestHooks.maintenanceRecommendationOpened.titles, "Palivový filtr"), "Výběrový dialog má nabídnout i dieselové doporučení.")
        AssertEqual(VehimapTestHooks.messages.Length, 1, "Po potvrzení výběru má být zobrazen jen výsledek přidání.")
        AssertContains(VehimapTestHooks.messages[1].text, "Přidáno doporučených plánů: 2.", "Výsledek má respektovat jen ručně vybraný počet šablon.")
        AssertTrue(CountMaintenancePlansByTitle("veh_1", "Palivový filtr diesel") = 1, "Akce doporučených šablon má uložit i ručně upravený název.")
        AssertTrue(CountMaintenancePlansByTitle("veh_1", "Motorový olej a filtr") = 0, "Nevybraná doporučená šablona se nesmí přidat.")

        VehimapTestHooks := {
            messages: [],
            maintenanceRecommendationSelection: {
                cancel: true
            }
        }
        AddRecommendedVehicleMaintenancePlans()
        AssertEqual(VehimapTestHooks.messages.Length, 0, "Při zrušení výběrového dialogu se nemá zobrazit výsledek ani chyba.")

        VehimapTestHooks := {
            messages: [],
            maintenanceRecommendationSelection: {}
        }
        AddRecommendedVehicleMaintenancePlans()
        AssertEqual(VehimapTestHooks.messages.Length, 1, "Při doplnění zbytku doporučených šablon má být zobrazen výsledek přidání.")

        VehimapTestHooks := {
            messages: []
        }
        AddRecommendedVehicleMaintenancePlans()
        AssertEqual(VehimapTestHooks.messages.Length, 1, "Po doplnění všech doporučených šablon má zůstat jen informační hláška.")
        AssertContains(VehimapTestHooks.messages[1].text, "už nenašel žádné další doporučené servisní šablony", "Informační hláška má vysvětlit, že už nic nechybí.")
    } finally {
        VehimapTestHooks := 0
        MaintenanceVehicleId := ""
        MaintenancePlansFile := originalMaintenancePlansFile
    }
}

SmokeTestMaintenanceRecommendationOfferAfterCreate() {
    global Vehicles, VehicleMetaEntries, VehimapTestHooks

    ResetSmokeData()
    Vehicles := [
        {
            id: "veh_1",
            name: "Nové vozidlo",
            category: "Osobní vozidla",
            vehicleNote: "Rodinné auto",
            makeModel: "Volkswagen Passat",
            plate: "2AB3456",
            year: "2022",
            power: "110",
            lastTk: "05/2025",
            nextTk: "05/2027",
            greenCardFrom: "05/2025",
            greenCardTo: "05/2026"
        }
    ]
    VehicleMetaEntries := [
        {vehicleId: "veh_1", state: "Běžný provoz", tags: "test", powertrain: "Nafta", climateProfile: "Má klimatizaci", timingDrive: "Řemen", transmission: "Automatická"}
    ]

    try {
        VehimapTestHooks := {
            messages: [],
            msgBoxResults: ["Yes"],
            skipPostCreateMaintenanceRecommendationOpen: true
        }

        AssertTrue(OfferVehicleMaintenanceRecommendationsAfterCreate("veh_1"), "Po vytvoření vozidla má jít otevřít nabídka doporučených servisních šablon.")
        AssertEqual(VehimapTestHooks.messages.Length, 1, "Po vytvoření vozidla se má zobrazit jedna nabídka doporučených šablon.")
        AssertContains(VehimapTestHooks.messages[1].text, "doporučených servisních šablon", "Nabídka po vytvoření vozidla má vysvětlit další krok.")
        AssertTrue(VehimapTestHooks.HasOwnProp("postCreateMaintenanceRecommendation"), "Kladná volba má otevřít doporučené šablony i z toku založení vozidla.")
        AssertContains(VehimapTestHooks.postCreateMaintenanceRecommendation.profileLabel, "naftový pohon", "Nabídka po vytvoření vozidla má nést správný servisní profil.")

        VehimapTestHooks := {
            messages: [],
            msgBoxResults: ["No"],
            skipPostCreateMaintenanceRecommendationOpen: true
        }
        AssertTrue(!OfferVehicleMaintenanceRecommendationsAfterCreate("veh_1"), "Při odmítnutí nabídky se doporučené šablony nemají otevřít.")
        AssertEqual(VehimapTestHooks.messages.Length, 1, "I odmítnutá nabídka má zobrazit jeden potvrzovací dialog.")
        AssertTrue(!VehimapTestHooks.HasOwnProp("postCreateMaintenanceRecommendation"), "Záporná volba nesmí otevřít doporučené šablony.")

        VehimapTestHooks := {
            messages: []
        }
        AssertTrue(!OfferVehicleMaintenanceRecommendationsAfterCreate("missing"), "Neexistující vozidlo nesmí nabídku servisních šablon otevírat.")
        AssertEqual(VehimapTestHooks.messages.Length, 0, "Pro neexistující vozidlo se nemá zobrazit žádná hláška.")
    } finally {
        VehimapTestHooks := 0
    }
}

SmokeTestVehicleStarterBundleAction() {
    global Vehicles, VehicleMetaEntries, VehicleMaintenancePlans, VehicleRecords, VehicleReminders, VehimapTestHooks, MaintenancePlansFile, RecordsFile, RemindersFile

    ResetSmokeData()
    currentYear := FormatTime(A_Now, "yyyy")
    originalMaintenancePlansFile := MaintenancePlansFile
    originalRecordsFile := RecordsFile
    originalRemindersFile := RemindersFile
    tempRoot := A_Temp "\vehimap_smoke_vehicle_bundle_action"
    if DirExist(tempRoot) {
        DirDelete(tempRoot, true)
    }
    DirCreate(tempRoot)
    MaintenancePlansFile := tempRoot "\maintenance.tsv"
    RecordsFile := tempRoot "\records.tsv"
    RemindersFile := tempRoot "\reminders.tsv"
    Vehicles := [
        {
            id: "veh_1",
            name: "Balíček test",
            category: "Osobní vozidla",
            vehicleNote: "Rodinné auto",
            makeModel: "Volkswagen Passat",
            plate: "2AB3456",
            year: "2022",
            power: "110",
            lastTk: "05/2025",
            nextTk: "05/2027",
            greenCardFrom: "05/2025",
            greenCardTo: "05/2026"
        }
    ]
    VehicleMetaEntries := [
        {vehicleId: "veh_1", state: "Běžný provoz", tags: "test", powertrain: "Nafta", climateProfile: "Má klimatizaci", timingDrive: "Řemen", transmission: "Automatická"}
    ]
    VehicleMaintenancePlans := []
    VehicleRecords := []
    VehicleReminders := []

    try {
        VehimapTestHooks := {
            messages: [],
            vehicleStarterBundleSelection: {
                selectedKeys: [
                    BuildVehicleStarterBundleDraftMatchKey("maintenance", "Motorový olej a filtr"),
                    BuildVehicleStarterBundleDraftMatchKey("record", "Povinné ručení"),
                    BuildVehicleStarterBundleDraftMatchKey("reminder", "Pravidelná kontrola stavu vozidla")
                ],
                updates: [
                    {section: "maintenance", matchTitle: "Motorový olej a filtr", title: "Motorový olej roční", note: "Upraveno v balíčku"},
                    {section: "record", matchTitle: "Povinné ručení", provider: "Kooperativa", validTo: "12/" currentYear, note: "Doplnit číslo smlouvy"},
                    {section: "reminder", matchTitle: "Pravidelná kontrola stavu vozidla", title: "Roční kontrola stavu", dueDate: "15.05." currentYear, note: "Připomenout jarní kontrolu"}
                ]
            }
        }

        AssertTrue(OpenVehicleStarterBundleForVehicle(Vehicles[1]), "Ruční akce balíčku pro vozidlo má otevřít výběr doporučených položek.")
        AssertTrue(VehimapTestHooks.HasOwnProp("vehicleStarterBundleOpened"), "Otevření balíčku má registrovat nabídnuté položky do test hooku.")
        AssertContains(VehimapTestHooks.vehicleStarterBundleOpened.profileLabel, "naftový pohon", "Balíček má nést i servisní profil vozidla.")
        AssertTrue(HasMaintenanceTemplateWithTitleByText(VehimapTestHooks.vehicleStarterBundleOpened.recordTitles, "Povinné ručení"), "Balíček má nabídnout i placeholder dokladu povinného ručení.")
        AssertTrue(HasMaintenanceTemplateWithTitleByText(VehimapTestHooks.vehicleStarterBundleOpened.reminderTitles, "Pravidelná kontrola stavu vozidla"), "Balíček má nabídnout i obecnou připomínku podle kategorie.")
        AssertEqual(VehimapTestHooks.messages.Length, 1, "Po potvrzení výběru balíčku má být zobrazena právě jedna výsledková hláška.")
        AssertContains(VehimapTestHooks.messages[1].text, "Přidáno servisních plánů: 1.", "Balíček má respektovat ručně vybraný počet servisních plánů.")
        AssertContains(VehimapTestHooks.messages[1].text, "Přidáno dokladů: 1.", "Balíček má respektovat ručně vybraný počet dokladů.")
        AssertContains(VehimapTestHooks.messages[1].text, "Přidáno připomínek: 1.", "Balíček má respektovat ručně vybraný počet připomínek.")
        AssertEqual(CountMaintenancePlansByTitle("veh_1", "Motorový olej roční"), 1, "Balíček má uložit i ručně upravený servisní plán.")
        AssertEqual(CountVehicleRecordsByTitle("veh_1", "Povinné ručení"), 1, "Balíček má vytvořit vybraný placeholder dokladu.")
        AssertEqual(CountVehicleRemindersByTitle("veh_1", "Roční kontrola stavu"), 1, "Balíček má vytvořit i ručně přejmenovanou připomínku.")
        AssertEqual(CountVehicleRecordsByTitle("veh_1", "Havarijní pojištění"), 0, "Nevybraný doklad se nesmí do balíčku přidat.")
        AssertEqual(CountVehicleRemindersByTitle("veh_1", "Pravidelná kontrola stavu vozidla"), 0, "Nevybraná výchozí připomínka se nesmí uložit pod původním názvem.")
    } finally {
        VehimapTestHooks := 0
        MaintenancePlansFile := originalMaintenancePlansFile
        RecordsFile := originalRecordsFile
        RemindersFile := originalRemindersFile
    }
}

SmokeTestVehicleStarterBundleOfferAfterCreate() {
    global Vehicles, VehicleMetaEntries, VehimapTestHooks

    ResetSmokeData()
    Vehicles := [
        {
            id: "veh_1",
            name: "Nové vozidlo pro balíček",
            category: "Osobní vozidla",
            vehicleNote: "Rodinné auto",
            makeModel: "Volkswagen Passat",
            plate: "2AB3456",
            year: "2022",
            power: "110",
            lastTk: "05/2025",
            nextTk: "05/2027",
            greenCardFrom: "05/2025",
            greenCardTo: "05/2026"
        }
    ]
    VehicleMetaEntries := [
        {vehicleId: "veh_1", state: "Běžný provoz", tags: "test", powertrain: "Nafta", climateProfile: "Má klimatizaci", timingDrive: "Řemen", transmission: "Automatická"}
    ]

    try {
        VehimapTestHooks := {
            messages: [],
            msgBoxResults: ["Yes"],
            skipPostCreateVehicleBundleOpen: true
        }

        AssertTrue(OfferVehicleStarterBundleAfterCreate("veh_1"), "Po vytvoření vozidla má jít nabídnout celý balíček doporučených položek.")
        AssertEqual(VehimapTestHooks.messages.Length, 1, "Po vytvoření vozidla se má zobrazit právě jedna nabídka balíčku.")
        AssertContains(VehimapTestHooks.messages[1].text, "balíček doporučených položek", "Nabídka po vytvoření vozidla má vysvětlit další krok.")
        AssertContains(VehimapTestHooks.messages[1].text, "Doklady: 3.", "Nabídka má uvést počet doporučených placeholderů dokladů.")
        AssertContains(VehimapTestHooks.messages[1].text, "Připomínky: 1.", "Nabídka má uvést i počet doporučených připomínek.")
        AssertTrue(VehimapTestHooks.HasOwnProp("postCreateVehicleBundle"), "Kladná volba má otevřít balíček i z toku založení vozidla.")
        AssertContains(VehimapTestHooks.postCreateVehicleBundle.profileLabel, "naftový pohon", "Balíček po vytvoření vozidla má nést správný servisní profil.")
        AssertEqual(VehimapTestHooks.postCreateVehicleBundle.recordCount, 3, "Silniční vozidlo má po vytvoření nabídnout tři základní placeholdery dokladů.")
        AssertEqual(VehimapTestHooks.postCreateVehicleBundle.reminderCount, 1, "Po vytvoření vozidla má být nabídnuta jedna obecná připomínka.")
        AssertTrue(VehimapTestHooks.postCreateVehicleBundle.maintenanceCount >= 1, "Po vytvoření vozidla má být nabídnut alespoň jeden servisní plán.")

        VehimapTestHooks := {
            messages: [],
            msgBoxResults: ["No"],
            skipPostCreateVehicleBundleOpen: true
        }
        AssertTrue(!OfferVehicleStarterBundleAfterCreate("veh_1"), "Při odmítnutí nabídky se balíček nemá otevřít.")
        AssertEqual(VehimapTestHooks.messages.Length, 1, "I odmítnutá nabídka má zobrazit jeden potvrzovací dialog.")
        AssertTrue(!VehimapTestHooks.HasOwnProp("postCreateVehicleBundle"), "Záporná volba nesmí otevřít balíček.")

        VehimapTestHooks := {
            messages: []
        }
        AssertTrue(!OfferVehicleStarterBundleAfterCreate("missing"), "Neexistující vozidlo nesmí balíček po vytvoření otevírat.")
        AssertEqual(VehimapTestHooks.messages.Length, 0, "Pro neexistující vozidlo se nemá zobrazit žádná hláška.")
    } finally {
        VehimapTestHooks := 0
    }
}

SmokeTestVehicleMetaProfiles() {
    global VehicleMetaEntries

    ResetSmokeData()
    VehicleMetaEntries := [
        {vehicleId: "veh_1", state: "Běžný provoz", tags: "rodina, test", powertrain: "Nafta", climateProfile: "Má klimatizaci", timingDrive: "Řetěz", transmission: "Automatická"}
    ]

    metaContent := BuildVehicleMetaDataContent()
    AssertContains(metaContent, "# Vehimap meta v2", "Servisní profil vozidla se má ukládat do meta formátu v2.")
    AssertContains(metaContent, "Nafta", "Meta obsah má ukládat i pohon vozidla.")

    loadedMeta := []
    errorMessage := ""
    AssertTrue(TryParseVehicleMetaBackupContent(metaContent, &loadedMeta, &errorMessage), "Meta sekce se musí umět znovu načíst.")
    AssertEqual(loadedMeta.Length, 1, "Ze servisního profilu se má načíst jedna položka.")
    AssertEqual(loadedMeta[1].powertrain, "Nafta", "Pohon se musí po round-tripu zachovat.")
    AssertEqual(loadedMeta[1].climateProfile, "Má klimatizaci", "Klimatizace se musí po round-tripu zachovat.")
    AssertEqual(loadedMeta[1].timingDrive, "Řetěz", "Typ rozvodů se musí po round-tripu zachovat.")
    AssertEqual(loadedMeta[1].transmission, "Automatická", "Převodovka se musí po round-tripu zachovat.")

    legacyMeta := "# Vehimap meta v1`nveh_2`tVeterán`tarchiv`n"
    loadedLegacy := []
    AssertTrue(TryParseVehicleMetaBackupContent(legacyMeta, &loadedLegacy, &errorMessage), "Starý meta formát v1 musí jít stále načíst.")
    AssertEqual(loadedLegacy[1].powertrain, "", "Starý meta formát má doplnit nové servisní pole prázdnými hodnotami.")
}
SmokeTestMaintenanceBackupRoundTrip() {
    global Vehicles, VehicleMaintenancePlans, SettingsFile

    ResetSmokeData()
    tempSettings := A_Temp "\vehimap_smoke_maintenance_backup_settings.ini"
    try FileDelete(tempSettings)
    SettingsFile := tempSettings
    EnsureSettingsDefaults()

    Vehicles := [
        {
            id: "veh_1",
            name: "Backup test",
            category: "Osobní vozidla",
                vehicleNote: "",
            makeModel: "Skoda Fabia",
            plate: "1AB2345",
            year: "",
            power: "",
            lastTk: "",
            nextTk: "03/2028",
            greenCardFrom: "",
            greenCardTo: "04/2027"
        }
    ]
    VehicleMaintenancePlans := [
        {id: "mnt_1", vehicleId: "veh_1", title: "Motorový olej", intervalKm: "15000", intervalMonths: "12", lastServiceDate: "12.02.2026", lastServiceOdometer: "120000", isActive: 1, note: "Kontrola po zimě"}
    ]

    backupContent := BuildCurrentBackupContent()
    AssertContains(backupContent, "# Vehimap backup v5", "Záloha s údržbou se má ukládat jako formát v5.")
    AssertContains(backupContent, "# Vehimap maintenance v1", "Záloha má obsahovat sekci plánů údržby.")

    settingsContent := ""
    vehiclesContent := ""
    historyContent := ""
    fuelContent := ""
    recordsContent := ""
    metaContent := ""
    remindersContent := ""
    maintenanceContent := ""
    errorMessage := ""
    loadedPlans := []

    AssertTrue(
        TryParseBackupContent(backupContent, &settingsContent, &vehiclesContent, &historyContent, &fuelContent, &recordsContent, &metaContent, &remindersContent, &maintenanceContent, &errorMessage),
        "Zálohu s plány údržby musí jít znovu načíst."
    )
    AssertContains(maintenanceContent, "Motorový olej", "Načtená maintenance sekce má obsahovat uložený servisní úkon.")
    AssertTrue(TryParseVehicleMaintenancePlansBackupContent(maintenanceContent, &loadedPlans, &errorMessage), "Maintenance sekce ze zálohy musí jít samostatně parseovat.")
    AssertEqual(loadedPlans.Length, 1, "Ze zálohy se má načíst jeden servisní plán.")
    AssertEqual(loadedPlans[1].title, "Motorový olej", "Název servisního plánu se musí v záloze zachovat.")
}

SmokeTestRecordPathInfo() {
    tempRoot := A_Temp "\vehimap_smoke_paths"
    tempFile := tempRoot "\priloha.pdf"
    missingFile := tempRoot "\chybi.pdf"
    missingFolder := tempRoot "\neni\chybi.pdf"

    if DirExist(tempRoot) {
        DirDelete(tempRoot, true)
    }
    DirCreate(tempRoot)
    FileAppend("ok", tempFile, "UTF-8")

    AssertEqual(GetVehicleRecordPathInfo({filePath: tempFile}).kind, "file", "Existující soubor má být rozpoznaný jako soubor.")
    AssertEqual(GetVehicleRecordPathInfo({filePath: tempRoot}).kind, "folder", "Existující složka má být rozpoznaná jako složka.")
    AssertEqual(GetVehicleRecordPathInfo({filePath: missingFile}).kind, "missing_file", "Chybějící soubor ve známé složce má být rozpoznaný.")
    AssertEqual(GetVehicleRecordPathInfo({filePath: missingFolder}).kind, "missing_folder", "Chybějící složka má být rozpoznaná.")
    AssertEqual(GetVehicleRecordPathInfo({filePath: ""}).kind, "empty", "Prázdná cesta má být rozpoznaná jako empty.")
}

SmokeTestFleetCostSummary() {
    global Vehicles, VehicleHistory, VehicleFuelLog, VehicleRecords, VehicleMetaEntries

    ResetSmokeData()
    currentYear := FormatTime(A_Now, "yyyy")
    Vehicles := [
        {id: "veh_1", name: "Skoda", category: "Osobní vozidla", vehicleNote: "", makeModel: "", plate: "1AB2345", year: "", power: "", lastTk: "", nextTk: "03/2027", greenCardFrom: "", greenCardTo: "04/2026"},
        {id: "veh_2", name: "Yamaha", category: "Motocykly", vehicleNote: "", makeModel: "", plate: "2AB3456", year: "", power: "", lastTk: "", nextTk: "03/2027", greenCardFrom: "", greenCardTo: "04/2026"},
        {id: "veh_3", name: "Transit", category: "Nákladní vozidla", vehicleNote: "", makeModel: "", plate: "3AB4567", year: "", power: "", lastTk: "", nextTk: "03/2027", greenCardFrom: "", greenCardTo: "04/2026"},
        {id: "veh_4", name: "Bez nákladu", category: "Osobní vozidla", vehicleNote: "", makeModel: "", plate: "", year: "", power: "", lastTk: "", nextTk: "", greenCardFrom: "", greenCardTo: ""}
    ]
    VehicleMetaEntries := [
        {vehicleId: "veh_3", state: "Archiv", tags: ""}
    ]
    VehicleFuelLog := [
        {id: "fuel_1", vehicleId: "veh_1", entryDate: "01.02." currentYear, odometer: "10000", liters: "40", totalCost: "1000", fullTank: 1, fuelType: "Benzin", note: ""},
        {id: "fuel_2", vehicleId: "veh_2", entryDate: "03.02." currentYear, odometer: "5000", liters: "15", totalCost: "nevim", fullTank: 1, fuelType: "Benzin", note: ""}
    ]
    VehicleHistory := [
        {id: "hist_1", vehicleId: "veh_1", eventDate: "05.02." currentYear, eventType: "Servis", odometer: "10100", cost: "2000", note: ""}
    ]
    VehicleRecords := [
        {id: "record_1", vehicleId: "veh_2", recordType: "Povinné ručení", title: "Pojistka", provider: "", validFrom: "02/" currentYear, validTo: "03/" currentYear, price: "1500", filePath: "", note: ""},
        {id: "record_2", vehicleId: "veh_1", recordType: "Doklad", title: "Bez data", provider: "", validFrom: "", validTo: "", price: "900", filePath: "", note: ""}
    ]

    summary := BuildFleetCostPeriodSummary(currentYear, 1, 12)
    AssertEqual(summary.parsedCount, 3, "Fleet cost summary měl započítat tři číselné položky.")
    AssertEqual(summary.totalFuel, 1000.0, "Fleet součet tankování nesedí.")
    AssertEqual(summary.totalHistory, 2000.0, "Fleet součet historie nesedí.")
    AssertEqual(summary.totalRecords, 1500.0, "Fleet součet dokladů nesedí.")
    AssertEqual(summary.total, 4500.0, "Fleet celkový součet nesedí.")
    AssertEqual(summary.skippedCount, 1, "Fleet summary měl zachytit jednu nečíselnou částku.")
    AssertEqual(summary.undatedCount, 1, "Fleet summary měl zachytit jednu položku bez použitelného data.")
    AssertEqual(summary.activeVehicleCount, 3, "Fleet summary má počítat jen aktivní vozidla.")
    AssertEqual(summary.activeWithoutCostCount, 1, "Fleet summary má ukázat aktivní vozidla bez číselného nákladu.")
    AssertEqual(summary.totalDistanceKm, 100, "Fleet summary má sečíst ujeté kilometry tam, kde jsou dostupné.")
    AssertEqual(summary.costPerKmUnavailableCount, 1, "Fleet summary má hlídat vozidla s nákladem bez vypočtené ceny za kilometr.")
    AssertEqual(summary.rows[1].vehicle.id, "veh_1", "Nejvýš má být ve fleet přehledu vozidlo s nejvyšším součtem.")
    AssertEqual(summary.rows[1].distanceSummary.distanceKm, 100, "První řádek fleet summary má obsahovat ujeté kilometry vozidla.")
    AssertEqual(summary.rows[1].costPerKm, 30.0, "První řádek fleet summary má obsahovat cenu za kilometr.")

    summaryText := BuildFleetCostPeriodSummaryText(summary)
    AssertContains(summaryText, "Bez číselného nákladu: 1 z 3 aktivních vozidel.", "Fleet souhrn má upozornit na aktivní vozidla bez nákladu.")
    AssertContains(summaryText, "Nezapočteno: 1 s nečíselnou částkou, 1 bez použitelného data.", "Fleet souhrn má zmínit přeskočené položky.")
    AssertContains(summaryText, "Ujeto km: 100 km.", "Fleet souhrn má ukázat ujeté kilometry.")
    AssertContains(summaryText, "Cena / km: 30,00 Kč/km.", "Fleet souhrn má ukázat cenu za kilometr tam, kde jde spočítat.")
    AssertContains(summaryText, "Cena / km nedostupná u 1 vozidel s nákladem.", "Fleet souhrn má upozornit i na nedostupnou cenu za kilometr.")

    rowSummaryText := BuildFleetCostRowsSummaryText(summary)
    AssertContains(rowSummaryText, "Nejvíc pálí:", "Fleet přehled má vypsat i rychlé highlighty problémových stavů.")
    AssertContains(rowSummaryText, "Cena / km chybí u 1 vozidel s nákladem.", "Fleet přehled má zmínit nedostupnou cenu za kilometr.")

    zeroRow := ""
    unavailableRow := ""
    for row in summary.rows {
        if (row.vehicle.id = "veh_4") {
            zeroRow := row
        } else if (row.vehicle.id = "veh_2") {
            unavailableRow := row
        }
    }
    AssertTrue(IsObject(zeroRow), "Vozidlo bez nákladů musí být ve fleet přehledu zachované.")
    AssertTrue(IsObject(unavailableRow), "Vozidlo s náklady bez km musí být ve fleet přehledu zachované.")
    AssertContains(zeroRow.status, "Bez nákladu v období", "Řádek bez nákladů má mít jasný stav.")
    AssertContains(unavailableRow.status, "Cena / km: chybí km v období", "Řádek s náklady bez km má mít jasný stav.")
}

SmokeTestVehicleCostComparison() {
    global Vehicles, VehicleHistory, VehicleFuelLog, VehicleRecords

    ResetSmokeData()
    currentYear := FormatTime(A_Now, "yyyy") + 0
    previousYear := currentYear - 1

    Vehicles := [
        {id: "veh_1", name: "Srovnávací auto", category: "Osobní vozidla", vehicleNote: "", makeModel: "Skoda Octavia", plate: "1AB2345", year: "", power: "", lastTk: "", nextTk: "03/2028", greenCardFrom: "", greenCardTo: "04/2027"}
    ]
    VehicleFuelLog := [
        {id: "fuel_prev", vehicleId: "veh_1", entryDate: "10.11." previousYear, odometer: "9500", liters: "35", totalCost: "800", fullTank: 1, fuelType: "Benzin", note: ""},
        {id: "fuel_now", vehicleId: "veh_1", entryDate: "10.01." currentYear, odometer: "10000", liters: "40", totalCost: "1000", fullTank: 1, fuelType: "Benzin", note: ""}
    ]
    VehicleHistory := [
        {id: "hist_prev", vehicleId: "veh_1", eventDate: "15.12." previousYear, eventType: "Servis", odometer: "9600", cost: "1200", note: ""},
        {id: "hist_now", vehicleId: "veh_1", eventDate: "15.02." currentYear, eventType: "Servis", odometer: "10100", cost: "2000", note: ""}
    ]
    VehicleRecords := [
        {id: "record_prev", vehicleId: "veh_1", recordType: "Doklad", title: "Loňský doklad", provider: "", validFrom: "11/" previousYear, validTo: "12/" previousYear, price: "400", filePath: "", note: ""},
        {id: "record_now", vehicleId: "veh_1", recordType: "Doklad", title: "Aktuální doklad", provider: "", validFrom: "01/" currentYear, validTo: "02/" currentYear, price: "500", filePath: "", note: ""}
    ]

    summary := BuildVehicleCostPeriodSummary("veh_1", currentYear, 1, 2)
    AssertEqual(summary.total, 3500.0, "Součet nákladů za vybrané období nesedí.")
    AssertTrue(IsObject(summary.distanceSummary) && summary.distanceSummary.available, "Souhrn nákladů má obsahovat dostupné km v období.")
    AssertEqual(summary.distanceSummary.distanceKm, 100, "Souhrn nákladů má správně spočítat ujeté kilometry.")
    AssertEqual(summary.costPerKm, 35.0, "Souhrn nákladů má správně spočítat cenu za kilometr.")
    AssertTrue(IsObject(summary.comparison), "Souhrn nákladů má vrátit i srovnání s předchozím obdobím.")
    AssertEqual(summary.comparison.previousLabel, BuildMonthIndexRangeLabel(GetMonthIndex(previousYear, 11), GetMonthIndex(previousYear, 12)), "Srovnání má použít stejně dlouhé předchozí období.")
    AssertEqual(summary.comparison.totalDifference, 1100.0, "Rozdíl celkových nákladů vůči minulému období nesedí.")
    AssertTrue(summary.comparison.totalPercentAvailable, "Procentní změna celkových nákladů má být dostupná.")
    AssertTrue(summary.comparison.costPerKmAvailable, "Srovnání ceny za kilometr má být dostupné.")
    AssertEqual(summary.comparison.costPerKmDifference, 11.0, "Rozdíl ceny za kilometr vůči minulému období nesedí.")

    text := BuildVehicleCostPeriodSummaryText(summary)
    AssertContains(text, "Ujeto km: 100 km.", "Souhrn vozidla má ukázat ujeté kilometry.")
    AssertContains(text, "Cena / km: 35,00 Kč/km.", "Souhrn vozidla má ukázat cenu za kilometr.")
    AssertContains(text, "Oproti " BuildMonthIndexRangeLabel(GetMonthIndex(previousYear, 11), GetMonthIndex(previousYear, 12)), "Souhrn vozidla má ukázat srovnání proti minulému období.")
}

SmokeTestDashboardCosts() {
    global Vehicles, VehicleHistory, VehicleFuelLog, VehicleRecords

    ResetSmokeData()
    currentYear := FormatTime(A_Now, "yyyy")
    Vehicles := [
        {id: "veh_1", name: "Skoda", category: "Osobní vozidla", vehicleNote: "", makeModel: "", plate: "", year: "", power: "", lastTk: "", nextTk: "03/2027", greenCardFrom: "", greenCardTo: "04/2026"},
        {id: "veh_2", name: "Yamaha", category: "Motocykly", vehicleNote: "", makeModel: "", plate: "", year: "", power: "", lastTk: "", nextTk: "03/2027", greenCardFrom: "", greenCardTo: "04/2026"},
        {id: "veh_3", name: "Transit", category: "Nákladní vozidla", vehicleNote: "", makeModel: "", plate: "", year: "", power: "", lastTk: "", nextTk: "03/2027", greenCardFrom: "", greenCardTo: "04/2026"}
    ]
    VehicleFuelLog := [
        {id: "fuel_1", vehicleId: "veh_1", entryDate: "01.02." currentYear, odometer: "10000", liters: "40", totalCost: "1000", fullTank: 1, fuelType: "Benzin", note: ""}
    ]
    VehicleHistory := [
        {id: "hist_1", vehicleId: "veh_1", eventDate: "05.02." currentYear, eventType: "Servis", odometer: "10100", cost: "2000", note: ""}
    ]
    VehicleRecords := [
        {id: "record_1", vehicleId: "veh_2", recordType: "Povinné ručení", title: "Pojistka", provider: "", validFrom: "02/" currentYear, validTo: "03/" currentYear, price: "1500", filePath: "", note: ""}
    ]

    summary := BuildDashboardCurrentYearCostSummary()
    AssertEqual(summary.parsedCount, 3, "Dashboard měl započítat tři číselné náklady.")
    AssertEqual(summary.totalFuel, 1000.0, "Součet tankování nesedí.")
    AssertEqual(summary.totalHistory, 2000.0, "Součet historie nesedí.")
    AssertEqual(summary.totalRecords, 1500.0, "Součet dokladů nesedí.")
    AssertEqual(summary.topVehicleId, "veh_1", "Nejvyšší náklad měl patřit prvnímu vozidlu.")
    AssertEqual(summary.zeroCostVehicleCount, 1, "Dashboard má počítat aktivní vozidla bez číselného nákladu v aktuálním roce.")
    AssertEqual(summary.topVehicles.Length, 2, "Dashboard má vrátit pořadí jen pro vozidla s číselnými náklady.")

    summaryText := BuildDashboardCostSummaryText()
    AssertContains(summaryText, "Rok " currentYear, "Text nákladového souhrnu má obsahovat aktuální rok.")
    AssertContains(summaryText, "Nejvýš:", "Text nákladového souhrnu má ukázat i pořadí nejdražších vozidel.")
    AssertContains(summaryText, "Bez číselného nákladu letos: 1 z 3 aktivních vozidel.", "Text nákladového souhrnu má upozornit na aktivní vozidla bez nákladů.")
}

SmokeTestDashboardProblemHighlights() {
    global Vehicles, VehicleRecords

    ResetSmokeData()
    tempRoot := A_Temp "\vehimap_smoke_dashboard_highlights"
    missingFile := tempRoot "\chybi.pdf"
    if DirExist(tempRoot) {
        DirDelete(tempRoot, true)
    }
    DirCreate(tempRoot)

    Vehicles := [
        {id: "veh_1", name: "TK auto", category: "Osobní vozidla", vehicleNote: "", makeModel: "", plate: "1AB2345", year: "", power: "", lastTk: "", nextTk: "01/2000", greenCardFrom: "", greenCardTo: "12/2099"},
        {id: "veh_2", name: "Doklad auto", category: "Motocykly", vehicleNote: "", makeModel: "", plate: "2AB3456", year: "", power: "", lastTk: "", nextTk: "12/2099", greenCardFrom: "", greenCardTo: "12/2099"},
        {id: "veh_3", name: "Bez SPZ", category: "Osobní vozidla", vehicleNote: "", makeModel: "", plate: "", year: "", power: "", lastTk: "", nextTk: "12/2099", greenCardFrom: "", greenCardTo: "12/2099"}
    ]
    VehicleRecords := [
        {id: "record_1", vehicleId: "veh_2", recordType: "Doklad", title: "Chybějící příloha", provider: "", validFrom: "", validTo: "", price: "", filePath: missingFile, note: ""}
    ]

    entries := BuildDashboardEntries()
    SortDashboardEntries(&entries)

    text := BuildDashboardVehicleSummaryText(entries)
    AssertContains(text, "Nejvíc pálí:", "Dashboard má vypisovat rychlé highlighty nejvážnějších stavů.")
    AssertContains(text, "TK auto (TK", "Highlighty mají obsahovat vozidlo s nejbližším problémem.")
    AssertContains(text, "Doklad auto (Doklad Chybí soubor)", "Highlighty mají obsahovat i problémové přílohy dokladů.")
    AssertContains(text, "Bez SPZ (SPZ chybí)", "Highlighty mají obsahovat i chybějící klíčové údaje vozidla.")
}

SmokeTestDashboardMaintenanceActions() {
    global Vehicles, VehicleFuelLog, VehicleMaintenancePlans, SettingsFile

    ResetSmokeData()
    tempSettings := A_Temp "\vehimap_smoke_dashboard_maintenance_actions.ini"
    try FileDelete(tempSettings)
    SettingsFile := tempSettings
    EnsureSettingsDefaults()
    IniWrite("31", SettingsFile, "notifications", "maintenance_reminder_days")
    IniWrite("1000", SettingsFile, "notifications", "maintenance_reminder_km")

    currentYear := FormatTime(A_Now, "yyyy")
    Vehicles := [
        {id: "veh_1", name: "Servis auto", category: "Osobní vozidla", vehicleNote: "", makeModel: "Skoda Octavia", plate: "1AB2345", year: "", power: "", lastTk: "", nextTk: "03/2028", greenCardFrom: "", greenCardTo: "04/2027"},
        {id: "veh_2", name: "TK auto", category: "Motocykly", vehicleNote: "", makeModel: "Honda", plate: "2AB3456", year: "", power: "", lastTk: "", nextTk: FormatTime(A_Now, "MM/yyyy"), greenCardFrom: "", greenCardTo: "12/2099"}
    ]
    VehicleFuelLog := [
        {id: "fuel_1", vehicleId: "veh_1", entryDate: "20.03." currentYear, odometer: "14650", liters: "35", totalCost: "1700", fullTank: 1, fuelType: "Benzin", note: ""}
    ]
    VehicleMaintenancePlans := [
        {id: "mnt_1", vehicleId: "veh_1", title: "Motorový olej", intervalKm: "5000", intervalMonths: "12", lastServiceDate: "01.04." currentYear, lastServiceOdometer: "10000", isActive: 1, note: ""}
    ]

    entries := BuildDashboardEntries()
    maintenanceEntry := ""
    technicalEntry := ""
    for entry in entries {
        if (entry.kind = "maintenance" && !IsObject(maintenanceEntry)) {
            maintenanceEntry := entry
        } else if (entry.kind = "technical" && !IsObject(technicalEntry)) {
            technicalEntry := entry
        }
    }

    AssertTrue(IsObject(maintenanceEntry), "Dashboard má nabídnout servisní položku pro akční centrum.")
    AssertTrue(CanOpenDashboardVehicleHistory(maintenanceEntry), "Servisní položka má umožnit otevření historie vozidla.")
    AssertTrue(CanCompleteDashboardMaintenance(maintenanceEntry), "Servisní položka má umožnit přímé dokončení servisu.")
    AssertTrue(IsObject(GetDashboardMaintenancePlan(maintenanceEntry)), "Servisní položka má být navázaná na konkrétní plán údržby.")

    AssertTrue(IsObject(technicalEntry), "Dashboard má stále obsahovat i technické termíny.")
    AssertTrue(CanOpenDashboardVehicleHistory(technicalEntry), "I běžná dashboard položka má umožnit otevření historie vozidla.")
    AssertTrue(!CanCompleteDashboardMaintenance(technicalEntry), "Dokončení servisu musí být vyhrazené jen servisním položkám.")
}

SmokeTestDashboardDataSummary() {
    global Vehicles, VehicleRecords

    ResetSmokeData()
    tempRoot := A_Temp "\vehimap_smoke_data"
    missingFile := tempRoot "\chybi.pdf"
    if DirExist(tempRoot) {
        DirDelete(tempRoot, true)
    }
    DirCreate(tempRoot)

    Vehicles := [
        {id: "veh_1", name: "Bez SPZ", category: "Osobní vozidla", vehicleNote: "", makeModel: "", plate: "", year: "", power: "", lastTk: "", nextTk: "", greenCardFrom: "", greenCardTo: ""},
        {id: "veh_2", name: "OK", category: "Osobní vozidla", vehicleNote: "", makeModel: "", plate: "2AB3456", year: "", power: "", lastTk: "", nextTk: "05/2027", greenCardFrom: "", greenCardTo: "06/2026"}
    ]
    VehicleRecords := [
        {id: "record_1", vehicleId: "veh_1", recordType: "Doklad", title: "Bez cesty", provider: "", validFrom: "", validTo: "", price: "", filePath: "", note: ""},
        {id: "record_2", vehicleId: "veh_2", recordType: "Doklad", title: "Chybějící příloha", provider: "", validFrom: "", validTo: "", price: "", filePath: missingFile, note: ""}
    ]

    text := BuildDashboardDataSummaryText()
    AssertContains(text, "Bez SPZ: 1.", "Dashboard má počítat vozidla bez SPZ.")
    AssertContains(text, "Bez příští TK: 1.", "Dashboard má počítat vozidla bez příští TK.")
    AssertContains(text, "Bez vyplněné ZK: 1.", "Dashboard má počítat vozidla bez zelené karty.")
    AssertContains(text, "Dokladů s nedostupnou přílohou: 1.", "Dashboard má počítat nedostupné přílohy.")
    AssertContains(text, "Dokladů bez cesty: 1.", "Dashboard má počítat doklady bez cesty.")
}

SmokeTestDashboardEntries() {
    global Vehicles, VehicleRecords

    ResetSmokeData()
    tempRoot := A_Temp "\vehimap_smoke_dashboard_entries"
    missingFile := tempRoot "\chybi.pdf"
    if DirExist(tempRoot) {
        DirDelete(tempRoot, true)
    }
    DirCreate(tempRoot)

    Vehicles := [
        {id: "veh_1", name: "Bez dat", category: "Osobní vozidla", vehicleNote: "", makeModel: "", plate: "", year: "", power: "", lastTk: "", nextTk: "", greenCardFrom: "", greenCardTo: ""},
        {id: "veh_2", name: "Brzká TK", category: "Motocykly", vehicleNote: "", makeModel: "", plate: "2AB3456", year: "", power: "", lastTk: "", nextTk: FormatTime(A_Now, "MM/yyyy"), greenCardFrom: "", greenCardTo: "12/2099"}
    ]
    VehicleRecords := [
        {id: "record_1", vehicleId: "veh_1", recordType: "Doklad", title: "Bez cesty", provider: "", validFrom: "", validTo: "", price: "", filePath: "", note: ""},
        {id: "record_2", vehicleId: "veh_2", recordType: "Doklad", title: "Chybějící soubor", provider: "", validFrom: "", validTo: "", price: "", filePath: missingFile, note: ""}
    ]

    entries := BuildDashboardEntries()
    SortDashboardEntries(&entries)

    AssertTrue(entries.Length >= 5, "Dashboard měl vrátit termíny i datové nedostatky.")
    AssertEqual(entries[1].kind, "technical", "Dashboard má řadit skutečné termíny před datové nedostatky.")
    AssertDashboardEntryPresent(entries, "green", "veh_1", "Chybí", "Nevyplněno")
    AssertDashboardEntryPresent(entries, "vehicle_field", "veh_1", "Doplnit v editaci", "SPZ")
    AssertDashboardEntryPresent(entries, "vehicle_field", "veh_1", "Doplnit v editaci", "Příští TK")
    AssertDashboardEntryPresent(entries, "record_path", "veh_1", "Bez cesty", "Bez cesty")
    AssertDashboardEntryPresent(entries, "record_path", "veh_2", "Chybí soubor", "Chybějící soubor")
}

SmokeTestOverviewDataIssues() {
    global Vehicles, VehicleRecords

    ResetSmokeData()
    tempRoot := A_Temp "\vehimap_smoke_overview_entries"
    missingFile := tempRoot "\chybi.pdf"
    if DirExist(tempRoot) {
        DirDelete(tempRoot, true)
    }
    DirCreate(tempRoot)

    Vehicles := [
        {id: "veh_1", name: "Bez dat", category: "Osobní vozidla", vehicleNote: "", makeModel: "Skoda", plate: "", year: "", power: "", lastTk: "", nextTk: "", greenCardFrom: "", greenCardTo: ""},
        {id: "veh_2", name: "Aktivní", category: "Motocykly", vehicleNote: "", makeModel: "Honda", plate: "2AB3456", year: "", power: "", lastTk: "", nextTk: FormatTime(A_Now, "MM/yyyy"), greenCardFrom: "", greenCardTo: "12/2099"}
    ]
    VehicleRecords := [
        {id: "record_1", vehicleId: "veh_1", recordType: "Doklad", title: "Bez cesty", provider: "", validFrom: "", validTo: "", price: "", filePath: "", note: ""},
        {id: "record_2", vehicleId: "veh_2", recordType: "Doklad", title: "Chybějící soubor", provider: "", validFrom: "", validTo: "", price: "", filePath: missingFile, note: ""}
    ]

    entries := BuildUpcomingOverviewEntries(true, true)
    dataIssues := FilterUpcomingOverviewEntries(entries, "data_issue")

    AssertTrue(entries.Length >= 5, "Přehled termínů měl obsahovat termíny i datové nedostatky.")
    AssertEqual(dataIssues.Length, 4, "Filtr datových nedostatků měl vrátit čtyři položky.")
    AssertContains(BuildUpcomingOverviewSummary(dataIssues, entries), "datových nedostatků", "Souhrn přehledu má zmínit datové nedostatky.")
    AssertDashboardEntryPresent(dataIssues, "vehicle_field", "veh_1", "Doplnit v editaci", "SPZ")
    AssertDashboardEntryPresent(dataIssues, "vehicle_field", "veh_1", "Doplnit v editaci", "Příští TK")
    AssertDashboardEntryPresent(dataIssues, "record_path", "veh_1", "Bez cesty", "Bez cesty")
    AssertDashboardEntryPresent(dataIssues, "record_path", "veh_2", "Chybí soubor", "Chybějící soubor")
}

SmokeTestSortSettings() {
    global SettingsFile

    tempSettings := A_Temp "\vehimap_smoke_settings.ini"
    try FileDelete(tempSettings)
    SettingsFile := tempSettings
    EnsureSettingsDefaults()

    SaveHistorySortSettings(5, false)
    SaveFuelSortSettings(7, true)
    SaveRecordsSortSettings(6, true)
    SaveReminderSortSettings(4, true)
    SaveMaintenanceSortSettings(3, true)
    SaveOverviewFilterSetting("data_issue")
    SaveOverviewIncludeDataIssuesSetting(true)
    IniWrite("45", SettingsFile, "notifications", "maintenance_reminder_days")
    IniWrite("1500", SettingsFile, "notifications", "maintenance_reminder_km")

    AssertEqual(GetHistorySortColumnSetting(), 5, "History sort column se neuložil.")
    AssertEqual(GetHistorySortDescendingSetting(), false, "History sort descending se neuložil.")
    AssertEqual(GetFuelSortColumnSetting(), 7, "Fuel sort column se neuložil.")
    AssertEqual(GetFuelSortDescendingSetting(), true, "Fuel sort descending se neuložil.")
    AssertEqual(GetRecordsSortColumnSetting(), 6, "Records sort column se neuložil.")
    AssertEqual(GetRecordsSortDescendingSetting(), true, "Records sort descending se neuložil.")
    AssertEqual(GetReminderSortColumnSetting(), 4, "Reminder sort column se neuložil.")
    AssertEqual(GetReminderSortDescendingSetting(), true, "Reminder sort descending se neuložil.")
    AssertEqual(GetMaintenanceSortColumnSetting(), 3, "Maintenance sort column se neuložil.")
    AssertEqual(GetMaintenanceSortDescendingSetting(), true, "Maintenance sort descending se neuložil.")
    AssertEqual(GetOverviewFilterSetting(), "data_issue", "Overview filter se neuložil.")
    AssertEqual(GetOverviewFilterIndex(), 6, "Overview filter index pro datové nedostatky nesedí.")
    AssertEqual(GetOverviewIncludeDataIssuesSetting(), 1, "Overview include_data_issues se neuložil.")
    AssertEqual(GetMaintenanceReminderDays(), 45, "Počet dnů pro upozornění na údržbu se neuložil.")
    AssertEqual(GetMaintenanceReminderKm(), 1500, "Kilometrový limit pro upozornění na údržbu se neuložil.")
}

ResetSmokeData() {
    global Vehicles, VehicleHistory, VehicleFuelLog, VehicleRecords, VehicleMetaEntries, VehicleReminders, VehicleMaintenancePlans

    Vehicles := []
    VehicleHistory := []
    VehicleFuelLog := []
    VehicleRecords := []
    VehicleMetaEntries := []
    VehicleReminders := []
    VehicleMaintenancePlans := []
}

HasMaintenanceTemplateWithTitle(items, title) {
    for item in items {
        if IsObject(item) && item.HasOwnProp("title") && item.title = title {
            return true
        }
    }

    return false
}

CountMaintenancePlansByTitle(vehicleId, title) {
    count := 0

    for plan in VehicleMaintenancePlans {
        if (plan.vehicleId = vehicleId && plan.title = title) {
            count += 1
        }
    }

    return count
}

CountVehicleRecordsByTitle(vehicleId, title) {
    global VehicleRecords

    count := 0
    for entry in VehicleRecords {
        if (entry.vehicleId = vehicleId && entry.title = title) {
            count += 1
        }
    }

    return count
}

CountVehicleRemindersByTitle(vehicleId, title) {
    global VehicleReminders

    count := 0
    for entry in VehicleReminders {
        if (entry.vehicleId = vehicleId && entry.title = title) {
            count += 1
        }
    }

    return count
}

HasMaintenanceTemplateWithTitleByText(items, title) {
    for item in items {
        if (item = title) {
            return true
        }
    }

    return false
}

AssertSearchKindPresent(results, expectedKind) {
    for result in results {
        if (result.kind = expectedKind) {
            return
        }
    }

    throw Error("Ve výsledcích chybí očekávaný typ " expectedKind ".")
}

AssertContains(haystack, needle, message := "") {
    if InStr(haystack, needle) {
        return
    }

    if (message = "") {
        message := "Text neobsahuje očekávanou hodnotu."
    }
    throw Error(message)
}

AssertTrue(condition, message := "") {
    if condition {
        return
    }

    if (message = "") {
        message := "Podmínka není splněná."
    }
    throw Error(message)
}

AssertEqual(actual, expected, message := "") {
    if (actual == expected) {
        return
    }

    if (message = "") {
        message := "Hodnoty nejsou shodné."
    }
    throw Error(message " Skutečnost: " actual " Očekáváno: " expected)
}

AssertDashboardEntryPresent(entries, expectedKind, vehicleId, expectedStatus := "", expectedTerm := "") {
    for entry in entries {
        if (entry.kind != expectedKind) {
            continue
        }
        if !entry.HasOwnProp("vehicle") || !IsObject(entry.vehicle) || entry.vehicle.id != vehicleId {
            continue
        }
        if (expectedStatus != "" && entry.status != expectedStatus) {
            continue
        }
        if (expectedTerm != "" && entry.term != expectedTerm) {
            continue
        }
        return
    }

    throw Error("V přehledu chybí očekávaná položka typu " expectedKind " pro vozidlo " vehicleId ".")
}

WriteSmokeOutput(text) {
    static firstWrite := true

    resultFile := FileOpen(GetSmokeResultPath(), firstWrite ? "w" : "a", "UTF-8")
    if !IsObject(resultFile) {
        throw Error("Nepodařilo se otevřít smoke result soubor pro zápis.")
    }
    resultFile.Write(text "`n")
    resultFile.Close()
    firstWrite := false
    try FileAppend(text "`n", "*")
}

GetSmokeResultPath() {
    return A_ScriptDir "\result.txt"
}
