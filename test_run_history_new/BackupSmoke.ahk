#Requires AutoHotkey v2.0
#Include "Evicar.ahk"
SetTimer(RunBackupSmoke, -900)
RunBackupSmoke() {
    settingsContent := GetSettingsContentForBackup()
    vehiclesContent := GetVehiclesContentForBackup()
    historyContent := GetHistoryContentForBackup()
    backupContent := BuildBackupContent(settingsContent, vehiclesContent, historyContent)
    parsedSettings := ""
    parsedVehicles := ""
    parsedHistory := ""
    err := ""
    lines := []
    ok := TryParseBackupContent(backupContent, &parsedSettings, &parsedVehicles, &parsedHistory, &err)
    lines.Push("backup_parse=" (ok ? "ok" : err))
    importedHistory := []
    okHistory := TryParseHistoryBackupContent(parsedHistory, &importedHistory, &err)
    lines.Push("history_parse=" (okHistory ? importedHistory.Length : err))
    WriteTextFileUtf8(A_ScriptDir "\backup_result.txt", JoinLines(lines))
    ExitApp()
}
