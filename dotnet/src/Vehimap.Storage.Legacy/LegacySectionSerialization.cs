// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Services;

namespace Vehimap.Storage.Legacy;

internal static class LegacySectionSerialization
{
    public const string VehiclesHeaderV4 = "# Vehimap data v4";
    public const string VehiclesHeaderV3 = "# Vehimap data v3";
    public const string HistoryHeaderV1 = "# Vehimap history v1";
    public const string FuelHeaderV1 = "# Vehimap fuel v1";
    public const string FuelHeaderV2 = "# Vehimap fuel v2";
    public const string RecordsHeaderV1 = "# Vehimap records v1";
    public const string RecordsHeaderV2 = "# Vehimap records v2";
    public const string MetaHeaderV1 = "# Vehimap meta v1";
    public const string MetaHeaderV2 = "# Vehimap meta v2";
    public const string RemindersHeaderV1 = "# Vehimap reminders v1";
    public const string RemindersHeaderV2 = "# Vehimap reminders v2";
    public const string MaintenanceHeaderV1 = "# Vehimap maintenance v1";
    public const string AttachmentsHeaderV1 = "# Vehimap attachments v1";

    public const string VehiclesFileName = "vehicles.tsv";
    public const string HistoryFileName = "history.tsv";
    public const string FuelFileName = "fuel.tsv";
    public const string RecordsFileName = "records.tsv";
    public const string MetaFileName = "vehicle_meta.tsv";
    public const string RemindersFileName = "reminders.tsv";
    public const string MaintenanceFileName = "maintenance.tsv";
    public const string SettingsFileName = "settings.ini";
    public const string AttachmentsDirectoryName = "attachments";

