using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PortraitHelper.Records;

public record SavedPreset
{
    public Guid Id;
    public string Name;
    public PortraitPreset? Preset;
    public HashSet<Guid> Tags;

    [JsonConstructor]
    public SavedPreset(Guid id, string name, PortraitPreset? preset, HashSet<Guid> tags)
    {
        Id = id;
        Name = name;
        Preset = preset;
        Tags = tags;
    }
}
