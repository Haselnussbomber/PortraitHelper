using System.Threading;
using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Microsoft.Extensions.Hosting;
using PortraitHelper.Windows.CharaCardEditor;

namespace PortraitHelper.Services.CharaCardEditor;

[RegisterSingleton<IHostedService>(Duplicate = DuplicateStrategy.Append), AutoConstruct]
public partial class CharaCardMenuBarManager : IHostedService
{
    private readonly AddonObserver _addonObserver;
    private readonly MenuBar _menuBar;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (IsAddonOpen(AgentId.CharaCardDesignSetting))
            OnAddonOpen("CharaCardDesignSetting");

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
        if (addonName == "CharaCardDesignSetting")
            _menuBar.Open();
    }

    private void OnAddonClose(string addonName)
    {
        if (addonName == "CharaCardDesignSetting")
        {
            _menuBar.Close();
        }
    }
}
