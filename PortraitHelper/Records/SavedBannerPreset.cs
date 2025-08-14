using System.Text.Json.Serialization;

namespace PortraitHelper.Records;

public record SavedBannerPreset
{
    public Guid Id;
    public string Name;
    public BannerPreset? Preset;

    [JsonConstructor]
    public SavedBannerPreset(Guid id, string name, BannerPreset? preset)
    {
        Id = id;
        Name = name;
        Preset = preset;
    }
}
