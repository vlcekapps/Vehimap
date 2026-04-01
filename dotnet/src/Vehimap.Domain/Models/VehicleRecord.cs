using Vehimap.Domain.Enums;

namespace Vehimap.Domain.Models;

public sealed record VehicleRecord(
    string Id,
    string VehicleId,
    string RecordType,
    string Title,
    string Provider,
    string ValidFrom,
    string ValidTo,
    string Price,
    VehicleRecordAttachmentMode AttachmentMode,
    string FilePath,
    string Note);
