namespace AllBeginningsMod.Common.Camera;

internal static class Modifiers {
    public static CameraModifierDescription SmallShake = new CameraModifierDescription { Identifier = "Small_Shake", Duration = 25f, ShakeIntensity = 0.25f };
    public static CameraModifierDescription SmallZoom = new CameraModifierDescription { Identifier = "Small_Zoom", Duration = 60f, ZoomAdjustment = 1f };
}