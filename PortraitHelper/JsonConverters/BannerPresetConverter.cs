using System.Text.Json;
using System.Text.Json.Serialization;
using PortraitHelper.Records;

namespace PortraitHelper.JsonConverters;

public class BannerPresetConverter : JsonConverter<Records.BannerPreset>
{
    public override void Write(Utf8JsonWriter writer, Records.BannerPreset value, JsonSerializerOptions options)
        => writer.WriteStringValue(value?.ToExportedString());

    public override Records.BannerPreset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => Records.BannerPreset.FromExportedString(reader.GetString());
}
