LoadVehicles() {
    global Vehicles, VehiclesFile

    Vehicles := []
    if !FileExist(VehiclesFile) {
        return
    }

    content := FileRead(VehiclesFile, "UTF-8")
    content := StrReplace(content, Chr(0xFEFF))
    lines := StrSplit(content, "`n", "`r")
    firstNonEmptyLine := ""
    dataStarted := false

    for rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        firstNonEmptyLine := line
        break
    }

    if (firstNonEmptyLine = "") {
        return
    }

    if (firstNonEmptyLine != "# Vehimap data v3" && firstNonEmptyLine != "# Vehimap data v4") {
        ShowVehiclesFileFormatError("Soubor vozidel není v podporovaném formátu. Vehimap očekává hlavičku '# Vehimap data v4'.")
        return
    }

    for index, rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        if !dataStarted {
            dataStarted := true
            continue
        }

        if (SubStr(line, 1, 1) = "#") {
            ShowVehiclesFileFormatError("Soubor vozidel obsahuje neplatnou hlavičku nebo komentář na řádku " index ". Vehimap očekává jen jednu hlavičku '# Vehimap data v4'.")
            Vehicles := []
            return
        }

        fields := StrSplit(line, "`t")
        if (fields.Length != 12) {
            ShowVehiclesFileFormatError("Soubor vozidel je poškozený nebo není ve formátu v4. Řádek " index " musí obsahovat přesně 12 polí oddělených tabulátory.")
            Vehicles := []
            return
        }

        Vehicles.Push({
            id: UnescapeField(fields[1]),
            name: UnescapeField(fields[2]),
            category: NormalizeCategory(UnescapeField(fields[3])),
            vehicleNote: UnescapeField(fields[4]),
            makeModel: UnescapeField(fields[5]),
            plate: UnescapeField(fields[6]),
            year: UnescapeField(fields[7]),
            power: UnescapeField(fields[8]),
            lastTk: UnescapeField(fields[9]),
            nextTk: UnescapeField(fields[10]),
            greenCardFrom: UnescapeField(fields[11]),
            greenCardTo: UnescapeField(fields[12])
        })
    }
}

ShowVehiclesFileFormatError(message) {
    global AppTitle, VehiclesFile

    MsgBox(message "`n`nZkontrolujte soubor:`n" VehiclesFile, AppTitle, 0x30)
}

SaveVehicles() {
    global Vehicles, VehiclesFile

    lines := ["# Vehimap data v4"]
    for vehicle in Vehicles {
        lines.Push(
            EscapeField(vehicle.id) "`t"
            EscapeField(vehicle.name) "`t"
            EscapeField(vehicle.category) "`t"
            EscapeField(vehicle.vehicleNote) "`t"
            EscapeField(vehicle.makeModel) "`t"
            EscapeField(vehicle.plate) "`t"
            EscapeField(vehicle.year) "`t"
            EscapeField(vehicle.power) "`t"
            EscapeField(vehicle.lastTk) "`t"
            EscapeField(vehicle.nextTk) "`t"
            EscapeField(vehicle.greenCardFrom) "`t"
            EscapeField(vehicle.greenCardTo)
        )
    }

    output := JoinLines(lines)
    if FileExist(VehiclesFile) {
        FileDelete(VehiclesFile)
    }
    FileAppend(output, VehiclesFile, "UTF-8")
}

LoadVehicleHistory() {
    global AppTitle, VehicleHistory, HistoryFile

    VehicleHistory := []
    if !FileExist(HistoryFile) {
        return
    }

    content := FileRead(HistoryFile, "UTF-8")
    content := StrReplace(content, Chr(0xFEFF))
    lines := StrSplit(content, "`n", "`r")
    firstNonEmptyLine := ""
    dataStarted := false

    for rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        firstNonEmptyLine := line
        break
    }

    if (firstNonEmptyLine = "") {
        return
    }

    if (firstNonEmptyLine != "# Vehimap history v1") {
        MsgBox("Soubor historie není v podporovaném formátu.`n`nZkontrolujte soubor:`n" HistoryFile, AppTitle, 0x30)
        return
    }

    for index, rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        if !dataStarted {
            dataStarted := true
            continue
        }

        if (SubStr(line, 1, 1) = "#") {
            MsgBox("Soubor historie obsahuje neplatnou hlavičku nebo komentář na řádku " index ".`n`nZkontrolujte soubor:`n" HistoryFile, AppTitle, 0x30)
            VehicleHistory := []
            return
        }

        fields := StrSplit(line, "`t")
        if (fields.Length != 6 && fields.Length != 7) {
            MsgBox("Soubor historie je poškozený. Řádek " index " musí obsahovat 6 nebo 7 polí oddělených tabulátory.`n`nZkontrolujte soubor:`n" HistoryFile, AppTitle, 0x30)
            VehicleHistory := []
            return
        }

        VehicleHistory.Push({
            id: UnescapeField(fields[1]),
            vehicleId: UnescapeField(fields[2]),
            eventDate: UnescapeField(fields[3]),
            eventType: UnescapeField(fields[4]),
            odometer: UnescapeField(fields[5]),
            cost: UnescapeField(fields[6]),
            note: (fields.Length = 7) ? UnescapeField(fields[7]) : ""
        })
    }
}

SaveVehicleHistory() {
    global VehicleHistory, HistoryFile

    output := BuildHistoryDataContent()
    if FileExist(HistoryFile) {
        FileDelete(HistoryFile)
    }
    FileAppend(output, HistoryFile, "UTF-8")
}

BuildHistoryDataContent() {
    global VehicleHistory

    lines := ["# Vehimap history v1"]
    for event in VehicleHistory {
        lines.Push(
            EscapeField(event.id) "`t"
            EscapeField(event.vehicleId) "`t"
            EscapeField(event.eventDate) "`t"
            EscapeField(event.eventType) "`t"
            EscapeField(event.odometer) "`t"
            EscapeField(event.cost) "`t"
            EscapeField(event.note)
        )
    }

    return JoinLines(lines)
}

DeleteVehicleHistory(vehicleId) {
    global VehicleHistory

    filtered := []
    for event in VehicleHistory {
        if (event.vehicleId != vehicleId) {
            filtered.Push(event)
        }
    }

    VehicleHistory := filtered
    SaveVehicleHistory()
}

GetVehicleHistoryEntries(vehicleId) {
    global VehicleHistory

    entries := []
    for event in VehicleHistory {
        if (event.vehicleId = vehicleId) {
            entries.Push(event)
        }
    }

    SortVehicleHistoryByDateDescending(&entries)
    return entries
}

GetRecentVehicleHistoryEntries(vehicleId, maxCount := 5) {
    entries := GetVehicleHistoryEntries(vehicleId)
    if (entries.Length <= maxCount) {
        return entries
    }

    recent := []
    Loop maxCount {
        recent.Push(entries[A_Index])
    }

    return recent
}

GetVehicleHistoryCount(vehicleId) {
    return GetVehicleHistoryEntries(vehicleId).Length
}

BuildVehicleHistorySummaryText(vehicleId) {
    entries := GetVehicleHistoryEntries(vehicleId)
    if (entries.Length = 0) {
        return "K tomuto vozidlu zatím není uložená žádná historie událostí."
    }

    latest := entries[1]
    summary := "Celkem událostí: " entries.Length ". Poslední událost: " latest.eventType " (" latest.eventDate ")."
    if (latest.odometer != "") {
        summary .= " Tachometr: " FormatHistoryOdometer(latest.odometer) "."
    }
    return summary
}

FindVehicleHistoryEventById(eventId) {
    global VehicleHistory

    for event in VehicleHistory {
        if (event.id = eventId) {
            return event
        }
    }

    return ""
}

FindVehicleHistoryEventIndexById(eventId) {
    global VehicleHistory

    for index, event in VehicleHistory {
        if (event.id = eventId) {
            return index
        }
    }

    return 0
}

GenerateHistoryEventId() {
    return "hist_" A_Now "_" Random(1000, 9999)
}

