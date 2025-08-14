using PortraitHelper.Components;
using PortraitHelper.Services.BannerEditor;
using PortraitHelper.Windows.BannerEditor.Overlays;

namespace PortraitHelper.Windows.BannerEditor.MenuBarButtons;

[RegisterSingleton, AutoConstruct]
public partial class AdvancedImportButton : MenuBarOverlayButton<AdvancedImportOverlay>
{
    private readonly TextService _textService;
    private readonly ClipboardService _clipboardService;

    [AutoPostConstruct]
    private void Initialize()
    {
        Key = "ToggleAdvancedImportMode";
        Icon = FontAwesomeIcon.FileImport;
        TooltipText = _textService.Translate("MenuBar.ToggleAdvancedImportMode.Label");
    }

    public override bool IsDisabled => _clipboardService.ClipboardPreset == null;
}
