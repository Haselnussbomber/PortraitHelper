using PortraitHelper.Enums;

namespace PortraitHelper.Interfaces;

public interface IOverlay : IDisposable
{
    OverlayType Type { get; }
    bool IsWindow { get; }

    void Open(bool focus = true);
    void Close();
}
