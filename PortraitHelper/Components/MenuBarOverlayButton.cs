using PortraitHelper.Interfaces;
using PortraitHelper.Services.BannerEditor;

namespace PortraitHelper.Components;

[AutoConstruct]
public partial class MenuBarOverlayButton<T> : MenuBarButton where T : IOverlay
{
    private readonly BannerMenuBarState _state;

    public override bool IsActive => _state.Overlay is T;

    public override void OnClick()
    {
        if (IsActive)
        {
            _state.CloseOverlay();
        }
        else
        {
            _state.OpenOverlay<T>();
        }
    }
}
