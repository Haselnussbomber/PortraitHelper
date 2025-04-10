using Dalamud.Interface.Utility.Raii;
using HaselCommon.Gui;
using HaselCommon.Services;
using ImGuiNET;
using PortraitHelper.Config;
using PortraitHelper.Enums;

namespace PortraitHelper.Windows.Overlays;

#pragma warning disable CS9107

[RegisterScoped]
public unsafe class AlignmentToolSettingsOverlay(
    WindowManager windowManager,
    TextService textService,
    LanguageProvider languageProvider,
    PluginConfig pluginConfig,
    ExcelService excelService)
    : Overlay(windowManager, textService, languageProvider, pluginConfig, excelService)
{
    public override OverlayType Type => OverlayType.LeftPane;

    public override void Draw()
    {
        base.Draw();

        ImGuiUtils.DrawSection(
            textService.Translate("PortraitHelperWindows.AlignmentToolSettingsOverlay.Title.Inner"),
            pushDown: false,
            respectUiTheme: !IsWindow);

        var changed = false;

        changed |= ImGui.Checkbox(textService.Translate("PortraitHelperWindows.AlignmentToolSettingsOverlay.ShowAlignmentTool.Label"), ref PluginConfig.ShowAlignmentTool);

        using var _ = ImRaii.Disabled(!PluginConfig.ShowAlignmentTool);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.TextUnformatted(textService.Translate("PortraitHelperWindows.AlignmentToolSettingsOverlay.VerticalLines.Label"));
        ImGui.Indent();

        changed |= ImGui.SliderInt("##Vertical Lines", ref PluginConfig.AlignmentToolVerticalLines, 0, 10);
        changed |= ImGui.ColorEdit4("##Vertical Color", ref PluginConfig.AlignmentToolVerticalColor);

        ImGui.Unindent();
        ImGui.TextUnformatted(textService.Translate("PortraitHelperWindows.AlignmentToolSettingsOverlay.HorizontalLines.Label"));
        ImGui.Indent();

        changed |= ImGui.SliderInt("##Horizontal Lines", ref PluginConfig.AlignmentToolHorizontalLines, 0, 10);
        changed |= ImGui.ColorEdit4("##Horizontal Color", ref PluginConfig.AlignmentToolHorizontalColor);

        ImGui.Unindent();

        if (changed)
        {
            PluginConfig.Save();
        }
    }
}
