using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Common.Math;
using PortraitHelper.Components;
using PortraitHelper.Enums;
using PortraitHelper.Services.BannerEditor;

namespace PortraitHelper.Windows.BannerEditor.Overlays;

[RegisterTransient, AutoConstruct]
public unsafe partial class AdvancedImportOverlay : Overlay
{
    private readonly BannerMenuBarState _state;
    private readonly TextService _textService;
    private readonly ExcelService _excelService;
    private readonly BannerService _bannerService;
    private readonly ClipboardService _clipboardService;

    public override void Draw()
    {
        base.Draw();

        if (_clipboardService.ClipboardPreset == null)
        {
            Close();
            return;
        }

        if (IsWindow)
            ImGuiUtils.PushCursorY(ImGui.GetStyle().ItemSpacing.Y * 2);

        var state = AgentBannerEditor.Instance()->EditorState;
        var unknown = _textService.GetAddonText(624) ?? "Unknown";

        if (ImGui.Button(_textService.GetAddonText(14923) ?? "Select All"))
            _clipboardService.CurrentImportFlags = BannerImportFlags.All;

        ImGui.SameLine();

        if (ImGui.Button(_textService.GetAddonText(14924) ?? "Deselect All"))
            _clipboardService.CurrentImportFlags = BannerImportFlags.None;

        ImGui.SameLine();

        if (ImGui.Button(_textService.Translate("AdvancedImportOverlay.ImportSelectedSettingsButton.Label")))
        {
            _bannerService.ImportPresetToState(_clipboardService.ClipboardPreset, _clipboardService.CurrentImportFlags);
            _state.CloseOverlay();
        }

        ImGuiUtils.DrawSection(_textService.GetAddonText(14684) ?? "Design", respectUiTheme: !IsWindow);

        var isBannerBgUnlocked = _bannerService.IsBannerBgUnlocked(_clipboardService.ClipboardPreset.BannerBg);
        DrawImportSetting(
            _textService.GetAddonText(14687) ?? "Background",
            BannerImportFlags.BannerBg,
            () =>
            {
                if (_excelService.TryGetRow<BannerBg>(_clipboardService.ClipboardPreset.BannerBg, out var bannerBgRow))
                    ImGui.TextUnformatted(bannerBgRow.Name.ExtractText());
                else
                    ImGui.TextUnformatted(unknown);

                if (!isBannerBgUnlocked)
                    ImGuiUtils.TextUnformattedColored(Color.Red, _textService.Translate("AdvancedImportOverlay.NotUnlocked"));
            },
            isBannerBgUnlocked
        );

        var isBannerFrameUnlocked = _bannerService.IsBannerFrameUnlocked(_clipboardService.ClipboardPreset.BannerFrame);
        DrawImportSetting(
            _textService.GetAddonText(14688) ?? "Frame",
            BannerImportFlags.BannerFrame,
            () =>
            {
                if (_excelService.TryGetRow<BannerFrame>(_clipboardService.ClipboardPreset.BannerFrame, out var bannerFrameRow))
                    ImGui.TextUnformatted(bannerFrameRow.Name.ExtractText());
                else
                    ImGui.TextUnformatted(unknown);

                if (!isBannerFrameUnlocked)
                    ImGuiUtils.TextUnformattedColored(Color.Red, _textService.Translate("AdvancedImportOverlay.NotUnlocked"));
            },
            isBannerFrameUnlocked
        );

        var isBannerDecorationUnlocked = _bannerService.IsBannerDecorationUnlocked(_clipboardService.ClipboardPreset.BannerDecoration);
        DrawImportSetting(
            _textService.GetAddonText(14689) ?? "Accent",
            BannerImportFlags.BannerDecoration,
            () =>
            {
                if (_excelService.TryGetRow<BannerDecoration>(_clipboardService.ClipboardPreset.BannerDecoration, out var bannerDecorationRow))
                    ImGui.TextUnformatted(bannerDecorationRow.Name.ExtractText());
                else
                    ImGui.TextUnformatted(unknown);

                if (!isBannerDecorationUnlocked)
                    ImGuiUtils.TextUnformattedColored(Color.Red, _textService.Translate("AdvancedImportOverlay.NotUnlocked"));
            },
            isBannerDecorationUnlocked
        );

        DrawImportSetting(
            _textService.GetAddonText(14711) ?? "Zoom",
            BannerImportFlags.CameraZoom,
            () => ImGui.TextUnformatted(_clipboardService.ClipboardPreset.CameraZoom.ToString())
        );

        DrawImportSetting(
            _textService.GetAddonText(14712) ?? "Rotation",
            BannerImportFlags.ImageRotation,
            () => ImGui.TextUnformatted(_clipboardService.ClipboardPreset.ImageRotation.ToString())
        );

        ImGuiUtils.DrawSection(_textService.GetAddonText(14685) ?? "Character", respectUiTheme: !IsWindow);

        var isBannerTimelineUnlocked = _bannerService.IsBannerTimelineUnlocked(_clipboardService.ClipboardPreset.BannerTimeline);
        DrawImportSetting(
            _textService.GetAddonText(14690) ?? "Pose",
            BannerImportFlags.BannerTimeline,
            () =>
            {
                ImGui.TextUnformatted(_bannerService.GetBannerTimelineName(_clipboardService.ClipboardPreset.BannerTimeline));

                if (!isBannerTimelineUnlocked)
                    ImGuiUtils.TextUnformattedColored(Color.Red, _textService.Translate("AdvancedImportOverlay.NotUnlocked"));
            },
            isBannerTimelineUnlocked
        );

        DrawImportSetting(
            _textService.GetAddonText(14691) ?? "Expression",
            BannerImportFlags.Expression,
            () =>
            {
                var id = _clipboardService.ClipboardPreset.Expression;
                var expressionName = unknown;

                if (id == 0)
                {
                    expressionName = _textService.GetAddonText(14727) ?? "None";
                }
                else
                {
                    for (var i = 0; i < state->Expressions.SortedEntriesCount; i++)
                    {
                        var entry = state->Expressions.SortedEntries[i];
                        if (entry->RowId == id
                        && entry->SupplementalRow != 0
                        && _excelService.TryGetRow<BannerFacial>(entry->RowId, out var bannerFacialRow)
                        && _excelService.TryGetRow<Emote>(bannerFacialRow.Emote.RowId, out var emoteRow))
                        {
                            expressionName = emoteRow.Name.ExtractText();
                            break;
                        }
                    }
                }

                ImGui.TextUnformatted(expressionName);
            }
        );

        DrawImportSetting(
            _textService.Translate("Setting.AnimationTimestamp.Label"),
            BannerImportFlags.AnimationProgress,
            () => ImGui.TextUnformatted(_textService.Translate("Setting.AnimationTimestamp.ValueFormat", _clipboardService.ClipboardPreset.AnimationProgress))
        );

        DrawImportSetting(
            _textService.GetAddonText(5972) ?? "Camera Position",
            BannerImportFlags.CameraPosition,
            () => DrawHalfVector4(_clipboardService.ClipboardPreset.CameraPosition)
        );

        DrawImportSetting(
            _textService.Translate("Setting.CameraTarget.Label"),
            BannerImportFlags.CameraTarget,
            () => DrawHalfVector4(_clipboardService.ClipboardPreset.CameraTarget)
        );

        DrawImportSetting(
            _textService.Translate("Setting.HeadDirection.Label"),
            BannerImportFlags.HeadDirection,
            () => DrawHalfVector2(_clipboardService.ClipboardPreset.HeadDirection)
        );

        DrawImportSetting(
            _textService.Translate("Setting.EyeDirection.Label"),
            BannerImportFlags.EyeDirection,
            () => DrawHalfVector2(_clipboardService.ClipboardPreset.EyeDirection)
        );

        ImGuiUtils.DrawSection(_textService.GetAddonText(14692) ?? "Ambient Lighting", respectUiTheme: !IsWindow);

        var labelBrightness = _textService.GetAddonText(14694) ?? "Brightness";
        var labelColor = _textService.GetAddonText(7008) ?? "Color";

        DrawImportSetting(
            labelBrightness,
            BannerImportFlags.AmbientLightingBrightness,
            () => ImGui.TextUnformatted(_clipboardService.ClipboardPreset.AmbientLightingBrightness.ToString())
        );

        DrawImportSetting(
            labelColor,
            BannerImportFlags.AmbientLightingColor,
            () => DrawColor(
                _clipboardService.ClipboardPreset.AmbientLightingColorRed,
                _clipboardService.ClipboardPreset.AmbientLightingColorGreen,
                _clipboardService.ClipboardPreset.AmbientLightingColorBlue
            )
        );

        ImGuiUtils.DrawSection(_textService.GetAddonText(14693) ?? "Directional Lighting", respectUiTheme: !IsWindow);

        DrawImportSetting(
            labelBrightness,
            BannerImportFlags.DirectionalLightingBrightness,
            () => ImGui.TextUnformatted(_clipboardService.ClipboardPreset.DirectionalLightingBrightness.ToString())
        );

        DrawImportSetting(
            labelColor,
            BannerImportFlags.DirectionalLightingColor,
            () => DrawColor(
                _clipboardService.ClipboardPreset.DirectionalLightingColorRed,
                _clipboardService.ClipboardPreset.DirectionalLightingColorGreen,
                _clipboardService.ClipboardPreset.DirectionalLightingColorBlue
            )
        );

        DrawImportSetting(
            _textService.GetAddonText(14696) ?? "Vertical Angle",
            BannerImportFlags.DirectionalLightingVerticalAngle,
            () => ImGui.TextUnformatted(_clipboardService.ClipboardPreset.DirectionalLightingVerticalAngle.ToString())
        );

        DrawImportSetting(
            _textService.GetAddonText(14695) ?? "Horizontal Angle",
            BannerImportFlags.DirectionalLightingHorizontalAngle,
            () => ImGui.TextUnformatted(_clipboardService.ClipboardPreset.DirectionalLightingHorizontalAngle.ToString())
        );

        if (IsWindow)
            ImGuiUtils.PushCursorY(ImGui.GetStyle().ItemSpacing.Y);
    }

