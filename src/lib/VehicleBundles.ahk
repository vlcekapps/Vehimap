OpenSelectedVehicleStarterBundle(*) {
    global AppTitle

    vehicle := GetSelectedVehicle()
    if !IsObject(vehicle) {
        MsgBox("Nejprve vyberte vozidlo, pro které chcete připravit balíček.", AppTitle, 0x40)
        return
    }

    OpenVehicleStarterBundleForVehicle(vehicle)
}

OpenVehicleStarterBundleForVehicle(vehicle) {
    global AppTitle

    preview := GetVehicleStarterBundlePreview(vehicle.id)
    if !preview.HasOwnProp("vehicle") || !IsObject(preview.vehicle) {
        return false
    }

    if (preview.totalMissingCount = 0) {
        AppMsgBox(
            "Vehimap pro vozidlo " vehicle.name " už nenašel žádné další doporučené položky balíčku.",
            AppTitle,
            0x40
        )
        return false
    }

    OpenVehicleStarterBundleDialog(preview)
    return true
}

OfferVehicleStarterBundleAfterCreate(vehicleId) {
    global AppTitle

    preview := GetVehicleStarterBundlePreview(vehicleId)
    if !preview.HasOwnProp("vehicle") || !IsObject(preview.vehicle) || preview.totalMissingCount = 0 {
        return false
    }

    text := "Vehimap pro nové vozidlo " preview.vehicle.name " našel balíček doporučených položek.`n`n"
        . "Servisní plány: " preview.maintenanceMissing.Length ".`n"
        . "Doklady: " preview.recordMissing.Length ".`n"
        . "Připomínky: " preview.reminderMissing.Length ".`n`n"
        . "Chcete je otevřít a případně rovnou přidat?"
    result := AppMsgBox(text, AppTitle, 0x24)
    if (result != "Yes") {
        return false
    }

    hooks := GetVehimapTestHooks()
    if IsObject(hooks) {
        hooks.postCreateVehicleBundle := {
            vehicleId: vehicleId,
            profileLabel: preview.profileLabel,
            maintenanceCount: preview.maintenanceMissing.Length,
            recordCount: preview.recordMissing.Length,
            reminderCount: preview.reminderMissing.Length
        }
        if (hooks.HasOwnProp("skipPostCreateVehicleBundleOpen") && hooks.skipPostCreateVehicleBundleOpen) {
            return true
        }
    }

    OpenVehicleStarterBundleDialog(preview)
    return true
}

