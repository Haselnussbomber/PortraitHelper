using PortraitHelper.Windows.BannerEditor.Dialogs;
using PortraitHelper.Components;
using PortraitHelper.Records;
using PortraitHelper.Services.BannerEditor;

namespace PortraitHelper.Windows.BannerEditor.MenuBarButtons;

[RegisterSingleton, AutoConstruct]
public unsafe partial class SaveAsPresetButton : MenuBarButton
{
    private readonly BannerMenuBarState _state;
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
        _createPresetDialog.Open(_state.PortraitName, Records.BannerPreset.FromState(), _bannerService.GetCurrentCharaViewImage());
    }
}
