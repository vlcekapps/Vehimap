#Requires AutoHotkey v2.0

VehimapTestMode := true
#Include ..\Vehimap.ahk

RunSmokeTests()

RunSmokeTests() {
    failures := []
    tests := [
        {name: "global search", fn: Func("SmokeTestGlobalSearch")},
        {name: "record path info", fn: Func("SmokeTestRecordPathInfo")},
        {name: "dashboard costs", fn: Func("SmokeTestDashboardCosts")},
        {name: "dashboard data summary", fn: Func("SmokeTestDashboardDataSummary")},
        {name: "dashboard entries", fn: Func("SmokeTestDashboardEntries")},
        {name: "sort settings", fn: Func("SmokeTestSortSettings")}
    ]

    for test in tests {
        try {
            test.fn.Call()
        } catch as err {
            failures.Push(test.name ": " err.Message)
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

SmokeTestGlobalSearch() {
    global Vehicles, VehicleHistory, VehicleFuelLog, VehicleRecords, VehicleMetaEntries, VehicleReminders

    ResetSmokeData()
    Vehicles := [
        {
            id: "veh_1",
            name: "Alfa test",
            category: "Osobní vozidla",
            vehicleType: "Hatchback",
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

    results := BuildGlobalSearchResults("alfa")
    AssertTrue(results.Length >= 5, "Globální hledání mělo vrátit alespoň 5 výsledků.")
    AssertSearchKindPresent(results, "vehicle")
    AssertSearchKindPresent(results, "history")
    AssertSearchKindPresent(results, "fuel")
    AssertSearchKindPresent(results, "record")
    AssertSearchKindPresent(results, "reminder")
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

SmokeTestDashboardCosts() {
    global Vehicles, VehicleHistory, VehicleFuelLog, VehicleRecords

    ResetSmokeData()
    currentYear := FormatTime(A_Now, "yyyy")
    Vehicles := [
        {id: "veh_1", name: "Skoda", category: "Osobní vozidla", vehicleType: "", makeModel: "", plate: "", year: "", power: "", lastTk: "", nextTk: "03/2027", greenCardFrom: "", greenCardTo: "04/2026"},
        {id: "veh_2", name: "Yamaha", category: "Motocykly", vehicleType: "", makeModel: "", plate: "", year: "", power: "", lastTk: "", nextTk: "03/2027", greenCardFrom: "", greenCardTo: "04/2026"}
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
    AssertContains(BuildDashboardCostSummaryText(), "Rok " currentYear, "Text nákladového souhrnu má obsahovat aktuální rok.")
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
        {id: "veh_1", name: "Bez SPZ", category: "Osobní vozidla", vehicleType: "", makeModel: "", plate: "", year: "", power: "", lastTk: "", nextTk: "", greenCardFrom: "", greenCardTo: ""},
        {id: "veh_2", name: "OK", category: "Osobní vozidla", vehicleType: "", makeModel: "", plate: "2AB3456", year: "", power: "", lastTk: "", nextTk: "05/2027", greenCardFrom: "", greenCardTo: "06/2026"}
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
        {id: "veh_1", name: "Bez dat", category: "OsobnĂ­ vozidla", vehicleType: "", makeModel: "", plate: "", year: "", power: "", lastTk: "", nextTk: "", greenCardFrom: "", greenCardTo: ""},
        {id: "veh_2", name: "BrzkĂˇ TK", category: "Motocykly", vehicleType: "", makeModel: "", plate: "2AB3456", year: "", power: "", lastTk: "", nextTk: FormatTime(A_Now, "MM/yyyy"), greenCardFrom: "", greenCardTo: "12/2099"}
    ]
    VehicleRecords := [
        {id: "record_1", vehicleId: "veh_1", recordType: "Doklad", title: "Bez cesty", provider: "", validFrom: "", validTo: "", price: "", filePath: "", note: ""},
        {id: "record_2", vehicleId: "veh_2", recordType: "Doklad", title: "ChybÄ›jĂ­cĂ­ soubor", provider: "", validFrom: "", validTo: "", price: "", filePath: missingFile, note: ""}
    ]

    entries := BuildDashboardEntries()
    SortDashboardEntries(&entries)

    AssertTrue(entries.Length >= 5, "Dashboard mÄ›l vrĂˇtit termĂ­ny i datovĂ© nedostatky.")
    AssertEqual(entries[1].kind, "technical", "Dashboard mĂˇ Å™adit skuteÄŤnĂ© termĂ­ny pÅ™ed datovĂ© nedostatky.")
    AssertDashboardEntryPresent(entries, "green", "veh_1", "ChybĂ­", "NevyplnÄ›no")
    AssertDashboardEntryPresent(entries, "vehicle_field", "veh_1", "Doplnit v editaci", "SPZ")
    AssertDashboardEntryPresent(entries, "vehicle_field", "veh_1", "Doplnit v editaci", "PÅ™Ă­ĹˇtĂ­ TK")
    AssertDashboardEntryPresent(entries, "record_path", "veh_1", "Bez cesty", "Bez cesty")
    AssertDashboardEntryPresent(entries, "record_path", "veh_2", "ChybĂ­ soubor", "ChybÄ›jĂ­cĂ­ soubor")
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

    AssertEqual(GetHistorySortColumnSetting(), 5, "History sort column se neuložil.")
    AssertEqual(GetHistorySortDescendingSetting(), false, "History sort descending se neuložil.")
    AssertEqual(GetFuelSortColumnSetting(), 7, "Fuel sort column se neuložil.")
    AssertEqual(GetFuelSortDescendingSetting(), true, "Fuel sort descending se neuložil.")
    AssertEqual(GetRecordsSortColumnSetting(), 6, "Records sort column se neuložil.")
    AssertEqual(GetRecordsSortDescendingSetting(), true, "Records sort descending se neuložil.")
    AssertEqual(GetReminderSortColumnSetting(), 4, "Reminder sort column se neuložil.")
    AssertEqual(GetReminderSortDescendingSetting(), true, "Reminder sort descending se neuložil.")
}

ResetSmokeData() {
    global Vehicles, VehicleHistory, VehicleFuelLog, VehicleRecords, VehicleMetaEntries, VehicleReminders

    Vehicles := []
    VehicleHistory := []
    VehicleFuelLog := []
    VehicleRecords := []
    VehicleMetaEntries := []
    VehicleReminders := []
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

    throw Error("V dashboardu chybĂ­ oÄŤekĂˇvanĂˇ poloĹľka typu " expectedKind " pro vozidlo " vehicleId ".")
}

WriteSmokeOutput(text) {
    FileAppend(text "`n", "*")
}
