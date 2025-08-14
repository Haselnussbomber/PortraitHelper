using System.Threading;
using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Microsoft.Extensions.Hosting;
using PortraitHelper.Windows;

namespace PortraitHelper.Services;

[RegisterSingleton<IHostedService>(Duplicate = DuplicateStrategy.Append), AutoConstruct]
public partial class MenuBarManager : IHostedService
{
    private readonly AddonObserver _addonObserver;
    private readonly MenuBar _menuBar;
    private readonly ThumbnailService _thumbnailService;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (IsAddonOpen(AgentId.BannerEditor))
            OnAddonOpen("BannerEditor");

        _addonObserver.AddonOpen += OnAddonOpen;
        _addonObserver.AddonClose += OnAddonClose;

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _addonObserver.AddonOpen -= OnAddonOpen;
        _addonObserver.AddonClose -= OnAddonClose;

        _menuBar.Close();

        return Task.CompletedTask;
    }

    private void OnAddonOpen(string addonName)
    {
        if (addonName == "BannerEditor")
            _menuBar.Open();
    }

    private void OnAddonClose(string addonName)
    {
        if (addonName == "BannerEditor")
        {
            _menuBar.Close();
            _thumbnailService.Clear();
        }
    }
}
