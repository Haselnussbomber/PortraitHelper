using System.IO;
using Dalamud.Plugin;

namespace PortraitHelper.Extensions;

public static class IPluginInterfaceExtensions
{
    public static string GetPortraitThumbnailPath(this IDalamudPluginInterface pluginInterface, Guid id)
    {
        var portraitsPath = Path.Join(pluginInterface.ConfigDirectory.FullName, "Portraits");

        if (!Directory.Exists(portraitsPath))
            Directory.CreateDirectory(portraitsPath);

        return Path.Join(portraitsPath, $"{id.ToString("D").ToLowerInvariant()}.png");
    }
}
