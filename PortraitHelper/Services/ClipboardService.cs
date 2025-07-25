using System.IO;
using System.Threading.Tasks;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using Microsoft.Extensions.Logging;
using PortraitHelper.Enums;
using PortraitHelper.Records;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.System.Memory;
using Windows.Win32.System.Ole;

namespace PortraitHelper.Services;

[RegisterSingleton, AutoConstruct]
public partial class ClipboardService : IDisposable
{
    private readonly ILogger<ClipboardService> _logger;
    private readonly IGameInteropProvider _gameInteropProvider;

    private Hook<UIClipboard.Delegates.OnClipboardDataChanged>? _onClipboardDataChangedHook;

    public ImportFlags CurrentImportFlags { get; set; } = ImportFlags.All;
    public PortraitPreset? ClipboardPreset { get; set; }

    [AutoPostConstruct]
    private unsafe void Initialize()
    {
        _onClipboardDataChangedHook = _gameInteropProvider.HookFromAddress<UIClipboard.Delegates.OnClipboardDataChanged>(
            UIClipboard.MemberFunctionPointers.OnClipboardDataChanged,
            OnClipboardDataChangedDetour);

        _onClipboardDataChangedHook?.Enable();
    }

    void IDisposable.Dispose()
    {
        _onClipboardDataChangedHook?.Dispose();
    }

