using System.Collections.Generic;
using HaselCommon.Extensions.Collections;
using PortraitHelper.Config;
using PortraitHelper.Windows;

namespace PortraitHelper.Services;

[RegisterSingleton, AutoConstruct]
public partial class PresetCardManager : IDisposable
{
    private readonly PluginConfig _pluginConfig;
    private readonly Dictionary<Guid, PresetCard> _presetCards = [];

    public void Dispose()
    {
        Clear();
    }

    public void Clear()
    {
        _presetCards.Dispose();
    }

    public PresetCard[] GetPresetCards()
    {
        foreach (var preset in _pluginConfig.Presets)
        {
            if (!_presetCards.ContainsKey(preset.Id))
            {
                _presetCards.Add(preset.Id, new PresetCard(preset));
            }
        }

        return [.. _presetCards.Values];
    }

    public void Remove(Guid id)
    {
        if (_presetCards.TryGetValue(id, out var card))
        {
            _presetCards.Remove(id);
            card.Dispose();
        }
    }
}
