namespace Vehimap.Application.Models;

public sealed record VehicleStarterBundleTemplate(
    VehicleStarterBundleSection Section,
    string SectionLabel,
    string Title,
    string IntervalKm,
    string IntervalMonths,
    string RecordType,
    string Provider,
    string ValidFrom,
    string ValidTo,
    string Price,
    string DueDate,
    string ReminderDays,
    string RepeatMode,
    string Note);
