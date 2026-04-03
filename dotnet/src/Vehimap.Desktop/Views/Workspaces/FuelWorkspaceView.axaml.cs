using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Vehimap.Desktop.ViewModels;
using Vehimap.Desktop.ViewModels.Workspaces;

namespace Vehimap.Desktop.Views.Workspaces;

public partial class FuelWorkspaceView : WorkspaceViewBase<FuelWorkspaceViewModel>
{
    public FuelWorkspaceView()
    {
        AvaloniaXamlLoader.Load(this);
        RegisterShiftTabBackNavigation("FuelListBox", "FuelEditorDateBox");
        ApplyHostMode();
    }

    protected override DesktopFocusTarget? GetDefaultFocusTarget() =>
        ViewModel?.IsEditingFuel == true ? DesktopFocusTarget.FuelEditorDate : DesktopFocusTarget.FuelList;

    protected override bool SupportsFocusTarget(DesktopFocusTarget target) =>
        target is DesktopFocusTarget.FuelList or DesktopFocusTarget.FuelEditorDate;

    protected override Control? ResolveFocusTarget(DesktopFocusTarget target) =>
        target switch
        {
            DesktopFocusTarget.FuelList => this.FindControl<ListBox>("FuelListBox"),
            DesktopFocusTarget.FuelEditorDate => this.FindControl<TextBox>("FuelEditorDateBox"),
            _ => null
        };

    protected override void OnAllowEditingChanged()
    {
        ApplyHostMode();
    }

    private void ApplyHostMode()
    {
        if (this.FindControl<Control>("FuelActionPanel") is { } actionPanel)
        {
            actionPanel.IsVisible = AllowEditing;
        }

        if (this.FindControl<Control>("FuelEditorHost") is { } editorHost)
        {
            editorHost.IsVisible = AllowEditing;
        }
    }
}
