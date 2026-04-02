using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

namespace Vehimap.Desktop.Services;

public sealed class AvaloniaFileDialogService : IFileDialogService
{
    public async Task<string?> PickOpenFileAsync(string title, string fileTypeName, string defaultExtension, CancellationToken cancellationToken = default)
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime { MainWindow: { } mainWindow })
        {
            return null;
        }

        var files = await mainWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType(fileTypeName)
                {
                    Patterns = [$"*.{defaultExtension.TrimStart('.')}"]
                }
            ]
        }).ConfigureAwait(true);

        return files.Count == 0 ? null : files[0].Path.LocalPath;
    }

    public async Task<string?> PickSaveFileAsync(string title, string suggestedFileName, string fileTypeName, string defaultExtension, CancellationToken cancellationToken = default)
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime { MainWindow: { } mainWindow })
        {
            return null;
        }

        var file = await mainWindow.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            SuggestedFileName = suggestedFileName,
            DefaultExtension = defaultExtension.TrimStart('.'),
            FileTypeChoices =
            [
                new FilePickerFileType(fileTypeName)
                {
                    Patterns = [$"*.{defaultExtension.TrimStart('.')}"]
                }
            ]
        }).ConfigureAwait(true);

        return file?.Path.LocalPath;
    }
}
