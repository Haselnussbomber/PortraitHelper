using PortraitHelper.Config;
using PortraitHelper.Enums;

namespace PortraitHelper.Windows.Overlays;

[RegisterTransient, AutoConstruct]
public unsafe partial class AlignmentToolSettingsOverlay : Overlay
{
    private readonly TextService _textService;
    private readonly PluginConfig _pluginConfig;

    public override OverlayType Type => OverlayType.LeftPane;

    public override void Draw()
    {
        base.Draw();

        ImGuiUtils.DrawSection(
            _textService.Translate("AlignmentToolSettingsOverlay.Title.Inner"),
            pushDown: false,
            respectUiTheme: !IsWindow);

        var changed = false;

        changed |= ImGui.Checkbox(_textService.Translate("AlignmentToolSettingsOverlay.ShowAlignmentTool.Label"), ref _pluginConfig.ShowAlignmentTool);

        using var _ = ImRaii.Disabled(!_pluginConfig.ShowAlignmentTool);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.TextUnformatted(_textService.Translate("AlignmentToolSettingsOverlay.VerticalLines.Label"));
        ImGui.Indent();

        changed |= ImGui.SliderInt("##Vertical Lines", ref _pluginConfig.AlignmentToolVerticalLines, 0, 10);
        var vec4Vertical = (Vector4)_pluginConfig.AlignmentToolVerticalColor;
        changed |= ImGui.ColorEdit4("##Vertical Color", ref vec4Vertical);

        ImGui.Unindent();
        ImGui.TextUnformatted(_textService.Translate("AlignmentToolSettingsOverlay.HorizontalLines.Label"));
        ImGui.Indent();

        changed |= ImGui.SliderInt("##Horizontal Lines", ref _pluginConfig.AlignmentToolHorizontalLines, 0, 10);
        var vec4Horizontal = (Vector4)_pluginConfig.AlignmentToolHorizontalColor;
        changed |= ImGui.ColorEdit4("##Horizontal Color", ref vec4Horizontal);

        ImGui.Unindent();

        if (changed)
        {
            _pluginConfig.AlignmentToolVerticalColor = Color.FromVector4(vec4Vertical);
            _pluginConfig.AlignmentToolHorizontalColor = Color.FromVector4(vec4Horizontal);
            _pluginConfig.Save();
        }
    }
}
