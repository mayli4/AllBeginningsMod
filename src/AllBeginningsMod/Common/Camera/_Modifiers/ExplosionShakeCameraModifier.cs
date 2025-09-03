using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Graphics.CameraModifiers;

namespace AllBeginningsMod.Common.Camera;

internal class ExplosionShakeCameraModifier : ICameraModifier {
    private float strength;
    private readonly float diminish;
    private readonly Vector2? sourcePosition;
    private readonly float? maxRange;
    private readonly string uniqueIdentity;
    
    public ExplosionShakeCameraModifier(float strength, float diminish, Vector2 sourcePosition, float maxRange, string uniqueIdentity = null) : this(strength, diminish, uniqueIdentity) {
        this.sourcePosition = sourcePosition;
        this.maxRange = maxRange;
    }

    public ExplosionShakeCameraModifier(float strength, float diminish, string uniqueIdentity = "") {
        this.strength = strength;
        this.diminish = diminish;
        this.uniqueIdentity = uniqueIdentity;
    }

    public string UniqueIdentity => uniqueIdentity;
    public bool Finished => strength < 0.001f;
    public void Update(ref CameraInfo cameraPosition) {
        float rangeMultiplier = 1f;
        if(maxRange is not null && sourcePosition is not null) {
            rangeMultiplier = 1f - MathF.Min(Main.LocalPlayer.DistanceSQ(sourcePosition.Value) / (maxRange.Value * maxRange.Value), 1f);
        }

        cameraPosition.CameraPosition += Main.rand.NextVector2Unit() * strength * rangeMultiplier;
        strength *= diminish;
    }
}