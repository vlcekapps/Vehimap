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
                : $", poskytovatel {Provider}";
            var notePart = string.IsNullOrWhiteSpace(Note)
                ? string.Empty
                : $", poznámka {Note}";

            return $"{Title}, {RecordType}{providerPart}, platnost {Validity}, režim {AttachmentMode}, stav přílohy {AttachmentState}{notePart}";
        }
    }

    public override string ToString() => AccessibleLabel;
}
