using AllBeginningsMod.Common;
using AllBeginningsMod.Common.Rendering;
using AllBeginningsMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace AllBeginningsMod.Content.Projectiles; 
internal class ExplosionVFXProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Item_0";
    public static void Spawn(IEntitySource source, Vector2 position, Color flashColor, Color glowColor, Func<float, Color> smokeColor, int size, int timeLeft) {
        ExplosionVFXProjectile projectile = (ExplosionVFXProjectile)Projectile.NewProjectileDirect(
            source,
            position,
            Vector2.Zero,
            ModContent.ProjectileType<ExplosionVFXProjectile>(),
            0,
            0f
        ).ModProjectile;

        projectile.Projectile.timeLeft = projectile.maxTimeLeft = timeLeft;
        projectile.Projectile.width = projectile.Projectile.height = size;
        projectile.Projectile.Center = position;
        projectile.flashColor = flashColor;
        projectile.glowColor = glowColor;
        projectile.smokeColor = smokeColor;
        projectile.Projectile.rotation = Main.rand.NextFloatDirection() * 14f;
    }

    private int maxTimeLeft;
    private Color flashColor;
    private Color glowColor;
    private Func<float, Color> smokeColor;
    private float Progress => 1f - (float)Projectile.timeLeft / maxTimeLeft;

    public override void SetDefaults() {
        Projectile.aiStyle = -1;
        Projectile.penetrate = -1;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.hide = true;
    }

    private bool runAIOnSpawn = true;
    public override void AI() {
        if(runAIOnSpawn) {
            for(int i = 0; i < Projectile.width * 0.1f; i++) {
                Dust.NewDustPerfect(
                    Main.rand.NextVector2FromRectangle(Projectile.Hitbox),
                    Main.rand.NextFromList(DustID.Smoke, DustID.Torch),
                    Scale: Main.rand.NextFloat(1f, 1.5f)
                );
            }

            if(!Main.dedServ) {
                Lighting.AddLight(Projectile.Center, 0.1f * Projectile.width * flashColor.ToVector3());
            }

            runAIOnSpawn = false;
        }

        Projectile.velocity.Y -= 0.005f;
        Projectile.rotation += 0.005f;
        Projectile.Resize(Projectile.width + 6, Projectile.height + 6);
    }

    public override bool ShouldUpdatePosition() {
        return false;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        overWiresUI.Add(index);
    }

    private Texture2D glowTexture;
    private Texture2D flareTexture;
    private Texture2D smokeTexture;
    private Texture2D noiseTexture1;
    private Texture2D noiseTexture2;
    private Effect effect;
    
    public override bool PreDraw(ref Color lightColor) {
        glowTexture = Assets.Assets.Textures.Sample.Glow1.Value;
        flareTexture = Assets.Assets.Textures.Sample.Flare1.Value;
        smokeTexture = Assets.Assets.Textures.Sample.SmokeGlow.Value;
        noiseTexture1 = Assets.Assets.Textures.Sample.PerlinNoise.Value;
        noiseTexture2 = Assets.Assets.Textures.Sample.Noise2.Value;

        Renderer.QueueRenderAction(() =>
        {
            effect ??= Assets.Assets.Effects.Compiled.Pixel.ExplosionSmoke.Value;

            effect.Parameters["progress"].SetValue(Progress);
            effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.002f + Projectile.rotation);

            effect.Parameters["noiseTexture1"].SetValue(noiseTexture1);
            effect.Parameters["noiseScale1"].SetValue(0.2f);
            effect.Parameters["smokeCut"].SetValue(0.15f);
            effect.Parameters["smokeCutSmoothness"].SetValue(0.7f);

            effect.Parameters["noiseTexture2"].SetValue(noiseTexture2);
            effect.Parameters["noiseScale2"].SetValue(0.3f);
            effect.Parameters["edgeColor"].SetValue(Color.Black.ToVector4());

            Main.spriteBatch.End(out SpriteBatchSnapshot snapshot);
            Main.spriteBatch.Begin(snapshot with { CustomEffect = effect });
            Main.spriteBatch.Draw(
                smokeTexture,
                Projectile.Center - Main.screenPosition,
                null,
                smokeColor(Progress),
                Projectile.rotation,
                smokeTexture.Size() * 0.5f,
                Projectile.Size * 0.0015f,
                SpriteEffects.None,
                0f
            );

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(snapshot);
        }, RenderLayer.Projectiles, style: RenderStyle.Pixelated);

        float flashScale = 1f - MathF.Min(0.15f, Progress) / 0.15f;

        Main.spriteBatch.End(out SpriteBatchSnapshot snapshot);
        Main.spriteBatch.Begin(snapshot with { BlendState = BlendState.Additive });
        Main.spriteBatch.Draw(
            glowTexture,
            Projectile.Center - Main.screenPosition,
            null,
            glowColor * 0.9f,
            Projectile.rotation,
            glowTexture.Size() / 2f,
            0.003f * Projectile.width * flashScale,
            SpriteEffects.None,
                0f
            );

        Main.spriteBatch.Draw(
            flareTexture,
            Projectile.Center - Main.screenPosition + Main.rand.NextVector2Unit() * 5f,
            null,
            flashColor * 0.65f,
            Projectile.rotation,
            flareTexture.Size() / 2f,
            0.02f * Projectile.width * flashScale * Main.rand.NextFloat(1.5f),
            SpriteEffects.None,
            0f
        );

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(snapshot);

        // float blobScale = MathF.Pow(1f - MathF.Min(0.25f, Progress) / 0.25f, 2);
        //
        // FilterSystem.ApplyFilter(
        //     EffectLoader.GetFilter("Water"),
        //     effect =>
        //     {
        //         effect.Parameters["noise"].SetValue(
        //             ModContent.Request<Texture2D>("AllBeginningsMod/Assets/Textures/Sample/Noise3", AssetRequestMode.ImmediateLoad).Value
        //         );
        //
        //         effect.Parameters["strength"].SetValue(0.4f * blobScale);
        //         effect.Parameters["maxLen"].SetValue(0.0001f * Projectile.width);
        //         effect.Parameters["maxLenSmooth"].SetValue(0.15f);
        //         effect.Parameters["center"].SetValue(Projectile.Center - Main.screenPosition);
        //     }
        // );

        return false;
    }
}