using Microsoft.Extensions.Hosting;
using PortraitHelper.Config;

namespace PortraitHelper;

public sealed class Plugin : IDalamudPlugin
{
    private readonly IHost _host;

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
#if CUSTOM_CS
        pluginInterface.InitializeCustomClientStructs();
#endif

        _host = new HostBuilder()
            .UseContentRoot(pluginInterface.AssemblyLocation.Directory!.FullName)
            .ConfigureServices(services =>
            {
                services.AddDalamud(pluginInterface);
                services.AddSingleton(PluginConfig.Load);
                services.AddHaselCommon();
                services.AddPortraitHelper();
            })
            .Build();

        _host.Start();
    }

    void IDisposable.Dispose()
    {
        _host.StopAsync().GetAwaiter().GetResult();
        _host.Dispose();
    }
}
