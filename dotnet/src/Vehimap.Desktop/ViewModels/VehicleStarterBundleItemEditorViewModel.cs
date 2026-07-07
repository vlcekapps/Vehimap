// SPDX-License-Identifier: GPL-3.0-or-later
using CommunityToolkit.Mvvm.ComponentModel;
using System.Globalization;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Desktop.Localization;
using Vehimap.Storage.Legacy;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class VehicleStarterBundleItemEditorViewModel : ObservableObject
{
    private static readonly AppNumberFormatService NumberFormatService = new();
    private static readonly AppUnitFormatService UnitFormatService = new();

    private readonly AppCulturePreferences _culturePreferences;
    private readonly AppUnitPreferences _unitPreferences;

    public VehicleStarterBundleItemEditorViewModel(
        VehicleStarterBundleTemplate template,
        AppCulturePreferences? culturePreferences = null,
        AppUnitPreferences? unitPreferences = null)
    {
        _culturePreferences = culturePreferences ?? new AppCulturePreferences();
        _unitPreferences = UnitFormatService.Normalize(unitPreferences ?? new AppUnitPreferences());
        Section = template.Section;
        SectionLabel = template.SectionLabel;
        Title = template.Title;
        IntervalDistance = FormatCanonicalDistanceForEditor(template.IntervalKm);
        IntervalMonths = template.IntervalMonths;
        RecordType = template.Section == VehicleStarterBundleSection.Record
            ? KnownValueOptions.NormalizeRecordTypeValue(template.RecordType)
            : template.RecordType;
        Provider = template.Provider;
        ValidFrom = template.ValidFrom;
        ValidTo = template.ValidTo;
        Price = template.Price;
        DueDate = template.DueDate;
        ReminderDays = template.ReminderDays;
        RepeatMode = template.Section == VehicleStarterBundleSection.Reminder
            ? KnownValueOptions.NormalizeReminderRepeatModeValue(template.RepeatMode)
            : template.RepeatMode;
        Note = template.Note;
        Category = template.Category;
        Subcategory = template.Subcategory;
    }

    public VehicleStarterBundleSection Section { get; }

    public string SectionLabel { get; }

    public bool IsMaintenance => Section == VehicleStarterBundleSection.Maintenance;

    public bool IsRecord => Section == VehicleStarterBundleSection.Record;

    public bool IsReminder => Section == VehicleStarterBundleSection.Reminder;

    public string Category { get; }

    public string Subcategory { get; }

    public string AccessibleLabel
    {
        get
        {
            var category = Category.Trim();
            var subcategory = Subcategory.Trim();
            if (!string.IsNullOrWhiteSpace(category) && !string.IsNullOrWhiteSpace(subcategory))
            {
                return LF("VehicleStarterBundle.AccessibleLabel.Full", SectionLabel, category, subcategory, Title);
            }

            return string.IsNullOrWhiteSpace(category)
                ? LF("VehicleStarterBundle.AccessibleLabel.Simple", SectionLabel, Title)
                : LF("VehicleStarterBundle.AccessibleLabel.Category", SectionLabel, category, Title);
        }
    }

    [ObservableProperty]
    private bool isSelected = true;

    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private string intervalDistance;

    [ObservableProperty]
    private string intervalMonths;

    [ObservableProperty]
    private string recordType;

    public IReadOnlyList<LocalizedOptionViewModel> RecordTypeOptions => KnownValueOptions.RecordTypes(RecordType);

    public LocalizedOptionViewModel SelectedRecordTypeOption
    {
        get => KnownValueOptions.SelectRecordType(RecordType);
        set => RecordType = value?.Value ?? string.Empty;
    }

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

    public IReadOnlyList<LocalizedOptionViewModel> ReminderRepeatModeOptions => KnownValueOptions.ReminderRepeatModes(RepeatMode);

    public LocalizedOptionViewModel SelectedReminderRepeatModeOption
    {
        get => KnownValueOptions.SelectReminderRepeatMode(RepeatMode);
        set => RepeatMode = value?.Value ?? string.Empty;
    }

    [ObservableProperty]
    private string note;

    public override string ToString() => AccessibleLabel;

    partial void OnTitleChanged(string value) => OnPropertyChanged(nameof(AccessibleLabel));

    partial void OnRecordTypeChanged(string value)
    {
        OnPropertyChanged(nameof(SelectedRecordTypeOption));
        OnPropertyChanged(nameof(RecordTypeOptions));
    }

    partial void OnRepeatModeChanged(string value)
    {
        OnPropertyChanged(nameof(SelectedReminderRepeatModeOption));
        OnPropertyChanged(nameof(ReminderRepeatModeOptions));
    }

    public VehicleStarterBundleTemplate ToTemplate()
    {
        var recordType = IsRecord
            ? KnownValueOptions.NormalizeRecordTypeValue(RecordType)
            : RecordType.Trim();
        var repeatMode = IsReminder
            ? KnownValueOptions.NormalizeReminderRepeatModeValue(RepeatMode)
            : RepeatMode.Trim();
        var intervalKm = NormalizeEditorDistanceToKilometers(IntervalDistance);

        return new(
            Section,
            SectionLabel,
            Title.Trim(),
            intervalKm,
            IntervalMonths.Trim(),
            recordType,
            Provider.Trim(),
            ValidFrom.Trim(),
            ValidTo.Trim(),
            Price.Trim(),
            DueDate.Trim(),
            ReminderDays.Trim(),
            repeatMode,
            Note.Trim(),
            Category.Trim(),
            Subcategory.Trim());
    }

    private string FormatCanonicalDistanceForEditor(string? canonicalKilometers)
    {
        var value = (canonicalKilometers ?? string.Empty).Trim();
        if (value.Length == 0 || !VehimapValueParser.TryParseDecimalNumber(value, out var kilometers))
        {
            return value;
        }

        var decimalPlaces = string.Equals(AppUnitFormatService.NormalizeDistanceUnit(_unitPreferences.DistanceUnit), AppUnitFormatService.Miles, StringComparison.Ordinal)
            ? 1
            : 0;
        var distance = UnitFormatService.ConvertDistanceFromKilometers(kilometers, _unitPreferences);
        return NumberFormatService.FormatDecimal(distance, _culturePreferences, decimalPlaces);
    }

    private string NormalizeEditorDistanceToKilometers(string? text)
    {
        var value = (text ?? string.Empty).Trim();
        if (value.Length == 0)
        {
            return string.Empty;
        }

        if (!NumberFormatService.TryParseDecimal(value, _culturePreferences, out var distance)
            && !VehimapValueParser.TryParseDecimalNumber(value, out distance))
        {
            return value;
        }

        var kilometers = UnitFormatService.ConvertDistanceToKilometers(distance, _unitPreferences);
        return ((int)Math.Round(kilometers, MidpointRounding.AwayFromZero)).ToString(CultureInfo.InvariantCulture);
    }

    private static string LF(string key, params object?[] args) => DesktopLocalization.Localizer.Format(key, args);
}
