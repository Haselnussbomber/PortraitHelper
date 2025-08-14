using PortraitHelper.Components;
using PortraitHelper.Services;
using PortraitHelper.Windows.Overlays;

namespace PortraitHelper.Windows.MenuBarButtons;

[RegisterSingleton, AutoConstruct]
public partial class AdvancedEditButton : MenuBarOverlayButton<AdvancedEditOverlay>
{
    private readonly TextService _textService;
    private readonly ClipboardService _clipboardService;

    [AutoPostConstruct]
    private void Initialize()
    {
        Key = "ToggleAdvancedEditMode";
        Icon = FontAwesomeIcon.FilePen;
        TooltipText = _textService.Translate("MenuBar.ToggleAdvancedEditMode.Label");
    }

    public override bool IsDisabled => _clipboardService.ClipboardPreset == null;
}