    private unsafe void OnClipboardDataChangedDetour(UIClipboard* uiClipboard)
    {
        _onClipboardDataChangedHook!.Original(uiClipboard);

        try
        {
            ClipboardPreset = PortraitPreset.FromExportedString(uiClipboard->Data.SystemClipboardText.ToString());
            if (ClipboardPreset != null)
                _logger.LogDebug("Parsed ClipboardPreset: {ClipboardPreset}", ClipboardPreset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading preset");
        }
    }

    public async Task SetClipboardPortraitPreset(PortraitPreset? preset)
    {
        await OpenClipboard();

        try
        {
            PInvoke.EmptyClipboard();

            if (preset != null)
            {
                var clipboardText = Marshal.StringToHGlobalAnsi(preset.ToExportedString());
                if (PInvoke.SetClipboardData((uint)CLIPBOARD_FORMAT.CF_TEXT, (HANDLE)clipboardText) != 0)
                    ClipboardPreset = preset;
            }
            else
            {
                ClipboardPreset = null;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during PortraitPreset.ToClipboard");
        }
        finally
        {
            PInvoke.CloseClipboard();
        }
    }

    public async Task SetClipboardImage(Image<Rgba32> image)
    {
        await OpenClipboard();

        if (!PInvoke.EmptyClipboard())
            return;

        SetDIB(image);
        SetDIBV5(image);
        SetPNG(image);
        PInvoke.CloseClipboard();
    }

    private async Task OpenClipboard()
    {
        while (!PInvoke.OpenClipboard(HWND.Null))
        {
            await Task.Delay(100);
        }
    }

    private unsafe void SetDIB(Image<Rgba32> image)
    {
        var hMem = PInvoke.GlobalAlloc(GLOBAL_ALLOC_FLAGS.GMEM_MOVEABLE, (nuint)(sizeof(BITMAPINFOHEADER) + image.Width * image.Height * sizeof(Bgra32))); // tagBITMAPINFO
        if (hMem.IsNull)
            return;

        var data = (nint)PInvoke.GlobalLock(hMem);
        if (data == 0)
        {
            PInvoke.GlobalFree(hMem);
            return;
        }

        var header = (BITMAPINFOHEADER*)data;
        header->biSize = (uint)sizeof(BITMAPINFOHEADER);
        header->biWidth = image.Width;
        header->biHeight = -image.Height;
        header->biPlanes = 1;
        header->biBitCount = 32; // bits per pixel
        header->biCompression = 0; // BI_RGB
        header->biSizeImage = 0;
        header->biXPelsPerMeter = 0;
        header->biYPelsPerMeter = 0;
        header->biClrUsed = 0;
        header->biClrImportant = 0;

        var pixelSpan = new Span<Bgra32>((void*)(data + header->biSize), image.Width * image.Height);

        using (var bgra = image.CloneAs<Bgra32>())
            bgra.CopyPixelDataTo(pixelSpan);

        foreach (ref var pixel in pixelSpan)
            pixel.A = 0; // rgbReserved of RGBQUAD "must be zero"

        PInvoke.GlobalUnlock(hMem);
        PInvoke.SetClipboardData((uint)CLIPBOARD_FORMAT.CF_DIB, (HANDLE)(nint)hMem);
    }

    private unsafe void SetDIBV5(Image<Rgba32> image)
    {
        var hMem = PInvoke.GlobalAlloc(GLOBAL_ALLOC_FLAGS.GMEM_MOVEABLE, (nuint)(sizeof(BITMAPV5HEADER) + image.Width * image.Height * sizeof(Bgra32)));
        if (hMem.IsNull)
            return;

        var data = (nint)PInvoke.GlobalLock(hMem);
        if (data == 0)
        {
            PInvoke.GlobalFree(hMem);
            return;
        }

        var bitmapInfo = (BITMAPV5HEADER*)data;
        bitmapInfo->bV5Size = (uint)Marshal.SizeOf(typeof(BITMAPV5HEADER));
        bitmapInfo->bV5Width = image.Width;
        bitmapInfo->bV5Height = -image.Height; // negative height for top-down image
        bitmapInfo->bV5Planes = 1;
        bitmapInfo->bV5BitCount = 32; // 4 bytes per pixel (Rgba32)
        bitmapInfo->bV5Compression = 0; // BI_RGB
        bitmapInfo->bV5SizeImage = (uint)(image.Width * image.Height * sizeof(Bgra32));
        bitmapInfo->bV5XPelsPerMeter = 0;
        bitmapInfo->bV5YPelsPerMeter = 0;
        bitmapInfo->bV5ClrUsed = 0;
        bitmapInfo->bV5ClrImportant = 0;
        bitmapInfo->bV5RedMask = 0x00FF0000;
        bitmapInfo->bV5GreenMask = 0x0000FF00;
        bitmapInfo->bV5BlueMask = 0x000000FF;
        bitmapInfo->bV5AlphaMask = 0xFF000000;
        bitmapInfo->bV5CSType = 0x73524742; // 'sRGB'
        bitmapInfo->bV5Endpoints = new CIEXYZTRIPLE();
        bitmapInfo->bV5GammaRed = 0;
        bitmapInfo->bV5GammaGreen = 0;
        bitmapInfo->bV5GammaBlue = 0;
        bitmapInfo->bV5Intent = 4; // LCS_COLORIMETRIC
        bitmapInfo->bV5ProfileData = 0;
        bitmapInfo->bV5ProfileSize = 0;
        bitmapInfo->bV5Reserved = 0;

        var pixelSpan = new Span<Bgra32>((void*)(data + bitmapInfo->bV5Size), image.Width * image.Height);

        using (var bgra = image.CloneAs<Bgra32>())
            bgra.CopyPixelDataTo(pixelSpan);

        foreach (ref var pixel in pixelSpan)
            pixel.A = 0; // rgbReserved of RGBQUAD "must be zero"

        PInvoke.GlobalUnlock(hMem);
        PInvoke.SetClipboardData((uint)CLIPBOARD_FORMAT.CF_DIBV5, (HANDLE)(nint)hMem);
    }

    private unsafe void SetPNG(Image<Rgba32> image)
    {
        var format = PInvoke.RegisterClipboardFormat("PNG");
        if (format == 0)
            return;

        using var ms = new MemoryStream();
        image.SaveAsPng(ms);
        var bytes = ms.ToArray();

        var hMem = PInvoke.GlobalAlloc(GLOBAL_ALLOC_FLAGS.GMEM_MOVEABLE, (nuint)bytes.Length);
        if (hMem.IsNull)
            return;

        var data = (nint)PInvoke.GlobalLock(hMem);
        if (data == 0)
        {
            PInvoke.GlobalFree(hMem);
            return;
        }

        Marshal.Copy(bytes, 0, data, bytes.Length);

        PInvoke.GlobalUnlock(hMem);
        PInvoke.SetClipboardData(format, (HANDLE)(nint)hMem);
    }
}
