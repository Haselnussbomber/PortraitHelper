using PortraitHelper.Components;
using PortraitHelper.Records;
using PortraitHelper.Services;
using PortraitHelper.Windows.Dialogs;

namespace PortraitHelper.Windows.MenuBarButtons;

[RegisterSingleton, AutoConstruct]
public unsafe partial class SaveAsPresetButton : MenuBarButton
{
    private readonly MenuBarState _state;
    private readonly TextService _textService;
    private readonly BannerService _bannerService;
    private readonly CreatePresetDialog _createPresetDialog;

    [AutoPostConstruct]
    private void Initialize()
    {
        Key = "SaveAsPreset";
        Icon = FontAwesomeIcon.Download;
        TooltipText = _textService.Translate("MenuBar.SaveAsPreset.Label");
    }

    public override void OnClick()
    {
        _createPresetDialog.Open(_state.PortraitName, PortraitPreset.FromState(), _bannerService.GetCurrentCharaViewImage());
    }
}