LoadVehicleFuelLog() {
    global AppTitle, VehicleFuelLog, FuelLogFile

    VehicleFuelLog := []
    if !FileExist(FuelLogFile) {
        return
    }

    content := FileRead(FuelLogFile, "UTF-8")
    content := StrReplace(content, Chr(0xFEFF))
    lines := StrSplit(content, "`n", "`r")
    firstNonEmptyLine := ""
    dataStarted := false

    for rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        firstNonEmptyLine := line
        break
    }

    if (firstNonEmptyLine = "") {
        return
    }

    if (firstNonEmptyLine != "# Vehimap fuel v1") {
        MsgBox("Soubor kilometrů a tankování není v podporovaném formátu.`n`nZkontrolujte soubor:`n" FuelLogFile, AppTitle, 0x30)
        return
    }

    for index, rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        if !dataStarted {
            dataStarted := true
            continue
        }

        if (SubStr(line, 1, 1) = "#") {
            MsgBox("Soubor kilometrů a tankování obsahuje neplatnou hlavičku nebo komentář na řádku " index ".`n`nZkontrolujte soubor:`n" FuelLogFile, AppTitle, 0x30)
            VehicleFuelLog := []
            return
        }

        fields := StrSplit(line, "`t")
        if (fields.Length != 9) {
            MsgBox("Soubor kilometrů a tankování je poškozený. Řádek " index " musí obsahovat přesně 9 polí oddělených tabulátory.`n`nZkontrolujte soubor:`n" FuelLogFile, AppTitle, 0x30)
            VehicleFuelLog := []
            return
        }

        VehicleFuelLog.Push({
            id: UnescapeField(fields[1]),
            vehicleId: UnescapeField(fields[2]),
            entryDate: UnescapeField(fields[3]),
            odometer: UnescapeField(fields[4]),
            liters: UnescapeField(fields[5]),
            totalCost: UnescapeField(fields[6]),
            fullTank: (UnescapeField(fields[7]) = "1") ? 1 : 0,
            fuelType: UnescapeField(fields[8]),
            note: UnescapeField(fields[9])
        })
    }
}

SaveVehicleFuelLog() {
    global FuelLogFile

    WriteTextFileUtf8(FuelLogFile, BuildFuelDataContent())
}

BuildFuelDataContent() {
    global VehicleFuelLog

    lines := ["# Vehimap fuel v1"]
    for entry in VehicleFuelLog {
        lines.Push(
            EscapeField(entry.id) "`t"
            EscapeField(entry.vehicleId) "`t"
            EscapeField(entry.entryDate) "`t"
            EscapeField(entry.odometer) "`t"
            EscapeField(entry.liters) "`t"
            EscapeField(entry.totalCost) "`t"
            EscapeField(entry.fullTank ? "1" : "0") "`t"
            EscapeField(entry.fuelType) "`t"
            EscapeField(entry.note)
        )
    }

    return JoinLines(lines)
}

DeleteVehicleFuelEntries(vehicleId) {
    global VehicleFuelLog

    filtered := []
    changed := false
    for entry in VehicleFuelLog {
        if (entry.vehicleId = vehicleId) {
            changed := true
        } else {
            filtered.Push(entry)
        }
    }

    VehicleFuelLog := filtered
    if changed {
        SaveVehicleFuelLog()
    }
}

GetVehicleFuelEntries(vehicleId) {
    global VehicleFuelLog

    entries := []
    for entry in VehicleFuelLog {
        if (entry.vehicleId = vehicleId) {
            entries.Push(entry)
        }
    }

    SortVehicleFuelEntries(&entries)
    return entries
}

GetVehicleFuelEntryCount(vehicleId) {
    count := 0
    global VehicleFuelLog

    for entry in VehicleFuelLog {
        if (entry.vehicleId = vehicleId) {
            count += 1
        }
    }

    return count
}

BuildVehicleFuelSummaryText(vehicleId) {
    entries := GetVehicleFuelEntries(vehicleId)
    if (entries.Length = 0) {
        return "K tomuto vozidlu zatím nejsou uloženy žádné záznamy kilometrů ani tankování."
    }

    summary := "Záznamů: " entries.Length ". Poslední tachometr: " FormatHistoryOdometer(entries[1].odometer) "."
    latestFuelEntry := ""
    for entry in entries {
        if (entry.liters != "" || entry.totalCost != "") {
            latestFuelEntry := entry
            break
        }
    }

    if IsObject(latestFuelEntry) {
        summary .= " Poslední tankování: "
        if (latestFuelEntry.liters != "") {
            summary .= FormatFuelLiters(latestFuelEntry.liters)
        } else {
            summary .= "bez údajů o litrech"
        }
        if (latestFuelEntry.totalCost != "") {
            summary .= " za " FormatFuelMoney(latestFuelEntry.totalCost)
        }
        summary .= " (" latestFuelEntry.entryDate ")."
    }

    return summary
}

FindVehicleFuelEntryById(entryId) {
    global VehicleFuelLog

    for entry in VehicleFuelLog {
        if (entry.id = entryId) {
            return entry
        }
    }

    return ""
}

FindVehicleFuelEntryIndexById(entryId) {
    global VehicleFuelLog

    for index, entry in VehicleFuelLog {
        if (entry.id = entryId) {
            return index
        }
    }

    return 0
}

GenerateFuelEntryId() {
    return "fuel_" A_Now "_" Random(1000, 9999)
}

SortVehicleFuelEntries(&items) {
    count := items.Length
    if (count < 2) {
        return
    }

    Loop count - 1 {
        i := A_Index + 1
        current := items[i]
        j := i - 1

        while (j >= 1 && CompareVehicleFuelEntries(current, items[j]) < 0) {
            items[j + 1] := items[j]
            j -= 1
        }
        items[j + 1] := current
    }
}

CompareVehicleFuelEntries(left, right) {
    leftStamp := ParseEventDateStamp(left.entryDate)
    rightStamp := ParseEventDateStamp(right.entryDate)

    if (leftStamp > rightStamp) {
        return -1
    }
    if (leftStamp < rightStamp) {
        return 1
    }

    leftOdometer := (Trim(left.odometer) = "") ? -1 : left.odometer + 0
    rightOdometer := (Trim(right.odometer) = "") ? -1 : right.odometer + 0
    if (leftOdometer > rightOdometer) {
        return -1
    }
    if (leftOdometer < rightOdometer) {
        return 1
    }

    return CompareTextValues(left.id, right.id)
}

NormalizeDecimalText(value) {
    value := Trim(StrReplace(value, " ", ""))
    if (value = "") {
        return ""
    }

    value := StrReplace(value, ".", ",")
    if !RegExMatch(value, "^\d+(,\d+)?$") {
        return ""
    }

    parts := StrSplit(value, ",")
    integerPart := RegExReplace(parts[1], "^0+(?=\d)", "")
    if (integerPart = "") {
        integerPart := "0"
    }

    if (parts.Length = 1) {
        return integerPart
    }

    decimalPart := RegExReplace(parts[2], "0+$")
    if (decimalPart = "") {
        return integerPart
    }

    return integerPart "," decimalPart
}

FormatFuelLiters(value) {
    value := Trim(StrReplace(value, ".", ","))
    if (value = "") {
        return ""
    }

    return value " l"
}

FormatFuelMoney(value) {
    value := Trim(StrReplace(value, ".", ","))
    if (value = "") {
        return ""
    }

    return value " Kč"
}

