// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Mobile.ViewModels;

public enum MobileVehicleEvidenceKind
{
    None,
    History,
    Fuel,
    Records,
    Reminders
}

public sealed record MobileEvidenceDetailLineViewModel(
    string AutomationId,
    string Text);

public sealed class MobileVehicleEvidenceItemViewModel
{
    public MobileVehicleEvidenceItemViewModel(
        string id,
        string automationIdPrefix,
        string primaryText,
        string secondaryText,
        string tertiaryText,
        string accessibleLabel,
        string itemType,
        string itemStatus,
        IReadOnlyList<MobileEvidenceDetailLineViewModel> detailLines)
    {
        Id = id;
        AutomationId = $"{automationIdPrefix}_{SanitizeAutomationId(id)}";
        PrimaryText = primaryText;
        SecondaryText = secondaryText;
        TertiaryText = tertiaryText;
        AccessibleLabel = accessibleLabel;
        ItemType = itemType;
        ItemStatus = itemStatus;
        DetailLines = detailLines;
    }

    public string Id { get; }

    public string AutomationId { get; }

    public string PrimaryText { get; }

    public string SecondaryText { get; }

    public string TertiaryText { get; }

    public string AccessibleLabel { get; }

    public string ItemType { get; }

    public string ItemStatus { get; }

    public IReadOnlyList<MobileEvidenceDetailLineViewModel> DetailLines { get; }

    public override string ToString() => AccessibleLabel;

    private static string SanitizeAutomationId(string value)
    {
        var characters = value.Where(character => char.IsLetterOrDigit(character) || character == '_').ToArray();
        return characters.Length == 0 ? "Unknown" : new string(characters);
    }
}