OpenVehicleStarterBundleDialog(preview) {
    global AppTitle, MainGui, DetailGui, DetailVehicleId
    global VehicleBundleGui, VehicleBundleList, VehicleBundleSummaryLabel, VehicleBundleControls, VehicleBundleItems, VehicleBundleVehicleId, VehicleBundleSelectedIndex, VehicleBundleLoading, VehicleBundleParentGui
    global RecordTypeOptions, ReminderRepeatOptions

    if IsObject(VehicleBundleGui) {
        WinActivate("ahk_id " VehicleBundleGui.Hwnd)
        return
    }

    hooks := GetVehimapTestHooks()
    if IsObject(hooks) && hooks.HasOwnProp("vehicleStarterBundleSelection") {
        RunVehicleStarterBundleSelectionInTestMode(preview, hooks.vehicleStarterBundleSelection)
        return
    }

    ownerGui := 0
    if IsObject(DetailGui) && DetailVehicleId = preview.vehicle.id {
        ownerGui := DetailGui
    } else if IsObject(MainGui) {
        ownerGui := MainGui
    }
    if !IsObject(ownerGui) {
        return
    }

    VehicleBundleItems := BuildVehicleStarterBundleDrafts(preview)
    VehicleBundleVehicleId := preview.vehicle.id
    VehicleBundleSelectedIndex := 0
    VehicleBundleLoading := false
    VehicleBundleControls := {}
    VehicleBundleParentGui := ownerGui

    VehicleBundleGui := Gui("+Owner" ownerGui.Hwnd, AppTitle " - Balíček pro vozidlo")
    VehicleBundleGui.SetFont("s10", "Segoe UI")
    VehicleBundleGui.OnEvent("Close", CloseVehicleStarterBundleDialog)
    VehicleBundleGui.OnEvent("Escape", CloseVehicleStarterBundleDialog)

    ownerGui.Opt("+Disabled")

    VehicleBundleGui.AddText("x20 y18 w960", "Vyberte doporučené servisní plány, placeholdery dokladů a připomínky pro vozidlo " preview.vehicle.name ". Vybranou položku můžete před přidáním upravit.")
    VehicleBundleGui.AddText("x20 y44 w960", "Profil doporučení: " preview.profileLabel)
    VehicleBundleSummaryLabel := VehicleBundleGui.AddText("x20 y70 w960", "")

    VehicleBundleGui.AddText("x20 y104 w360", "Doporučené položky balíčku")
    VehicleBundleList := VehicleBundleGui.AddListView("x20 y128 w390 h330 Checked Grid -Multi", ["Sekce", "Položka", "Detail"])
    VehicleBundleList.OnEvent("ItemSelect", OnVehicleStarterBundleSelectionChanged)
    VehicleBundleList.OnEvent("Click", OnVehicleStarterBundleListClicked)
    VehicleBundleList.ModifyCol(1, "85")
    VehicleBundleList.ModifyCol(2, "165")
    VehicleBundleList.ModifyCol(3, "120")

    selectAllButton := VehicleBundleGui.AddButton("x20 y468 w110 h28", "Vybrat vše")
    selectAllButton.OnEvent("Click", SelectAllVehicleStarterBundleItems)

    clearAllButton := VehicleBundleGui.AddButton("x140 y468 w130 h28", "Vymazat výběr")
    clearAllButton.OnEvent("Click", ClearAllVehicleStarterBundleSelections)

    VehicleBundleGui.AddText("x430 y104 w550", "Úprava vybrané položky")
    VehicleBundleGui.AddText("x430 y128 w550", "Pole se přizpůsobují typu vybrané položky. Změny se uloží jen do právě přidávaného balíčku.")

    VehicleBundleGui.AddGroupBox("x430 y155 w550 h120", "Servisní plán")
    VehicleBundleGui.AddText("x450 y182 w155", "Název úkonu")
    VehicleBundleControls.maintenanceTitle := VehicleBundleGui.AddEdit("x610 y179 w340")
    VehicleBundleControls.maintenanceTitle.OnEvent("Change", OnVehicleStarterBundleDraftFieldChanged)
    VehicleBundleGui.AddText("x450 y217 w155", "Interval kilometrů")
    VehicleBundleControls.maintenanceIntervalKm := VehicleBundleGui.AddEdit("x610 y214 w140")
    VehicleBundleControls.maintenanceIntervalKm.OnEvent("Change", OnVehicleStarterBundleDraftFieldChanged)
    VehicleBundleGui.AddText("x760 y217 w120", "Interval měsíců")
    VehicleBundleControls.maintenanceIntervalMonths := VehicleBundleGui.AddEdit("x880 y214 w70")
    VehicleBundleControls.maintenanceIntervalMonths.OnEvent("Change", OnVehicleStarterBundleDraftFieldChanged)

    VehicleBundleGui.AddGroupBox("x430 y285 w550 h155", "Doklad")
    VehicleBundleGui.AddText("x450 y312 w155", "Druh záznamu")
    VehicleBundleControls.recordType := VehicleBundleGui.AddDropDownList("x610 y309 w170", RecordTypeOptions)
    VehicleBundleControls.recordType.OnEvent("Change", OnVehicleStarterBundleDraftFieldChanged)
    VehicleBundleGui.AddText("x790 y312 w60", "Název")
    VehicleBundleControls.recordTitle := VehicleBundleGui.AddEdit("x850 y309 w100")
    VehicleBundleControls.recordTitle.OnEvent("Change", OnVehicleStarterBundleDraftFieldChanged)
    VehicleBundleGui.AddText("x450 y347 w155", "Poskytovatel")
    VehicleBundleControls.recordProvider := VehicleBundleGui.AddEdit("x610 y344 w340")
    VehicleBundleControls.recordProvider.OnEvent("Change", OnVehicleStarterBundleDraftFieldChanged)
    VehicleBundleGui.AddText("x450 y382 w155", "Platné od")
    VehicleBundleControls.recordValidFrom := VehicleBundleGui.AddEdit("x610 y379 w140")
    VehicleBundleControls.recordValidFrom.OnEvent("Change", OnVehicleStarterBundleDraftFieldChanged)
    VehicleBundleGui.AddText("x760 y382 w120", "Platné do")
    VehicleBundleControls.recordValidTo := VehicleBundleGui.AddEdit("x880 y379 w70")
    VehicleBundleControls.recordValidTo.OnEvent("Change", OnVehicleStarterBundleDraftFieldChanged)
    VehicleBundleGui.AddText("x450 y417 w155", "Cena")
    VehicleBundleControls.recordPrice := VehicleBundleGui.AddEdit("x610 y414 w140")
    VehicleBundleControls.recordPrice.OnEvent("Change", OnVehicleStarterBundleDraftFieldChanged)

    VehicleBundleGui.AddGroupBox("x430 y450 w550 h155", "Připomínka")
    VehicleBundleGui.AddText("x450 y477 w155", "Název připomínky")
    VehicleBundleControls.reminderTitle := VehicleBundleGui.AddEdit("x610 y474 w340")
    VehicleBundleControls.reminderTitle.OnEvent("Change", OnVehicleStarterBundleDraftFieldChanged)
    VehicleBundleGui.AddText("x450 y512 w155", "Termín")
    VehicleBundleControls.reminderDueDate := VehicleBundleGui.AddEdit("x610 y509 w140")
    VehicleBundleControls.reminderDueDate.OnEvent("Change", OnVehicleStarterBundleDraftFieldChanged)
    VehicleBundleGui.AddText("x760 y512 w120", "Dnů předem")
    VehicleBundleControls.reminderDays := VehicleBundleGui.AddEdit("x880 y509 w70")
    VehicleBundleControls.reminderDays.OnEvent("Change", OnVehicleStarterBundleDraftFieldChanged)
    VehicleBundleGui.AddText("x450 y547 w155", "Opakování")
    VehicleBundleControls.reminderRepeatMode := VehicleBundleGui.AddDropDownList("x610 y544 w170", ReminderRepeatOptions)
    VehicleBundleControls.reminderRepeatMode.OnEvent("Change", OnVehicleStarterBundleDraftFieldChanged)

    VehicleBundleGui.AddText("x430 y620 w120", "Poznámka")
    VehicleBundleControls.note := VehicleBundleGui.AddEdit("x430 y645 w550 h80 Multi")
    VehicleBundleControls.note.OnEvent("Change", OnVehicleStarterBundleDraftFieldChanged)

    saveButton := VehicleBundleGui.AddButton("x640 y740 w170 h30 Default", "Přidat vybrané")
    saveButton.OnEvent("Click", SaveSelectedVehicleStarterBundleFromDialog)

    cancelButton := VehicleBundleGui.AddButton("x820 y740 w120 h30", "Zrušit")
    cancelButton.OnEvent("Click", CloseVehicleStarterBundleDialog)

    VehicleBundleGui.Show("w1000 h790")
    PopulateVehicleStarterBundleList(1)
}

