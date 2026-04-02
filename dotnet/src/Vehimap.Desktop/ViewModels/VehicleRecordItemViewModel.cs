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
    public string AccessibleLabel =>
        $"{Title}, {RecordType}, platnost {Validity}, režim {AttachmentMode}, stav přílohy {AttachmentState}, poznámka {Note}";

    public override string ToString() => AccessibleLabel;
}
