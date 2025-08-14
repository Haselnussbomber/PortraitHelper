using PortraitHelper.Components;
using PortraitHelper.Windows.Overlays;

namespace PortraitHelper.Windows.MenuBarButtons;

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
