namespace Vehimap.Desktop.Services;

public interface IFileDialogService
{
    Task<string?> PickOpenFileAsync(string title, string fileTypeName, string defaultExtension, CancellationToken cancellationToken = default);
    Task<string?> PickSaveFileAsync(string title, string suggestedFileName, string fileTypeName, string defaultExtension, CancellationToken cancellationToken = default);
}
