using System.Globalization;
using System.Text.RegularExpressions;

namespace Vehimap.Application.Services;

public static partial class SemVersionService
{
    public static int Compare(string left, string right)
    {
        var leftVersion = Parse(left);
        var rightVersion = Parse(right);

        var coreComparison = leftVersion.Major.CompareTo(rightVersion.Major);
        if (coreComparison != 0)
        {
            return coreComparison;
        }

        coreComparison = leftVersion.Minor.CompareTo(rightVersion.Minor);
        if (coreComparison != 0)
        {
            return coreComparison;
        }

        coreComparison = leftVersion.Patch.CompareTo(rightVersion.Patch);
        if (coreComparison != 0)
        {
            return coreComparison;
        }

        return ComparePrerelease(leftVersion.PrereleaseIdentifiers, rightVersion.PrereleaseIdentifiers);
    }

    public static bool IsValid(string value)
    {
        try
        {
            Parse(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static string NormalizeToFileVersion(string version)
    {
        var parsed = Parse(version);
        return FormattableString.Invariant($"{parsed.Major}.{parsed.Minor}.{parsed.Patch}.0");
    }

    private static ParsedSemVersion Parse(string value)
    {
        var match = SemVersionRegex().Match(value.Trim());
        if (!match.Success)
        {
            throw new InvalidOperationException($"Neplatná semver verze: {value}");
        }

        var prerelease = match.Groups["prerelease"].Success
            ? match.Groups["prerelease"].Value.Split('.', StringSplitOptions.RemoveEmptyEntries)
            : [];

        return new ParsedSemVersion(
            int.Parse(match.Groups["major"].Value, CultureInfo.InvariantCulture),
            int.Parse(match.Groups["minor"].Value, CultureInfo.InvariantCulture),
            int.Parse(match.Groups["patch"].Value, CultureInfo.InvariantCulture),
            prerelease);
    }

    private static int ComparePrerelease(IReadOnlyList<string> left, IReadOnlyList<string> right)
    {
        if (left.Count == 0 && right.Count == 0)
        {
            return 0;
        }

        if (left.Count == 0)
        {
            return 1;
        }

        if (right.Count == 0)
        {
            return -1;
        }

        var maxCount = Math.Max(left.Count, right.Count);
        for (var index = 0; index < maxCount; index++)
        {
            if (index >= left.Count)
            {
                return -1;
            }

            if (index >= right.Count)
            {
                return 1;
            }

            var leftId = left[index];
            var rightId = right[index];
            var leftNumeric = int.TryParse(leftId, NumberStyles.None, CultureInfo.InvariantCulture, out var leftNumber);
            var rightNumeric = int.TryParse(rightId, NumberStyles.None, CultureInfo.InvariantCulture, out var rightNumber);

            if (leftNumeric && rightNumeric)
            {
                var numericComparison = leftNumber.CompareTo(rightNumber);
                if (numericComparison != 0)
                {
                    return numericComparison;
                }

                continue;
            }

            if (leftNumeric)
            {
                return -1;
            }

            if (rightNumeric)
            {
                return 1;
            }

            var textComparison = string.Compare(leftId, rightId, StringComparison.Ordinal);
            if (textComparison != 0)
            {
                return textComparison;
            }
        }

        return 0;
    }

    private sealed record ParsedSemVersion(int Major, int Minor, int Patch, IReadOnlyList<string> PrereleaseIdentifiers);

    [GeneratedRegex("^(?<major>\\d+)\\.(?<minor>\\d+)\\.(?<patch>\\d+)(?:-(?<prerelease>[0-9A-Za-z-]+(?:\\.[0-9A-Za-z-]+)*))?$", RegexOptions.Compiled)]
    private static partial Regex SemVersionRegex();
}
