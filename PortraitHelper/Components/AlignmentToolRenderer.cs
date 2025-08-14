using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using PortraitHelper.Windows.BannerEditor.Overlays;
using PortraitHelper.Config;
using PortraitHelper.Services.BannerEditor;

namespace PortraitHelper.Components;

[RegisterSingleton, AutoConstruct]
public unsafe partial class AlignmentToolRenderer
{
    private readonly BannerMenuBarState _state;
    private readonly PluginConfig _pluginConfig;

    public void Draw()
    {
        if (!_pluginConfig.ShowAlignmentTool)
            return;

        if (ImGuiHelpers.GlobalScale <= 1 && _state.Overlay is AdvancedImportOverlay or PresetBrowserOverlay)
            return;

        if (!TryGetAddon<AddonBannerEditor>(AgentId.BannerEditor, out var addon))
            return;

        var rightPanel = addon->GetNodeById(107);
        var charaView = addon->GetNodeById(130);
        var scale = addon->Scale;

        var position = new Vector2(
            addon->X + rightPanel->X * scale,
            addon->Y + rightPanel->Y * scale
        );

        var size = new Vector2(
            charaView->GetWidth() * scale,
            charaView->GetHeight() * scale
        );

        ImGui.SetNextWindowPos(position);
        ImGui.SetNextWindowSize(size);

        ImGui.Begin("AlignmentTool", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoInputs);

        var drawList = ImGui.GetWindowDrawList();

        if (_pluginConfig.AlignmentToolVerticalLines > 0)
        {
            var x = size.X / (_pluginConfig.AlignmentToolVerticalLines + 1);

            for (var i = 1; i <= _pluginConfig.AlignmentToolVerticalLines + 1; i++)
            {
                drawList.AddLine(
                    position + new Vector2(i * x, 0),
                    position + new Vector2(i * x, size.Y),
                    ImGui.ColorConvertFloat4ToU32(_pluginConfig.AlignmentToolVerticalColor)
                );
            }
        }

        if (_pluginConfig.AlignmentToolHorizontalLines > 0)
        {
            var y = size.Y / (_pluginConfig.AlignmentToolHorizontalLines + 1);

            for (var i = 1; i <= _pluginConfig.AlignmentToolHorizontalLines + 1; i++)
            {
                drawList.AddLine(
                    position + new Vector2(0, i * y),
                    position + new Vector2(size.X, i * y),
                    ImGui.ColorConvertFloat4ToU32(_pluginConfig.AlignmentToolHorizontalColor)
                );
            }
        }

        ImGui.End();
    }
}
