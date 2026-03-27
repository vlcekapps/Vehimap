#Requires AutoHotkey v2.0
#Include "Evicar.ahk"
SetTimer(RunSmoke, -900)
RunSmoke() {
    global MainSearchCtrl, MainStatusFilterCtrl, VisibleVehicleIds, DetailGui, HistoryGui
    lines := []
    lines.Push("initial=" VisibleVehicleIds.Length)
    MainSearchCtrl.Text := "oct"
    RefreshVehicleList()
    lines.Push("search=" VisibleVehicleIds.Length)
    MainSearchCtrl.Text := ""
    MainStatusFilterCtrl.Value := 4
    RefreshVehicleList()
    lines.Push("missing_green=" VisibleVehicleIds.Length)
    SetMainVehicleFilters("", "all")
    RefreshVehicleList()
    vehicle := FindVehicleById("v1")
    OpenVehicleDetailDialog(vehicle)
    Sleep 200
    lines.Push("detail=" (IsObject(DetailGui) ? "open" : "closed"))
    CloseVehicleDetailDialog()
    OpenVehicleHistoryDialog(vehicle)
    Sleep 200
    lines.Push("history=" (IsObject(HistoryGui) ? "open" : "closed"))
    lines.Push("history_count=" GetVehicleHistoryCount(vehicle.id))
    CloseVehicleHistoryDialog()
    lines.Push("history_backup_header=" SubStr(BuildHistoryDataContent(), 1, 18))
    WriteTextFileUtf8(A_ScriptDir "\result.txt", JoinLines(lines))
    ExitApp()
}