GetLatestVehicleOdometerText(vehicleId) {
    global VehicleHistory, VehicleFuelLog

    bestStamp := ""
    bestOdometer := ""
    bestOdometerValue := -1

    for entry in VehicleFuelLog {
        if (entry.vehicleId != vehicleId || Trim(entry.odometer) = "") {
            continue
        }

        stamp := ParseEventDateStamp(entry.entryDate)
        odometerValue := entry.odometer + 0
        if (bestStamp = "" || stamp > bestStamp || (stamp = bestStamp && odometerValue > bestOdometerValue)) {
            bestStamp := stamp
            bestOdometer := entry.odometer
            bestOdometerValue := odometerValue
        }
    }

    for event in VehicleHistory {
        if (event.vehicleId != vehicleId || Trim(event.odometer) = "") {
            continue
        }

        stamp := ParseEventDateStamp(event.eventDate)
        odometerValue := event.odometer + 0
        if (bestStamp = "" || stamp > bestStamp || (stamp = bestStamp && odometerValue > bestOdometerValue)) {
            bestStamp := stamp
            bestOdometer := event.odometer
            bestOdometerValue := odometerValue
        }
    }

    return bestOdometer
}

LoadVehicleRecords() {
    global AppTitle, VehicleRecords, RecordsFile

    VehicleRecords := []
    if !FileExist(RecordsFile) {
        return
    }

    content := FileRead(RecordsFile, "UTF-8")
    content := StrReplace(content, Chr(0xFEFF))
    lines := StrSplit(content, "`n", "`r")
    firstNonEmptyLine := ""
    dataStarted := false

    for rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        firstNonEmptyLine := line
        break
    }

    if (firstNonEmptyLine = "") {
        return
    }

    if (firstNonEmptyLine != "# Vehimap records v1" && firstNonEmptyLine != "# Vehimap records v2") {
        MsgBox("Soubor pojištění a dokladů není v podporovaném formátu.`n`nZkontrolujte soubor:`n" RecordsFile, AppTitle, 0x30)
        return
    }

    for index, rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        if !dataStarted {
            dataStarted := true
            continue
        }

        if (SubStr(line, 1, 1) = "#") {
            MsgBox("Soubor pojištění a dokladů obsahuje neplatnou hlavičku nebo komentář na řádku " index ".`n`nZkontrolujte soubor:`n" RecordsFile, AppTitle, 0x30)
            VehicleRecords := []
            return
        }

        fields := StrSplit(line, "`t")
        if (
            (firstNonEmptyLine = "# Vehimap records v1" && fields.Length != 10)
            || (firstNonEmptyLine = "# Vehimap records v2" && fields.Length != 11)
        ) {
            MsgBox("Soubor pojištění a dokladů je poškozený. Řádek " index " musí odpovídat hlavičce souboru.`n`nZkontrolujte soubor:`n" RecordsFile, AppTitle, 0x30)
            VehicleRecords := []
            return
        }

        VehicleRecords.Push(NormalizeVehicleRecordEntry({
            id: UnescapeField(fields[1]),
            vehicleId: UnescapeField(fields[2]),
            recordType: UnescapeField(fields[3]),
            title: UnescapeField(fields[4]),
            provider: UnescapeField(fields[5]),
            validFrom: UnescapeField(fields[6]),
            validTo: UnescapeField(fields[7]),
            price: UnescapeField(fields[8]),
            attachmentMode: (fields.Length >= 11) ? UnescapeField(fields[9]) : "external",
            filePath: (fields.Length >= 11) ? UnescapeField(fields[10]) : UnescapeField(fields[9]),
            note: (fields.Length >= 11) ? UnescapeField(fields[11]) : UnescapeField(fields[10])
        }))
    }
}

SaveVehicleRecords() {
    global RecordsFile

    WriteTextFileUtf8(RecordsFile, BuildRecordsDataContent())
}

BuildRecordsDataContent() {
    global VehicleRecords

    lines := ["# Vehimap records v2"]
    for rawEntry in VehicleRecords {
        entry := NormalizeVehicleRecordEntry(rawEntry)
        lines.Push(
            EscapeField(entry.id) "`t"
            EscapeField(entry.vehicleId) "`t"
            EscapeField(entry.recordType) "`t"
            EscapeField(entry.title) "`t"
            EscapeField(entry.provider) "`t"
            EscapeField(entry.validFrom) "`t"
            EscapeField(entry.validTo) "`t"
            EscapeField(entry.price) "`t"
            EscapeField(entry.attachmentMode) "`t"
            EscapeField(entry.filePath) "`t"
            EscapeField(entry.note)
        )
    }

    return JoinLines(lines)
}

DeleteVehicleRecords(vehicleId) {
    global VehicleRecords

    filtered := []
    changed := false
    for entry in VehicleRecords {
        if (entry.vehicleId = vehicleId) {
            changed := true
        } else {
            filtered.Push(entry)
        }
    }

    VehicleRecords := filtered
    if changed {
        SaveVehicleRecords()
        DeleteManagedVehicleAttachmentDirectory(vehicleId)
    }
}

GetVehicleRecords(vehicleId) {
    global VehicleRecords

    entries := []
    for entry in VehicleRecords {
        if (entry.vehicleId = vehicleId) {
            entries.Push(entry)
        }
    }

    SortVehicleRecords(&entries)
    return entries
}

GetVehicleRecordCount(vehicleId) {
    count := 0
    global VehicleRecords

    for entry in VehicleRecords {
        if (entry.vehicleId = vehicleId) {
            count += 1
        }
    }

    return count
}

BuildVehicleRecordsSummaryText(vehicleId) {
    entries := GetVehicleRecords(vehicleId)
    if (entries.Length = 0) {
        return "K tomuto vozidlu zatím není uložen žádný záznam pojištění ani dokladů."
    }

    summary := "Záznamů: " entries.Length "."
    missingPathCount := 0
    emptyPathCount := 0
    for entry in entries {
        pathKind := GetVehicleRecordPathInfo(entry).kind
        if (pathKind = "missing_file" || pathKind = "missing_folder") {
            missingPathCount += 1
        } else if (pathKind = "empty") {
            emptyPathCount += 1
        }
    }

    nearestRecord := ""
    for entry in entries {
        if (Trim(entry.validTo) != "") {
            nearestRecord := entry
            break
        }
    }

    if IsObject(nearestRecord) {
        summary .= " Nejbližší platnost: " nearestRecord.title
        if (nearestRecord.provider != "") {
            summary .= " (" nearestRecord.provider ")"
        }
        summary .= " do " nearestRecord.validTo "."
    } else {
        summary .= " U žádného záznamu není vyplněné datum platnosti."
    }

    if (missingPathCount > 0) {
        summary .= " Nedostupných cest: " missingPathCount "."
    }
    if (emptyPathCount > 0) {
        summary .= " Bez vyplněné cesty: " emptyPathCount "."
    }

    return summary
}

FindVehicleRecordById(entryId) {
    global VehicleRecords

    for entry in VehicleRecords {
        if (entry.id = entryId) {
            return entry
        }
    }

    return ""
}

FindVehicleRecordIndexById(entryId) {
    global VehicleRecords

    for index, entry in VehicleRecords {
        if (entry.id = entryId) {
            return index
        }
    }

    return 0
}

NormalizeVehicleRecordEntry(entry) {
    return {
        id: entry.HasOwnProp("id") ? entry.id : "",
        vehicleId: entry.HasOwnProp("vehicleId") ? entry.vehicleId : "",
        recordType: entry.HasOwnProp("recordType") ? entry.recordType : "",
        title: entry.HasOwnProp("title") ? entry.title : "",
        provider: entry.HasOwnProp("provider") ? entry.provider : "",
        validFrom: entry.HasOwnProp("validFrom") ? entry.validFrom : "",
        validTo: entry.HasOwnProp("validTo") ? entry.validTo : "",
        price: entry.HasOwnProp("price") ? entry.price : "",
        attachmentMode: NormalizeVehicleRecordAttachmentMode(entry.HasOwnProp("attachmentMode") ? entry.attachmentMode : ""),
        filePath: entry.HasOwnProp("filePath") ? entry.filePath : "",
        note: entry.HasOwnProp("note") ? entry.note : ""
    }
}

NormalizeVehicleRecordAttachmentMode(mode) {
    mode := StrLower(Trim(mode))
    if (mode = "managed") {
        return "managed"
    }

    return "external"
}

