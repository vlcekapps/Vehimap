namespace Vehimap.Domain.Models;

public sealed class VehimapSettings
{
    public Dictionary<string, Dictionary<string, string>> Sections { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    public string GetValue(string section, string key, string defaultValue = "")
    {
        if (!Sections.TryGetValue(section, out var values))
        {
            return defaultValue;
        }

        return values.TryGetValue(key, out var value) ? value : defaultValue;
    }

    public void SetValue(string section, string key, string value)
    {
        if (!Sections.TryGetValue(section, out var values))
        {
            values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            Sections[section] = values;
        }

        values[key] = value;
    }
}
