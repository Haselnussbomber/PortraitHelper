using System.Text.Json.Serialization;

namespace PortraitHelper.Records;

public record SavedPresetTag
{
    public Guid Id;
    public string Name;

    [JsonConstructor]
    public SavedPresetTag(Guid Id, string Name)
    {
        this.Id = Id;
        this.Name = Name;
    }

    public SavedPresetTag(string Name) : this(Guid.NewGuid(), Name)
    {
    }
}
