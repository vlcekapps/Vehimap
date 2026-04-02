using System.Globalization;

namespace Vehimap.Application.Services;

public static class LegacyUpdateManifestParser
{
    public static LegacyUpdateManifest Parse(string content)
    {
        string currentSection = string.Empty;
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var rawLine in Normalize(content).Split('\n'))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith(';') || line.StartsWith('#'))
            {
                continue;
            }

            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                currentSection = line[1..^1].Trim();
                continue;
            }

            if (!currentSection.Equals("release", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim();
            values[key] = value;
        }

        if (!values.TryGetValue("version", out var version) || string.IsNullOrWhiteSpace(version))
        {
            throw new InvalidOperationException("Manifest neobsahuje položku release/version.");
        }

        if (!SemVersionService.IsValid(version))
        {
            throw new InvalidOperationException($"Manifest obsahuje neplatnou verzi: {version}");
        }

        values.TryGetValue("published_at", out var publishedAt);
        values.TryGetValue("notes_url", out var notesUrl);
        values.TryGetValue("asset_url", out var assetUrl);
        values.TryGetValue("asset_sha256", out var sha256);
        values.TryGetValue("asset_size", out var assetSizeRaw);

        long? assetSize = null;
        if (!string.IsNullOrWhiteSpace(assetSizeRaw))
        {
            if (!long.TryParse(assetSizeRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedSize) || parsedSize <= 0)
            {
                throw new InvalidOperationException("Manifest neobsahuje platnou velikost assetu.");
            }

            assetSize = parsedSize;
        }

        return new LegacyUpdateManifest(
            version.Trim(),
            string.IsNullOrWhiteSpace(publishedAt) ? null : publishedAt.Trim(),
            string.IsNullOrWhiteSpace(notesUrl) ? null : notesUrl.Trim(),
            string.IsNullOrWhiteSpace(assetUrl) ? null : assetUrl.Trim(),
            string.IsNullOrWhiteSpace(sha256) ? null : sha256.Trim().ToLowerInvariant(),
            assetSize);
    }

    private static string Normalize(string content) =>
        content.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
}

public sealed record LegacyUpdateManifest(
    string Version,
    string? PublishedAt,
    string? NotesUrl,
    string? AssetUrl,
    string? AssetSha256,
    long? AssetSize);
