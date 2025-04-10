using System.Text.Json.Serialization;

namespace PortraitHelper.Records;

public record SavedPreset
{
    public Guid Id;
    public string Name;
    public PortraitPreset? Preset;

    [JsonConstructor]
    public SavedPreset(Guid id, string name, PortraitPreset? preset)
    {
        Id = id;
        Name = name;
        Preset = preset;
    }
}
