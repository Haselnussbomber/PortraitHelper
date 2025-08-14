using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using PortraitHelper.Config;
using PortraitHelper.Records;
using PortraitHelper.Services;
using PortraitHelper.Windows.Dialogs;
using PortraitHelper.Windows.Overlays;

namespace PortraitHelper.Windows;

[RegisterSingleton, AutoConstruct]
public unsafe partial class MenuBar : SimpleWindow
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MenuBar> _logger;
    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly PluginConfig _pluginConfig;
    private readonly TextService _textService;
    private readonly BannerService _bannerService;
    private readonly ClipboardService _clipboardService;
    private readonly CreatePresetDialog _createPresetDialog;

    private IServiceScope? _serviceScope;
    private PortraitPreset? _initialPreset;
    private string _portraitName = string.Empty;

    private AdvancedImportOverlay? _advancedImportOverlay;
    private AdvancedEditOverlay? _advancedEditOverlay;
    private PresetBrowserOverlay? _presetBrowserOverlay;
    private AlignmentToolSettingsOverlay? _alignmentToolSettingsOverlay;

    [AutoPostConstruct]
    private void Initialize()
    {
        Flags |= ImGuiWindowFlags.NoSavedSettings;
        Flags |= ImGuiWindowFlags.NoDecoration;
        Flags |= ImGuiWindowFlags.NoMove;

        DisableWindowSounds = true;
        RespectCloseHotkey = false;
    }

    public override void OnOpen()
    {
        _serviceScope = _serviceProvider.CreateScope();
        base.OnOpen();
    }

    public override void OnClose()
    {
        _initialPreset = null;
        _portraitName = string.Empty;
        CloseOverlays();
        _advancedImportOverlay?.Dispose();
        _advancedEditOverlay?.Dispose();
        _presetBrowserOverlay?.Dispose();
        _alignmentToolSettingsOverlay?.Dispose();
        _serviceScope?.Dispose();
        _serviceScope = null;
        base.OnClose();
    }

    public void CloseOverlays()
    {
        _advancedImportOverlay?.Close();
        _advancedEditOverlay?.Close();
        _presetBrowserOverlay?.Close();
        _alignmentToolSettingsOverlay?.Close();
    }

    public override bool DrawConditions()
    {
        var agent = AgentBannerEditor.Instance();
        return agent->EditorState != null && agent->IsAddonReady();
    }

    public override void PreDraw()
    {
        var agent = AgentBannerEditor.Instance();

        if (_initialPreset != null || !agent->EditorState->CharaView->CharaViewPortraitCharacterLoaded)
            return;

        _initialPreset = PortraitPreset.FromState();

        if (agent->EditorState->OpenType == AgentBannerEditorState.EditorOpenType.AdventurerPlate)
        {
            _portraitName = _textService.GetAddonText(14761) ?? "Adventurer Plate";
        }
        else if (agent->EditorState->OpenerEnabledGearsetIndex > -1)
        {
            var actualGearsetId = RaptureGearsetModule.Instance()->ResolveIdFromEnabledIndex((byte)agent->EditorState->OpenerEnabledGearsetIndex);
            if (actualGearsetId > -1)
            {
                var gearset = RaptureGearsetModule.Instance()->GetGearset(actualGearsetId);
                if (gearset != null)
                    _portraitName = $"{_textService.GetAddonText(756) ?? "Gear Set"} #{gearset->Id + 1}: {gearset->NameString}";
            }
        }
    }

    public override void Draw()
    {
        if (_initialPreset == null)
        {
            ImGui.SetCursorPosY(ImGui.GetCursorPos().Y + 2);
            ImGui.TextUnformatted(_textService.Translate("MenuBar.Initializing"));
            UpdatePosition();
            return;
        }

        using var id = ImRaii.PushId("##PortraitHelper_PortraitHelper");

        var agent = AgentBannerEditor.Instance();

        if (!agent->EditorState->HasDataChanged)
        {
            ImGuiUtils.IconButton("Reset", FontAwesomeIcon.Undo, _textService.GetAddonText(4830) ?? "Reset", disabled: true);
        }
        else if (ImGuiUtils.IconButton("Reset", FontAwesomeIcon.Undo, _textService.GetAddonText(4830) ?? "Reset"))
        {
            _bannerService.ImportPresetToState(_initialPreset);
            agent->EditorState->SetHasChanged(false);
        }

        ImGui.SameLine();
        if (ImGuiUtils.IconButton("Copy", FontAwesomeIcon.Copy, _textService.Translate("MenuBar.ExportToClipboard.Label"))) // GetAddonText(100) ?? "Copy"
            Task.Run(() => _clipboardService.SetClipboardPortraitPreset(PortraitPreset.FromState()));

        ImGui.SameLine();
        if (_clipboardService.ClipboardPreset == null)
        {
            ImGuiUtils.IconButton("Paste", FontAwesomeIcon.Paste, _textService.Translate("MenuBar.ImportFromClipboard.Label"), disabled: true); // GetAddonText(101) ?? "Paste"
        }
        else
        {
            if (ImGuiUtils.IconButton("Paste", FontAwesomeIcon.Paste, _textService.Translate("MenuBar.ImportFromClipboardAllSettings.Label")))
            {
                _bannerService.ImportPresetToState(_clipboardService.ClipboardPreset);
                CloseOverlays();
            }
        }

        ImGui.SameLine();
        if (_clipboardService.ClipboardPreset == null)
        {
            ImGuiUtils.IconButton("ViewModeAdvancedImport", FontAwesomeIcon.FileImport, _textService.Translate("MenuBar.ToggleAdvancedImportMode.Label"), disabled: true);
        }
        else if (_advancedImportOverlay != null && _advancedImportOverlay.IsOpen)
        {
            using var Color = ImRaii.PushColor(ImGuiCol.Button, 0xFFE19942)
                                     .Push(ImGuiCol.ButtonActive, 0xFFB06C2B)
                                     .Push(ImGuiCol.ButtonHovered, 0xFFCE8231);

            if (ImGuiUtils.IconButton("ViewModeNormal", FontAwesomeIcon.FileImport, _textService.Translate("MenuBar.ToggleAdvancedImportMode.Label")))
            {
                _advancedImportOverlay?.Close();
                _advancedImportOverlay?.Dispose();
                _advancedImportOverlay = null;
            }
        }
        else if (ImGuiUtils.IconButton("ViewModeAdvancedImport", FontAwesomeIcon.FileImport, _textService.Translate("MenuBar.ToggleAdvancedImportMode.Label")))
        {
            CloseOverlays();
            _advancedImportOverlay ??= _serviceScope!.ServiceProvider.GetRequiredService<AdvancedImportOverlay>();
            _advancedImportOverlay.Open(this);
        }

        ImGui.SameLine();
        if (_advancedEditOverlay != null && _advancedEditOverlay.IsOpen)
        {
            using var Color = ImRaii.PushColor(ImGuiCol.Button, 0xFFE19942)
                                     .Push(ImGuiCol.ButtonActive, 0xFFB06C2B)
                                     .Push(ImGuiCol.ButtonHovered, 0xFFCE8231);

            if (ImGuiUtils.IconButton("ViewModeNormal", FontAwesomeIcon.FilePen, _textService.Translate("MenuBar.ToggleAdvancedEditMode.Label")))
            {
                _advancedEditOverlay?.Close();
                _advancedEditOverlay?.Dispose();
                _advancedEditOverlay = null;
            }
        }
        else if (ImGuiUtils.IconButton("ViewModeAdvancedEdit", FontAwesomeIcon.FilePen, _textService.Translate("MenuBar.ToggleAdvancedEditMode.Label")))
        {
            CloseOverlays();
            _advancedEditOverlay ??= _serviceScope!.ServiceProvider.GetRequiredService<AdvancedEditOverlay>();
            _advancedEditOverlay.Open();
        }

        // ----

        ImGuiUtils.VerticalSeparator();

        // ----

        ImGui.SameLine();
        if (ImGuiUtils.IconButton("SaveAsPreset", FontAwesomeIcon.Download, _textService.Translate("MenuBar.SaveAsPreset.Label")))
        {
            _createPresetDialog.Open(_portraitName, PortraitPreset.FromState(), _bannerService.GetCurrentCharaViewImage());
        }

        ImGui.SameLine();
        if (_presetBrowserOverlay != null && _presetBrowserOverlay.IsOpen)
        {
            using var Color = ImRaii.PushColor(ImGuiCol.Button, 0xFFE19942)
                                     .Push(ImGuiCol.ButtonActive, 0xFFB06C2B)
                                     .Push(ImGuiCol.ButtonHovered, 0xFFCE8231);

            if (ImGuiUtils.IconButton("ViewModeNormal2", FontAwesomeIcon.List, _textService.Translate("MenuBar.TogglePresetBrowser.Label")))
            {
                _presetBrowserOverlay?.Close();
                _presetBrowserOverlay?.Dispose();
                _presetBrowserOverlay = null;
            }
        }
        else if (ImGuiUtils.IconButton("ViewModePresetBrowser", FontAwesomeIcon.List, _textService.Translate("MenuBar.TogglePresetBrowser.Label")))
        {
            CloseOverlays();
            _presetBrowserOverlay ??= _serviceScope!.ServiceProvider.GetRequiredService<PresetBrowserOverlay>();
            _presetBrowserOverlay.Open(this);
        }

        // ----

        ImGuiUtils.VerticalSeparator();

        // ----

        ImGui.SameLine();
        if (_alignmentToolSettingsOverlay != null && _alignmentToolSettingsOverlay.IsOpen)
        {
            using var Color = ImRaii.PushColor(ImGuiCol.Button, 0xFFE19942)
                                     .Push(ImGuiCol.ButtonActive, 0xFFB06C2B)
                                     .Push(ImGuiCol.ButtonHovered, 0xFFCE8231);

            if (ImGuiUtils.IconButton("ToggleAlignmentToolOff", FontAwesomeIcon.Hashtag, _textService.Translate("MenuBar.ToggleAlignmentTool.Label.CloseSettings")))
            {
                if (ImGui.IsKeyDown(ImGuiKey.LeftShift) || ImGui.IsKeyDown(ImGuiKey.RightShift))
                {
                    _alignmentToolSettingsOverlay?.Close();
                    _alignmentToolSettingsOverlay?.Dispose();
                    _alignmentToolSettingsOverlay = null;
                }
                else
                {
                    _pluginConfig.ShowAlignmentTool = !_pluginConfig.ShowAlignmentTool;
                    _pluginConfig.Save();
                }
            }
        }
        else if (ImGuiUtils.IconButton("ToggleAlignmentToolOn", FontAwesomeIcon.Hashtag, _textService.Translate("MenuBar.ToggleAlignmentTool.Label.OpenSettings")))
        {
            if (ImGui.IsKeyDown(ImGuiKey.LeftShift) || ImGui.IsKeyDown(ImGuiKey.RightShift))
            {
                CloseOverlays();
                _alignmentToolSettingsOverlay ??= _serviceScope!.ServiceProvider.GetRequiredService<AlignmentToolSettingsOverlay>();
                _alignmentToolSettingsOverlay.Open();
            }
            else
            {
                _pluginConfig.ShowAlignmentTool = !_pluginConfig.ShowAlignmentTool;
                _pluginConfig.Save();
            }
        }

        if (!string.IsNullOrEmpty(_portraitName))
        {
            ImGuiUtils.VerticalSeparator();
            ImGui.SameLine();
            ImGui.TextUnformatted(_portraitName);
        }

        UpdatePosition();

        _createPresetDialog.Draw();
    }

    public void UpdatePosition()
    {
        var scale = ImGuiHelpers.GlobalScale;
        var scaledown = 1 / scale;
        var height = (ImGui.GetTextLineHeight() + ImGui.GetStyle().FramePadding.Y * 2 + ImGui.GetStyle().WindowPadding.Y * 2) * scaledown;

        var addon = GetAddon<AddonBannerEditor>(AgentId.BannerEditor);

        Position = new(
            addon->X + 4,
            addon->Y + 3 - height * scale
        );

        Size = new(
            (addon->GetScaledWidth(true) - 8) * scaledown,
            height
        );
    }

    public override void PostDraw()
    {
        if (IsOpen && DrawConditions() && _pluginConfig.ShowAlignmentTool)
        {
            var addon = GetAddon<AddonBannerEditor>(AgentId.BannerEditor);

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

            if (!(ImGuiHelpers.GlobalScale <= 1 && ((_advancedImportOverlay != null && _advancedImportOverlay.IsOpen) || (_presetBrowserOverlay != null && _presetBrowserOverlay.IsOpen))))
            {
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
    }
}
