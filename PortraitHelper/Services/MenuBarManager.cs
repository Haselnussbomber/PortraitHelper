using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using HaselCommon.Services;
using PortraitHelper.Windows;

namespace PortraitHelper.Services;

[RegisterSingleton, AutoConstruct]
public partial class MenuBarManager : IDisposable
{
    private readonly AddonObserver _addonObserver;
    private readonly MenuBar _menuBar;

    [AutoPostConstruct]
    private void Initialize()
    {
        if (IsAddonOpen(AgentId.BannerEditor))
            OnAddonOpen("BannerEditor");

        _addonObserver.AddonOpen += OnAddonOpen;
        _addonObserver.AddonClose += OnAddonClose;
    }

    void IDisposable.Dispose()
    {
        _addonObserver.AddonOpen -= OnAddonOpen;
        _addonObserver.AddonClose -= OnAddonClose;

        _menuBar.Close();
    }

    private void OnAddonOpen(string addonName)
    {
        if (addonName == "BannerEditor")
            _menuBar.Open();
    }

    private void OnAddonClose(string addonName)
    {
        if (addonName == "BannerEditor")
            _menuBar.Close();
    }
}
