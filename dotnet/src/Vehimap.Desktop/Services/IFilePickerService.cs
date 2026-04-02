namespace Vehimap.Desktop.Services;

public interface IFilePickerService
{
    Task<string?> PickFileAsync(string title, CancellationToken cancellationToken = default);
}
