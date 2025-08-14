using System.IO;
using System.Text.Json.Serialization;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using PortraitHelper.JsonConverters;

namespace PortraitHelper.Records;

[JsonConverter(typeof(CharaCardPresetConverter))]
public sealed record CharaCardPreset
{
    public const int Magic = 0x53434850; // PHCS => Portrait Helper CharaCard String
    public ushort Version = 1;

    // TODO

    public static CharaCardPreset? FromExportedString(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return null;

        byte[] rawInput;
        try
        {
            rawInput = Convert.FromBase64String(input);
        }
        catch
        {
            return null;
        }

        if (rawInput.Length < 8)
            return null;

        using var inputStream = new MemoryStream(rawInput);
        using var reader = new BinaryReader(inputStream);

        var magic = reader.ReadInt32();
        if (magic != Magic)
            return null;

        var preset = new CharaCardPreset
        {
            Version = reader.ReadUInt16()
        };

        switch (preset.Version)
        {
            case 1:
                // TODO
                break;

            default:
                throw new Exception($"Unknown Preset version {preset.Version}");
        }

        return preset;
    }

    public static unsafe CharaCardPreset? FromState()
    {
        var state = AgentBannerEditor.Instance()->EditorState;
        var preset = new CharaCardPreset();

        return preset;
    }

    public string ToExportedString()
    {
        using var outputStream = new MemoryStream();
        using var writer = new BinaryWriter(outputStream);

        writer.Write(Magic);
        writer.Write(Version);
        // TODO
        writer.Flush();

        return Convert.ToBase64String(outputStream.ToArray());
    }
}
