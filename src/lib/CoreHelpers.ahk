NormalizeMonthYear(value) {
    value := Trim(value)
    if (value = "") {
        return ""
    }

    if !RegExMatch(value, "^\s*(\d{1,2})\s*[/.-]\s*(\d{4})\s*$", &match) {
        return ""
    }

    month := match[1] + 0
    year := match[2] + 0
    if (month < 1 || month > 12 || year < 1900 || year > 2200) {
        return ""
    }

    return Format("{:02}/{:04}", month, year)
}

ParseDueStamp(monthYear) {
    normalized := NormalizeMonthYear(monthYear)
    if (normalized = "") {
        return ""
    }

    parts := StrSplit(normalized, "/")
    month := parts[1] + 0
    year := parts[2] + 0
    day := DaysInMonth(year, month)
    return Format("{:04}{:02}{:02}235959", year, month, day)
}

DaysInMonth(year, month) {
    static thirtyOneDayMonths := Map(1, 1, 3, 1, 5, 1, 7, 1, 8, 1, 10, 1, 12, 1)

    if thirtyOneDayMonths.Has(month) {
        return 31
    }

    if (month = 2) {
        return VehimapLeapYearInternal(year) ? 29 : 28
    }

    return 30
}

VehimapLeapYearInternal(year) {
    return (Mod(year, 4) = 0 && Mod(year, 100) != 0) || Mod(year, 400) = 0
}

NormalizeCategory(category) {
    global Categories

    if (category = "Osobní") {
        return "Osobní vozidla"
    }

    if (category = "Nákladní") {
        return "Nákladní vozidla"
    }

    for allowed in Categories {
        if (allowed = category) {
            return allowed
        }
    }
    return "Ostatní"
}

GetCategoryIndex(category) {
    global Categories

    category := NormalizeCategory(category)
    for index, allowed in Categories {
        if (allowed = category) {
            return index
        }
    }

    return Categories.Length
}

SetDropDownToText(ctrl, wantedText, items := "") {
    global Categories

    defaultIndex := 0
    if IsObject(items) {
        if (items.Length = Categories.Length && items[1] = Categories[1]) {
            wantedText := NormalizeCategory(wantedText)
            defaultIndex := Categories.Length
        } else if (items.Length > 0) {
            defaultIndex := 1
        }

        for index, item in items {
            if (item = wantedText) {
                ctrl.Value := index
                return
            }
        }
    }

    if defaultIndex {
        ctrl.Value := defaultIndex
        return
    }

    try ctrl.Text := wantedText
}

GenerateVehicleId() {
    return A_Now "_" Random(1000, 9999)
}

