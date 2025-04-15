using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PortraitHelper.Services;

public record ImageResult
{
    public Guid Id { get; init; }
    public Image<Rgba32>? Image { get; set; }
    public Exception? Exception { get; set; }
}

public record ThumbnailResult
{
    public Guid Id { get; init; }
    public IDalamudTextureWrap? Texture { get; set; }
    public Exception? Exception { get; set; }
}

[RegisterSingleton, AutoConstruct]
public partial class ThumbnailService : IDisposable
{
    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly ITextureProvider _textureProvider;

    private readonly CancellationTokenSource _disposeCTS = new();

    private readonly Dictionary<Guid, ConfiguredTaskAwaitable> _imageTasks = [];
    private readonly Dictionary<Guid, ImageResult> _images = [];

    private readonly Dictionary<Guid, ConfiguredTaskAwaitable> _thumbnailTasks = [];
    private readonly Dictionary<(Guid, Size), ThumbnailResult> _thumbnails = [];

    private bool _disposing;

    public void Dispose()
    {
        _disposing = true;
        _disposeCTS.Cancel();
        _disposeCTS.Dispose();

        foreach (var result in _images.Values)
        {
            result.Image?.Dispose();
        }

        _images.Clear();

        foreach (var result in _thumbnails.Values)
        {
            result.Texture?.Dispose();
        }

        _thumbnails.Clear();
    }

    public string GetPortraitThumbnailPath(Guid id)
    {
        var portraitsPath = Path.Join(_pluginInterface.ConfigDirectory.FullName, "Portraits");

        if (!Directory.Exists(portraitsPath))
            Directory.CreateDirectory(portraitsPath);

        return Path.Join(portraitsPath, $"{id.ToString("D").ToLowerInvariant()}.png");
    }

    public bool TryGetThumbnail(Guid id, Size size, out bool exists, out IDalamudTextureWrap? textureWrap, out Exception? exception)
    {
        exists = false;
        textureWrap = null;
        exception = null;

        if (_disposing)
            return false;

        if (_thumbnails.TryGetValue((id, size), out var thumbnailResult))
        {
            exists = true;
            textureWrap = thumbnailResult.Texture;
            exception = thumbnailResult.Exception;
            return textureWrap != null;
        }

        var path = GetPortraitThumbnailPath(id);
        if (!File.Exists(path))
            return false;

        exists = true;

        // -- Original Image

        if (!_images.TryGetValue(id, out var imageResult))
        {
            _images.Add(id, imageResult = new() { Id = id });
        }

        if (imageResult.Exception != null)
        {
            exception = imageResult.Exception;
            return false;
        }

        if (imageResult.Image == null)
        {
            if (!_imageTasks.TryGetValue(id, out var imageTask))
            {
                imageTask = Task.Run(async () =>
                {
                    try
                    {
                        imageResult.Image = await Image.LoadAsync<Rgba32>(path, _disposeCTS.Token);
                    }
                    catch (Exception ex)
                    {
                        imageResult.Exception = ex;
                    }

                    _imageTasks.Remove(id);
                }, _disposeCTS.Token).ConfigureAwait(false);

                _imageTasks.Add(id, imageTask);
            }

            return false;
        }

        // -- Thumbnail

        _thumbnails.Add((id, size), thumbnailResult = new() { Id = id });

        _thumbnailTasks.Add(id, Task.Run(() =>
        {
            try
            {
                using var scaledImage = imageResult.Image.Clone();

                if (_disposing)
                    return;

                scaledImage.Mutate(i => i.Resize(size, KnownResamplers.Lanczos3, false));

                if (_disposing)
                    return;

                var data = new byte[4 * scaledImage.Width * scaledImage.Height];
                scaledImage.CopyPixelDataTo(data);

                if (_disposing)
                    return;

                thumbnailResult.Texture = _textureProvider.CreateFromRaw(RawImageSpecification.Rgba32(scaledImage.Width, scaledImage.Height), data);
            }
            catch (Exception ex)
            {
                thumbnailResult.Exception = ex;
            }

            _thumbnailTasks.Remove(id);
        }, _disposeCTS.Token).ConfigureAwait(false));

        return false;
    }
}
