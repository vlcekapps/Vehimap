using System.Globalization;
using System.Reflection;
using System.Resources;
using Vehimap.Application.Abstractions;

namespace Vehimap.Application.Services;

public sealed class ResourceAppLocalizer : IAppLocalizer
{
    private static readonly ResourceManager ResourceManager = new(
        "Vehimap.Resources.Strings",
        typeof(ResourceAppLocalizer).GetTypeInfo().Assembly);

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

        return ResourceManager.GetString(key, _culture)
            ?? ResourceManager.GetString(key, CultureInfo.GetCultureInfo(AppCultureService.EnglishLanguage))
            ?? key;
    }

    public string Format(string key, params object?[] args) =>
        string.Format(_culture, GetString(key), args);
}
