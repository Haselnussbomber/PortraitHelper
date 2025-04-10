using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using HaselCommon.Services;
using Microsoft.Extensions.Logging;
using PortraitHelper.Records;
using PortraitHelper.Utils;
using PortraitHelper.Windows;

namespace PortraitHelper.Services;

[RegisterSingleton, AutoConstruct]
public unsafe partial class MenuBarManager : IDisposable
{
    private readonly ILogger<MenuBarManager> _logger;
    private readonly IGameInteropProvider _gameInteropProvider;
    private readonly AddonObserver _addonObserver;
    private readonly MenuBar _menuBar;

    private Hook<UIClipboard.Delegates.OnClipboardDataChanged>? _onClipboardDataChangedHook;

    [AutoPostConstruct]
    private void Initialize()
    {
        _onClipboardDataChangedHook = _gameInteropProvider.HookFromAddress<UIClipboard.Delegates.OnClipboardDataChanged>(
            UIClipboard.MemberFunctionPointers.OnClipboardDataChanged,
            OnClipboardDataChangedDetour);

        if (IsAddonOpen(AgentId.BannerEditor))
            OnAddonOpen("BannerEditor");

        _addonObserver.AddonOpen += OnAddonOpen;
        _addonObserver.AddonClose += OnAddonClose;

        _onClipboardDataChangedHook?.Enable();
    }

    void IDisposable.Dispose()
    {
        _addonObserver.AddonOpen -= OnAddonOpen;
        _addonObserver.AddonClose -= OnAddonClose;

        _menuBar.Close();

        _onClipboardDataChangedHook?.Disable();
        _onClipboardDataChangedHook?.Dispose();
    }

    private void OnAddonOpen(string addonName)
    {
        if (addonName != "BannerEditor")
            return;

        _menuBar.Open();
    }

    private void OnAddonClose(string addonName)
    {
        if (addonName != "BannerEditor")
            return;

        _menuBar.Close();
    }

    private void OnClipboardDataChangedDetour(UIClipboard* uiClipboard)
    {
        _onClipboardDataChangedHook!.Original(uiClipboard);

        try
        {
            ClipboardUtils.ClipboardPreset = PortraitPreset.FromExportedString(uiClipboard->Data.SystemClipboardText.ToString());
            if (ClipboardUtils.ClipboardPreset != null)
                _logger.LogDebug("Parsed ClipboardPreset: {ClipboardPreset}", ClipboardUtils.ClipboardPreset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading preset");
        }
    }
}