EscapeField(value) {
    value := StrReplace(value, "\", "\\")
    value := StrReplace(value, "`t", "\t")
    value := StrReplace(value, "`n", "\n")
    value := StrReplace(value, "`r")
    return value
}

UnescapeField(value) {
    placeholder := Chr(1)
    value := StrReplace(value, "\\", placeholder)
    value := StrReplace(value, "\t", "`t")
    value := StrReplace(value, "\n", "`n")
    value := StrReplace(value, placeholder, "\")
    return value
}

JoinLines(lines, separator := "`n") {
    output := ""
    for index, line in lines {
        if (index > 1) {
            output .= separator
        }
        output .= line
    }
    return output
}

JoinInline(parts, separator := " | ") {
    output := ""
    for index, part in parts {
        if (index > 1) {
            output .= separator
        }
        output .= part
    }
    return output
}

MoveGuiControl(ctrl, x?, y?, w?, h?) {
    if !IsObject(ctrl) {
        return
    }

    ctrl.GetPos(&currentX, &currentY, &currentW, &currentH)
    targetX := IsSet(x) ? x : currentX
    targetY := IsSet(y) ? y : currentY
    targetW := IsSet(w) ? w : currentW
    targetH := IsSet(h) ? h : currentH
    ctrl.Move(targetX, targetY, targetW, targetH)
}

SortVehiclesByDue(&items) {
    count := items.Length
    if (count < 2) {
        return
    }

    Loop count - 1 {
        i := A_Index + 1
        current := items[i]
        j := i - 1

        while (j >= 1 && CompareVehicles(current, items[j]) < 0) {
            items[j + 1] := items[j]
            j -= 1
        }
        items[j + 1] := current
    }
}

CompareVehicles(left, right) {
    leftKey := ParseDueStamp(left.nextTk)
    rightKey := ParseDueStamp(right.nextTk)

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

    return CompareTextValues(left.name, right.name)
}

SortUpcomingByDue(&items) {
    count := items.Length
    if (count < 2) {
        return
    }

    Loop count - 1 {
        i := A_Index + 1
        current := items[i]
        j := i - 1

        while (j >= 1 && CompareUpcoming(current, items[j]) < 0) {
            items[j + 1] := items[j]
            j -= 1
        }
        items[j + 1] := current
    }
}

CompareUpcoming(left, right) {
    if (left.dueStamp < right.dueStamp) {
        return -1
    }
    if (left.dueStamp > right.dueStamp) {
        return 1
    }
    return CompareVehicles(left.vehicle, right.vehicle)
}

SortOverviewEntries(&items) {
    count := items.Length
    if (count < 2) {
        return
    }

    Loop count - 1 {
        i := A_Index + 1
        current := items[i]
        j := i - 1

        while (j >= 1 && CompareOverviewEntries(current, items[j]) < 0) {
            items[j + 1] := items[j]
            j -= 1
        }
        items[j + 1] := current
    }
}

CompareOverviewEntries(left, right) {
    global OverviewSortColumn, OverviewSortDescending

    result := CompareOverviewEntriesByColumn(left, right, OverviewSortColumn)
    if (result = 0 && OverviewSortColumn != 6) {
        result := CompareOverviewEntriesByColumn(left, right, 6)
    }
    if (result = 0) {
        result := CompareVehicles(left.vehicle, right.vehicle)
    }

    return OverviewSortDescending ? -result : result
}

CompareOverviewEntriesByColumn(left, right, column) {
    switch column {
        case 1:
            return CompareOverviewText(left.kindLabel, right.kindLabel)
        case 2:
            return CompareOverviewText(left.vehicle.name, right.vehicle.name)
        case 3:
            return CompareOverviewText(left.vehicle.category, right.vehicle.category)
        case 4:
            return CompareOverviewText(left.vehicle.makeModel, right.vehicle.makeModel)
        case 5:
            return CompareOverviewText(left.vehicle.plate, right.vehicle.plate)
        case 6:
            return CompareOverviewDueStamp(left, right)
        case 7:
            return CompareOverviewText(left.status, right.status)
    }

    return 0
}

CompareOverviewText(leftText, rightText) {
    return CompareTextValues(leftText, rightText)
}

CompareOverviewDueStamp(left, right) {
    leftKey := GetOverviewDueSortKey(left)
    rightKey := GetOverviewDueSortKey(right)

    if (leftKey < rightKey) {
        return -1
    }
    if (leftKey > rightKey) {
        return 1
    }

    return CompareTextValues(left.kind, right.kind)
}

GetOverviewDueSortKey(entry) {
    if IsObject(entry) && entry.HasOwnProp("overviewSortKey") && Trim(entry.overviewSortKey) != "" {
        return entry.overviewSortKey
    }

    if IsObject(entry) && entry.HasOwnProp("dueStamp") {
        return entry.dueStamp
    }

    return "99999999999999"
}

CompareTextValues(leftText, rightText) {
    return StrCompare(StrLower(leftText), StrLower(rightText))
}

CompareNumberValues(leftValue, rightValue) {
    if (leftValue < rightValue) {
        return -1
    }
    if (leftValue > rightValue) {
        return 1
    }

    return 0
}

CompareOptionalStampValues(leftStamp, rightStamp) {
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

    return 0
}

CompareOptionalIntegerTexts(leftText, rightText) {
    leftValue := (Trim(leftText) = "") ? 2147483647 : leftText + 0
    rightValue := (Trim(rightText) = "") ? 2147483647 : rightText + 0
    return CompareNumberValues(leftValue, rightValue)
}

CompareOptionalDecimalTexts(leftText, rightText) {
    leftValue := 0.0
    rightValue := 0.0
    if !TryParseDecimalValue(leftText, &leftValue) {
        leftValue := 9999999999999999.0
    }
    if !TryParseDecimalValue(rightText, &rightValue) {
        rightValue := 9999999999999999.0
    }

    return CompareNumberValues(leftValue, rightValue)
}

CompareOptionalMoneyTexts(leftText, rightText) {
    leftValue := 0.0
    rightValue := 0.0
    if !TryParseMoneyAmount(leftText, &leftValue) {
        leftValue := 9999999999999999.0
    }
    if !TryParseMoneyAmount(rightText, &rightValue) {
        rightValue := 9999999999999999.0
    }

    return CompareNumberValues(leftValue, rightValue)
}

TryParseDecimalValue(text, &value) {
    value := 0.0
    normalized := NormalizeDecimalText(text)
    if (normalized = "") {
        return false
    }

    value := StrReplace(normalized, ",", ".") + 0.0
    return true
}

SortTextItemsDescending(&items) {
    count := items.Length
    if (count < 2) {
        return
    }

    Loop count - 1 {
        swapped := false
        Loop count - A_Index {
            left := items[A_Index]
            right := items[A_Index + 1]
            if (CompareTextValues(left, right) < 0) {
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

ShortenText(text, maxLength) {
    if (StrLen(text) <= maxLength) {
        return text
    }
    return SubStr(text, 1, maxLength - 1) Chr(8230)
}
