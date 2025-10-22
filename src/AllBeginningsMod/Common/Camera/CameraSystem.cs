using AllBeginningsMod.Common.Config;
using AllBeginningsMod.Utilities;
using System;
using System.Collections.Generic;
using Terraria.Graphics;
using Terraria.Utilities;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace AllBeginningsMod.Common.Camera;

#nullable enable

public struct CameraModifierDescription() {
    public required string Identifier { get; set; }
    public required float Duration { get; set; } = 0f;
    public Func<Vector2?>? MoveTargetCallback { get; set; }
    public float? ZoomAdjustment { get; set; }
    public float? ShakeIntensity { get; set; }
    public float FadeInLength { get; set; } = 0.5f;
    public float FadeOutLength { get; set; } = 0.5f;
}

[Autoload(Side = ModSide.Client)]
internal sealed class CameraSystem : ModSystem {
    public static bool IsScreenshakeEnabled => ClientConfig.Instance.EnableScreenshake;

    internal static readonly GenerationalArena<ModifierHandle, ModifierData> ActiveCameraModifiers = new(initialCapacity: 16);
    private static readonly Dictionary<string, ModifierHandle> _modifierHandles = new();

    private static Vector2 _smoothedCameraOffset;
    private static float _smoothedZoomAdjustment;

    private static bool _isOverridingVanillaCamera;

    private const float pos_smooth_speed = 0.12f;
    private const float zoom_smooth_speed = 0.08f;

    public static Vector2 ScreenSize { get; private set; }
    public static Vector2 ScreenHalf { get; private set; }
    public static Rectangle ScreenRect { get; private set; }
    public static Vector2 MouseWorld { get; private set; }
    public static Vector2 CurrentScreenCenter { get; private set; }
    public static float CurrentZoom { get; private set; } = 1f;

    internal struct ModifierInstance {
        public float SpawnTime;
        public float CurrentIntensity;
        public bool IsActive;
    }

    internal record struct ModifierData {
        public required CameraModifierDescription Description;
        public ModifierInstance Instance;
    }

    internal record struct ModifierHandle(uint Index, uint Version) : IHandle {
        public readonly bool IsValid => CameraSystem.ActiveCameraModifiers.IsValid(this);
        public readonly ref readonly ModifierInstance Instance => ref CameraSystem.ActiveCameraModifiers.Get(this).Instance;
        public readonly ref readonly CameraModifierDescription Description => ref CameraSystem.ActiveCameraModifiers.Get(this).Description;

        public readonly void Remove() {
            if(IsValid) {
                CameraSystem.RemoveModifier(Description.Identifier);
            }
        }
    }

    internal struct CameraParams {
        public Vector2 WeightedPositionTargetSum;
        public float TotalPositionWeight;

        public float TotalZoomAdjustment;
        public float MaxShakeIntensity;
        public float TotalOverallIntensityWeight;
    }

    public override void Load() {
        On_Main.DoDraw_UpdateCameraPosition += DoDraw_UpdateCameraPositionHook;
        Main.OnPostDraw += PostDrawUpdate;
    }

    public override void Unload() {
        On_Main.DoDraw_UpdateCameraPosition -= DoDraw_UpdateCameraPositionHook;
        Main.OnPostDraw -= PostDrawUpdate;

        ActiveCameraModifiers.Clear();
        _modifierHandles.Clear();
        _smoothedCameraOffset = Vector2.Zero;
        _smoothedZoomAdjustment = 0f;
        _isOverridingVanillaCamera = false;
    }

    public static ModifierHandle AddModifier(CameraModifierDescription description) {
        if(string.IsNullOrEmpty(description.Identifier)) {
            return default;
        }

        var newData = new ModifierData
        {
            Description = description,
            Instance = new ModifierInstance
            {
                SpawnTime = Main.GameUpdateCount,
                CurrentIntensity = 0f,
                IsActive = true
            }
        };

        if(_modifierHandles.TryGetValue(description.Identifier, out ModifierHandle existingHandle) && existingHandle.IsValid) {
            ref ModifierData dataToUpdate = ref ActiveCameraModifiers.Get(existingHandle);
            dataToUpdate = newData;
            _isOverridingVanillaCamera = true;
            return existingHandle;
        }
        else {
            ModifierHandle newHandle = ActiveCameraModifiers.Put(newData);
            _modifierHandles[description.Identifier] = newHandle;
            _isOverridingVanillaCamera = true;
            return newHandle;
        }
    }

    public static void RemoveModifier(string identifier) {
        if(string.IsNullOrEmpty(identifier)) return;

        if(_modifierHandles.TryGetValue(identifier, out var handle) && handle.IsValid) {
            ref ModifierData data = ref ActiveCameraModifiers.Get(handle);
            data.Instance.IsActive = false;
        }
    }

