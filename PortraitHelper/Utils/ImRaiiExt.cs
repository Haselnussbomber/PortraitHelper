using System.Text;
using FFXIVClientStructs.Interop;
using HaselCommon.Gui;
using ImGuiNET;
using static Dalamud.Interface.Utility.Raii.ImRaii;

namespace PortraitHelper.Utils;

public static unsafe class ImRaiiExt
{
    public static IEndObject PopupModal(string name, ImGuiWindowFlags flags)
    {
        return new EndConditionally(ImGui.EndPopup, BeginPopupModal(name, flags));
    }

    private static bool BeginPopupModal(string name, ImGuiWindowFlags flags)
    {
        if (name == null)
            return false;

        var nameBytes = Encoding.UTF8.GetBytes(name).AsSpan();
        Span<byte> nameNullTermianted = stackalloc byte[nameBytes.Length + 1];
        nameBytes.CopyTo(nameNullTermianted);
        nameNullTermianted[^1] = 0;

        return ImGuiNative.igBeginPopupModal(nameNullTermianted.GetPointer(0), null, flags) != 0;
    }
}