CloseVehicleStarterBundleDialog(*) {
    global VehicleBundleGui, VehicleBundleList, VehicleBundleSummaryLabel, VehicleBundleControls, VehicleBundleItems, VehicleBundleVehicleId, VehicleBundleSelectedIndex, VehicleBundleLoading, VehicleBundleParentGui

    if IsObject(VehicleBundleGui) {
        VehicleBundleGui.Destroy()
        VehicleBundleGui := 0
    }

    VehicleBundleList := 0
    VehicleBundleSummaryLabel := 0
    VehicleBundleControls := {}
    VehicleBundleItems := []
    VehicleBundleVehicleId := ""
    VehicleBundleSelectedIndex := 0
    VehicleBundleLoading := false

    if IsObject(VehicleBundleParentGui) {
        VehicleBundleParentGui.Opt("-Disabled")
        WinActivate("ahk_id " VehicleBundleParentGui.Hwnd)
        VehicleBundleParentGui := 0
    }
}

PopulateVehicleStarterBundleList(selectIndex := 1) {
    global VehicleBundleGui, VehicleBundleList, VehicleBundleItems

    if !IsObject(VehicleBundleGui) || !IsObject(VehicleBundleList) {
        return
    }

    VehicleBundleList.Opt("-Redraw")
    VehicleBundleList.Delete()
    for index, item in VehicleBundleItems {
        row := VehicleBundleList.Add("", item.sectionLabel, item.title, BuildVehicleStarterBundleItemDetailText(item))
        VehicleBundleList.Modify(row, item.selected ? "Check" : "-Check")
    }
    VehicleBundleList.Opt("+Redraw")

    UpdateVehicleStarterBundleSummary()
    SelectVehicleStarterBundleRow(selectIndex, true)
}

SelectVehicleStarterBundleRow(index, focusList := false) {
    global VehicleBundleList, VehicleBundleItems, VehicleBundleSelectedIndex

    if !IsObject(VehicleBundleList) || index < 1 || index > VehicleBundleItems.Length {
        VehicleBundleSelectedIndex := 0
        LoadVehicleStarterBundleDraftIntoControls(0)
        return
    }

    VehicleBundleSelectedIndex := index
    VehicleBundleList.Modify(index, focusList ? "Select Focus Vis" : "Select Vis")
    LoadVehicleStarterBundleDraftIntoControls(index)
}

OnVehicleStarterBundleSelectionChanged(*) {
    global VehicleBundleList

    if !IsObject(VehicleBundleList) {
        return
    }

    row := VehicleBundleList.GetNext(0)
    SelectVehicleStarterBundleRow(row)
}

OnVehicleStarterBundleListClicked(*) {
    SyncVehicleStarterBundleSelectionsFromList()
    UpdateVehicleStarterBundleSummary()
}

LoadVehicleStarterBundleDraftIntoControls(index) {
    global VehicleBundleControls, VehicleBundleItems, VehicleBundleLoading, RecordTypeOptions, ReminderRepeatOptions

    if !IsObject(VehicleBundleControls) || !VehicleBundleControls.HasOwnProp("note") {
        return
    }

    VehicleBundleLoading := true
    if (index < 1 || index > VehicleBundleItems.Length) {
        ClearVehicleStarterBundleControls()
        SetVehicleStarterBundleControlState("")
        VehicleBundleLoading := false
        return
    }

    item := VehicleBundleItems[index]
    ClearVehicleStarterBundleControls()
    SetVehicleStarterBundleControlState(item.section)

    switch item.section {
        case "maintenance":
            VehicleBundleControls.maintenanceTitle.Text := item.title
            VehicleBundleControls.maintenanceIntervalKm.Text := item.intervalKm
            VehicleBundleControls.maintenanceIntervalMonths.Text := item.intervalMonths
        case "record":
            SetDropDownToText(VehicleBundleControls.recordType, item.recordType, RecordTypeOptions)
            VehicleBundleControls.recordTitle.Text := item.title
            VehicleBundleControls.recordProvider.Text := item.provider
            VehicleBundleControls.recordValidFrom.Text := item.validFrom
            VehicleBundleControls.recordValidTo.Text := item.validTo
            VehicleBundleControls.recordPrice.Text := item.price
        case "reminder":
            VehicleBundleControls.reminderTitle.Text := item.title
            VehicleBundleControls.reminderDueDate.Text := item.dueDate
            VehicleBundleControls.reminderDays.Text := item.reminderDays
            SetDropDownToText(VehicleBundleControls.reminderRepeatMode, item.repeatMode, ReminderRepeatOptions)
    }

    VehicleBundleControls.note.Text := item.note
    VehicleBundleLoading := false
}

ClearVehicleStarterBundleControls() {
    global VehicleBundleControls

    VehicleBundleControls.maintenanceTitle.Text := ""
    VehicleBundleControls.maintenanceIntervalKm.Text := ""
    VehicleBundleControls.maintenanceIntervalMonths.Text := ""
    VehicleBundleControls.recordType.Choose(0)
    VehicleBundleControls.recordTitle.Text := ""
    VehicleBundleControls.recordProvider.Text := ""
    VehicleBundleControls.recordValidFrom.Text := ""
    VehicleBundleControls.recordValidTo.Text := ""
    VehicleBundleControls.recordPrice.Text := ""
    VehicleBundleControls.reminderTitle.Text := ""
    VehicleBundleControls.reminderDueDate.Text := ""
    VehicleBundleControls.reminderDays.Text := ""
    VehicleBundleControls.reminderRepeatMode.Choose(0)
    VehicleBundleControls.note.Text := ""
}

