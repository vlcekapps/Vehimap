// SPDX-License-Identifier: GPL-3.0-or-later
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.Loader;
using Vehimap.Application.Abstractions;

namespace Vehimap.Application.Services;

public sealed class ResourceAppLocalizer : IAppLocalizer
{
    private static readonly ResourceManager ResourceManager = new(
        "Vehimap.Resources.Strings",
        typeof(ResourceAppLocalizer).GetTypeInfo().Assembly);

    private static readonly Dictionary<string, ResourceManager?> CultureResourceManagers = new(StringComparer.OrdinalIgnoreCase);
    private static readonly object CultureResourceLock = new();

    private readonly CultureInfo _culture;

    public ResourceAppLocalizer()
        : this(CultureInfo.CurrentUICulture)
    {
    }

    public ResourceAppLocalizer(CultureInfo culture)
    {
        _culture = culture;
    }

    public string GetString(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return string.Empty;
        }

        return GetCultureSpecificString(key)
            ?? ResourceManager.GetString(key, _culture)
            ?? ResourceManager.GetString(key, CultureInfo.GetCultureInfo(AppCultureService.EnglishLanguage))
            ?? key;
    }

    public string Format(string key, params object?[] args) =>
        string.Format(_culture, GetString(key), args);

    private string? GetCultureSpecificString(string key)
    {
        if (string.Equals(_culture.Name, AppCultureService.EnglishLanguage, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var manager = GetCultureResourceManager(_culture);
        return manager?.GetString(key, CultureInfo.InvariantCulture);
    }

    private static ResourceManager? GetCultureResourceManager(CultureInfo culture)
    {
        lock (CultureResourceLock)
        {
            if (CultureResourceManagers.TryGetValue(culture.Name, out var cached))
            {
                return cached;
            }

            var manager = CreateCultureResourceManager(culture);
            CultureResourceManagers[culture.Name] = manager;
            return manager;
        }
    }

    private static ResourceManager? CreateCultureResourceManager(CultureInfo culture)
    {
        var applicationAssembly = typeof(ResourceAppLocalizer).Assembly;
        foreach (var cultureName in EnumerateCultureCandidates(culture))
        {
            var satelliteAssembly = TryLoadSatelliteAssembly(applicationAssembly, cultureName);
            if (satelliteAssembly is null)
            {
                continue;
            }

            var resourceBaseName = $"Vehimap.Resources.Strings.{cultureName}";
            if (satelliteAssembly.GetManifestResourceNames().Contains(resourceBaseName + ".resources", StringComparer.Ordinal))
            {
                return new ResourceManager(resourceBaseName, satelliteAssembly);
            }
        }

        return null;
    }

    private static IEnumerable<string> EnumerateCultureCandidates(CultureInfo culture)
    {
        if (!string.IsNullOrWhiteSpace(culture.Name))
        {
            yield return culture.Name;
        }

        if (!string.IsNullOrWhiteSpace(culture.TwoLetterISOLanguageName)
            && !string.Equals(culture.Name, culture.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase))
        {
            yield return culture.TwoLetterISOLanguageName;
        }
    }

    private static Assembly? TryLoadSatelliteAssembly(Assembly applicationAssembly, string cultureName)
    {
        try
        {
            return applicationAssembly.GetSatelliteAssembly(CultureInfo.GetCultureInfo(cultureName));
        }
        catch (FileNotFoundException)
        {
            var assemblyDirectory = Path.GetDirectoryName(applicationAssembly.Location);
            if (string.IsNullOrWhiteSpace(assemblyDirectory))
            {
                return null;
            }

            var satellitePath = Path.Combine(assemblyDirectory, cultureName, "Vehimap.Application.resources.dll");
            return File.Exists(satellitePath)
                ? AssemblyLoadContext.Default.LoadFromAssemblyPath(satellitePath)
                : null;
        }
    }
}