GetVehicleRecordAttachmentMode(entry) {
    if !IsObject(entry) {
        return "external"
    }

    return NormalizeVehicleRecordAttachmentMode(entry.HasOwnProp("attachmentMode") ? entry.attachmentMode : "")
}

IsVehicleRecordManagedAttachment(entry) {
    return GetVehicleRecordAttachmentMode(entry) = "managed"
}

NormalizeVehicleAttachmentRelativePath(path) {
    path := StrReplace(Trim(path), "\", "/")
    while (SubStr(path, 1, 2) = "./") {
        path := SubStr(path, 3)
    }
    if (StrLower(SubStr(path, 1, 5)) = "data/") {
        path := SubStr(path, 6)
    }
    while (SubStr(path, 1, 1) = "/") {
        path := SubStr(path, 2)
    }
    return path
}

GetManagedVehicleAttachmentAbsolutePath(relativePath) {
    global DataDir

    relativePath := NormalizeVehicleAttachmentRelativePath(relativePath)
    if (relativePath = "") {
        return ""
    }

    return DataDir "\" StrReplace(relativePath, "/", "\")
}

GetManagedVehicleAttachmentsRootPath() {
    global AttachmentsDir

    return AttachmentsDir
}

ResolveVehicleRecordFilePath(entry) {
    if !IsObject(entry) {
        return ""
    }

    path := Trim(entry.filePath)
    if (path = "") {
        return ""
    }

    if IsVehicleRecordManagedAttachment(entry) {
        if RegExMatch(path, "i)^[a-z]:[\\/]") || RegExMatch(path, "^\\\\") || RegExMatch(path, "^[\\/]") {
            return path
        }
        return GetManagedVehicleAttachmentAbsolutePath(path)
    }

    if RegExMatch(path, "i)^[a-z]:[\\/]") || RegExMatch(path, "^\\\\") || RegExMatch(path, "^[\\/]") {
        return path
    }

    return A_ScriptDir "\" path
}

GetVehicleRecordResolvedFileName(entry) {
    resolvedPath := ResolveVehicleRecordFilePath(entry)
    if (resolvedPath != "") {
        fileName := GetFileNameFromPath(resolvedPath)
        if (fileName != "") {
            return fileName
        }
    }

    return GetFileNameFromPath(IsObject(entry) && entry.HasOwnProp("filePath") ? entry.filePath : "")
}

GetVehicleRecordDisplayPath(entry) {
    if !IsObject(entry) {
        return ""
    }

    resolvedPath := ResolveVehicleRecordFilePath(entry)
    return resolvedPath != "" ? resolvedPath : Trim(entry.filePath)
}

GetVehicleRecordAttachmentModeLabel(modeOrEntry) {
    mode := IsObject(modeOrEntry) ? GetVehicleRecordAttachmentMode(modeOrEntry) : NormalizeVehicleRecordAttachmentMode(modeOrEntry)
    return (mode = "managed") ? "Spravovaná kopie" : "Externí cesta"
}

BuildManagedVehicleAttachmentRelativePath(vehicleId, sourcePath, preferredRelativePath := "") {
    fileName := MakeSafeAttachmentFileName(GetFileNameFromPath(sourcePath))
    if (fileName = "") {
        fileName := "priloha"
    }

    relativeDir := "attachments/" vehicleId
    baseName := fileName
    extension := ""
    dotPos := InStr(fileName, ".", , -1)
    if (dotPos > 1) {
        baseName := SubStr(fileName, 1, dotPos - 1)
        extension := SubStr(fileName, dotPos)
    }

    preferredRelativePath := NormalizeVehicleAttachmentRelativePath(preferredRelativePath)
    attempt := 1
    loop {
        candidateName := (attempt = 1) ? (baseName extension) : (baseName "-" attempt extension)
        candidate := relativeDir "/" candidateName
        if (preferredRelativePath != "" && candidate = preferredRelativePath) {
            return candidate
        }

        if !FileExist(GetManagedVehicleAttachmentAbsolutePath(candidate)) {
            return candidate
        }

        attempt += 1
    }
}

EnsureManagedVehicleAttachmentDirectory(vehicleId) {
    directoryPath := GetManagedVehicleAttachmentAbsolutePath("attachments/" vehicleId)
    if !InStr(FileExist(directoryPath), "D") {
        DirCreate(directoryPath)
    }
    return directoryPath
}

CopySourceFileToManagedVehicleAttachment(vehicleId, sourcePath, targetRelativePath := "") {
    sourcePath := Trim(sourcePath)
    if (sourcePath = "") {
        throw Error("Nebyl vybrán zdrojový soubor pro spravovanou kopii.")
    }
    if InStr(FileExist(sourcePath), "D") {
        throw Error("Vybraný objekt je složka. Pro spravovanou kopii vyberte soubor.")
    }
    if !FileExist(sourcePath) {
        throw Error("Vybraný soubor se nepodařilo najít.`n`n" sourcePath)
    }

    if (targetRelativePath = "") {
        targetRelativePath := BuildManagedVehicleAttachmentRelativePath(vehicleId, sourcePath)
    } else {
        targetRelativePath := NormalizeVehicleAttachmentRelativePath(targetRelativePath)
    }

    targetPath := GetManagedVehicleAttachmentAbsolutePath(targetRelativePath)
    SplitPath(targetPath, , &directoryPath)
    if !InStr(FileExist(directoryPath), "D") {
        DirCreate(directoryPath)
    }

    if !PathsPointToSameLocation(sourcePath, targetPath) {
        FileCopy(sourcePath, targetPath, true)
    }
    return targetRelativePath
}

PathsPointToSameLocation(leftPath, rightPath) {
    return StrLower(Trim(leftPath)) = StrLower(Trim(rightPath))
}

MakeSafeAttachmentFileName(fileName) {
    fileName := Trim(fileName)
    if (fileName = "") {
        return ""
    }

    fileName := StrReplace(fileName, '"', "_")
    fileName := RegExReplace(fileName, "[<>:/\\|?*]", "_")
    fileName := RegExReplace(fileName, "[\x00-\x1F]", "_")
    fileName := RegExReplace(fileName, "\s+", " ")
    fileName := Trim(fileName, " .")
    return fileName
}

DeleteManagedVehicleAttachmentDirectory(vehicleId) {
    directoryPath := GetManagedVehicleAttachmentAbsolutePath("attachments/" vehicleId)
    if InStr(FileExist(directoryPath), "D") {
        DirDelete(directoryPath, true)
    }
}

PruneVehicleManagedAttachments(vehicleId) {
    attachmentDir := GetManagedVehicleAttachmentAbsolutePath("attachments/" vehicleId)
    if !InStr(FileExist(attachmentDir), "D") {
        return
    }

    referenced := Map()
    for entry in VehicleRecords {
        if (entry.vehicleId = vehicleId && IsVehicleRecordManagedAttachment(entry) && Trim(entry.filePath) != "") {
            referenced[NormalizeVehicleAttachmentRelativePath(entry.filePath)] := true
        }
    }

    Loop Files attachmentDir "\*", "FR" {
        fullPath := A_LoopFileFullPath
        if InStr(FileExist(fullPath), "D") {
            continue
        }

        relativePath := NormalizeVehicleAttachmentRelativePath(SubStr(fullPath, StrLen(GetManagedVehicleAttachmentsRootPath()) + 2))
        if !referenced.Has(relativePath) {
            try FileDelete(fullPath)
        }
    }

    if InStr(FileExist(attachmentDir), "D") {
        try DirDelete(attachmentDir, false)
    }
}

GetManagedVehicleAttachmentBackupItems() {
    global VehicleRecords

    items := []
    seen := Map()
    missingCount := 0

    for entry in VehicleRecords {
        if !IsVehicleRecordManagedAttachment(entry) {
            continue
        }

        relativePath := NormalizeVehicleAttachmentRelativePath(entry.filePath)
        if (relativePath = "" || seen.Has(relativePath)) {
            continue
        }

        seen[relativePath] := true
        absolutePath := GetManagedVehicleAttachmentAbsolutePath(relativePath)
        if !FileExist(absolutePath) || InStr(FileExist(absolutePath), "D") {
            missingCount += 1
            continue
        }

        items.Push({
            relativePath: relativePath,
            absolutePath: absolutePath
        })
    }

    return {items: items, missingCount: missingCount}
}

ReadBinaryFileAsBase64(path) {
    file := FileOpen(path, "r")
    if !IsObject(file) {
        throw Error("Soubor se nepodařilo otevřít pro čtení.`n`n" path)
    }

    try {
        length := file.Length
        binaryData := Buffer(length)
        if (length > 0) {
            file.RawRead(binaryData, length)
        }
    } finally {
        file.Close()
    }

    return Base64EncodeBuffer(binaryData, length)
}

WriteBinaryFileFromBase64(path, base64Text) {
    bytes := Base64DecodeToBuffer(base64Text)
    SplitPath(path, , &directoryPath)
    if (directoryPath != "" && !InStr(FileExist(directoryPath), "D")) {
        DirCreate(directoryPath)
    }

    file := FileOpen(path, "w")
    if !IsObject(file) {
        throw Error("Soubor se nepodařilo otevřít pro zápis.`n`n" path)
    }

    try {
        if (bytes.Size > 0) {
            file.RawWrite(bytes, bytes.Size)
        }
    } finally {
        file.Close()
    }
}

Base64EncodeBuffer(binaryData, byteCount := "") {
    if (byteCount = "") {
        byteCount := binaryData.Size
    }

    charsNeeded := 0
    if !DllCall("Crypt32\CryptBinaryToStringW", "ptr", binaryData.Ptr, "uint", byteCount, "uint", 0x40000001, "ptr", 0, "uint*", &charsNeeded, "int") {
        throw OSError()
    }

    output := Buffer(charsNeeded * 2, 0)
    if !DllCall("Crypt32\CryptBinaryToStringW", "ptr", binaryData.Ptr, "uint", byteCount, "uint", 0x40000001, "ptr", output.Ptr, "uint*", &charsNeeded, "int") {
        throw OSError()
    }

    return StrGet(output, charsNeeded - 1, "UTF-16")
}

Base64DecodeToBuffer(base64Text) {
    byteCount := 0
    if !DllCall("Crypt32\CryptStringToBinaryW", "str", base64Text, "uint", 0, "uint", 0x00000001, "ptr", 0, "uint*", &byteCount, "ptr", 0, "ptr", 0, "int") {
        throw OSError()
    }

    output := Buffer(byteCount, 0)
    if !DllCall("Crypt32\CryptStringToBinaryW", "str", base64Text, "uint", 0, "uint", 0x00000001, "ptr", output.Ptr, "uint*", &byteCount, "ptr", 0, "ptr", 0, "int") {
        throw OSError()
    }

    return output
}

GenerateVehicleRecordId() {
    return "record_" A_Now "_" Random(1000, 9999)
}

SortVehicleRecords(&items) {
    count := items.Length
    if (count < 2) {
        return
    }

    Loop count - 1 {
        i := A_Index + 1
        current := items[i]
        j := i - 1

        while (j >= 1 && CompareVehicleRecordEntries(current, items[j]) < 0) {
            items[j + 1] := items[j]
            j -= 1
        }
        items[j + 1] := current
    }
}

CompareVehicleRecordEntries(left, right) {
    leftKey := ParseDueStamp(left.validTo)
    rightKey := ParseDueStamp(right.validTo)

    if (leftKey = "") {
        leftKey := "99999999999999"
    }
    if (rightKey = "") {
        rightKey := "99999999999999"
    }

    if (leftKey < rightKey) {
        return -1
    }
    if (leftKey > rightKey) {
        return 1
    }

    result := CompareTextValues(left.recordType, right.recordType)
    if (result != 0) {
        return result
    }

    return CompareTextValues(left.title, right.title)
}

GetFileNameFromPath(path) {
    path := Trim(path)
    if (path = "") {
        return ""
    }

    SplitPath(path, &fileName)
    return (fileName = "") ? path : fileName
}

LoadVehicleMeta() {
    global AppTitle, VehicleMetaEntries, VehicleMetaFile

    VehicleMetaEntries := []
    if !FileExist(VehicleMetaFile) {
        return
    }

    content := FileRead(VehicleMetaFile, "UTF-8")
    content := StrReplace(content, Chr(0xFEFF))
    lines := StrSplit(content, "`n", "`r")
    firstNonEmptyLine := ""
    dataStarted := false

    for rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        firstNonEmptyLine := line
        break
    }

    if (firstNonEmptyLine = "") {
        return
    }

    if (firstNonEmptyLine != "# Vehimap meta v1" && firstNonEmptyLine != "# Vehimap meta v2") {
        MsgBox("Soubor stavů, štítků a servisního profilu vozidel není v podporovaném formátu.`n`nZkontrolujte soubor:`n" VehicleMetaFile, AppTitle, 0x30)
        return
    }

    for index, rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        if !dataStarted {
            dataStarted := true
            continue
        }

        if (SubStr(line, 1, 1) = "#") {
            MsgBox("Soubor stavů, štítků a servisního profilu vozidel obsahuje neplatnou hlavičku nebo komentář na řádku " index ".`n`nZkontrolujte soubor:`n" VehicleMetaFile, AppTitle, 0x30)
            VehicleMetaEntries := []
            return
        }

        fields := StrSplit(line, "`t")
        if (
            (firstNonEmptyLine = "# Vehimap meta v1" && fields.Length != 3)
            || (firstNonEmptyLine = "# Vehimap meta v2" && fields.Length != 7)
        ) {
            MsgBox("Soubor stavů, štítků a servisního profilu vozidel je poškozený. Řádek " index " musí odpovídat hlavičce souboru.`n`nZkontrolujte soubor:`n" VehicleMetaFile, AppTitle, 0x30)
            VehicleMetaEntries := []
            return
        }

        VehicleMetaEntries.Push(NormalizeVehicleMetaEntry({
            vehicleId: UnescapeField(fields[1]),
            state: UnescapeField(fields[2]),
            tags: UnescapeField(fields[3]),
            powertrain: (fields.Length >= 4) ? UnescapeField(fields[4]) : "",
            climateProfile: (fields.Length >= 5) ? UnescapeField(fields[5]) : "",
            timingDrive: (fields.Length >= 6) ? UnescapeField(fields[6]) : "",
            transmission: (fields.Length >= 7) ? UnescapeField(fields[7]) : ""
        }))
    }
}