    public static string NormalizeTextForStorage(string? text)
    {
        return (text ?? string.Empty)
            .Replace("\uFEFF", string.Empty, StringComparison.Ordinal)
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal);
    }

    public static string EscapeField(string? value)
    {
        return (value ?? string.Empty)
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\t", "\\t", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal)
            .Replace("\r", string.Empty, StringComparison.Ordinal);
    }

    public static string UnescapeField(string? value)
    {
        const string placeholder = "\u0001";

        return (value ?? string.Empty)
            .Replace("\\\\", placeholder, StringComparison.Ordinal)
            .Replace("\\t", "\t", StringComparison.Ordinal)
            .Replace("\\n", "\n", StringComparison.Ordinal)
            .Replace(placeholder, "\\", StringComparison.Ordinal);
    }

    public static string NormalizeCategory(string? value)
    {
        return LegacyVehicleValueNormalization.NormalizeCategory(value);
    }

    public static VehicleRecordAttachmentMode NormalizeAttachmentMode(string? mode)
    {
        return string.Equals(mode?.Trim(), "managed", StringComparison.OrdinalIgnoreCase)
            ? VehicleRecordAttachmentMode.Managed
            : VehicleRecordAttachmentMode.External;
    }

    public static string NormalizeAttachmentRelativePath(string? path)
    {
        return ManagedAttachmentPathGuard.NormalizeAttachmentRelativePath(path);
    }

    public static string GetAttachmentsRootPath(string dataPath) =>
        Path.Combine(dataPath, AttachmentsDirectoryName);

    public static string ResolveManagedAttachmentPath(string dataPath, string relativePath)
    {
        return ManagedAttachmentPathGuard.ResolveManagedAttachmentPath(dataPath, relativePath);
    }

    public static VehimapSettings ParseSettings(string content)
    {
        var normalized = NormalizeTextForStorage(content);
        var settings = new VehimapSettings();
        var currentSection = "app";

        foreach (var rawLine in normalized.Split('\n'))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith(';') || line.StartsWith('#'))
            {
                continue;
            }

            if (line.StartsWith('[') && line.EndsWith(']') && line.Length > 2)
            {
                currentSection = line[1..^1].Trim();
                continue;
            }

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim();
            settings.SetValue(currentSection, key, value);
        }

        return settings;
    }

    public static string SerializeSettings(VehimapSettings settings)
    {
        var lines = new List<string>();
        foreach (var section in settings.Sections)
        {
            if (lines.Count > 0)
            {
                lines.Add(string.Empty);
            }

            lines.Add($"[{section.Key}]");
            foreach (var pair in section.Value)
            {
                lines.Add($"{pair.Key}={pair.Value}");
            }
        }

        return string.Join('\n', lines);
    }

    public static List<Vehicle> ParseVehicles(string content, IAppLocalizer? localizer = null)
    {
        var resolvedLocalizer = ResolveLocalizer(localizer);
        var (header, rows) = ReadDataRows(content, resolvedLocalizer);
        EnsureAllowedHeader(header, resolvedLocalizer, VehiclesHeaderV3, VehiclesHeaderV4);

        return rows.Select((row, index) =>
        {
            var fields = SplitTabRow(row);
            if (fields.Count != 12)
            {
                throw InvalidFieldCount(resolvedLocalizer, "LegacyData.Section.Vehicles", index + 2, FieldCount(resolvedLocalizer, 12));
            }

            return new Vehicle(
                UnescapeField(fields[0]),
                UnescapeField(fields[1]),
                NormalizeCategory(UnescapeField(fields[2])),
                UnescapeField(fields[3]),
                UnescapeField(fields[4]),
                UnescapeField(fields[5]),
                UnescapeField(fields[6]),
                UnescapeField(fields[7]),
                UnescapeField(fields[8]),
                UnescapeField(fields[9]),
                UnescapeField(fields[10]),
                UnescapeField(fields[11]));
        }).ToList();
    }

    public static string SerializeVehicles(IEnumerable<Vehicle> vehicles)
    {
        var lines = new List<string> { VehiclesHeaderV4 };
        foreach (var vehicle in vehicles)
        {
            lines.Add(string.Join('\t',
                EscapeField(vehicle.Id),
                EscapeField(vehicle.Name),
                EscapeField(vehicle.Category),
                EscapeField(vehicle.VehicleNote),
                EscapeField(vehicle.MakeModel),
                EscapeField(vehicle.Plate),
                EscapeField(vehicle.Year),
                EscapeField(vehicle.Power),
                EscapeField(vehicle.LastTk),
                EscapeField(vehicle.NextTk),
                EscapeField(vehicle.GreenCardFrom),
                EscapeField(vehicle.GreenCardTo)));
        }

        return string.Join('\n', lines);
    }

    public static List<VehicleHistoryEntry> ParseHistory(string content, IAppLocalizer? localizer = null)
    {
        var resolvedLocalizer = ResolveLocalizer(localizer);
        var (header, rows) = ReadDataRows(content, resolvedLocalizer);
        EnsureAllowedHeader(header, resolvedLocalizer, HistoryHeaderV1);

        return rows.Select((row, index) =>
        {
            var fields = SplitTabRow(row);
            if (fields.Count is not 6 and not 7)
            {
                throw InvalidFieldCount(resolvedLocalizer, "LegacyData.Section.History", index + 2, resolvedLocalizer.GetString("LegacySection.FieldCount.SixOrSeven"));
            }

            return new VehicleHistoryEntry(
                UnescapeField(fields[0]),
                UnescapeField(fields[1]),
                UnescapeField(fields[2]),
                UnescapeField(fields[3]),
                UnescapeField(fields[4]),
                UnescapeField(fields[5]),
                fields.Count > 6 ? UnescapeField(fields[6]) : string.Empty);
        }).ToList();
    }

    public static string SerializeHistory(IEnumerable<VehicleHistoryEntry> items)
    {
        var lines = new List<string> { HistoryHeaderV1 };
        foreach (var item in items)
        {
            lines.Add(string.Join('\t',
                EscapeField(item.Id),
                EscapeField(item.VehicleId),
                EscapeField(item.EventDate),
                EscapeField(item.EventType),
                EscapeField(item.Odometer),
                EscapeField(item.Cost),
                EscapeField(item.Note)));
        }

        return string.Join('\n', lines);
    }

    public static List<FuelEntry> ParseFuel(string content, IAppLocalizer? localizer = null)
    {
        var resolvedLocalizer = ResolveLocalizer(localizer);
        var (header, rows) = ReadDataRows(content, resolvedLocalizer);
        EnsureAllowedHeader(header, resolvedLocalizer, FuelHeaderV1, FuelHeaderV2);
        var isV2 = string.Equals(header, FuelHeaderV2, StringComparison.Ordinal);

        return rows.Select((row, index) =>
        {
            var fields = SplitTabRow(row);
            var expected = isV2 ? 11 : 9;
            if (fields.Count != expected)
            {
                throw InvalidFieldCount(resolvedLocalizer, "LegacyData.Section.Fuel", index + 2, FieldCount(resolvedLocalizer, expected));
            }

            return new FuelEntry(
                UnescapeField(fields[0]),
                UnescapeField(fields[1]),
                UnescapeField(fields[2]),
                UnescapeField(fields[3]),
                UnescapeField(fields[4]),
                UnescapeField(fields[5]),
                UnescapeField(fields[6]) == "1",
                UnescapeField(fields[7]),
                isV2 ? UnescapeField(fields[10]) : UnescapeField(fields[8]),
                isV2 ? UnescapeField(fields[8]) : string.Empty,
                isV2 ? UnescapeField(fields[9]) : string.Empty);
        }).ToList();
    }

    public static string SerializeFuel(IEnumerable<FuelEntry> items)
    {
        var lines = new List<string> { FuelHeaderV2 };
        foreach (var item in items)
        {
            lines.Add(string.Join('\t',
                EscapeField(item.Id),
                EscapeField(item.VehicleId),
                EscapeField(item.EntryDate),
                EscapeField(item.Odometer),
                EscapeField(item.Liters),
                EscapeField(item.TotalCost),
                EscapeField(item.FullTank ? "1" : "0"),
                EscapeField(item.FuelType),
                EscapeField(item.FuelDetail),
                EscapeField(item.Station),
                EscapeField(item.Note)));
        }

        return string.Join('\n', lines);
    }

    public static List<VehicleRecord> ParseRecords(string content, IAppLocalizer? localizer = null)
    {
        var resolvedLocalizer = ResolveLocalizer(localizer);
        var (header, rows) = ReadDataRows(content, resolvedLocalizer);
        EnsureAllowedHeader(header, resolvedLocalizer, RecordsHeaderV1, RecordsHeaderV2);
        var isV2 = string.Equals(header, RecordsHeaderV2, StringComparison.Ordinal);

        return rows.Select((row, index) =>
        {
            var fields = SplitTabRow(row);
            var expected = isV2 ? 11 : 10;
            if (fields.Count != expected)
            {
                throw InvalidFieldCount(resolvedLocalizer, "LegacyData.Section.Records", index + 2, FieldCount(resolvedLocalizer, expected));
            }

            return new VehicleRecord(
                UnescapeField(fields[0]),
                UnescapeField(fields[1]),
                UnescapeField(fields[2]),
                UnescapeField(fields[3]),
                UnescapeField(fields[4]),
                UnescapeField(fields[5]),
                UnescapeField(fields[6]),
                UnescapeField(fields[7]),
                isV2 ? NormalizeAttachmentMode(UnescapeField(fields[8])) : VehicleRecordAttachmentMode.External,
                isV2 ? UnescapeField(fields[9]) : UnescapeField(fields[8]),
                isV2 ? UnescapeField(fields[10]) : UnescapeField(fields[9]));
        }).ToList();
    }

    public static string SerializeRecords(IEnumerable<VehicleRecord> items)
    {
        var lines = new List<string> { RecordsHeaderV2 };
        foreach (var item in items)
        {
            lines.Add(string.Join('\t',
                EscapeField(item.Id),
                EscapeField(item.VehicleId),
                EscapeField(item.RecordType),
                EscapeField(item.Title),
                EscapeField(item.Provider),
                EscapeField(item.ValidFrom),
                EscapeField(item.ValidTo),
                EscapeField(item.Price),
                EscapeField(item.AttachmentMode == VehicleRecordAttachmentMode.Managed ? "managed" : "external"),
                EscapeField(item.FilePath),
                EscapeField(item.Note)));
        }

        return string.Join('\n', lines);
    }

    public static List<VehicleMeta> ParseVehicleMeta(string content, IAppLocalizer? localizer = null)
    {
        var resolvedLocalizer = ResolveLocalizer(localizer);
        var (header, rows) = ReadDataRows(content, resolvedLocalizer);
        EnsureAllowedHeader(header, resolvedLocalizer, MetaHeaderV1, MetaHeaderV2);
        var isV2 = string.Equals(header, MetaHeaderV2, StringComparison.Ordinal);

        return rows.Select((row, index) =>
        {
            var fields = SplitTabRow(row);
            var expected = isV2 ? 7 : 3;
            if (fields.Count != expected)
            {
                throw InvalidFieldCount(resolvedLocalizer, "LegacyData.Section.VehicleMeta", index + 2, FieldCount(resolvedLocalizer, expected));
            }

            return new VehicleMeta(
                UnescapeField(fields[0]),
                UnescapeField(fields[1]),
                UnescapeField(fields[2]),
                isV2 ? UnescapeField(fields[3]) : string.Empty,
                isV2 ? UnescapeField(fields[4]) : string.Empty,
                isV2 ? UnescapeField(fields[5]) : string.Empty,
                isV2 ? UnescapeField(fields[6]) : string.Empty);
        }).ToList();
    }

    public static string SerializeVehicleMeta(IEnumerable<VehicleMeta> items)
    {
        var lines = new List<string> { MetaHeaderV2 };
        foreach (var item in items)
        {
            lines.Add(string.Join('\t',
                EscapeField(item.VehicleId),
                EscapeField(item.State),
                EscapeField(LegacyVehicleMetaNormalization.NormalizeTagList(item.Tags)),
                EscapeField(item.Powertrain),
                EscapeField(item.ClimateProfile),
                EscapeField(item.TimingDrive),
                EscapeField(item.Transmission)));
        }

        return string.Join('\n', lines);
    }

    public static List<VehicleReminder> ParseReminders(string content, IAppLocalizer? localizer = null)
    {
        var resolvedLocalizer = ResolveLocalizer(localizer);
        var (header, rows) = ReadDataRows(content, resolvedLocalizer);
        EnsureAllowedHeader(header, resolvedLocalizer, RemindersHeaderV1, RemindersHeaderV2);
        var isV2 = string.Equals(header, RemindersHeaderV2, StringComparison.Ordinal);

        return rows.Select((row, index) =>
        {
            var fields = SplitTabRow(row);
            var expected = isV2 ? 7 : 6;
            if (fields.Count != expected)
            {
                throw InvalidFieldCount(resolvedLocalizer, "LegacyData.Section.Reminders", index + 2, FieldCount(resolvedLocalizer, expected));
            }

            return new VehicleReminder(
                UnescapeField(fields[0]),
                UnescapeField(fields[1]),
                UnescapeField(fields[2]),
                UnescapeField(fields[3]),
                UnescapeField(fields[4]),
                isV2 ? UnescapeField(fields[5]) : "Neopakovat",
                isV2 ? UnescapeField(fields[6]) : UnescapeField(fields[5]));
        }).ToList();
    }

    public static string SerializeReminders(IEnumerable<VehicleReminder> items)
    {
        var lines = new List<string> { RemindersHeaderV2 };
        foreach (var item in items)
        {
            lines.Add(string.Join('\t',
                EscapeField(item.Id),
                EscapeField(item.VehicleId),
                EscapeField(item.Title),
                EscapeField(item.DueDate),
                EscapeField(item.ReminderDays),
                EscapeField(item.RepeatMode),
                EscapeField(item.Note)));
        }

        return string.Join('\n', lines);
    }

    public static List<MaintenancePlan> ParseMaintenancePlans(string content, IAppLocalizer? localizer = null)
    {
        var resolvedLocalizer = ResolveLocalizer(localizer);
        var (header, rows) = ReadDataRows(content, resolvedLocalizer);
        EnsureAllowedHeader(header, resolvedLocalizer, MaintenanceHeaderV1);

        return rows.Select((row, index) =>
        {
            var fields = SplitTabRow(row);
            if (fields.Count != 9)
            {
                throw InvalidFieldCount(resolvedLocalizer, "LegacyData.Section.Maintenance", index + 2, FieldCount(resolvedLocalizer, 9));
            }

            return new MaintenancePlan(
                UnescapeField(fields[0]),
                UnescapeField(fields[1]),
                UnescapeField(fields[2]),
                UnescapeField(fields[3]),
                UnescapeField(fields[4]),
                UnescapeField(fields[5]),
                UnescapeField(fields[6]),
                UnescapeField(fields[7]) != "0",
                UnescapeField(fields[8]));
        }).ToList();
    }

    public static string SerializeMaintenancePlans(IEnumerable<MaintenancePlan> items)
    {
        var lines = new List<string> { MaintenanceHeaderV1 };
        foreach (var item in items)
        {
            lines.Add(string.Join('\t',
                EscapeField(item.Id),
                EscapeField(item.VehicleId),
                EscapeField(item.Title),
                EscapeField(item.IntervalKm),
                EscapeField(item.IntervalMonths),
                EscapeField(item.LastServiceDate),
                EscapeField(item.LastServiceOdometer),
                EscapeField(item.IsActive ? "1" : "0"),
                EscapeField(item.Note)));
        }

        return string.Join('\n', lines);
    }

    public static List<ManagedAttachment> ParseAttachmentsSection(string content, IAppLocalizer? localizer = null)
    {
        var resolvedLocalizer = ResolveLocalizer(localizer);
        var (header, rows) = ReadDataRows(content, resolvedLocalizer);
        EnsureAllowedHeader(header, resolvedLocalizer, AttachmentsHeaderV1);

        return rows.Select((row, index) =>
        {
            var fields = SplitTabRow(row);
            if (fields.Count != 2)
            {
                throw InvalidFieldCount(resolvedLocalizer, "LegacyData.Section.Attachments", index + 2, resolvedLocalizer.GetString("LegacySection.FieldCount.Two"));
            }

            string relativePath;
            try
            {
                relativePath = NormalizeAttachmentRelativePath(UnescapeField(fields[0]));
            }
            catch (InvalidDataException ex)
            {
                throw new FormatException(
                    resolvedLocalizer.Format(
                        "LegacySection.Error.InvalidAttachmentPath",
                        resolvedLocalizer.GetString("LegacyData.Section.Attachments"),
                        index + 2),
                    ex);
            }

            if (string.IsNullOrWhiteSpace(relativePath))
            {
                throw new FormatException(resolvedLocalizer.Format(
                    "LegacySection.Error.EmptyAttachmentPath",
                    resolvedLocalizer.GetString("LegacyData.Section.Attachments"),
                    index + 2));
            }

            try
            {
                return new ManagedAttachment(relativePath, Convert.FromBase64String(UnescapeField(fields[1])));
            }
            catch (FormatException ex)
            {
                throw new FormatException(
                    resolvedLocalizer.Format(
                        "LegacySection.Error.InvalidAttachmentContent",
                        resolvedLocalizer.GetString("LegacyData.Section.Attachments"),
                        index + 2),
                    ex);
            }
        }).ToList();
    }

    public static string SerializeAttachmentsSection(IEnumerable<ManagedAttachment> items)
    {
        var lines = new List<string> { AttachmentsHeaderV1 };
        foreach (var item in items)
        {
            lines.Add(string.Join('\t',
                EscapeField(NormalizeAttachmentRelativePath(item.RelativePath)),
                EscapeField(Convert.ToBase64String(item.Content))));
        }

        return string.Join('\n', lines);
    }

    private static (string Header, List<string> Rows) ReadDataRows(string content, IAppLocalizer localizer)
    {
        var normalized = NormalizeTextForStorage(content);
        var lines = normalized.Split('\n');
        var firstNonEmptyLine = lines.FirstOrDefault(line => !string.IsNullOrWhiteSpace(line))?.Trim();
        if (string.IsNullOrWhiteSpace(firstNonEmptyLine))
        {
            return (string.Empty, new List<string>());
        }

        var rows = new List<string>();
        var dataStarted = false;
        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim('\r', '\n');
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (!dataStarted)
            {
                dataStarted = true;
                continue;
            }

            if (line.StartsWith('#'))
            {
                throw new FormatException(localizer.GetString("LegacySection.Error.NestedHeader"));
            }

            rows.Add(line);
        }

        return (firstNonEmptyLine, rows);
    }

    private static void EnsureAllowedHeader(string header, IAppLocalizer localizer, params string[] allowed)
    {
        if (string.IsNullOrWhiteSpace(header))
        {
            return;
        }

        if (allowed.Any(item => string.Equals(item, header, StringComparison.Ordinal)))
        {
            return;
        }

        throw new FormatException(localizer.Format("LegacySection.Error.UnsupportedHeader", header));
    }

    private static List<string> SplitTabRow(string row) =>
        row.Split('\t').ToList();

    private static IAppLocalizer ResolveLocalizer(IAppLocalizer? localizer) =>
        localizer ?? new ResourceAppLocalizer();

    private static FormatException InvalidFieldCount(IAppLocalizer localizer, string sectionKey, int lineNumber, string expectedFields) =>
        new(localizer.Format(
            "LegacySection.Error.InvalidFieldCount",
            localizer.GetString(sectionKey),
            lineNumber,
            expectedFields));

    private static string FieldCount(IAppLocalizer localizer, int count) =>
        localizer.Format("LegacySection.FieldCount.Count", count);
}
