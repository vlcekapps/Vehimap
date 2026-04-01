namespace Vehimap.Storage.Legacy;

internal static class LegacyBackupSerialization
{
    public const string BackupHeaderV6 = "# Vehimap backup v6";

    public static LegacyBackupPayload Parse(string content)
    {
        var normalized = LegacySectionSerialization.NormalizeTextForStorage(content);
        var delimiter = normalized.IndexOf("\n\n", StringComparison.Ordinal);
        if (delimiter < 0)
        {
            throw new FormatException("Soubor zálohy nemá platnou hlavičku.");
        }

        var header = normalized[..delimiter];
        var payload = normalized[(delimiter + 2)..];
        var headerLines = header.Split('\n');
        if (headerLines.Length < 3)
        {
            throw new FormatException("Soubor není ve formátu zálohy Vehimap.");
        }

        var version = headerLines[0].Trim();
        var supported = new HashSet<string>(StringComparer.Ordinal)
        {
            "# Vehimap backup v1",
            "# Vehimap backup v2",
            "# Vehimap backup v3",
            "# Vehimap backup v4",
            "# Vehimap backup v5",
            "# Vehimap backup v6"
        };

        if (!supported.Contains(version))
        {
            throw new FormatException("Soubor není ve formátu zálohy Vehimap.");
        }

        var settingsLength = ReadLength(headerLines, 1, "settings_length");
        var vehiclesLength = ReadLength(headerLines, 2, "vehicles_length");
        var historyLength = version is "# Vehimap backup v2" or "# Vehimap backup v3" or "# Vehimap backup v4" or "# Vehimap backup v5" or "# Vehimap backup v6"
            ? ReadLength(headerLines, 3, "history_length")
            : 0;
        var fuelLength = version is "# Vehimap backup v3" or "# Vehimap backup v4" or "# Vehimap backup v5" or "# Vehimap backup v6"
            ? ReadLength(headerLines, 4, "fuel_length")
            : 0;
        var recordsLength = version is "# Vehimap backup v3" or "# Vehimap backup v4" or "# Vehimap backup v5" or "# Vehimap backup v6"
            ? ReadLength(headerLines, 5, "records_length")
            : 0;
        var metaLength = version is "# Vehimap backup v4" or "# Vehimap backup v5" or "# Vehimap backup v6"
            ? ReadLength(headerLines, 6, "meta_length")
            : 0;
        var remindersLength = version is "# Vehimap backup v4" or "# Vehimap backup v5" or "# Vehimap backup v6"
            ? ReadLength(headerLines, 7, "reminders_length")
            : 0;
        var maintenanceLength = version is "# Vehimap backup v5" or "# Vehimap backup v6"
            ? ReadLength(headerLines, 8, "maintenance_length")
            : 0;
        var attachmentsLength = version == "# Vehimap backup v6"
            ? ReadLength(headerLines, 9, "attachments_length")
            : 0;

        if (payload.Length != settingsLength + vehiclesLength + historyLength + fuelLength + recordsLength + metaLength + remindersLength + maintenanceLength + attachmentsLength)
        {
            throw new FormatException("Soubor zálohy je neúplný nebo poškozený.");
        }

        var offset = 0;
        var settingsContent = Slice(payload, ref offset, settingsLength);
        var vehiclesContent = Slice(payload, ref offset, vehiclesLength);
        var historyContent = historyLength > 0 ? Slice(payload, ref offset, historyLength) : $"{LegacySectionSerialization.HistoryHeaderV1}\n";
        var fuelContent = fuelLength > 0 ? Slice(payload, ref offset, fuelLength) : $"{LegacySectionSerialization.FuelHeaderV1}\n";
        var recordsContent = recordsLength > 0 ? Slice(payload, ref offset, recordsLength) : $"{LegacySectionSerialization.RecordsHeaderV2}\n";
        var metaContent = metaLength > 0 ? Slice(payload, ref offset, metaLength) : $"{LegacySectionSerialization.MetaHeaderV2}\n";
        var remindersContent = remindersLength > 0 ? Slice(payload, ref offset, remindersLength) : $"{LegacySectionSerialization.RemindersHeaderV1}\n";
        var maintenanceContent = maintenanceLength > 0 ? Slice(payload, ref offset, maintenanceLength) : $"{LegacySectionSerialization.MaintenanceHeaderV1}\n";
        var attachmentsContent = attachmentsLength > 0 ? Slice(payload, ref offset, attachmentsLength) : $"{LegacySectionSerialization.AttachmentsHeaderV1}\n";

        return new LegacyBackupPayload(
            settingsContent,
            vehiclesContent,
            historyContent,
            fuelContent,
            recordsContent,
            metaContent,
            remindersContent,
            maintenanceContent,
            attachmentsContent);
    }

    public static string Build(LegacyBackupPayload payload)
    {
        var settings = LegacySectionSerialization.NormalizeTextForStorage(payload.SettingsContent);
        var vehicles = LegacySectionSerialization.NormalizeTextForStorage(payload.VehiclesContent);
        var history = LegacySectionSerialization.NormalizeTextForStorage(payload.HistoryContent);
        var fuel = LegacySectionSerialization.NormalizeTextForStorage(payload.FuelContent);
        var records = LegacySectionSerialization.NormalizeTextForStorage(payload.RecordsContent);
        var meta = LegacySectionSerialization.NormalizeTextForStorage(payload.MetaContent);
        var reminders = LegacySectionSerialization.NormalizeTextForStorage(payload.RemindersContent);
        var maintenance = LegacySectionSerialization.NormalizeTextForStorage(payload.MaintenanceContent);
        var attachments = LegacySectionSerialization.NormalizeTextForStorage(payload.AttachmentsContent);

        var header = string.Join('\n',
            BackupHeaderV6,
            $"settings_length={settings.Length}",
            $"vehicles_length={vehicles.Length}",
            $"history_length={history.Length}",
            $"fuel_length={fuel.Length}",
            $"records_length={records.Length}",
            $"meta_length={meta.Length}",
            $"reminders_length={reminders.Length}",
            $"maintenance_length={maintenance.Length}",
            $"attachments_length={attachments.Length}");

        return $"{header}\n\n{settings}{vehicles}{history}{fuel}{records}{meta}{reminders}{maintenance}{attachments}";
    }

    private static int ReadLength(string[] lines, int index, string key)
    {
        if (index >= lines.Length)
        {
            throw new FormatException($"Soubor zálohy neobsahuje položku {key}.");
        }

        var prefix = $"{key}=";
        if (!lines[index].StartsWith(prefix, StringComparison.Ordinal) ||
            !int.TryParse(lines[index][prefix.Length..], out var length) ||
            length < 0)
        {
            throw new FormatException($"Soubor zálohy neobsahuje položku {key}.");
        }

        return length;
    }

    private static string Slice(string value, ref int offset, int length)
    {
        var result = value.Substring(offset, length);
        offset += length;
        return result;
    }
}

internal sealed record LegacyBackupPayload(
    string SettingsContent,
    string VehiclesContent,
    string HistoryContent,
    string FuelContent,
    string RecordsContent,
    string MetaContent,
    string RemindersContent,
    string MaintenanceContent,
    string AttachmentsContent);
