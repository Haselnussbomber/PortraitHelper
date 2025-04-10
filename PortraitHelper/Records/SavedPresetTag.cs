using System.Text.Json.Serialization;

namespace PortraitHelper.Records;

public record SavedPresetTag
{
    public Guid Id;
    public string Name;

    [JsonConstructor]
    public SavedPresetTag(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    public SavedPresetTag(string name) : this(Guid.NewGuid(), name)
    {
    }
}
