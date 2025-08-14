using PortraitHelper.Components;
using PortraitHelper.Services.BannerEditor;

namespace PortraitHelper.Windows.BannerEditor.MenuBarButtons;

[RegisterSingleton, AutoConstruct]
public unsafe partial class PasteButton : MenuBarButton
{
    private readonly BannerMenuBarState _state;
    private readonly TextService _textService;
    private readonly ClipboardService _clipboardService;
    private readonly BannerService _bannerService;

    [AutoPostConstruct]
    private void Initialize()
    {
        Key = "Paste";
        Icon = FontAwesomeIcon.Paste;
        TooltipText = _textService.Translate("MenuBar.ImportFromClipboard.Label");
    }

    public override bool IsDisabled => _clipboardService.ClipboardPreset == null;

    public override void OnClick()
    {
        _bannerService.ImportPresetToState(_clipboardService.ClipboardPreset!);
        _state.CloseOverlay();
    }
}
