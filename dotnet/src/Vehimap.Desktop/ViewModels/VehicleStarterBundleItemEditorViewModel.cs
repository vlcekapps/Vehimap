using CommunityToolkit.Mvvm.ComponentModel;
using Vehimap.Application.Models;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class VehicleStarterBundleItemEditorViewModel : ObservableObject
{
    public VehicleStarterBundleItemEditorViewModel(VehicleStarterBundleTemplate template)
    {
        Section = template.Section;
        SectionLabel = template.SectionLabel;
        Title = template.Title;
        IntervalKm = template.IntervalKm;
        IntervalMonths = template.IntervalMonths;
        RecordType = template.RecordType;
        Provider = template.Provider;
        ValidFrom = template.ValidFrom;
        ValidTo = template.ValidTo;
        Price = template.Price;
        DueDate = template.DueDate;
        ReminderDays = template.ReminderDays;
        RepeatMode = template.RepeatMode;
        Note = template.Note;
    }

    public VehicleStarterBundleSection Section { get; }

    public string SectionLabel { get; }

    public bool IsMaintenance => Section == VehicleStarterBundleSection.Maintenance;

    public bool IsRecord => Section == VehicleStarterBundleSection.Record;

    public bool IsReminder => Section == VehicleStarterBundleSection.Reminder;

    public string AccessibleLabel => $"{SectionLabel}: {Title}";

    [ObservableProperty]
    private bool isSelected = true;

    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private string intervalKm;

    [ObservableProperty]
    private string intervalMonths;

    [ObservableProperty]
    private string recordType;

    [ObservableProperty]
    private string provider;

    [ObservableProperty]
    private string validFrom;

    [ObservableProperty]
    private string validTo;

    [ObservableProperty]
    private string price;

    [ObservableProperty]
    private string dueDate;

    [ObservableProperty]
    private string reminderDays;

    [ObservableProperty]
    private string repeatMode;

    [ObservableProperty]
    private string note;

    public VehicleStarterBundleTemplate ToTemplate() =>
        new(
            Section,
            SectionLabel,
            Title.Trim(),
            IntervalKm.Trim(),
            IntervalMonths.Trim(),
            RecordType.Trim(),
            Provider.Trim(),
            ValidFrom.Trim(),
            ValidTo.Trim(),
            Price.Trim(),
            DueDate.Trim(),
            ReminderDays.Trim(),
            RepeatMode.Trim(),
            Note.Trim());
}
