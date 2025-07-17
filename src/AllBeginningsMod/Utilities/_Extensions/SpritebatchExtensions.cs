using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.GameContent;

namespace AllBeginningsMod.Utilities;

public static class SpritebatchExtensions {
    public static void Begin(this SpriteBatch spriteBatch, SpriteBatchSnapshot data) {
        spriteBatch.Begin(
            data.SortMode,
            data.BlendState,
            data.SamplerState,
            data.DepthStencilState,
            data.RasterizerState,
            data.CustomEffect,
            data.TransformMatrix
        );
    }

    public static void EndBegin(this SpriteBatch spriteBatch, SpriteBatchSnapshot data) {
        spriteBatch.End();
        spriteBatch.Begin(data);
    }

    public static SpriteBatchSnapshot CaptureEndBegin(this SpriteBatch spriteBatch, SpriteBatchSnapshot data) {
        SpriteBatchSnapshot captureData = spriteBatch.Capture();
        spriteBatch.EndBegin(data);

        return captureData;
    }

    public static void InsertDraw(this SpriteBatch spriteBatch, SpriteBatchSnapshot data, Action<SpriteBatch> drawAction) {/*
        if ((bool)SpriteBatchCache.BeginCalled.GetValue(spriteBatch)) {
            SpriteBatchSnapshot rebeginData = spriteBatch.CaptureEndBegin(data);
            drawAction(spriteBatch);
            spriteBatch.EndBegin(rebeginData);
        }
        else {

        }*/

        var initData = spriteBatch.CaptureEndBegin(data);
        drawAction(spriteBatch);
        spriteBatch.EndBegin(initData);
    }

    public static SpriteBatchSnapshot Capture(this SpriteBatch spriteBatch) {
        return new(spriteBatch);
    }

    public static void DrawAdditive(this SpriteBatch spriteBatch,
        Texture2D texture,
        Vector2 position,
        Rectangle? source,
        Color color,
        float rotation,
        Vector2 origin,
        Vector2 scale,
        SpriteEffects effects) {
        var data = spriteBatch.Capture();
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

        spriteBatch.Draw(
            texture,
            position,
            source,
            color,
            rotation,
            origin,
            scale,
            effects,
            0
        );

        spriteBatch.End();
        spriteBatch.Begin(data);
    }

    public static void End(this SpriteBatch spriteBatch, out SpriteBatchSnapshot snapshot) {
        snapshot = spriteBatch.Capture();
        spriteBatch.End();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void DrawLine(this SpriteBatch sb, Vector2 start, Vector2 end, Color? color = null, int width = 1, Texture2D? texture = null) {
        var offset = end - start;
        float angle = (float)Math.Atan2(offset.Y, offset.X);
        var rect = new Rectangle(
            (int)Math.Round(start.X), (int)Math.Round(start.Y),
            (int)offset.Length(), width
        );

        sb.Draw(texture ?? TextureAssets.BlackTile.Value, rect, null, color ?? Color.White, angle, Vector2.Zero, SpriteEffects.None, 0f);
    }

    public static void DrawRect(this SpriteBatch sb, Rectangle rect, Color? color = null, int thickness = 1, Texture2D? texture = null) {
        var finalColor = color ?? Color.White;

        sb.Draw(
            texture,
            new Rectangle(rect.X, rect.Y, rect.Width, thickness),
            finalColor
        );

        sb.Draw(
            texture,
            new Rectangle(
                rect.X,
                rect.Y + rect.Height - thickness,
                rect.Width,
                thickness
            ),
            finalColor
        );

        sb.Draw(
            texture,
            new Rectangle(
                rect.X,
                rect.Y + thickness,
                thickness,
                rect.Height - (thickness * 2)
            ),
            finalColor
        );

        sb.Draw(
            texture,
            new Rectangle(
                rect.X + rect.Width - thickness,
                rect.Y + thickness,
                thickness,
                rect.Height - (thickness * 2)
            ),
            finalColor
        );
    }
}