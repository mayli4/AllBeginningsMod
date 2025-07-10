using AllBeginningsMod.Core.Loaders;
using AllBeginningsMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace AllBeginningsMod.Common.Rendering; 
internal enum RenderLayer : byte {
    Tiles,
    Projectiles,
    NPCs,
    Players,
}

internal enum RenderOrder : byte {
    Over,
    Under,
}

internal enum RenderStyle {
    Default,
    Pixelated
}

internal class Renderer : ILoadable {
    /// <summary>
    /// You can be sure that the Main.spriteBatch was already started when running the action (unless an action before ended it then gg).
    /// </summary>
    /// <param name="action">The render action.</param>
    /// <param name="layer"></param>
    /// <param name="order"></param>
    /// <param name="style"></param>
    public static void QueueRenderAction(
        Action action,
        RenderLayer layer,
        RenderOrder order = RenderOrder.Under,
        RenderStyle style = RenderStyle.Default
    ) {
        int idx = (int)layer * 2 + (order == RenderOrder.Over ? 1 : 0);
        RenderActions[idx].Add((action, style));
    }

    // There are 4 layers * 2 types of ordering.
    private static readonly List<(Action, RenderStyle)>[] RenderActions
        = Enumerable.Range(0, 8).Select(_ => new List<(Action, RenderStyle)>()).ToArray();

    public void Load(Mod mod) {
        On_Main.DrawNPCs += On_Main_DrawNPCs;
        On_Main.DrawSuperSpecialProjectiles += On_Main_DrawSuperSpecialProjectiles;
        On_Main.DrawPlayers_AfterProjectiles += On_Main_DrawPlayers_AfterProjectiles;
        On_Main.DrawCachedProjs += On_Main_DrawCachedProjs;
    }

    public void Unload() {
        On_Main.DrawNPCs -= On_Main_DrawNPCs;
        On_Main.DrawSuperSpecialProjectiles -= On_Main_DrawSuperSpecialProjectiles;
        On_Main.DrawPlayers_AfterProjectiles -= On_Main_DrawPlayers_AfterProjectiles;
        On_Main.DrawCachedProjs -= On_Main_DrawCachedProjs;
    }

    private static void RunRenderActions(RenderLayer layer, RenderOrder order, bool shouldStartSpriteBatch) {
        int idx = (int)layer * 2 + (order == RenderOrder.Over ? 1 : 0);
        if(shouldStartSpriteBatch) {
            Main.spriteBatch.Begin(SpriteBatchData.Default());
        }

        foreach((Action action, RenderStyle style) in RenderActions[idx]) {
            switch(style) {
                case RenderStyle.Default:
                    action();
                    break;
                case RenderStyle.Pixelated:
                    Effect effect = EffectLoader.GetEffect("Pixel::Pixelate");
                    effect.Parameters["size"].SetValue(Main.ScreenSize.ToVector2());
                    effect.Parameters["resolution"].SetValue(2);
                    effect.Parameters["stepMin"].SetValue(0.3f);
                    effect.Parameters["stepMax"].SetValue(0.8f);

                    RenderWithEffect(action, effect);
                    break;
            }
        }

        RenderActions[idx].Clear();

        if(shouldStartSpriteBatch) {
            Main.spriteBatch.End();
        }
    }

    private static readonly PropertyInfo RenderTargetUsageProperty;
    static Renderer() {
        RenderTargetUsageProperty = typeof(RenderTarget2D).GetProperty("RenderTargetUsage", ReflectionUtilities.FlagsPublicInstance);
    }

    private static RenderTarget2D effectTarget;
    private static void RenderWithEffect(Action action, Effect effect) {
        SpriteBatch spriteBatch = Main.spriteBatch;
        GraphicsDevice device = Main.spriteBatch.GraphicsDevice;

        if(effectTarget is null || (effectTarget.Width, effectTarget.Height) != (Main.screenWidth, Main.screenHeight)) {
            effectTarget = new RenderTarget2D(
                device,
                Main.screenWidth,
                Main.screenHeight
            );
        }

        spriteBatch.End(out SpriteBatchData data);
        RenderTargetBinding[] bindings = device.GetRenderTargets();

        // This is so that the already drawn stuff doesn't magically dissapear when changing render targets.
        bool discardContents = ((RenderTarget2D)bindings[0].RenderTarget).RenderTargetUsage == RenderTargetUsage.DiscardContents;
        if(discardContents) {
            RenderTargetUsageProperty.SetValue(bindings[0].RenderTarget, RenderTargetUsage.PreserveContents);
        }

        device.SetRenderTarget(effectTarget);
        device.Clear(Color.Transparent);

        spriteBatch.Begin(SpriteBatchData.Default() with { TransformMatrix = Main.GameViewMatrix.EffectMatrix });
        action();
        spriteBatch.End();

        device.SetRenderTargets(bindings);

        if(discardContents) {
            RenderTargetUsageProperty.SetValue(bindings[0].RenderTarget, RenderTargetUsage.DiscardContents);
        }

        spriteBatch.Begin(SpriteBatchData.Default() with { Effect = effect });
        spriteBatch.Draw(effectTarget, Vector2.Zero, Color.White);
        spriteBatch.End();

        spriteBatch.Begin(data);
    }

    private void On_Main_DrawCachedProjs(On_Main.orig_DrawCachedProjs orig, Main self, List<int> projCache, bool startSpriteBatch) {
        orig(self, projCache, startSpriteBatch);
        RunRenderActions(RenderLayer.Projectiles, RenderOrder.Over, startSpriteBatch);
    }

    private void On_Main_DrawPlayers_AfterProjectiles(On_Main.orig_DrawPlayers_AfterProjectiles orig, Main self) {
        RunRenderActions(RenderLayer.Players, RenderOrder.Under, true);
        orig(self);
        RunRenderActions(RenderLayer.Players, RenderOrder.Over, true);
    }

    private void On_Main_DrawSuperSpecialProjectiles(On_Main.orig_DrawSuperSpecialProjectiles orig, Main self, List<int> projCache, bool startSpriteBatch) {
        RunRenderActions(RenderLayer.Projectiles, RenderOrder.Under, startSpriteBatch);
        orig(self, projCache, startSpriteBatch);
    }

    private void On_Main_DrawNPCs(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles) {
        if(behindTiles) {
            RunRenderActions(RenderLayer.Tiles, RenderOrder.Under, false);
        }

        RunRenderActions(RenderLayer.Tiles, RenderOrder.Over, false);
        RunRenderActions(RenderLayer.NPCs, RenderOrder.Under, false);
        orig(self, behindTiles);
        RunRenderActions(RenderLayer.NPCs, RenderOrder.Over, false);
    }
}