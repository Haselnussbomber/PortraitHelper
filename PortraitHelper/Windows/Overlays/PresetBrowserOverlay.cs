using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using HaselCommon.Extensions.Collections;
using HaselCommon.Gui;
using HaselCommon.Services;
using ImGuiNET;
using PortraitHelper.Config;
using PortraitHelper.Windows.Dialogs;

namespace PortraitHelper.Windows.Overlays;

[RegisterScoped, AutoConstruct]
public unsafe partial class PresetBrowserOverlay : Overlay
{
    private readonly TextService _textService;
    private readonly PluginConfig _pluginConfig;

    public Dictionary<Guid, PresetCard> PresetCards { get; init; } = [];

    public MenuBar MenuBar { get; internal set; } = null!;

    public DeletePresetDialog DeletePresetDialog { get; init; }
    public EditPresetDialog EditPresetDialog { get; init; }

    [AutoPostConstruct]
    private void Initialize()
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(600, 500),
            MaximumSize = new Vector2(4069),
        };
    }

    public override void OnClose()
    {
        PresetCards.Dispose();
        base.OnClose();
    }

    public void Open(MenuBar menuBar)
    {
        MenuBar = menuBar;
        Open();
    }

    public override void Draw()
    {
        base.Draw();

        DrawPresetBrowserContent();

        DeletePresetDialog.Draw();
        EditPresetDialog.Draw();
    }

    private void DrawPresetBrowserContent()
    {
        using var framePadding = ImRaii.PushStyle(ImGuiStyleVar.FramePadding, Vector2.Zero);
        using var child = ImRaii.Child("PresetBrowser_Content");
        if (!child) return;
        framePadding?.Dispose();

        ImGuiUtils.DrawSection(
            _textService.Translate("PortraitHelperWindows.PresetBrowserOverlay.Sidebar.Presets.Title"),
            pushDown: false,
            respectUiTheme: !IsWindow);

        var style = ImGui.GetStyle();
        ImGuiUtils.PushCursorY(style.ItemSpacing.Y);

        using var framePaddingChild = ImRaii.PushStyle(ImGuiStyleVar.FramePadding, Vector2.Zero);
        using var presetCardsChild = ImRaii.Child("PresetBrowser_Content_PresetCards");
        if (!presetCardsChild) return;
        framePaddingChild?.Dispose();

        using var indentSpacing = ImRaii.PushStyle(ImGuiStyleVar.IndentSpacing, style.ItemSpacing.X);
        using var indent = ImRaii.PushIndent();

        var presetCards = _pluginConfig.Presets
            .Select((preset) =>
            {
                if (!PresetCards.TryGetValue(preset.Id, out var card))
                {
                    var presetCard = new PresetCard(preset);
                    PresetCards.Add(preset.Id, presetCard);
                }

                return card;
            })
            .ToArray();

        var presetsPerRow = 3;
        var availableWidth = ImGui.GetContentRegionAvail().X - style.ItemInnerSpacing.X * presetsPerRow;

        var presetWidth = availableWidth / presetsPerRow;
        var scale = presetWidth / PresetCard.PortraitSize.X;

        ImGuiListClipperPtr clipper;
        unsafe
        {
            clipper = new ImGuiListClipperPtr(ImGuiNative.ImGuiListClipper_ImGuiListClipper());
        }

        clipper.Begin((int)Math.Ceiling(presetCards.Length / (float)presetsPerRow), PresetCard.PortraitSize.Y * scale);
        while (clipper.Step())
        {
            for (var row = clipper.DisplayStart; row < clipper.DisplayEnd; row++)
            {
                using (ImRaii.Group())
                {
                    for (int i = 0, index = row * presetsPerRow; i < presetsPerRow && index < presetCards.Length; i++, index++)
                    {
                        presetCards[index]?.Draw(this, scale, DefaultImGuiTextColor);

                        if (i < presetsPerRow - 1 && index + 1 < presetCards.Length)
                            ImGui.SameLine(0, style.ItemInnerSpacing.X);
                    }
                }
            }
        }
        clipper.Destroy();
    }
}