SetVehicleStarterBundleControlState(section) {
    maintenanceEnabled := (section = "maintenance")
    recordEnabled := (section = "record")
    reminderEnabled := (section = "reminder")

    SetVehicleStarterBundleControlEnabled("maintenanceTitle", maintenanceEnabled)
    SetVehicleStarterBundleControlEnabled("maintenanceIntervalKm", maintenanceEnabled)
    SetVehicleStarterBundleControlEnabled("maintenanceIntervalMonths", maintenanceEnabled)
    SetVehicleStarterBundleControlEnabled("recordType", recordEnabled)
    SetVehicleStarterBundleControlEnabled("recordTitle", recordEnabled)
    SetVehicleStarterBundleControlEnabled("recordProvider", recordEnabled)
    SetVehicleStarterBundleControlEnabled("recordValidFrom", recordEnabled)
    SetVehicleStarterBundleControlEnabled("recordValidTo", recordEnabled)
    SetVehicleStarterBundleControlEnabled("recordPrice", recordEnabled)
    SetVehicleStarterBundleControlEnabled("reminderTitle", reminderEnabled)
    SetVehicleStarterBundleControlEnabled("reminderDueDate", reminderEnabled)
    SetVehicleStarterBundleControlEnabled("reminderDays", reminderEnabled)
    SetVehicleStarterBundleControlEnabled("reminderRepeatMode", reminderEnabled)
    SetVehicleStarterBundleControlEnabled("note", section != "")
}

SetVehicleStarterBundleControlEnabled(controlName, enabled) {
    global VehicleBundleControls

    if !VehicleBundleControls.HasOwnProp(controlName) {
        return
    }

    VehicleBundleControls.%controlName%.Opt(enabled ? "-Disabled" : "+Disabled")
}

OnVehicleStarterBundleDraftFieldChanged(*) {
    global VehicleBundleControls, VehicleBundleItems, VehicleBundleSelectedIndex, VehicleBundleLoading, VehicleBundleList

    if VehicleBundleLoading || !IsObject(VehicleBundleControls) {
        return
    }

    index := VehicleBundleSelectedIndex
    if (index < 1 || index > VehicleBundleItems.Length) {
        return
    }

    item := VehicleBundleItems[index]
    switch item.section {
        case "maintenance":
            item.title := VehicleBundleControls.maintenanceTitle.Text
            item.intervalKm := VehicleBundleControls.maintenanceIntervalKm.Text
            item.intervalMonths := VehicleBundleControls.maintenanceIntervalMonths.Text
        case "record":
            item.recordType := VehicleBundleControls.recordType.Text
            item.title := VehicleBundleControls.recordTitle.Text
            item.provider := VehicleBundleControls.recordProvider.Text
            item.validFrom := VehicleBundleControls.recordValidFrom.Text
            item.validTo := VehicleBundleControls.recordValidTo.Text
            item.price := VehicleBundleControls.recordPrice.Text
        case "reminder":
            item.title := VehicleBundleControls.reminderTitle.Text
            item.dueDate := VehicleBundleControls.reminderDueDate.Text
            item.reminderDays := VehicleBundleControls.reminderDays.Text
            item.repeatMode := VehicleBundleControls.reminderRepeatMode.Text
    }

    item.note := VehicleBundleControls.note.Text

    if IsObject(VehicleBundleList) {
        VehicleBundleList.Modify(index, "", item.sectionLabel, item.title, BuildVehicleStarterBundleItemDetailText(item))
    }
}

SyncVehicleStarterBundleSelectionsFromList() {
    global VehicleBundleList, VehicleBundleItems

    if !IsObject(VehicleBundleList) {
        return
    }

    checkedRows := Map()
    row := 0
    while row := VehicleBundleList.GetNext(row, "Checked") {
        checkedRows[row] := true
    }

    for index, item in VehicleBundleItems {
        item.selected := checkedRows.Has(index)
    }
}

UpdateVehicleStarterBundleSummary() {
    global VehicleBundleSummaryLabel, VehicleBundleItems

    if !IsObject(VehicleBundleSummaryLabel) {
        return
    }

    selectedCount := 0
    maintenanceCount := 0
    recordCount := 0
    reminderCount := 0

    for item in VehicleBundleItems {
        if !item.selected {
            continue
        }
        selectedCount += 1
        switch item.section {
            case "maintenance":
                maintenanceCount += 1
            case "record":
                recordCount += 1
            case "reminder":
                reminderCount += 1
        }
    }

    VehicleBundleSummaryLabel.Text := "V balíčku je " VehicleBundleItems.Length " doporučených položek. Vybráno k přidání: " selectedCount ". Servis: " maintenanceCount ", doklady: " recordCount ", připomínky: " reminderCount "."
}

SelectAllVehicleStarterBundleItems(*) {
    SetAllVehicleStarterBundleSelections(true)
}

ClearAllVehicleStarterBundleSelections(*) {
    SetAllVehicleStarterBundleSelections(false)
}

SetAllVehicleStarterBundleSelections(selected) {
    global VehicleBundleList, VehicleBundleItems

    if !IsObject(VehicleBundleList) {
        return
    }

    for index, item in VehicleBundleItems {
        item.selected := selected ? 1 : 0
        VehicleBundleList.Modify(index, selected ? "Check" : "-Check")
    }

    UpdateVehicleStarterBundleSummary()
}

