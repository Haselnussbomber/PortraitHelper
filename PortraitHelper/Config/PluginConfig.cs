using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Dalamud.Configuration;
using Dalamud.Utility;
using PortraitHelper.Config.Migrations;
using PortraitHelper.Interfaces;
using PortraitHelper.Records;

namespace PortraitHelper.Config;

public partial class PluginConfig : IPluginConfiguration
{
    [JsonIgnore]
    public const int CURRENT_CONFIG_VERSION = 2;

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
        {
            var htConfigPath = new FileInfo(Path.Join(PluginInterface.ConfigFile.Directory!.FullName, "HaselTweaks.json"));
            if (htConfigPath.Exists)
            {
                try
                {
                    return MigrateFromHaselTweaks(htConfigPath);
                }
                catch (Exception e)
                {
                    PluginLog?.Error(e, "Error migrating HaselTweaks config");
                }
            }

            return new();
        }

        var json = File.ReadAllText(fileInfo.FullName);
        var node = JsonNode.Parse(json);
        if (node == null)
            return new();

        if (node is not JsonObject config)
            return new();

        var version = config[nameof(Version)]?.GetValue<int>();
        if (version == null)
            return new();

        var migrated = false;

        IConfigMigration[] migrations = [
            new Version2(),
        ];

        foreach (var migration in migrations)
        {
            if (version < migration.Version)
            {
                PluginLog.Information("Migrating from version {version} to {migrationVersion}...", version, migration.Version);

                migration.Migrate(ref config);
                version = migration.Version;
                config[nameof(Version)] = version;
                migrated = true;
            }
        }

        var pluginConfig = JsonSerializer.Deserialize<PluginConfig>(config, SerializerOptions) ?? new();

        if (migrated)
        {
            PluginLog.Information("Configuration migrated successfully.");
            pluginConfig.Save();
        }

        pluginConfig.BannerPresets.RemoveAll(preset => string.IsNullOrEmpty(preset.Name) || preset.Preset == null);

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
                FilesystemUtil.WriteAllTextSafe(PluginInterface!.ConfigFile.FullName, serialized);
                LastSavedConfigHash = hash;
                PluginLog?.Information("Configuration saved.");
            }
        }
        catch (Exception e)
        {
            PluginLog?.Error(e, "Error saving config");
        }
    }

    private static PluginConfig MigrateFromHaselTweaks(FileInfo htConfigPath)
    {
        PluginLog?.Information("Migrating from HaselTweaks");

        using var stream = htConfigPath.OpenRead();
        var rootNode = JsonNode.Parse(stream);
        if (rootNode == null)
            return new();

        var tweakConfig = rootNode["Tweaks"]?["PortraitHelper"];
        if (tweakConfig == null)
            return new();

        var config = new PluginConfig
        {
            ShowAlignmentTool = tweakConfig["ShowAlignmentTool"]?.AsValue().GetValue<bool>() ?? false,
            AlignmentToolVerticalLines = tweakConfig["AlignmentToolVerticalLines"]?.AsValue().GetValue<int>() ?? 2,
            AlignmentToolHorizontalLines = tweakConfig["AlignmentToolHorizontalLines"]?.AsValue().GetValue<int>() ?? 2
        };

        var vColor = tweakConfig["AlignmentToolVerticalColor"];
        if (vColor != null)
        {
            var r = vColor["X"]?.GetValue<float>() ?? 0f;
            var g = vColor["Y"]?.GetValue<float>() ?? 0f;
            var b = vColor["Z"]?.GetValue<float>() ?? 0f;
            var a = vColor["W"]?.GetValue<float>() ?? 1f;
            config.AlignmentToolVerticalColor = new Color(r, g, b, a);
        }

        var hColor = tweakConfig["AlignmentToolHorizontalColor"];
        if (hColor != null)
        {
            var r = hColor["X"]?.GetValue<float>() ?? 0f;
            var g = hColor["Y"]?.GetValue<float>() ?? 0f;
            var b = hColor["Z"]?.GetValue<float>() ?? 0f;
            var a = hColor["W"]?.GetValue<float>() ?? 1f;
            config.AlignmentToolVerticalColor = new Color(r, g, b, a);
        }

        var presets = tweakConfig["Presets"]?.AsArray();
        if (presets == null)
            return config;

        var portraitsPathHT = Path.Join(htConfigPath.Directory!.FullName, "HaselTweaks", "Portraits");
        var portraitsPathPH = Path.Join(PluginInterface!.ConfigDirectory.FullName, "Portraits");

        if (!Directory.Exists(portraitsPathPH))
            Directory.CreateDirectory(portraitsPathPH);

        foreach (var preset in presets)
        {
            if (preset == null)
                continue;

            var name = preset["Name"]!.AsValue().GetValue<string>();
            if (string.IsNullOrEmpty(name))
                continue;

            var id = preset["Id"]!.AsValue().GetValue<Guid>();
            var presetString = preset["Preset"]!.AsValue().GetValue<string>();

            var thumbFileName = $"{id.ToString("D").ToLowerInvariant()}.png";
            var thumbPathHT = Path.Join(portraitsPathHT, thumbFileName);
            var thumbPathPH = Path.Join(portraitsPathPH, thumbFileName);

            if (File.Exists(thumbPathHT))
            {
                if (!File.Exists(thumbPathPH))
                {
                    try
                    {
                        File.Copy(thumbPathHT, thumbPathPH);
                    }
                    catch (Exception e)
                    {
                        PluginLog?.Error(e, $"Error copying thumbnail {id}");
                        continue;
                    }
                }
            }
            else
            {
                PluginLog?.Error($"Thumbnail for {id} not found");
                continue;
            }

            try
            {
                config.BannerPresets.Add(new SavedBannerPreset(id, name, Records.BannerPreset.FromExportedString(presetString)));
            }
            catch (Exception e)
            {
                PluginLog?.Error(e, $"Error copying preset {id}");
            }
        }

        config.Save();

        return config;
    }
}

public partial class PluginConfig
{
    public int Version { get; set; } = CURRENT_CONFIG_VERSION;

    public List<SavedBannerPreset> BannerPresets = [];
    public bool ShowAlignmentTool = false;
    public int AlignmentToolVerticalLines = 2;
    public Color AlignmentToolVerticalColor = new(0, 0, 0, 1f);
    public int AlignmentToolHorizontalLines = 2;
    public Color AlignmentToolHorizontalColor = new(0, 0, 0, 1f);
}
