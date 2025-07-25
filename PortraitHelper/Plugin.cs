using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using HaselCommon.Extensions;
using Microsoft.Extensions.DependencyInjection;
using PortraitHelper.Config;
using PortraitHelper.Services;

namespace PortraitHelper;

public sealed class Plugin : IDalamudPlugin
{
    private readonly ServiceProvider _serviceProvider;

    public Plugin(IDalamudPluginInterface pluginInterface, IFramework framework)
    {
        _serviceProvider = new ServiceCollection()
            .AddDalamud(pluginInterface)
            .AddSingleton(PluginConfig.Load)
            .AddHaselCommon()
            .AddPortraitHelper()
            .BuildServiceProvider();

        framework.RunOnFrameworkThread(_serviceProvider.GetRequiredService<MenuBarManager>);
    }

    void IDisposable.Dispose()
    {
        _serviceProvider.Dispose();
    }
}
