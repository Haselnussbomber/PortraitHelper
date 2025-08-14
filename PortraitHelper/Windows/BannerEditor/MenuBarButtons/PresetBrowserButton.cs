using PortraitHelper.Components;
using PortraitHelper.Windows.BannerEditor.Overlays;

namespace PortraitHelper.Windows.BannerEditor.MenuBarButtons;

[RegisterSingleton, AutoConstruct]
public partial class PresetBrowserButton : MenuBarOverlayButton<PresetBrowserOverlay>
{
    private readonly TextService _textService;

    [AutoPostConstruct]
    private void Initialize()
    {
        Key = "TogglePresetBrowser";
        Icon = FontAwesomeIcon.List;
        TooltipText = _textService.Translate("MenuBar.TogglePresetBrowser.Label");
    }
}
