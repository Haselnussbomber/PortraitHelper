using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace PortraitHelper.Windows.CharaCardEditor;

[RegisterSingleton, AutoConstruct]
public unsafe partial class MenuBar : SimpleWindow
{
    [AutoPostConstruct]
    private void Initialize()
    {
        Flags |= ImGuiWindowFlags.NoSavedSettings;
        Flags |= ImGuiWindowFlags.NoDecoration;
        Flags |= ImGuiWindowFlags.NoMove;

        DisableWindowSounds = true;
        RespectCloseHotkey = false;
    }

    public override void Draw()
    {
        UpdatePosition();
    }

    public void UpdatePosition()
    {
        var scale = ImGuiHelpers.GlobalScale;
        var scaledown = 1 / scale;
        var height = (ImGui.GetTextLineHeight() + ImGui.GetStyle().FramePadding.Y * 2 + ImGui.GetStyle().WindowPadding.Y * 2) * scaledown;

        var addon = GetAddon<AtkUnitBase>(AgentId.CharaCardDesignSetting);

        Position = new(
            addon->X + 4,
            addon->Y + 3 - height * scale
        );

        Size = new(
            (addon->GetScaledWidth(true) - 8) * scaledown,
            height
        );
    }
}