SaveVehicleMeta() {
    global VehicleMetaFile

    WriteTextFileUtf8(VehicleMetaFile, BuildVehicleMetaDataContent())
}

BuildVehicleMetaDataContent() {
    global VehicleMetaEntries

    lines := ["# Vehimap meta v2"]
    for rawEntry in VehicleMetaEntries {
        entry := NormalizeVehicleMetaEntry(rawEntry)
        lines.Push(
            EscapeField(entry.vehicleId) "	"
            EscapeField(entry.state) "	"
            EscapeField(entry.tags) "	"
            EscapeField(entry.powertrain) "	"
            EscapeField(entry.climateProfile) "	"
            EscapeField(entry.timingDrive) "	"
            EscapeField(entry.transmission)
        )
    }

    return JoinLines(lines)
}
GetVehicleMeta(vehicleId) {
    global VehicleMetaEntries

    for entry in VehicleMetaEntries {
        if (entry.vehicleId = vehicleId) {
            return NormalizeVehicleMetaEntry(entry)
        }
    }

    return BuildDefaultVehicleMeta(vehicleId)
}
SaveVehicleMetaEntry(vehicleId, state := "", tags := "", powertrain := "", climateProfile := "", timingDrive := "", transmission := "") {
    global VehicleMetaEntries

    state := NormalizeVehicleState(state)
    tags := NormalizeTagList(tags)
    powertrain := NormalizeVehiclePowertrain(powertrain)
    climateProfile := NormalizeVehicleClimateProfile(climateProfile)
    timingDrive := NormalizeVehicleTimingDrive(timingDrive)
    transmission := NormalizeVehicleTransmission(transmission)
    index := FindVehicleMetaIndex(vehicleId)

    if (state = "" && tags = "" && powertrain = "" && climateProfile = "" && timingDrive = "" && transmission = "") {
        if index {
            VehicleMetaEntries.RemoveAt(index)
            SaveVehicleMeta()
        }
        return
    }

    entry := {
        vehicleId: vehicleId,
        state: state,
        tags: tags,
        powertrain: powertrain,
        climateProfile: climateProfile,
        timingDrive: timingDrive,
        transmission: transmission
    }

    if index {
        VehicleMetaEntries[index] := entry
    } else {
        VehicleMetaEntries.Push(entry)
    }

    SaveVehicleMeta()
}
FindVehicleMetaIndex(vehicleId) {
    global VehicleMetaEntries

    for index, entry in VehicleMetaEntries {
        if (entry.vehicleId = vehicleId) {
            return index
        }
    }

    return 0
}

