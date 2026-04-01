namespace Vehimap.Desktop.Services;

public interface ITextFileSaveService
{
    Task<string?> SaveTextAsync(string title, string suggestedFileName, string content, CancellationToken cancellationToken = default);
}
