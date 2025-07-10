using AllBeginningsMod.Core.Loaders;
using AllBeginningsMod.Common.Rendering;
using AllBeginningsMod.Content.Buffs;
using AllBeginningsMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AllBeginningsMod.Content.NPCs.Enemies.Nighttime; 
internal class DevilVampireAuraProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Item_0";
    private int maxTimeLeft = 1200;
    private float offsetRotation;
    private float progress;
    public override void SetDefaults() {
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.tileCollide = false;
        Projectile.width = Projectile.height = 300;
        Projectile.timeLeft = maxTimeLeft;
        Projectile.penetrate = -1;
        Projectile.knockBack = 0f;
    }

    public override void AI() {
        if(offsetRotation == 0) {
            offsetRotation = Main.rand.NextFloat(0.01f, MathHelper.TwoPi);
        }

        Projectile.position.Y -= 0.2f;
        progress = 1f - (float)Projectile.timeLeft / maxTimeLeft;
        if(progress < 0.3f) {
            if(Projectile.timeLeft % (int)(15f * progress + 1f) == 0) {
                Dust.NewDust(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    DustID.Demonite
                );
            }

            if(Projectile.friendly) {
                MathUtilities.ForEachNPCInRange(Projectile.Center, Projectile.width / 2f, npc =>
                {
                    npc.AddBuff(ModContent.BuffType<OnGasDebuff>(), 120);
                });
            }

            if(Projectile.hostile) {
                MathUtilities.ForEachPlayerInRange(Projectile.Center, Projectile.width / 2f, player =>
                {
                    player.AddBuff(ModContent.BuffType<OnGasDebuff>(), 120);
                });
            }
        }
    }

    public override bool CanHitPlayer(Player target) {
        return false;
    }

    public override bool? CanHitNPC(NPC target) {
        return false;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        overPlayers.Add(index);
    }

    private Texture2D baseTexture;
    private Texture2D noiseTexture1;
    private Texture2D noiseTexture2;
    public override bool PreDraw(ref Color lightColor) {
        baseTexture ??= Mod.Assets.Request<Texture2D>("Assets/Textures/Sample/SmokeGlow", AssetRequestMode.ImmediateLoad).Value;
        noiseTexture1 ??= Mod.Assets.Request<Texture2D>("Assets/Textures/Sample/Noise2", AssetRequestMode.ImmediateLoad).Value;
        noiseTexture2 ??= Mod.Assets.Request<Texture2D>("Assets/Textures/Sample/PortalNoise", AssetRequestMode.ImmediateLoad).Value;

        Renderer.QueueRenderAction(() =>
        {
            float effectProgress = MathF.Min(-MathF.Pow(progress / 2f - 1f, 2) + 1f, 1f);

            Effect effect = EffectLoader.GetEffect("Pixel::ExplosionSmoke");
            effect.Parameters["progress"].SetValue(effectProgress);
            effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.0001f + offsetRotation);

            effect.Parameters["noiseTexture1"].SetValue(noiseTexture1);
            effect.Parameters["noiseScale1"].SetValue(1f);
            effect.Parameters["smokeCut"].SetValue(0.05f);
            effect.Parameters["smokeCutSmoothness"].SetValue(0.4f);

            effect.Parameters["noiseTexture2"].SetValue(noiseTexture2);
            effect.Parameters["noiseScale2"].SetValue(1f);
            effect.Parameters["edgeColor"].SetValue((Color.Black * (1f - effectProgress)).ToVector4());

            float size = Projectile.width + 250;

            Main.spriteBatch.End(out SpriteBatchData snapshot);
            Main.spriteBatch.Begin(snapshot with { Effect = effect });
            Main.spriteBatch.Draw(
                baseTexture,
                new Rectangle(
                    (int)(Projectile.Center.X - size / 2f - Main.screenPosition.X),
                    (int)(Projectile.Center.Y - size / 2f - Main.screenPosition.Y),
                    (int)size,
                    (int)size
                ),
                Color.Lerp(Color.DarkViolet, Color.Transparent, progress + 0.5f)
            );
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(snapshot);
        }, RenderLayer.Projectiles, style: RenderStyle.Pixelated);

        return false;
    }
}