DeleteVehicleMeta(vehicleId) {
    global VehicleMetaEntries

    index := FindVehicleMetaIndex(vehicleId)
    if !index {
        return
    }

    VehicleMetaEntries.RemoveAt(index)
    SaveVehicleMeta()
}

BuildDefaultVehicleMeta(vehicleId := "") {
    return {
        vehicleId: vehicleId,
        state: "",
        tags: "",
        powertrain: "",
        climateProfile: "",
        timingDrive: "",
        transmission: ""
    }
}

NormalizeVehicleMetaEntry(entry) {
    normalized := BuildDefaultVehicleMeta(entry.HasOwnProp("vehicleId") ? entry.vehicleId : "")
    normalized.state := NormalizeVehicleState(entry.HasOwnProp("state") ? entry.state : "")
    normalized.tags := NormalizeTagList(entry.HasOwnProp("tags") ? entry.tags : "")
    normalized.powertrain := NormalizeVehiclePowertrain(entry.HasOwnProp("powertrain") ? entry.powertrain : "")
    normalized.climateProfile := NormalizeVehicleClimateProfile(entry.HasOwnProp("climateProfile") ? entry.climateProfile : "")
    normalized.timingDrive := NormalizeVehicleTimingDrive(entry.HasOwnProp("timingDrive") ? entry.timingDrive : "")
    normalized.transmission := NormalizeVehicleTransmission(entry.HasOwnProp("transmission") ? entry.transmission : "")
    return normalized
}
NormalizeVehicleState(state) {
    global VehicleStateOptions

    return NormalizeVehicleMetaOption(state, VehicleStateOptions)
}

NormalizeVehiclePowertrain(powertrain) {
    global VehiclePowertrainOptions

    return NormalizeVehicleMetaOption(powertrain, VehiclePowertrainOptions)
}

NormalizeVehicleClimateProfile(profile) {
    global VehicleClimateProfileOptions

    return NormalizeVehicleMetaOption(profile, VehicleClimateProfileOptions)
}

NormalizeVehicleTimingDrive(timingDrive) {
    global VehicleTimingDriveOptions

    return NormalizeVehicleMetaOption(timingDrive, VehicleTimingDriveOptions)
}

NormalizeVehicleTransmission(transmission) {
    global VehicleTransmissionOptions

    return NormalizeVehicleMetaOption(transmission, VehicleTransmissionOptions)
}

NormalizeVehicleMetaOption(value, options) {
    value := Trim(value)
    if (value = "") {
        return ""
    }

    for item in options {
        if (item = value) {
            return item
        }
    }

    return value
}
NormalizeTagList(tags) {
    tags := Trim(tags)
    if (tags = "") {
        return ""
    }

    tags := StrReplace(tags, ";", ",")
    rawItems := StrSplit(tags, ",")
    normalized := []
    seen := Map()

    for item in rawItems {
        cleanItem := Trim(item)
        if (cleanItem = "") {
            continue
        }

        key := StrLower(cleanItem)
        if seen.Has(key) {
            continue
        }

        seen[key] := true
        normalized.Push(cleanItem)
    }

    return JoinInline(normalized, ", ")
}

LoadVehicleReminders() {
    global AppTitle, VehicleReminders, RemindersFile

    VehicleReminders := []
    if !FileExist(RemindersFile) {
        return
    }

    content := FileRead(RemindersFile, "UTF-8")
    content := StrReplace(content, Chr(0xFEFF))
    lines := StrSplit(content, "`n", "`r")
    firstNonEmptyLine := ""
    dataStarted := false

    for rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        firstNonEmptyLine := line
        break
    }

    if (firstNonEmptyLine = "") {
        return
    }

    if (firstNonEmptyLine != "# Vehimap reminders v1" && firstNonEmptyLine != "# Vehimap reminders v2") {
        MsgBox("Soubor vlastních připomínek není v podporovaném formátu.`n`nZkontrolujte soubor:`n" RemindersFile, AppTitle, 0x30)
        return
    }

    isV2 := (firstNonEmptyLine = "# Vehimap reminders v2")

    for index, rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        if !dataStarted {
            dataStarted := true
            continue
        }

        if (SubStr(line, 1, 1) = "#") {
            MsgBox("Soubor vlastních připomínek obsahuje neplatnou hlavičku nebo komentář na řádku " index ".`n`nZkontrolujte soubor:`n" RemindersFile, AppTitle, 0x30)
            VehicleReminders := []
            return
        }

        fields := StrSplit(line, "`t")
        expectedFieldCount := isV2 ? 7 : 6
        if (fields.Length != expectedFieldCount) {
            MsgBox("Soubor vlastních připomínek je poškozený. Řádek " index " musí obsahovat přesně " expectedFieldCount " polí oddělených tabulátory.`n`nZkontrolujte soubor:`n" RemindersFile, AppTitle, 0x30)
            VehicleReminders := []
            return
        }

        reminderDays := UnescapeField(fields[5])
        if !RegExMatch(reminderDays, "^\d{1,3}$") {
            reminderDays := "30"
        }

        VehicleReminders.Push({
            id: UnescapeField(fields[1]),
            vehicleId: UnescapeField(fields[2]),
            title: UnescapeField(fields[3]),
            dueDate: UnescapeField(fields[4]),
            reminderDays: reminderDays,
            repeatMode: isV2 ? NormalizeReminderRepeat(UnescapeField(fields[6])) : "Neopakovat",
            note: isV2 ? UnescapeField(fields[7]) : UnescapeField(fields[6])
        })
    }
}

SaveVehicleReminders() {
    global RemindersFile

    WriteTextFileUtf8(RemindersFile, BuildVehicleRemindersDataContent())
}

BuildVehicleRemindersDataContent() {
    global VehicleReminders

    lines := ["# Vehimap reminders v2"]
    for entry in VehicleReminders {
        lines.Push(
            EscapeField(entry.id) "`t"
            EscapeField(entry.vehicleId) "`t"
            EscapeField(entry.title) "`t"
            EscapeField(entry.dueDate) "`t"
            EscapeField(entry.reminderDays) "`t"
            EscapeField(GetReminderRepeatLabel(entry.HasOwnProp("repeatMode") ? entry.repeatMode : "")) "`t"
            EscapeField(entry.note)
        )
    }

    return JoinLines(lines)
}

DeleteVehicleReminders(vehicleId) {
    global VehicleReminders

    filtered := []
    changed := false
    for entry in VehicleReminders {
        if (entry.vehicleId = vehicleId) {
            changed := true
        } else {
            filtered.Push(entry)
        }
    }

    VehicleReminders := filtered
    if changed {
        SaveVehicleReminders()
    }
}

GetVehicleReminderEntries(vehicleId) {
    global VehicleReminders

    entries := []
    for entry in VehicleReminders {
        if (entry.vehicleId = vehicleId) {
            entries.Push(entry)
        }
    }

    SortVehicleReminderEntries(&entries)
    return entries
}

