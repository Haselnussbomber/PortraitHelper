using System.Text.Json.Serialization;

namespace PortraitHelper.Records;

public record SavedCharaCardPreset
{
    public Guid Id;
    public string Name;
    public BannerPreset? Preset;

    [JsonConstructor]
    public SavedCharaCardPreset(Guid id, string name, BannerPreset? preset)
    {
        Id = id;
        Name = name;
        Preset = preset;
    }
}
