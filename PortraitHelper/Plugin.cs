using Dalamud.Plugin;
using HaselCommon;
using Microsoft.Extensions.DependencyInjection;
using PortraitHelper.Config;
using PortraitHelper.Services;

namespace PortraitHelper;

public sealed class Plugin : IDalamudPlugin
{
    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        Service.Collection
            .AddDalamud(pluginInterface)
            .AddSingleton(PluginConfig.Load)
            .AddHaselCommon()
            .AddPortraitHelper();

        Service.Initialize(() =>
        {
            Service.Get<MenuBarManager>();
        });
    }

    void IDisposable.Dispose()
    {
        Service.Dispose();
    }
}
