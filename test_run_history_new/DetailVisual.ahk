#Requires AutoHotkey v2.0
#Include "Evicar.ahk"
SetTimer(OpenDetailView, -900)
OpenDetailView() {
    vehicle := FindVehicleById("v1")
    OpenVehicleDetailDialog(vehicle)
}
