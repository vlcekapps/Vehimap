using Vehimap.Application;

namespace Vehimap.Desktop.ViewModels;

public sealed class UpdateDialogViewModel
{
    public UpdateDialogViewModel(UpdateCheckResult result)
    {
        Result = result;
        Heading = result.FailureReason is not null
            ? "Kontrola aktualizací se nepodařila"
            : result.IsUpdateAvailable
                ? "Je dostupná novější verze"
                : "Kontrola aktualizací";
        Summary = result.FailureReason ?? result.Message;
        Details = BuildDetails(result);
        PrimaryActionLabel = result.IsUpdateAvailable
            ? result.CanInstallAutomatically
                ? "Stáhnout a nainstalovat"
                : !string.IsNullOrWhiteSpace(result.NotesUrl)
                    ? "Otevřít release stránku"
                    : "Stáhnout asset"
            : "Zavřít";
    }

    public UpdateCheckResult Result { get; }

    public string Heading { get; }

    public string Summary { get; }

    public string Details { get; }

    public string PrimaryActionLabel { get; }

    public bool ShowPrimaryAction => Result.IsUpdateAvailable && (Result.CanInstallAutomatically || !string.IsNullOrWhiteSpace(Result.NotesUrl) || !string.IsNullOrWhiteSpace(Result.AssetUrl));

    public bool ShowSecondaryAssetAction => Result.IsUpdateAvailable && !Result.CanInstallAutomatically && !string.IsNullOrWhiteSpace(Result.AssetUrl) && !string.IsNullOrWhiteSpace(Result.NotesUrl);

    private static string BuildDetails(UpdateCheckResult result)
    {
        var lines = new List<string>
        {
            $"Aktuální verze: {result.CurrentVersion}",
            $"Nejnovější verze: {result.LatestVersion}"
        };

        if (!string.IsNullOrWhiteSpace(result.PublishedAt))
        {
            lines.Add($"Vydáno: {result.PublishedAt}");
        }

        if (result.AssetSize is > 0)
        {
            lines.Add($"Velikost assetu: {FormatBytes(result.AssetSize.Value)}");
        }

        if (!string.IsNullOrWhiteSpace(result.NotesUrl))
        {
            lines.Add($"Release poznámky: {result.NotesUrl}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string FormatBytes(long sizeBytes)
    {
        if (sizeBytes < 1024)
        {
            return $"{sizeBytes} B";
        }

        var sizeKb = sizeBytes / 1024d;
        if (sizeKb < 1024)
        {
            return $"{sizeKb:0.0} KB";
        }

        var sizeMb = sizeKb / 1024d;
        if (sizeMb < 1024)
        {
            return $"{sizeMb:0.0} MB";
        }

        var sizeGb = sizeMb / 1024d;
        return $"{sizeGb:0.00} GB";
    }
}