    private void DrawImportSetting(string label, BannerImportFlags flag, System.Action drawFn, bool isUnlocked = true)
    {
        using var id = ImRaii.PushId(flag.ToString());

        ImGui.Columns(2, "##Columns", false);

        var isEnabled = isUnlocked && _clipboardService.CurrentImportFlags.HasFlag(flag);
        using var _textColor = !isEnabled ? (Color.From(ImGuiCol.Text) with { A = 0.5f }).Push(ImGuiCol.Text) : null;
        using var _disabled = ImRaii.Disabled(!isUnlocked);

        if (ImGui.Checkbox(label + "##Checkbox", ref isEnabled))
        {
            if (isEnabled)
                _clipboardService.CurrentImportFlags |= flag;
            else
                _clipboardService.CurrentImportFlags &= ~flag;
        }

        _disabled?.Dispose();
        _textColor?.Dispose();

        using (ImRaii.Disabled(!isEnabled))
        {
            ImGui.NextColumn();
            drawFn();
        }

        ImGui.Columns(1);
    }

    private void DrawColor(byte r, byte g, byte b)
    {
        var vec = new System.Numerics.Vector3(r / 255f, g / 255f, b / 255f);
        using var table = ImRaii.Table("##Table", 4);
        if (!table)
            return;

        var scale = ImGuiHelpers.GlobalScale;
        ImGui.TableSetupColumn("Preview", ImGuiTableColumnFlags.WidthFixed, 26 * scale);
        ImGui.TableSetupColumn("R", ImGuiTableColumnFlags.WidthFixed, 40 * scale);
        ImGui.TableSetupColumn("G", ImGuiTableColumnFlags.WidthFixed, 40 * scale);
        ImGui.TableSetupColumn("B", ImGuiTableColumnFlags.WidthFixed, 40 * scale);

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.ColorEdit3("##ColorEdit3", ref vec, ImGuiColorEditFlags.NoPicker | ImGuiColorEditFlags.NoOptions | ImGuiColorEditFlags.NoLabel | ImGuiColorEditFlags.NoInputs);

        void drawColumn(string label, Half value)
        {
            ImGui.TableNextColumn();
            label = _textService.Translate("AdvancedImportOverlay.ColorInput." + label);
            var labelWidth = ImGui.CalcTextSize(label).X;
            ImGui.TextUnformatted(label);

            var valueStr = _textService.Translate("AdvancedImportOverlay.ColorInput.ValueFormat", value);
            ImGui.SameLine(0, ImGui.GetContentRegionAvail().X - labelWidth - ImGui.CalcTextSize(valueStr).X);
            ImGui.TextUnformatted(valueStr);
        }

        drawColumn("R", r);
        drawColumn("G", g);
        drawColumn("B", b);
    }

