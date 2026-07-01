using System.Text.Json;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Domain.Models;

namespace Vehimap.Application.Services;

public sealed class InstallerLocaleSeedService
{
    public const string SeedFileName = "installer-preferences.json";

    private readonly AppLocaleDefaultsService _defaultsService;
    private readonly IAppLocalizer _localizer;

    public InstallerLocaleSeedService()
        : this(new AppLocaleDefaultsService(), new ResourceAppLocalizer())
    {
    }

    public InstallerLocaleSeedService(AppLocaleDefaultsService defaultsService)
        : this(defaultsService, new ResourceAppLocalizer())
    {
    }

    public InstallerLocaleSeedService(AppLocaleDefaultsService defaultsService, IAppLocalizer? localizer)
    {
        _defaultsService = defaultsService;
        _localizer = localizer ?? new ResourceAppLocalizer();
    }

    public async Task<InstallerLocaleSeedApplyResult> ApplyIfPresentAsync(
        VehimapDataRoot dataRoot,
        VehimapSettings settings,
        CancellationToken cancellationToken = default)
    {
        var seedPath = GetSeedPath(dataRoot);
        if (!File.Exists(seedPath))
        {
            return InstallerLocaleSeedApplyResult.NotFound;
        }

        InstallerLocaleSeedDocument? seed;
        try
        {
            await using var stream = File.OpenRead(seedPath);
            seed = await JsonSerializer.DeserializeAsync<InstallerLocaleSeedDocument>(
                    stream,
                    new JsonSerializerOptions(JsonSerializerDefaults.Web),
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            return MoveInvalidSeed(seedPath, _localizer.Format("InstallerLocaleSeed.InvalidRead", ex.Message));
        }

        var language = AppCultureService.NormalizeLanguage(seed?.Language);
        if (string.Equals(language, AppCultureService.SystemLanguage, StringComparison.Ordinal))
        {
            return MoveInvalidSeed(seedPath, _localizer.GetString("InstallerLocaleSeed.InvalidLanguage"));
        }

        var hasLanguage = HasSetting(settings, "app", "language");
        var effectiveLanguage = hasLanguage
            ? settings.GetValue("app", "language", language)
            : language;
        var defaults = _defaultsService.GetDefaultsForLanguage(effectiveLanguage);
        var changed = false;
        changed |= SetIfMissing(settings, "app", "language", language);
        changed |= SetIfMissing(settings, "app", "thousands_separator", defaults.ThousandsSeparator);
        changed |= SetIfMissing(settings, "app", "decimal_separator", defaults.DecimalSeparator);
        changed |= SetIfMissing(settings, "app", "distance_unit", defaults.DistanceUnit);
        changed |= SetIfMissing(settings, "app", "volume_unit", defaults.VolumeUnit);
        changed |= SetIfMissing(settings, "app", "currency", defaults.Currency);

        return new InstallerLocaleSeedApplyResult(
            true,
            changed,
            true,
            seedPath,
            changed
                ? _localizer.GetString("InstallerLocaleSeed.Applied")
                : _localizer.GetString("InstallerLocaleSeed.SkippedExisting"));
    }

    public void CompleteSeed(InstallerLocaleSeedApplyResult result)
    {
        if (!result.SeedFound || !result.SeedValid || string.IsNullOrWhiteSpace(result.SeedPath))
        {
            return;
        }

        try
        {
            if (File.Exists(result.SeedPath))
            {
                File.Delete(result.SeedPath);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    public static string GetSeedPath(VehimapDataRoot dataRoot) =>
        Path.Combine(dataRoot.DataPath, SeedFileName);

    private static bool SetIfMissing(VehimapSettings settings, string section, string key, string value)
    {
        if (HasSetting(settings, section, key))
        {
            return false;
        }

        settings.SetValue(section, key, value);
        return true;
    }

    private static bool HasSetting(VehimapSettings settings, string section, string key) =>
        settings.Sections.TryGetValue(section, out var values) && values.ContainsKey(key);

    private static InstallerLocaleSeedApplyResult MoveInvalidSeed(string seedPath, string message)
    {
        var invalidPath = seedPath + ".invalid";
        try
        {
            if (File.Exists(invalidPath))
            {
                File.Delete(invalidPath);
            }

            File.Move(seedPath, invalidPath);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }

        return new InstallerLocaleSeedApplyResult(true, false, false, seedPath, message);
    }

    private sealed record InstallerLocaleSeedDocument(string? Language);
}
