using PortraitHelper.Components;
using PortraitHelper.Config;
using PortraitHelper.Services;
using PortraitHelper.Windows.Overlays;

namespace PortraitHelper.Windows.MenuBarButtons;

[RegisterSingleton, AutoConstruct]
public partial class AlignmentToolButton : MenuBarOverlayButton<AlignmentToolSettingsOverlay>
{
    private readonly TextService _textService;
    private readonly PluginConfig _pluginConfig;
    private readonly MenuBarState _state;

    [AutoPostConstruct]
    private void Initialize()
    {
        Key = "ToggleAlignmentTool";
        Icon = FontAwesomeIcon.Hashtag;
        TooltipText = _textService.Translate("MenuBar.ToggleAlignmentTool.Label");
    }

    public override bool IsActive => _pluginConfig.ShowAlignmentTool;

    public override void OnClick()
    {
        if (ImGui.IsKeyDown(ImGuiKey.LeftShift) || ImGui.IsKeyDown(ImGuiKey.RightShift))
        {
            if (_state.Overlay is AlignmentToolSettingsOverlay)
            {
                _state.CloseOverlay();
            }
            else
            {
                _state.OpenOverlay<AlignmentToolSettingsOverlay>();
            }
        }
        else
        {
            _pluginConfig.ShowAlignmentTool = !_pluginConfig.ShowAlignmentTool;
            _pluginConfig.Save();
        }
    }
}
