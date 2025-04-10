using HaselCommon.Services;
using ImGuiNET;
using PortraitHelper.Config;
using PortraitHelper.ImGuiComponents;
using PortraitHelper.Records;

namespace PortraitHelper.Windows.Dialogs;

[RegisterScoped, AutoConstruct]
public partial class EditPresetDialog : ConfirmationDialog
{
    private readonly PluginConfig _pluginConfig;
    private readonly TextService _textService;

    private ConfirmationButton _saveButton;
    private string? _name;
    private SavedPreset? _preset;

    [AutoPostConstruct]
    private void Initialize()
    {
        WindowName = _textService.Translate("PortraitHelperWindows.EditPresetDialog.Title");

        AddButton(_saveButton = new ConfirmationButton(_textService.Translate("ConfirmationButtonWindow.Save"), OnSave));
        AddButton(new ConfirmationButton(_textService.Translate("ConfirmationButtonWindow.Cancel"), Close));
    }

    public void Open(SavedPreset preset)
    {
        _preset = preset;
        _name = preset.Name;
        Show();
    }

    public void Close()
    {
        Hide();
        _name = null;
        _preset = null;
    }

    public override bool DrawCondition()
        => base.DrawCondition() && _name != null && _preset != null;

    public override void InnerDraw()
    {
        ImGui.TextUnformatted(_textService.Translate("PortraitHelperWindows.EditPresetDialog.Name.Label"));

        ImGui.Spacing();

        ImGui.InputText("##PresetName", ref _name, 30);

        var disabled = string.IsNullOrEmpty(_name.Trim());
        if (!disabled && (ImGui.IsKeyPressed(ImGuiKey.Enter) || ImGui.IsKeyPressed(ImGuiKey.KeypadEnter)))
        {
            OnSave();
        }

        _saveButton.Disabled = disabled;
    }

    private void OnSave()
    {
        if (_preset == null || string.IsNullOrEmpty(_name?.Trim()))
        {
            Close();
            return;
        }

        _preset.Name = _name.Trim();

        _pluginConfig.Save();

        Close();
    }
}
