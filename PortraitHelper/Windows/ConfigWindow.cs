using System.Numerics;
using HaselCommon.Gui;
using HaselCommon.Services;
using ImGuiNET;
using PortraitHelper.Config;

namespace PortraitHelper.Windows;

[RegisterSingleton, AutoConstruct]
public unsafe partial class ConfigWindow : SimpleWindow
{
    private readonly PluginConfig _pluginConfig;
    private readonly TextService _textService;

    [AutoPostConstruct]
    private void Initialize()
    {
        AllowClickthrough = false;
        AllowPinning = false;

        Flags |= ImGuiWindowFlags.AlwaysAutoResize;

        Size = new Vector2(380, -1);
        SizeCondition = ImGuiCond.Appearing;
    }

    public override void Draw()
    {

    }
}