SaveSelectedVehicleStarterBundleFromDialog(*) {
    global AppTitle, VehicleBundleVehicleId, VehicleBundleItems

    result := ApplyVehicleStarterBundleDrafts(VehicleBundleVehicleId, VehicleBundleItems)
    if result.HasOwnProp("errorMessage") {
        AppMsgBox(result.errorMessage, AppTitle, 0x30)
        if (result.HasOwnProp("invalidIndex")) {
            SelectVehicleStarterBundleRow(result.invalidIndex, false)
        }
        if (result.HasOwnProp("focusField")) {
            FocusVehicleStarterBundleField(result.focusField)
        }
        return
    }

    CloseVehicleStarterBundleDialog()
    RefreshVehicleStarterBundleDependentState(result.vehicle.id)
    AppMsgBox(BuildVehicleStarterBundleResultText(result), AppTitle, 0x40)
}

FocusVehicleStarterBundleField(fieldName) {
    global VehicleBundleControls

    if IsObject(VehicleBundleControls) && VehicleBundleControls.HasOwnProp(fieldName) {
        VehicleBundleControls.%fieldName%.Focus()
    }
}

BuildVehicleStarterBundleDrafts(preview) {
    drafts := []

    for template in preview.maintenanceMissing {
        drafts.Push({
            section: "maintenance",
            sectionLabel: "Servis",
            title: template.title,
            intervalKm: template.intervalKm,
            intervalMonths: template.intervalMonths,
            note: template.note,
            selected: 1
        })
    }

    for template in preview.recordMissing {
        drafts.Push({
            section: "record",
            sectionLabel: "Doklad",
            recordType: template.recordType,
            title: template.title,
            provider: template.provider,
            validFrom: template.validFrom,
            validTo: template.validTo,
            price: template.price,
            note: template.note,
            selected: 1
        })
    }

    for template in preview.reminderMissing {
        drafts.Push({
            section: "reminder",
            sectionLabel: "Připomínka",
            title: template.title,
            dueDate: template.dueDate,
            reminderDays: template.reminderDays,
            repeatMode: template.repeatMode,
            note: template.note,
            selected: 1
        })
    }

    return drafts
}

ApplyVehicleStarterBundleDrafts(vehicleId, drafts, persist := true) {
    maintenanceDrafts := []
    recordDrafts := []
    reminderDrafts := []
    selectedCount := 0

    for index, draft in drafts {
        if !draft.selected {
            continue
        }

        selectedCount += 1
        normalizedDraft := ""
        errorMessage := ""
        focusField := ""

        switch draft.section {
            case "maintenance":
                if !TryNormalizeVehicleMaintenanceRecommendationDraft(draft, &normalizedDraft, &errorMessage, &focusField) {
                    mappedFocusField := (focusField = "title") ? "maintenanceTitle" : ((focusField = "intervalKm") ? "maintenanceIntervalKm" : "maintenanceIntervalMonths")
                    return {errorMessage: errorMessage, focusField: mappedFocusField, invalidIndex: index}
                }
                maintenanceDrafts.Push(normalizedDraft)
            case "record":
                if !TryNormalizeVehicleStarterBundleRecordDraft(draft, &normalizedDraft, &errorMessage, &focusField) {
                    return {errorMessage: errorMessage, focusField: focusField, invalidIndex: index}
                }
                recordDrafts.Push(normalizedDraft)
            case "reminder":
                if !TryNormalizeVehicleStarterBundleReminderDraft(draft, &normalizedDraft, &errorMessage, &focusField) {
                    return {errorMessage: errorMessage, focusField: focusField, invalidIndex: index}
                }
                reminderDrafts.Push(normalizedDraft)
        }
    }

    if (selectedCount = 0) {
        firstDraft := drafts.Length > 0 ? drafts[1] : ""
        focusField := "maintenanceTitle"
        if IsObject(firstDraft) {
            switch firstDraft.section {
                case "record":
                    focusField := "recordTitle"
                case "reminder":
                    focusField := "reminderTitle"
            }
        }
        return {
            errorMessage: "Vyberte alespoň jednu položku balíčku, kterou chcete přidat.",
            focusField: focusField,
            invalidIndex: 1
        }
    }

    maintenanceResult := AddVehicleMaintenanceRecommendedTemplates(vehicleId, maintenanceDrafts, false)
    recordResult := AddVehicleStarterBundleRecords(vehicleId, recordDrafts, false)
    reminderResult := AddVehicleStarterBundleReminders(vehicleId, reminderDrafts, false)

    if persist {
        if (maintenanceResult.addedPlans.Length > 0) {
            SaveVehicleMaintenancePlans()
        }
        if (recordResult.addedRecords.Length > 0) {
            SaveVehicleRecords()
        }
        if (reminderResult.addedReminders.Length > 0) {
            SaveVehicleReminders()
        }
    }

    return {
        vehicle: maintenanceResult.vehicle,
        selectedCount: selectedCount,
        maintenanceAdded: maintenanceResult.addedPlans.Length,
        maintenanceSkipped: maintenanceResult.skippedCount,
        recordAdded: recordResult.addedRecords.Length,
        recordSkipped: recordResult.skippedCount,
        reminderAdded: reminderResult.addedReminders.Length,
        reminderSkipped: reminderResult.skippedCount,
        addedMaintenancePlans: maintenanceResult.addedPlans,
        addedRecords: recordResult.addedRecords,
        addedReminders: reminderResult.addedReminders
    }
}

