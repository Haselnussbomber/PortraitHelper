using System.Text.Json.Nodes;
using PortraitHelper.Interfaces;

namespace PortraitHelper.Config.Migrations;

/// <summary>
/// Migration for Version 2.<br/>
/// Changes:<br/>
/// - Presets was renamed to BannerPresets
/// </summary>
public class Version2 : IConfigMigration
{
    public int Version => 2;

    public void Migrate(ref JsonObject config)
    {
        var presets = (JsonArray?)config["Presets"];
        if (presets == null)
            return;

        var bannerPresets = (JsonArray?)config["BannerPresets"];
        if (bannerPresets != null)
            return;

        config.Add("BannerPresets", presets.DeepClone());
        config.Remove("Presets");
    }
}
