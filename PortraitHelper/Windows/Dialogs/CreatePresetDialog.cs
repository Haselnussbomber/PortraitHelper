using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using HaselCommon.Services;
using ImGuiNET;
using PortraitHelper.Config;
using PortraitHelper.ImGuiComponents;
using PortraitHelper.Records;
using PortraitHelper.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;

namespace PortraitHelper.Windows.Dialogs;

[RegisterScoped, AutoConstruct]
public partial class CreatePresetDialog : ConfirmationDialog
{
    private readonly INotificationManager _notificationManager;
    private readonly PluginConfig _pluginConfig;
    private readonly TextService _textService;
    private readonly BannerService _bannerService;

    private ConfirmationButton _saveButton;
    private string? _name;
    private PortraitPreset? _preset;
    private Image<Bgra32>? _image;

    [AutoPostConstruct]
    private void Initialize()
    {
        WindowName = _textService.Translate("PortraitHelperWindows.CreatePresetDialog.Title");

        AddButton(_saveButton = new ConfirmationButton(_textService.Translate("ConfirmationButtonWindow.Save"), OnSave));
    }

    public void Open(string name, PortraitPreset? preset, Image<Bgra32>? image)
    {
        _name = name;
        _preset = preset;
        _image = image;
        Show();
    }

    public void Close()
    {
        Hide();
        _name = null;
        _preset = null;
        _image?.Dispose();
        _image = null;
    }

    public override bool DrawCondition()
        => base.DrawCondition() && _name != null && _preset != null && _image != null;

    public override void InnerDraw()
    {
        ImGui.TextUnformatted(_textService.Translate("PortraitHelperWindows.CreatePresetDialog.Name.Label"));
        ImGui.Spacing();
        ImGui.InputText("##PresetName", ref _name, 100);

        var disabled = string.IsNullOrEmpty(_name.Trim());
        if (!disabled && (ImGui.IsKeyPressed(ImGuiKey.Enter) || ImGui.IsKeyPressed(ImGuiKey.KeypadEnter)))
        {
            OnSave();
        }

        _saveButton.Disabled = disabled;
    }

    private void OnSave()
    {
        if (_preset == null || _image == null || string.IsNullOrEmpty(_name?.Trim()))
        {
            _notificationManager.AddNotification(new()
            {
                Title = "Could not save portrait"
            });
            Close();
            return;
        }

        Hide();

        Task.Run(() =>
        {
            var guid = Guid.NewGuid();
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

            Close();
        });
    }
}