TryNormalizeVehicleStarterBundleRecordDraft(draft, &normalizedDraft, &errorMessage, &focusField) {
    recordType := Trim(draft.recordType)
    title := Trim(draft.title)
    provider := Trim(draft.provider)
    validFrom := NormalizeMonthYear(draft.validFrom)
    validTo := NormalizeMonthYear(draft.validTo)
    price := Trim(draft.price)
    note := Trim(draft.note)

    if (recordType = "") {
        errorMessage := "Každý vybraný doklad musí mít vyplněný druh záznamu."
        focusField := "recordType"
        return false
    }
    if (title = "") {
        errorMessage := "Každý vybraný doklad musí mít vyplněný název."
        focusField := "recordTitle"
        return false
    }
    if (Trim(draft.validFrom) != "" && validFrom = "") {
        errorMessage := "Pole Platné od musí být ve formátu MM/RRRR."
        focusField := "recordValidFrom"
        return false
    }
    if (Trim(draft.validTo) != "" && validTo = "") {
        errorMessage := "Pole Platné do musí být ve formátu MM/RRRR."
        focusField := "recordValidTo"
        return false
    }
    if (validFrom != "" && validTo != "" && ParseDueStamp(validFrom) > ParseDueStamp(validTo)) {
        errorMessage := "U dokladu nesmí být pole Platné od později než pole Platné do."
        focusField := "recordValidFrom"
        return false
    }
    if (price != "" && !TryParseMoneyAmount(price, &parsedPrice)) {
        errorMessage := "Pole Cena musí být prázdné nebo v číselném formátu."
        focusField := "recordPrice"
        return false
    }

    normalizedDraft := {
        recordType: recordType,
        title: title,
        provider: provider,
        validFrom: validFrom,
        validTo: validTo,
        price: price,
        note: note
    }
    return true
}

TryNormalizeVehicleStarterBundleReminderDraft(draft, &normalizedDraft, &errorMessage, &focusField) {
    title := Trim(draft.title)
    dueDate := NormalizeEventDate(draft.dueDate)
    reminderDaysText := Trim(draft.reminderDays)
    repeatMode := NormalizeReminderRepeat(draft.repeatMode)
    note := Trim(draft.note)

    if (title = "") {
        errorMessage := "Každá vybraná připomínka musí mít vyplněný název."
        focusField := "reminderTitle"
        return false
    }
    if (dueDate = "") {
        errorMessage := "Pole Termín připomínky je povinné a musí být ve formátu DD.MM.RRRR."
        focusField := "reminderDueDate"
        return false
    }
    if !RegExMatch(reminderDaysText, "^\d{1,3}$") {
        errorMessage := "Pole Dnů předem musí být celé číslo od 0 do 999."
        focusField := "reminderDays"
        return false
    }

    normalizedDraft := {
        title: title,
        dueDate: dueDate,
        reminderDays: reminderDaysText,
        repeatMode: repeatMode,
        note: note
    }
    return true
}

AddVehicleStarterBundleRecords(vehicleId, templates, persist := true) {
    global VehicleRecords

    vehicle := FindVehicleById(vehicleId)
    addedRecords := []
    existingKeys := BuildVehicleStarterBundleExistingRecordKeyMap(vehicleId)
    skippedCount := 0

    for template in templates {
        key := BuildVehicleStarterBundleRecordKey(template.recordType, template.title)
        if (key != "" && existingKeys.Has(key)) {
            skippedCount += 1
            continue
        }

        entry := {
            id: GenerateVehicleRecordId(),
            vehicleId: vehicleId,
            recordType: template.recordType,
            title: template.title,
            provider: template.provider,
            validFrom: template.validFrom,
            validTo: template.validTo,
            price: template.price,
            attachmentMode: "managed",
            filePath: "",
            note: template.note
        }
        VehicleRecords.Push(entry)
        addedRecords.Push(entry)
        if (key != "") {
            existingKeys[key] := true
        }
    }

    if persist && addedRecords.Length > 0 {
        SaveVehicleRecords()
    }

    return {vehicle: vehicle, addedRecords: addedRecords, skippedCount: skippedCount}
}

AddVehicleStarterBundleReminders(vehicleId, templates, persist := true) {
    global VehicleReminders

    vehicle := FindVehicleById(vehicleId)
    addedReminders := []
    existingKeys := BuildVehicleStarterBundleExistingReminderKeyMap(vehicleId)
    skippedCount := 0

    for template in templates {
        key := BuildVehicleStarterBundleReminderKey(template.title, template.repeatMode)
        if (key != "" && existingKeys.Has(key)) {
            skippedCount += 1
            continue
        }

        entry := {
            id: GenerateVehicleReminderId(),
            vehicleId: vehicleId,
            title: template.title,
            dueDate: template.dueDate,
            reminderDays: template.reminderDays,
            repeatMode: template.repeatMode,
            note: template.note
        }
        VehicleReminders.Push(entry)
        addedReminders.Push(entry)
        if (key != "") {
            existingKeys[key] := true
        }
    }

    if persist && addedReminders.Length > 0 {
        SaveVehicleReminders()
    }

    return {vehicle: vehicle, addedReminders: addedReminders, skippedCount: skippedCount}
}

BuildVehicleStarterBundleResultText(result) {
    return "Balíček pro vozidlo " result.vehicle.name " byl zpracován.`n`n"
        . "Přidáno servisních plánů: " result.maintenanceAdded ". Přeskočeno: " result.maintenanceSkipped ".`n"
        . "Přidáno dokladů: " result.recordAdded ". Přeskočeno: " result.recordSkipped ".`n"
        . "Přidáno připomínek: " result.reminderAdded ". Přeskočeno: " result.reminderSkipped "."
}

