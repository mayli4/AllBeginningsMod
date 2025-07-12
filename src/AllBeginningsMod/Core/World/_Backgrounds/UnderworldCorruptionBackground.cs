using AllBeginningsMod.Content.Biomes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace AllBeginningsMod.Core.World;

//this sucks soo bad dude, i dont wanna have to touych this ever again
// most code taken from lion8cake with permission

public class UnderworldCorruptionBackground : ModSystem {
    public Asset<Texture2D>[] BackgroundTextures = new Asset<Texture2D>[5];

    public override void PostSetupContent() {
        if (Main.dedServ)
            return;
        
        for (int i = 0; i < BackgroundTextures.Length; i++) {
            BackgroundTextures[i] = ModContent.Request<Texture2D>("AllBeginningsMod/Assets/Textures/Backgrounds/UnderworldCorruption/UnderworldCorruptionBackground_" + i);
        }
    }

    public override void Load() {
        IL_Main.DrawBG += UnderworldCorruptionBackground_DrawBG;
        IL_Main.DrawCapture += UnderworldCorruptionBackground_DrawCapture;
    }

    private void UnderworldCorruptionBackground_DrawBG(ILContext il) {
        ILCursor c = new ILCursor(il);
        c.GotoNext(
            MoveType.After,
            i => i.MatchLdarg0(),
            i => i.MatchLdcI4(0),
            i => i.MatchCall<Main>("DrawUnderworldBackground")
        );
        c.EmitDelegate(() => {
            DrawCorruptionUnderworldBackground(false);
        });
    }

    private void UnderworldCorruptionBackground_DrawCapture(ILContext il) {
        ILCursor c = new ILCursor(il);
        c.GotoNext(
            MoveType.After,
            i => i.MatchLdarg0(),
            i => i.MatchLdcI4(1),
            i => i.MatchCall<Main>("DrawUnderworldBackground")
        );
        c.EmitDelegate(() => {
            DrawCorruptionUnderworldBackground(true);
        });
    }

    protected void DrawCorruptionUnderworldBackground(bool flat) {
        if (!Main.LocalPlayer.InModBiome<UnderworldCorruptionBiome>())
            return;

        if (!(Main.screenPosition.Y + Main.screenHeight < (Main.maxTilesY - 220) * 16f)) {
            Vector2 screenOffset = Main.screenPosition + new Vector2(
                (Main.screenWidth >> 1),
                (Main.screenHeight >> 1)
            );
            
            float pushUp = (Main.GameViewMatrix.Zoom.Y - 1f) * 0.5f * 200f;
            SkyManager.Instance.ResetDepthTracker();

            int[] drawOrder = { 0, 1, 2, 3, 4 };

            foreach (int textureIndex in drawOrder) {
                DrawCorruptionUnderworldBackgroundLayer(
                    flat,
                    screenOffset,
                    pushUp,
                    textureIndex,
                    1f
                );
            }

            if (!Main.mapFullscreen) {
                SkyManager.Instance.DrawRemainingDepth(Main.spriteBatch);
            }
        }
    }

    private void DrawCorruptionUnderworldBackgroundLayer(bool flat, Vector2 screenOffset, float pushUp, int layerTextureIndex, float alpha) {
        var value = BackgroundTextures[layerTextureIndex].Value;

        Vector2 textureCenter = new Vector2(value.Width, value.Height) * 0.5f;
        Rectangle sourceRectangle = new(0, 0, value.Width, value.Height);
        float scaleFactor = 1.3f;
        Vector2 positionOffset = Vector2.Zero;

        int effectiveOldIndex = layerTextureIndex + 4;
        
        float parallaxFactor = (flat ? 1f : (effectiveOldIndex * 2 + 3f));
        Vector2 inverseParallax = new(1f / parallaxFactor);

        switch (layerTextureIndex) {
            case 0:
                scaleFactor = 0.5f;
                positionOffset.Y -= 0f;
                break;
            case 1:
                int frame = (int)(Main.GlobalTimeWrappedHourly * 8f) % 4;
                sourceRectangle = new(
                    frame % 2 * (value.Width >> 1),
                    (frame >> 1) * (value.Height >> 1),
                    value.Width >> 1,
                    value.Height >> 1
                );
                textureCenter = new Vector2(sourceRectangle.Width, sourceRectangle.Height) * 0.5f;
                positionOffset.Y += 90f;
                break;
            case 2:
                {
                    int num13 = (int)(Main.GlobalTimeWrappedHourly * 8f) % 4;
                    sourceRectangle = new(
                        num13 % 2 * (value.Width >> 1),
                        (num13 >> 1) * (value.Height >> 1),
                        value.Width >> 1,
                        value.Height >> 1
                    );
                    textureCenter = new Vector2(sourceRectangle.Width, sourceRectangle.Height) * 0.5f;
                    positionOffset.Y += 90f;
                    break;
                }
            case 3:
                {
                    int num12 = (int)(Main.GlobalTimeWrappedHourly * 8f) % 4;
                    sourceRectangle = new(
                        num12 % 2 * (value.Width >> 1),
                        (num12 >> 1) * (value.Height >> 1),
                        value.Width >> 1,
                        value.Height >> 1
                    );
                    textureCenter = new Vector2(sourceRectangle.Width, sourceRectangle.Height) * 0.5f;
                    positionOffset.X -= 530f;
                    positionOffset.Y -= 90f;
                    break;
                }
            case 4:
                {
                    int num11 = (int)(Main.GlobalTimeWrappedHourly * 8f) % 4;
                    sourceRectangle = new(
                        num11 % 2 * (value.Width >> 1),
                        (num11 >> 1) * (value.Height >> 1),
                        value.Width >> 1,
                        value.Height >> 1
                    );
                    textureCenter = new Vector2(sourceRectangle.Width, sourceRectangle.Height) * 0.5f;
                    positionOffset.Y += 90f;
                    break;
                }
        }
        
        if (flat) {
            scaleFactor *= 1.5f;
        }
        
        textureCenter *= scaleFactor; 

        SkyManager.Instance.DrawToDepth(Main.spriteBatch, 1f / inverseParallax.X);

        if (flat) {
            positionOffset.Y += (BackgroundTextures[0].Height() >> 1) * 1.3f - textureCenter.Y;
        }

        positionOffset.Y -= pushUp;

        float scaledWidth = scaleFactor * sourceRectangle.Width;
        int startTileX = (int)(
            (screenOffset.X * inverseParallax.X - textureCenter.X + positionOffset.X -
                (Main.screenWidth >> 1)) / scaledWidth
        );

        var tilesToDraw = (int)Math.Ceiling(Main.screenWidth / scaledWidth) + 2;

        var drawPosition = (new Vector2(startTileX * scaledWidth, Main.UnderworldLayer * 16f) 
            + textureCenter - screenOffset) 
            * inverseParallax 
            + screenOffset 
            - Main.screenPosition - textureCenter 
            + positionOffset;

        drawPosition = drawPosition.Floor();

        while (drawPosition.X + scaledWidth < 0f) {
            startTileX++;
            drawPosition.X += scaledWidth;
        }

        for (int i = startTileX - 2; i <= startTileX + tilesToDraw + 2; i++) {
            Color textureColor = Color.White * alpha;

            Main.spriteBatch.Draw(
                value,
                drawPosition,
                sourceRectangle,
                textureColor,
                0f,
                Vector2.Zero,
                scaleFactor,
                SpriteEffects.None,
                0f
            );

            drawPosition.X += scaledWidth;
        }
    }
}