GetVehicleReminderCount(vehicleId) {
    count := 0
    global VehicleReminders

    for entry in VehicleReminders {
        if (entry.vehicleId = vehicleId) {
            count += 1
        }
    }

    return count
}

BuildVehicleReminderSummaryText(vehicleId) {
    entries := GetVehicleReminderEntries(vehicleId)
    if (entries.Length = 0) {
        return "K tomuto vozidlu zatím nejsou uloženy žádné vlastní připomínky."
    }

    nearest := entries[1]
    status := GetReminderExpirationStatusText(nearest.dueDate, nearest.reminderDays + 0)
    summary := "Připomínek: " entries.Length ". Nejbližší: " nearest.title " (" nearest.dueDate
    if (status != "") {
        summary .= ", " status
    }
    repeatLabel := GetReminderRepeatLabel(nearest.HasOwnProp("repeatMode") ? nearest.repeatMode : "")
    if (repeatLabel != "Neopakovat") {
        summary .= ", " repeatLabel
    }
    summary .= ")."
    return summary
}

FindVehicleReminderById(entryId) {
    global VehicleReminders

    for entry in VehicleReminders {
        if (entry.id = entryId) {
            return entry
        }
    }

    return ""
}

FindVehicleReminderIndexById(entryId) {
    global VehicleReminders

    for index, entry in VehicleReminders {
        if (entry.id = entryId) {
            return index
        }
    }

    return 0
}

GenerateVehicleReminderId() {
    return "rem_" A_Now "_" Random(1000, 9999)
}

LoadVehicleMaintenancePlans() {
    global AppTitle, VehicleMaintenancePlans, MaintenancePlansFile

    VehicleMaintenancePlans := []
    if !FileExist(MaintenancePlansFile) {
        return
    }

    content := FileRead(MaintenancePlansFile, "UTF-8")
    content := StrReplace(content, Chr(0xFEFF))
    lines := StrSplit(content, "`n", "`r")
    firstNonEmptyLine := ""
    dataStarted := false

    for rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        firstNonEmptyLine := line
        break
    }

    if (firstNonEmptyLine = "") {
        return
    }

    if (firstNonEmptyLine != "# Vehimap maintenance v1") {
        MsgBox("Soubor plánů údržby není v podporovaném formátu.`n`nZkontrolujte soubor:`n" MaintenancePlansFile, AppTitle, 0x30)
        return
    }

    for index, rawLine in lines {
        line := Trim(rawLine, "`r`n")
        if (line = "") {
            continue
        }

        if !dataStarted {
            dataStarted := true
            continue
        }

        if (SubStr(line, 1, 1) = "#") {
            MsgBox("Soubor plánů údržby obsahuje neplatnou hlavičku nebo komentář na řádku " index ".`n`nZkontrolujte soubor:`n" MaintenancePlansFile, AppTitle, 0x30)
            VehicleMaintenancePlans := []
            return
        }

        fields := StrSplit(line, "`t")
        if (fields.Length != 9) {
            MsgBox("Soubor plánů údržby je poškozený. Řádek " index " musí obsahovat přesně 9 polí oddělených tabulátory.`n`nZkontrolujte soubor:`n" MaintenancePlansFile, AppTitle, 0x30)
            VehicleMaintenancePlans := []
            return
        }

        intervalKm := NormalizeOdometerText(UnescapeField(fields[4]))
        intervalMonths := NormalizePositiveIntegerText(UnescapeField(fields[5]))
        lastServiceDate := NormalizeEventDate(UnescapeField(fields[6]))
        lastServiceOdometer := NormalizeOdometerText(UnescapeField(fields[7]))
        isActive := (UnescapeField(fields[8]) = "0") ? 0 : 1

        VehicleMaintenancePlans.Push({
            id: UnescapeField(fields[1]),
            vehicleId: UnescapeField(fields[2]),
            title: UnescapeField(fields[3]),
            intervalKm: intervalKm,
            intervalMonths: intervalMonths,
            lastServiceDate: lastServiceDate,
            lastServiceOdometer: lastServiceOdometer,
            isActive: isActive,
            note: UnescapeField(fields[9])
        })
    }
}

SaveVehicleMaintenancePlans() {
    global MaintenancePlansFile

    WriteTextFileUtf8(MaintenancePlansFile, BuildVehicleMaintenanceDataContent())
}

BuildVehicleMaintenanceDataContent() {
    global VehicleMaintenancePlans

    lines := ["# Vehimap maintenance v1"]
    for plan in VehicleMaintenancePlans {
        lines.Push(
            EscapeField(plan.id) "`t"
            EscapeField(plan.vehicleId) "`t"
            EscapeField(plan.title) "`t"
            EscapeField(plan.intervalKm) "`t"
            EscapeField(plan.intervalMonths) "`t"
            EscapeField(plan.lastServiceDate) "`t"
            EscapeField(plan.lastServiceOdometer) "`t"
            EscapeField(plan.isActive ? "1" : "0") "`t"
            EscapeField(plan.note)
        )
    }

    return JoinLines(lines)
}

DeleteVehicleMaintenancePlans(vehicleId) {
    global VehicleMaintenancePlans

    filtered := []
    changed := false
    for plan in VehicleMaintenancePlans {
        if (plan.vehicleId = vehicleId) {
            changed := true
        } else {
            filtered.Push(plan)
        }
    }

    VehicleMaintenancePlans := filtered
    if changed {
        SaveVehicleMaintenancePlans()
    }
}

GetVehicleMaintenancePlans(vehicleId, includeInactive := true) {
    global VehicleMaintenancePlans

    plans := []
    for plan in VehicleMaintenancePlans {
        if (plan.vehicleId != vehicleId) {
            continue
        }
        if !includeInactive && !plan.isActive {
            continue
        }
        plans.Push(plan)
    }

    return plans
}

GetVehicleMaintenancePlanCount(vehicleId) {
    count := 0
    global VehicleMaintenancePlans

    for plan in VehicleMaintenancePlans {
        if (plan.vehicleId = vehicleId) {
            count += 1
        }
    }

    return count
}

FindVehicleMaintenancePlanById(planId) {
    global VehicleMaintenancePlans

    for plan in VehicleMaintenancePlans {
        if (plan.id = planId) {
            return plan
        }
    }

    return ""
}

FindVehicleMaintenancePlanIndexById(planId) {
    global VehicleMaintenancePlans

    for index, plan in VehicleMaintenancePlans {
        if (plan.id = planId) {
            return index
        }
    }

    return 0
}

GenerateVehicleMaintenancePlanId() {
    return "mnt_" A_Now "_" Random(1000, 9999)
}

NormalizeReminderRepeat(repeatMode) {
    global ReminderRepeatOptions

    repeatMode := Trim(repeatMode)
    if (repeatMode = "") {
        return "Neopakovat"
    }

    for option in ReminderRepeatOptions {
        if (option = repeatMode) {
            return option
        }
    }

    return "Neopakovat"
}

GetReminderRepeatLabel(repeatMode) {
    return NormalizeReminderRepeat(repeatMode)
}

GetReminderRepeatYears(repeatMode) {
    repeatMode := NormalizeReminderRepeat(repeatMode)

    switch repeatMode {
        case "Každý rok":
            return 1
        case "Každé 2 roky":
            return 2
        case "Každých 5 let":
            return 5
        default:
            return 0
    }
}

AddYearsToEventDate(eventDate, yearsToAdd) {
    normalized := NormalizeEventDate(eventDate)
    if (normalized = "" || yearsToAdd = 0) {
        return normalized
    }

    parts := StrSplit(normalized, ".")
    day := parts[1] + 0
    month := parts[2] + 0
    year := (parts[3] + 0) + yearsToAdd
    maxDay := DaysInMonth(year, month)
    if (day > maxDay) {
        day := maxDay
    }

    return Format("{:02}.{:02}.{:04}", day, month, year)
}