BuildVehicleStarterBundleItemDetailText(item) {
    switch item.section {
        case "maintenance":
            return BuildVehicleMaintenanceIntervalText(item)
        case "record":
            validity := ""
            if (Trim(item.validFrom) != "" || Trim(item.validTo) != "") {
                validity := Trim(item.validFrom) != "" ? item.validFrom : "?"
                validity .= " - "
                validity .= Trim(item.validTo) != "" ? item.validTo : "?"
            }
            baseText := Trim(item.recordType)
            if (validity != "") {
                baseText .= " / " validity
            }
            return baseText
        case "reminder":
            return item.dueDate " / " item.repeatMode
        default:
            return ""
    }
}

RefreshVehicleStarterBundleDependentState(vehicleId) {
    global DetailGui, DetailVehicleId, DetailHistorySummaryLabel, DetailReminderSummaryLabel, DetailFuelSummaryLabel, DetailRecordsSummaryLabel, DetailMaintenanceSummaryLabel
    global RecordsGui, RecordsVehicleId, ReminderGui, ReminderVehicleId, MaintenanceGui, MaintenanceVehicleId, GlobalSearchGui

    if IsObject(RecordsGui) && RecordsVehicleId = vehicleId {
        PopulateVehicleRecordsList()
    }
    if IsObject(ReminderGui) && ReminderVehicleId = vehicleId {
        PopulateVehicleReminderList()
    }
    if IsObject(MaintenanceGui) && MaintenanceVehicleId = vehicleId {
        PopulateVehicleMaintenanceList()
    }
    if IsObject(DetailGui) && DetailVehicleId = vehicleId {
        if IsObject(DetailHistorySummaryLabel) {
            DetailHistorySummaryLabel.Text := BuildVehicleHistorySummaryText(vehicleId)
        }
        if IsObject(DetailReminderSummaryLabel) {
            DetailReminderSummaryLabel.Text := BuildVehicleReminderSummaryText(vehicleId)
        }
        if IsObject(DetailFuelSummaryLabel) {
            DetailFuelSummaryLabel.Text := BuildVehicleFuelSummaryText(vehicleId)
        }
        if IsObject(DetailRecordsSummaryLabel) {
            DetailRecordsSummaryLabel.Text := BuildVehicleRecordsSummaryText(vehicleId)
        }
        if IsObject(DetailMaintenanceSummaryLabel) {
            DetailMaintenanceSummaryLabel.Text := BuildVehicleMaintenanceSummaryText(vehicleId)
        }
        PopulateVehicleDetailHistoryList(vehicleId)
    }
    if IsObject(GlobalSearchGui) {
        PopulateGlobalSearchList()
    }

    RefreshVehicleList(vehicleId)
    CheckDueVehicles(false, false)
    UpdateTrayIconTip(true)
}

RunVehicleStarterBundleSelectionInTestMode(preview, selection) {
    global AppTitle

    drafts := BuildVehicleStarterBundleDrafts(preview)
    RegisterVehicleStarterBundlePreviewInHooks(preview, drafts)
    ApplyVehicleStarterBundleSelectionHook(&drafts, selection)

    if IsObject(selection) && selection.HasOwnProp("cancel") && selection.cancel {
        return
    }

    result := ApplyVehicleStarterBundleDrafts(preview.vehicle.id, drafts)
    if result.HasOwnProp("errorMessage") {
        AppMsgBox(result.errorMessage, AppTitle, 0x30)
        return
    }

    RefreshVehicleStarterBundleDependentState(preview.vehicle.id)
    AppMsgBox(BuildVehicleStarterBundleResultText(result), AppTitle, 0x40)
}

RegisterVehicleStarterBundlePreviewInHooks(preview, drafts) {
    hooks := GetVehimapTestHooks()
    if !IsObject(hooks) {
        return
    }

    opened := {
        vehicleId: preview.vehicle.id,
        profileLabel: preview.profileLabel,
        maintenanceTitles: [],
        recordTitles: [],
        reminderTitles: []
    }
    for draft in drafts {
        switch draft.section {
            case "maintenance":
                opened.maintenanceTitles.Push(draft.title)
            case "record":
                opened.recordTitles.Push(draft.title)
            case "reminder":
                opened.reminderTitles.Push(draft.title)
        }
    }
    hooks.vehicleStarterBundleOpened := opened
}

ApplyVehicleStarterBundleSelectionHook(&drafts, selection) {
    if !IsObject(selection) {
        return
    }

    if selection.HasOwnProp("selectedKeys") && IsObject(selection.selectedKeys) {
        wanted := Map()
        for key in selection.selectedKeys {
            wanted[key] := true
        }
        for draft in drafts {
            draft.selected := wanted.Has(BuildVehicleStarterBundleDraftMatchKey(draft.section, draft.title))
        }
    }

    if selection.HasOwnProp("updates") && IsObject(selection.updates) {
        for update in selection.updates {
            matchSection := update.HasOwnProp("section") ? update.section : ""
            matchTitle := update.HasOwnProp("matchTitle") ? update.matchTitle : ""
            matchKey := BuildVehicleStarterBundleDraftMatchKey(matchSection, matchTitle)
            if (matchKey = "") {
                continue
            }

            for draft in drafts {
                if (BuildVehicleStarterBundleDraftMatchKey(draft.section, draft.title) != matchKey) {
                    continue
                }

                for fieldName, value in update.OwnProps() {
                    if (fieldName = "section" || fieldName = "matchTitle") {
                        continue
                    }
                    draft.%fieldName% := value
                }
                break
            }
        }
    }
}

BuildVehicleStarterBundleDraftMatchKey(section, title) {
    normalizedSection := Trim(section)
    normalizedTitle := NormalizeVehicleMaintenancePlanTitleKey(title)
    if (normalizedSection = "" || normalizedTitle = "") {
        return ""
    }
    return normalizedSection "|" normalizedTitle
}