    private void DrawHalfVector2(HalfVector2 vec)
    {
        using var table = ImRaii.Table("##Table", 2);
        if (!table)
            return;

        var scale = ImGuiHelpers.GlobalScale;
        ImGui.TableSetupColumn("X", ImGuiTableColumnFlags.WidthFixed, 50 * scale);
        ImGui.TableSetupColumn("Y", ImGuiTableColumnFlags.WidthFixed, 50 * scale);

        ImGui.TableNextRow();

        void drawColumn(string label, Half value)
        {
            ImGui.TableNextColumn();
            label = _textService.Translate("AdvancedImportOverlay.VectorInput." + label);
            var labelWidth = ImGui.CalcTextSize(label).X;
            ImGui.TextUnformatted(label);

            var valueStr = _textService.Translate("AdvancedImportOverlay.VectorInput.ValueFormat", value);
            ImGui.SameLine(0, ImGui.GetContentRegionAvail().X - labelWidth - ImGui.CalcTextSize(valueStr).X);
            ImGui.TextUnformatted(valueStr);
        }

        drawColumn("X", vec.X);
        drawColumn("Y", vec.Y);
    }

    private void DrawHalfVector4(HalfVector4 vec)
    {
        using var table = ImRaii.Table("##Table", 4);
        if (!table)
            return;

        var scale = ImGuiHelpers.GlobalScale;
        ImGui.TableSetupColumn("X", ImGuiTableColumnFlags.WidthFixed, 50 * scale);
        ImGui.TableSetupColumn("Y", ImGuiTableColumnFlags.WidthFixed, 50 * scale);
        ImGui.TableSetupColumn("Z", ImGuiTableColumnFlags.WidthFixed, 50 * scale);
        ImGui.TableSetupColumn("W", ImGuiTableColumnFlags.WidthFixed, 50 * scale);

        ImGui.TableNextRow();

        void drawColumn(string label, Half value)
        {
            ImGui.TableNextColumn();
            label = _textService.Translate("AdvancedImportOverlay.VectorInput." + label);
            var labelWidth = ImGui.CalcTextSize(label).X;
            ImGui.TextUnformatted(label);

            var valueStr = _textService.Translate("AdvancedImportOverlay.VectorInput.ValueFormat", value);
            ImGui.SameLine(0, ImGui.GetContentRegionAvail().X - labelWidth - ImGui.CalcTextSize(valueStr).X);
            ImGui.TextUnformatted(valueStr);
        }

        drawColumn("X", vec.X);
        drawColumn("Y", vec.Y);
        drawColumn("Z", vec.Z);
        drawColumn("W", vec.W);
    }
}
