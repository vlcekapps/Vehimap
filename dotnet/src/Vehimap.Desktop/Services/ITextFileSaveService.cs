namespace Vehimap.Desktop.Services;

public interface ITextFileSaveService
{
    Task<string?> SaveTextAsync(string title, string suggestedFileName, string content, CancellationToken cancellationToken = default);

    Task<string?> SaveTextAsync(
        string title,
        string suggestedFileName,
        string content,
        string fileTypeName,
        string defaultExtension,
        IReadOnlyList<string> patterns,
        CancellationToken cancellationToken = default)
    {
        return SaveTextAsync(title, suggestedFileName, content, cancellationToken);
    }
}