GetVehicleStarterBundlePreview(vehicleId) {
    vehicle := FindVehicleById(vehicleId)
    if !IsObject(vehicle) {
        return {vehicle: "", profileLabel: "", maintenanceMissing: [], recordMissing: [], reminderMissing: [], totalMissingCount: 0}
    }

    maintenancePreview := GetVehicleMaintenanceRecommendationPreview(vehicleId)
    recordMissing := GetMissingVehicleStarterBundleRecordTemplates(vehicle)
    reminderMissing := GetMissingVehicleStarterBundleReminderTemplates(vehicle)

    return {
        vehicle: vehicle,
        profileLabel: maintenancePreview.profileLabel,
        maintenanceMissing: maintenancePreview.missing,
        recordMissing: recordMissing,
        reminderMissing: reminderMissing,
        totalMissingCount: maintenancePreview.missing.Length + recordMissing.Length + reminderMissing.Length
    }
}

GetMissingVehicleStarterBundleRecordTemplates(vehicle) {
    missing := []
    existingKeys := BuildVehicleStarterBundleExistingRecordKeyMap(vehicle.id)

    for template in GetVehicleStarterBundleRecordTemplates(vehicle) {
        key := BuildVehicleStarterBundleRecordKey(template.recordType, template.title)
        if (key != "" && existingKeys.Has(key)) {
            continue
        }
        missing.Push(template)
    }

    return missing
}

GetMissingVehicleStarterBundleReminderTemplates(vehicle) {
    missing := []
    existingKeys := BuildVehicleStarterBundleExistingReminderKeyMap(vehicle.id)

    for template in GetVehicleStarterBundleReminderTemplates(vehicle) {
        key := BuildVehicleStarterBundleReminderKey(template.title, template.repeatMode)
        if (key != "" && existingKeys.Has(key)) {
            continue
        }
        missing.Push(template)
    }

    return missing
}

GetVehicleStarterBundleRecordTemplates(vehicle) {
    if !IsRoadVehicleCategory(vehicle.category) {
        return [{recordType: "Doklad", title: "Obecný doklad", provider: "", validFrom: "", validTo: "", price: "", note: "Doplňte název dokladu, platnost a případnou přílohu."}]
    }

    return [
        {recordType: "Povinné ručení", title: "Povinné ručení", provider: "", validFrom: "", validTo: "", price: "", note: "Doplňte číslo smlouvy, platnost a případnou přílohu."},
        {recordType: "Havarijní pojištění", title: "Havarijní pojištění", provider: "", validFrom: "", validTo: "", price: "", note: "Doplňte rozsah pojištění, platnost a případnou přílohu."},
        {recordType: "Asistence", title: "Asistence", provider: "", validFrom: "", validTo: "", price: "", note: "Doplňte poskytovatele asistence a důležité kontakty."}
    ]
}

GetVehicleStarterBundleReminderTemplates(vehicle) {
    dueDate := FormatTime(DateAdd(A_Now, 30, "Days"), "dd.MM.yyyy")

    switch vehicle.category {
        case "Motocykly":
            return [{title: "Předsezónní kontrola motocyklu", dueDate: dueDate, reminderDays: "14", repeatMode: "Každý rok", note: "Zkontrolujte brzdy, řetěz, pneumatiky, kapaliny a baterii."}]
        case "Nákladní vozidla", "Autobusy":
            return [{title: "Pravidelná provozní kontrola", dueDate: dueDate, reminderDays: "14", repeatMode: "Každý rok", note: "Zkontrolujte kapaliny, osvětlení, pneumatiky a povinnou výbavu."}]
        case "Ostatní":
            return [{title: "Pravidelná kontrola stavu", dueDate: dueDate, reminderDays: "14", repeatMode: "Každý rok", note: "Doplňte vlastní kontrolní kroky podle typu zařízení."}]
        default:
            return [{title: "Pravidelná kontrola stavu vozidla", dueDate: dueDate, reminderDays: "14", repeatMode: "Každý rok", note: "Zkontrolujte výbavu, kapaliny, osvětlení a stav pneumatik."}]
    }
}

BuildVehicleStarterBundleExistingRecordKeyMap(vehicleId) {
    global VehicleRecords

    keys := Map()
    for entry in VehicleRecords {
        if (entry.vehicleId != vehicleId) {
            continue
        }
        key := BuildVehicleStarterBundleRecordKey(entry.recordType, entry.title)
        if (key != "") {
            keys[key] := true
        }
    }
    return keys
}

BuildVehicleStarterBundleExistingReminderKeyMap(vehicleId) {
    global VehicleReminders

    keys := Map()
    for entry in VehicleReminders {
        if (entry.vehicleId != vehicleId) {
            continue
        }
        key := BuildVehicleStarterBundleReminderKey(entry.title, entry.repeatMode)
        if (key != "") {
            keys[key] := true
        }
    }
    return keys
}

BuildVehicleStarterBundleRecordKey(recordType, title) {
    normalizedType := StrLower(Trim(recordType))
    normalizedTitle := NormalizeVehicleMaintenancePlanTitleKey(title)
    if (normalizedType = "" || normalizedTitle = "") {
        return ""
    }
    return normalizedType "|" normalizedTitle
}

BuildVehicleStarterBundleReminderKey(title, repeatMode) {
    normalizedTitle := NormalizeVehicleMaintenancePlanTitleKey(title)
    normalizedRepeat := StrLower(Trim(repeatMode))
    if (normalizedTitle = "") {
        return ""
    }
    return normalizedTitle "|" normalizedRepeat
}

IsRoadVehicleCategory(category) {
    return Trim(category) != "" && category != "Ostatní"
}