    private static void DoDraw_UpdateCameraPositionHook(On_Main.orig_DoDraw_UpdateCameraPosition orig) {
        orig();
        var baseCenter = Main.screenPosition + ScreenHalf;

        var accumulatedParams = new CameraParams();

        foreach(var handle in ActiveCameraModifiers) {
            ref readonly CameraModifierDescription description = ref handle.Description;
            ref readonly ModifierInstance instance = ref handle.Instance;

            float intensity = instance.CurrentIntensity;
            if(intensity <= 0f) continue;

            if(description.MoveTargetCallback?.Invoke() is { } desiredWorldPosition) {
                accumulatedParams.WeightedPositionTargetSum += desiredWorldPosition * intensity;
                accumulatedParams.TotalPositionWeight += intensity;
            }

            accumulatedParams.TotalOverallIntensityWeight += intensity;

            if(description.ZoomAdjustment.HasValue) {
                accumulatedParams.TotalZoomAdjustment += description.ZoomAdjustment.Value * intensity;
            }

            if(description.ShakeIntensity.HasValue) {
                accumulatedParams.MaxShakeIntensity = MathF.Max(accumulatedParams.MaxShakeIntensity, description.ShakeIntensity.Value * intensity);
            }
        }

        Vector2 finalTargetPosition;
        if(accumulatedParams.TotalPositionWeight > 0f) {
            finalTargetPosition = accumulatedParams.WeightedPositionTargetSum / accumulatedParams.TotalPositionWeight;
        }
        else {
            finalTargetPosition = baseCenter;
        }

        float finalZoomAdjustment = accumulatedParams.TotalZoomAdjustment;
        float finalMaxShakeIntensity = accumulatedParams.MaxShakeIntensity;
        bool anyModifierProvidingOverallWeight = accumulatedParams.TotalOverallIntensityWeight > 0f;

        if(anyModifierProvidingOverallWeight) {
            if(!_isOverridingVanillaCamera) {
                _smoothedCameraOffset = Vector2.Zero;
                _smoothedZoomAdjustment = 0f;
            }
            _isOverridingVanillaCamera = true;
        }
        else if(!_isOverridingVanillaCamera) {
            CurrentZoom = 1f;
            UpdateCache();
            return;
        }

        Vector2 desiredOffset;

        if(accumulatedParams.TotalPositionWeight > 0) {
            desiredOffset = finalTargetPosition - baseCenter;
        }
        else {
            desiredOffset = Vector2.Zero;
        }

        _smoothedCameraOffset = Vector2.Lerp(_smoothedCameraOffset, desiredOffset, pos_smooth_speed);
        _smoothedZoomAdjustment = MathHelper.Lerp(_smoothedZoomAdjustment, finalZoomAdjustment, zoom_smooth_speed);

        if(!anyModifierProvidingOverallWeight) {
            float offsetDistanceSq = _smoothedCameraOffset.LengthSquared();
            float zoomDiff = MathF.Abs(_smoothedZoomAdjustment - 0f);

            if(offsetDistanceSq < 0.1f && zoomDiff < 0.001f) {
                _smoothedCameraOffset = Vector2.Zero;
                _smoothedZoomAdjustment = 0f;
                _isOverridingVanillaCamera = false;
            }
        }

        //todo reevaluate later? maybe noise based, we inclue fnl anyways

        Vector2 shakeOffset = Vector2.Zero;
        if(finalMaxShakeIntensity > 0f) {
            float shakeX = Main.rand.NextFloat(-1f, 1f);
            float shakeY = Main.rand.NextFloat(-1f, 1f);
            shakeOffset = new Vector2(shakeX, shakeY) * finalMaxShakeIntensity;
        }

        var screenOffset = IsScreenshakeEnabled ? shakeOffset : Vector2.Zero;

        Main.screenPosition = (baseCenter + _smoothedCameraOffset) - ScreenHalf + screenOffset;

        CurrentZoom = 1f + _smoothedZoomAdjustment;
        CurrentZoom = MathHelper.Clamp(CurrentZoom, 0.5f, 2.0f);

        UpdateCache();
    }

    public override void ModifyTransformMatrix(ref SpriteViewMatrix transform) {
        transform.Zoom *= CurrentZoom;
    }

    private static void PostDrawUpdate(GameTime gameTime) {
        float currentTime = Main.GameUpdateCount;


        foreach(ModifierHandle handle in ActiveCameraModifiers) {
            ref ModifierData data = ref ActiveCameraModifiers.Get(handle);

            if(string.IsNullOrEmpty(data.Description.Identifier)) {
                continue;
            }

            float timeSinceSpawn = currentTime - data.Instance.SpawnTime;

            if(!data.Instance.IsActive) {
                float fadeOutTimePassed = timeSinceSpawn - data.Description.Duration - data.Description.FadeInLength;
                if(data.Description.FadeOutLength > 0f) {
                    data.Instance.CurrentIntensity = MathHelper.Lerp(1f, 0f, fadeOutTimePassed / data.Description.FadeOutLength);
                }
                else {
                    data.Instance.CurrentIntensity = 0f;
                }
            }
            else {
                if(timeSinceSpawn < data.Description.FadeInLength) {
                    data.Instance.CurrentIntensity = MathHelper.Lerp(0f, 1f, timeSinceSpawn / data.Description.FadeInLength);
                }
                else if(data.Description.Duration > 0f && timeSinceSpawn > data.Description.FadeInLength + data.Description.Duration) {
                    data.Instance.IsActive = false;
                    data.Instance.CurrentIntensity = 1f;
                }
                else {
                    data.Instance.CurrentIntensity = 1f;
                }
            }
            data.Instance.CurrentIntensity = MathHelper.Clamp(data.Instance.CurrentIntensity, 0f, 1f);

            bool canBeRemoved = !data.Instance.IsActive && data.Instance.CurrentIntensity <= 0.001f;
            if(canBeRemoved) {
                handle.Remove();
            }
        }

        if(Main.keyState.IsKeyDown(Keys.J) && !Main.oldKeyState.IsKeyDown(Keys.J)) {
            AddModifier(new CameraModifierDescription { Identifier = "TestShake", ShakeIntensity = 5f, Duration = 60f });
        }
    }

    private static void UpdateCache() {
        ScreenSize = new Vector2(Main.screenWidth, Main.screenHeight);
        ScreenHalf = ScreenSize * 0.5f;
        ScreenRect = new((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth, Main.screenHeight);
        CurrentScreenCenter = Main.screenPosition + ScreenHalf;
        MouseWorld = Main.MouseWorld;
    }
}