AddMonthsToEventDate(eventDate, monthsToAdd) {
    normalized := NormalizeEventDate(eventDate)
    if (normalized = "" || monthsToAdd = 0) {
        return normalized
    }

    parts := StrSplit(normalized, ".")
    day := parts[1] + 0
    month := parts[2] + 0
    year := parts[3] + 0

    totalMonths := (year * 12 + month - 1) + monthsToAdd
    targetYear := Floor(totalMonths / 12)
    targetMonth := Mod(totalMonths, 12) + 1
    maxDay := DaysInMonth(targetYear, targetMonth)
    if (day > maxDay) {
        day := maxDay
    }

    return Format("{:02}.{:02}.{:04}", day, targetMonth, targetYear)
}

SortVehicleReminderEntries(&items) {
    count := items.Length
    if (count < 2) {
        return
    }

    Loop count - 1 {
        i := A_Index + 1
        current := items[i]
        j := i - 1

        while (j >= 1 && CompareVehicleReminderEntries(current, items[j]) < 0) {
            items[j + 1] := items[j]
            j -= 1
        }
        items[j + 1] := current
    }
}

CompareVehicleReminderEntries(left, right) {
    leftStamp := ParseReminderDueStamp(left.dueDate)
    rightStamp := ParseReminderDueStamp(right.dueDate)

    if (leftStamp = "") {
        leftStamp := "99999999999999"
    }
    if (rightStamp = "") {
        rightStamp := "99999999999999"
    }

    if (leftStamp < rightStamp) {
        return -1
    }
    if (leftStamp > rightStamp) {
        return 1
    }

    return CompareTextValues(left.title, right.title)
}

ParseReminderDueStamp(reminderDate) {
    normalized := NormalizeEventDate(reminderDate)
    if (normalized = "") {
        return ""
    }

    parts := StrSplit(normalized, ".")
    return parts[3] parts[2] parts[1] "235959"
}

GetReminderExpirationStatusText(reminderDate, reminderDays := 30) {
    dueStamp := ParseReminderDueStamp(reminderDate)
    if (dueStamp = "") {
        return ""
    }

    if (dueStamp < A_Now) {
        return "Po termínu"
    }

    cutoff := DateAdd(A_Now, reminderDays, "Days")
    if (dueStamp <= cutoff) {
        daysLeft := DateDiff(dueStamp, A_Now, "Days")
        if (daysLeft < 1) {
            return "Dnes"
        }
        return "Do " daysLeft " dnů"
    }

    return ""
}

GetUpcomingCustomReminders(vehicleId := "") {
    global VehicleReminders

    upcoming := []
    for entry in VehicleReminders {
        if (vehicleId != "" && entry.vehicleId != vehicleId) {
            continue
        }

        vehicle := FindVehicleById(entry.vehicleId)
        if !IsObject(vehicle) {
            continue
        }

        dueStamp := ParseReminderDueStamp(entry.dueDate)
        if (dueStamp = "") {
            continue
        }

        reminderDays := entry.reminderDays + 0
        cutoff := DateAdd(A_Now, reminderDays, "Days")
        if (dueStamp <= cutoff) {
            upcoming.Push({
                kind: "custom",
                vehicle: vehicle,
                reminder: entry,
                dueStamp: dueStamp
            })
        }
    }

    SortUpcomingByDue(&upcoming)
    return upcoming
}

GetVehicleReminderStateText(vehicleId) {
    upcoming := GetUpcomingCustomReminders(vehicleId)
    if (upcoming.Length = 0) {
        return ""
    }

    entry := upcoming[1].reminder
    status := GetReminderExpirationStatusText(entry.dueDate, entry.reminderDays + 0)
    if (status = "") {
        return ""
    }

    return "Př: " status
}

NormalizeEventDate(value) {
    value := Trim(value)
    if (value = "") {
        return ""
    }

    if !RegExMatch(value, "^\s*(\d{1,2})\s*[./-]\s*(\d{1,2})\s*[./-]\s*(\d{4})\s*$", &match) {
        return ""
    }

    day := match[1] + 0
    month := match[2] + 0
    year := match[3] + 0
    if (day < 1 || day > 31 || month < 1 || month > 12 || year < 1900 || year > 2200) {
        return ""
    }

    stamp := Format("{:04}{:02}{:02}", year, month, day)
    if !IsValidDateStamp(stamp) {
        return ""
    }

    return Format("{:02}.{:02}.{:04}", day, month, year)
}

ParseEventDateStamp(eventDate) {
    normalized := NormalizeEventDate(eventDate)
    if (normalized = "") {
        return ""
    }

    parts := StrSplit(normalized, ".")
    return parts[3] parts[2] parts[1] "000000"
}

IsValidDateStamp(stamp) {
    try {
        DateDiff(stamp, stamp, "Days")
        return true
    } catch {
        return false
    }
}

NormalizeOdometerText(value) {
    value := Trim(StrReplace(value, " ", ""))
    if (value = "") {
        return ""
    }

    return RegExMatch(value, "^\d+$") ? value : ""
}

NormalizePositiveIntegerText(value) {
    value := Trim(StrReplace(value, " ", ""))
    if (value = "") {
        return ""
    }

    if !RegExMatch(value, "^\d+$") {
        return ""
    }

    return (value + 0) > 0 ? (value + 0) "" : ""
}

FormatHistoryOdometer(value) {
    value := Trim(value)
    if (value = "") {
        return ""
    }

    return value " km"
}

SortVehicleHistoryByDateDescending(&items) {
    count := items.Length
    if (count < 2) {
        return
    }

    Loop count - 1 {
        swapped := false
        Loop count - A_Index {
            left := items[A_Index]
            right := items[A_Index + 1]
            leftStamp := ParseEventDateStamp(left.eventDate)
            rightStamp := ParseEventDateStamp(right.eventDate)
            if (leftStamp < rightStamp) {
                items[A_Index] := right
                items[A_Index + 1] := left
                swapped := true
            }
        }
        if !swapped {
            break
        }
    }
}

FormatDisplayValue(value, emptyText := "Nevyplněno") {
    value := Trim(value)
    return (value = "") ? emptyText : value
}

BuildGreenCardRangeText(vehicle) {
    if (vehicle.greenCardFrom = "" && vehicle.greenCardTo = "") {
        return "Nevyplněno"
    }

    return FormatDisplayValue(vehicle.greenCardFrom, "od nevyplněno") " až " FormatDisplayValue(vehicle.greenCardTo, "do nevyplněno")
}

BuildVehicleDetailStatusText(vehicle) {
    status := GetVehicleStatusText(vehicle)
    return (status = "") ? "V pořádku" : status
}

BuildVehiclesDataContent() {
    global Vehicles

    lines := ["# Vehimap data v4"]
    for vehicle in Vehicles {
        lines.Push(
            EscapeField(vehicle.id) "`t"
            EscapeField(vehicle.name) "`t"
            EscapeField(vehicle.category) "`t"
            EscapeField(vehicle.vehicleNote) "`t"
            EscapeField(vehicle.makeModel) "`t"
            EscapeField(vehicle.plate) "`t"
            EscapeField(vehicle.year) "`t"
            EscapeField(vehicle.power) "`t"
            EscapeField(vehicle.lastTk) "`t"
            EscapeField(vehicle.nextTk) "`t"
            EscapeField(vehicle.greenCardFrom) "`t"
            EscapeField(vehicle.greenCardTo)
        )
    }

    return JoinLines(lines)
}

NormalizeTextForStorage(text) {
    text := StrReplace(text, Chr(0xFEFF))
    text := StrReplace(text, "`r`n", "`n")
    text := StrReplace(text, "`r", "`n")
    return text
}

WriteTextFileUtf8(path, content) {
    if FileExist(path) {
        FileDelete(path)
    }

    FileAppend(content, path, "UTF-8")
}

WriteTextFileUtf8NoBom(path, content) {
    if FileExist(path) {
        FileDelete(path)
    }

    file := FileOpen(path, "w", "UTF-8-RAW")
    file.Write(content)
    file.Close()
}
