using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using HaselCommon.Gui;
using HaselCommon.Services;
using ImGuiNET;
using PortraitHelper.Config;
using PortraitHelper.Records;
using PortraitHelper.Services;
using PortraitHelper.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;

namespace PortraitHelper.Windows.Dialogs;

[RegisterScoped, AutoConstruct]
public partial class CreatePresetDialog
{
    private readonly INotificationManager _notificationManager;
    private readonly PluginConfig _pluginConfig;
    private readonly TextService _textService;
    private readonly BannerService _bannerService;

    private bool _shouldOpen;
    private bool _isSaving;
    private string? _name;
    private PortraitPreset? _preset;
    private Image<Bgra32>? _image;

    public void Open(string name, PortraitPreset? preset, Image<Bgra32>? image)
    {
        _name = name;
        _preset = preset;
        _image = image;
        _isSaving = false;
        _shouldOpen = true;
    }

    public void Draw()
    {
        if (_preset == null || _image == null)
            return;

        var title = _textService.Translate("CreatePresetDialog.Title");

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

        using var disabledDueToSaving = ImRaii.Disabled(_isSaving);

        ImGui.TextUnformatted(_textService.Translate("CreatePresetDialog.Name.Label"));
        ImGui.InputText("##PresetName", ref _name, Constants.PresetNameMaxLength);

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
            if (ImGui.Button(_textService.Translate(_isSaving ? "ConfirmationButtonWindow.Saving" : "ConfirmationButtonWindow.Save"), new Vector2(120, 0)) || shouldSave)
            {
                _isSaving = true;

                Task.Run(() =>
                {
                    try
                    {
                        var guid = Guid.CreateVersion7();
                        var thumbPath = _bannerService.GetPortraitThumbnailPath(guid);

                        _image.Metadata.ExifProfile ??= new();
                        _image.Metadata.ExifProfile.SetValue(ExifTag.UserComment, _preset.ToExportedString());

                        _image.SaveAsPng(thumbPath, new PngEncoder
                        {
                            CompressionLevel = PngCompressionLevel.BestCompression,
                            ColorType = PngColorType.Rgb // no need for alpha channel
                        });

                        _pluginConfig.Presets.Insert(0, new(guid, _name.Trim(), _preset));
                        _pluginConfig.Save();

                        _name = string.Empty;
                        _preset = null;
                        _image.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _notificationManager.AddNotification(new()
                        {
                            Title = "Could not create preset",
                            Content = ex.Message,
                        });
                    }

                    _isSaving = false;
                    ImGui.CloseCurrentPopup();
                });
            }
        }

        ImGui.SetItemDefaultFocus();
        ImGui.SameLine();
        if (ImGui.Button(_textService.Translate("ConfirmationButtonWindow.Cancel"), new Vector2(120, 0)))
        {
            _name = string.Empty;
            _preset = null;
            _image.Dispose();
            ImGui.CloseCurrentPopup();
        }
    }
}
