using PortraitHelper.Components;
using PortraitHelper.Config;
using PortraitHelper.Windows.BannerEditor.Overlays;

namespace PortraitHelper.Windows.BannerEditor.MenuBarButtons;

[RegisterSingleton, AutoConstruct]
public partial class AlignmentToolButton : MenuBarOverlayButton<AlignmentToolSettingsOverlay>
{
    private readonly TextService _textService;
    private readonly PluginConfig _pluginConfig;

    [AutoPostConstruct]
    private void Initialize()
    {
        Key = "ToggleAlignmentTool";
        Icon = FontAwesomeIcon.Hashtag;
        TooltipText = _textService.Translate("MenuBar.ToggleAlignmentTool.Label");
    }

    public override void OnClick()
    {
        if (ImGui.IsKeyDown(ImGuiKey.LeftShift) || ImGui.IsKeyDown(ImGuiKey.RightShift))
        {
            base.OnClick();
        }
        else
        {
            _pluginConfig.ShowAlignmentTool = !_pluginConfig.ShowAlignmentTool;
            _pluginConfig.Save();
        }
    }
}
