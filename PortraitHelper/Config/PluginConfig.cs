using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Dalamud.Configuration;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using Microsoft.Extensions.DependencyInjection;
using PortraitHelper.Records;

namespace PortraitHelper.Config;

public partial class PluginConfig : IPluginConfiguration
{
    [JsonIgnore]
    public const int CURRENT_CONFIG_VERSION = 1;

    [JsonIgnore]
    public int LastSavedConfigHash { get; set; }

    [JsonIgnore]
    public static JsonSerializerOptions? SerializerOptions { get; private set; } = new()
    {
        IncludeFields = true,
        WriteIndented = true,
    };

    [JsonIgnore]
    private static IDalamudPluginInterface? PluginInterface;

    [JsonIgnore]
    private static IPluginLog? PluginLog;

    public static PluginConfig Load(IServiceProvider serviceProvider)
    {
        PluginInterface = serviceProvider.GetRequiredService<IDalamudPluginInterface>();
        PluginLog = serviceProvider.GetRequiredService<IPluginLog>();

        var fileInfo = PluginInterface.ConfigFile;
        if (!fileInfo.Exists || fileInfo.Length < 2)
            return new();

        var json = File.ReadAllText(fileInfo.FullName);
        var node = JsonNode.Parse(json);
        if (node == null)
            return new();

        if (node is not JsonObject config)
            return new();

        var version = config[nameof(Version)]?.GetValue<int>();
        if (version == null)
            return new();

        var pluginConfig = JsonSerializer.Deserialize<PluginConfig>(node, SerializerOptions);
        if (pluginConfig == null)
            return new();

        pluginConfig.Presets.RemoveAll(preset => string.IsNullOrEmpty(preset.Name) || preset.Preset == null);

        return pluginConfig;
    }

    public void Save()
    {
        try
        {
            var serialized = JsonSerializer.Serialize(this, SerializerOptions);
            var hash = serialized.GetHashCode();

            if (LastSavedConfigHash != hash)
            {
                Util.WriteAllTextSafe(PluginInterface!.ConfigFile.FullName, serialized);
                LastSavedConfigHash = hash;
                PluginLog?.Information("Configuration saved.");
            }
        }
        catch (Exception e)
        {
            PluginLog?.Error(e, "Error saving config");
        }
    }
}

public partial class PluginConfig
{
    public int Version { get; set; } = CURRENT_CONFIG_VERSION;

    public List<SavedPreset> Presets = [];
    public bool ShowAlignmentTool = false;
    public int AlignmentToolVerticalLines = 2;
    public Color AlignmentToolVerticalColor = new(0, 0, 0, 1f);
    public int AlignmentToolHorizontalLines = 2;
    public Color AlignmentToolHorizontalColor = new(0, 0, 0, 1f);

    public bool NotifyGearChecksumMismatch = true;
}
