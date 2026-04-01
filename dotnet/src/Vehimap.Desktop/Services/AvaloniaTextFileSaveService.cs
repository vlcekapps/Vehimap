using System.Text;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

namespace Vehimap.Desktop.Services;

public sealed class AvaloniaTextFileSaveService : ITextFileSaveService
{
    public async Task<string?> SaveTextAsync(string title, string suggestedFileName, string content, CancellationToken cancellationToken = default)
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime { MainWindow: { } mainWindow })
        {
            return null;
        }

        var file = await mainWindow.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            SuggestedFileName = suggestedFileName,
            DefaultExtension = "ics",
            FileTypeChoices =
            [
                new FilePickerFileType("iCalendar")
                {
                    Patterns = ["*.ics"]
                }
            ]
        }).ConfigureAwait(true);

        if (file is null)
        {
            return null;
        }

        await using var stream = await file.OpenWriteAsync().ConfigureAwait(true);
        await using var writer = new StreamWriter(stream, new UTF8Encoding(false));
        await writer.WriteAsync(content.AsMemory(), cancellationToken).ConfigureAwait(true);
        await writer.FlushAsync(cancellationToken).ConfigureAwait(true);
        return file.Path.LocalPath;
    }
}
