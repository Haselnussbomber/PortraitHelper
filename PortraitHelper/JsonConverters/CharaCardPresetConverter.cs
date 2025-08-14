using System.Text.Json;
using System.Text.Json.Serialization;
using PortraitHelper.Records;

namespace PortraitHelper.JsonConverters;

public class CharaCardPresetConverter : JsonConverter<CharaCardPreset>
{
    public override void Write(Utf8JsonWriter writer, CharaCardPreset value, JsonSerializerOptions options)
        => writer.WriteStringValue(value?.ToExportedString());

    public override CharaCardPreset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => CharaCardPreset.FromExportedString(reader.GetString());
}
