using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace PortraitHelper.Extensions;

public static class ExportedPortraitDataExtensions
{
    public static bool ToPortraitPreset(this ref ExportedPortraitData portraitData, BannerPreset preset)
    {
        preset.CameraPosition = portraitData.CameraPosition;
        preset.CameraTarget = portraitData.CameraTarget;
        preset.ImageRotation = portraitData.ImageRotation;
        preset.CameraZoom = portraitData.CameraZoom;
        preset.BannerTimeline = portraitData.BannerTimeline;
        preset.AnimationProgress = portraitData.AnimationProgress;
        preset.Expression = portraitData.Expression;
        preset.HeadDirection = portraitData.HeadDirection;
        preset.EyeDirection = portraitData.EyeDirection;
        preset.DirectionalLightingColorRed = portraitData.DirectionalLightingColorRed;
        preset.DirectionalLightingColorGreen = portraitData.DirectionalLightingColorGreen;
        preset.DirectionalLightingColorBlue = portraitData.DirectionalLightingColorBlue;
        preset.DirectionalLightingBrightness = portraitData.DirectionalLightingBrightness;
        preset.DirectionalLightingVerticalAngle = portraitData.DirectionalLightingVerticalAngle;
        preset.DirectionalLightingHorizontalAngle = portraitData.DirectionalLightingHorizontalAngle;
        preset.AmbientLightingColorRed = portraitData.AmbientLightingColorRed;
        preset.AmbientLightingColorGreen = portraitData.AmbientLightingColorGreen;
        preset.AmbientLightingColorBlue = portraitData.AmbientLightingColorBlue;
        preset.AmbientLightingBrightness = portraitData.AmbientLightingBrightness;
        preset.BannerBg = portraitData.BannerBg;
        return true;
    }

    public static bool FromPortraitPreset(this ref ExportedPortraitData portraitData, BannerPreset preset)
    {
        portraitData.CameraPosition = preset.CameraPosition;
        portraitData.CameraTarget = preset.CameraTarget;
        portraitData.ImageRotation = preset.ImageRotation;
        portraitData.CameraZoom = preset.CameraZoom;
        portraitData.BannerTimeline = preset.BannerTimeline;
        portraitData.AnimationProgress = preset.AnimationProgress;
        portraitData.Expression = preset.Expression;
        portraitData.HeadDirection = preset.HeadDirection;
        portraitData.EyeDirection = preset.EyeDirection;
        portraitData.DirectionalLightingColorRed = preset.DirectionalLightingColorRed;
        portraitData.DirectionalLightingColorGreen = preset.DirectionalLightingColorGreen;
        portraitData.DirectionalLightingColorBlue = preset.DirectionalLightingColorBlue;
        portraitData.DirectionalLightingBrightness = preset.DirectionalLightingBrightness;
        portraitData.DirectionalLightingVerticalAngle = preset.DirectionalLightingVerticalAngle;
        portraitData.DirectionalLightingHorizontalAngle = preset.DirectionalLightingHorizontalAngle;
        portraitData.AmbientLightingColorRed = preset.AmbientLightingColorRed;
        portraitData.AmbientLightingColorGreen = preset.AmbientLightingColorGreen;
        portraitData.AmbientLightingColorBlue = preset.AmbientLightingColorBlue;
        portraitData.AmbientLightingBrightness = preset.AmbientLightingBrightness;
        portraitData.BannerBg = preset.BannerBg;
        return true;
    }
}
