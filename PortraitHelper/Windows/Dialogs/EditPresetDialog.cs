using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using HaselCommon.Gui;
using HaselCommon.Services;
using Dalamud.Bindings.ImGui;
using PortraitHelper.Config;
using PortraitHelper.Records;
using PortraitHelper.Utils;

namespace PortraitHelper.Windows.Dialogs;

[RegisterSingleton, AutoConstruct]
public partial class EditPresetDialog
{
    private readonly TextService _textService;
    private readonly PluginConfig _pluginConfig;

    private bool _shouldOpen;
    private SavedPreset? _preset;
    private string _name;

    public void Open(SavedPreset preset)
    {
        _preset = preset;
        _name = preset.Name;
        _shouldOpen = true;
    }

    public void Draw()
    {
        if (_preset == null)
            return;

        var title = _textService.Translate("EditPresetDialog.Title");

        if (_shouldOpen)
        {
            ImGui.OpenPopup(title);
            _shouldOpen = false;
        }

        if (!ImGui.IsPopupOpen(title))
            return;

        // Always center this window when appearing
        var center = ImGui.GetMainViewport().GetCenter();
        ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new(0.5f, 0.5f));

        using var modal = ImRaiiExt.PopupModal(title, ImGuiWindowFlags.AlwaysAutoResize);
        if (!modal) return;

        ImGui.TextUnformatted(_textService.Translate("EditPresetDialog.Name.Label"));

        if (ImGui.IsWindowAppearing())
            ImGui.SetKeyboardFocusHere();

        var name = _name;
        if (ImGui.InputText("##PresetName", ref name, Constants.PresetNameMaxLength))
            _name = name;

        var disabled = string.IsNullOrWhiteSpace(_name);
        var shouldSave = !disabled && (ImGui.IsKeyPressed(ImGuiKey.Enter) || ImGui.IsKeyPressed(ImGuiKey.KeypadEnter));

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var combinedButtonWidths = ImGui.GetStyle().ItemSpacing.X
            + MathF.Max(Constants.DialogButtonMinWidth, ImGuiHelpers.GetButtonSize(_textService.Translate("ConfirmationButtonWindow.Save")).X)
            + MathF.Max(Constants.DialogButtonMinWidth, ImGuiHelpers.GetButtonSize(_textService.Translate("ConfirmationButtonWindow.Cancel")).X);

        ImGuiUtils.PushCursorX((ImGui.GetContentRegionAvail().X - combinedButtonWidths) / 2f);

        using (ImRaii.Disabled(disabled))
        {
            if (ImGui.Button(_textService.Translate("ConfirmationButtonWindow.Save"), new Vector2(120, 0)) || shouldSave)
            {
                _preset.Name = _name;
                _pluginConfig.Save();
                _preset = null;
                _name = string.Empty;
                ImGui.CloseCurrentPopup();
            }
        }

        ImGui.SetItemDefaultFocus();
        ImGui.SameLine();
        if (ImGui.Button(_textService.Translate("ConfirmationButtonWindow.Cancel"), new Vector2(120, 0)))
        {
            _preset = null;
            ImGui.CloseCurrentPopup();
        }
    }
}
