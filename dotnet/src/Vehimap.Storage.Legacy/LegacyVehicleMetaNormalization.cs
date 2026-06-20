namespace Vehimap.Storage.Legacy;

public static class LegacyVehicleMetaNormalization
{
    public static string NormalizeTagList(string? tags)
    {
        var value = (tags ?? string.Empty).Trim();
        if (value.Length == 0)
        {
            return string.Empty;
        }

        var normalized = new List<string>();
        var seen = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
        foreach (var rawItem in value.Replace(';', ',').Split(','))
        {
            var item = rawItem.Trim();
            if (item.Length == 0 || !seen.Add(item))
            {
                continue;
            }

            normalized.Add(item);
        }

        return string.Join(", ", normalized);
    }
}
