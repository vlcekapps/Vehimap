// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Desktop.Localization;

namespace Vehimap.Desktop.ViewModels;

public sealed record VehicleRecordItemViewModel(
    string Id,
    string RecordType,
    string Title,
    string Provider,
    string Validity,
    string Price,
    string AttachmentMode,
    string AttachmentState,
    string StoredPath,
    string ResolvedPath,
    bool FileExists,
    string Note)
{
    public string AccessibleLabel
    {
        get
        {
            var providerPart = string.IsNullOrWhiteSpace(Provider)
                ? string.Empty
                : DesktopLocalization.Localizer.Format("RecordItem.ProviderPart", Provider);
            var notePart = string.IsNullOrWhiteSpace(Note)
                ? string.Empty
                : DesktopLocalization.Localizer.Format("RecordItem.NotePart", Note);

            return DesktopLocalization.Localizer.Format(
                "RecordItem.AccessibleLabel",
                Title,
                RecordType,
                providerPart,
                Validity,
                AttachmentMode,
                AttachmentState,
                notePart);
        }
    }

    public override string ToString() => AccessibleLabel;
}
