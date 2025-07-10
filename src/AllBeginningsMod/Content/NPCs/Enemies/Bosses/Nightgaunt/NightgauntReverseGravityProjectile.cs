using AllBeginningsMod.Core.Loaders;
using AllBeginningsMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace AllBeginningsMod.Content.NPCs.Enemies.Bosses.Nightgaunt; 

internal class NightgauntReverseGravityProjectile : ModProjectile {
    public override string Texture => Assets.Assets.Textures.NPCs.Bosses.Nightgaunt.KEY_NightgauntReverseGravityProjectile;
    
    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Type] = 10;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    public override void SetDefaults() {
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.tileCollide = false;
        Projectile.Size = new Vector2(25, 45);
        Projectile.timeLeft = 400;
        Projectile.alpha = 255;
        Projectile.penetrate = -1;
    }

    public override void OnSpawn(IEntitySource source) {
        RotationOffset = Main.rand.NextFloat(MathHelper.PiOver2);
        Projectile.scale = Main.rand.NextFloat(0.85f, 1.1f);
        Projectile.netUpdate = true;
    }

    private ref float RotationOffset => ref Projectile.ai[0];
    public override void AI() {
        Projectile.velocity.Y -= 0.06f;
        if(Projectile.timeLeft > 60) {
            if(Projectile.alpha > 0) {
                Projectile.alpha -= 2;
            }
        }
        else {
            Projectile.alpha = 255 - (int)(255f * Projectile.timeLeft / 60f);
        }

        Projectile.frameCounter++;
        if(Projectile.frameCounter > 5) {
            Projectile.frameCounter = 0;
            Projectile.frame++;
            if(Projectile.frame > 5) {
                Projectile.frame = 0;
            }
        }
        Projectile.rotation = MathF.Sin(Projectile.timeLeft * 0.08f + RotationOffset) * 0.2f;
        Projectile.position += Projectile.velocity.RotatedBy(Projectile.rotation);

        if(!Main.dedServ) {
            if(Projectile.timeLeft % 8 == 0) {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.MagicMirror);
            }
            Lighting.AddLight(Projectile.Center - Vector2.UnitY * 30f, new Vector3(185, 140, 183) * 0.0025f);
        }
    }

    public override bool ShouldUpdatePosition() {
        return false;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Texture2D colorMaskTexture = ModContent.Request<Texture2D>(Texture + "_ColorMask", AssetRequestMode.ImmediateLoad).Value;
        Texture2D glowTexture = Mod.Assets.Request<Texture2D>("Assets/Textures/Misc/Glow2", AssetRequestMode.ImmediateLoad).Value;
        Rectangle source = new(0, Projectile.frame * 76, 50, 76);
        Color color = new(185, 140, 183);
        float alpha = 1f - Projectile.alpha / 255f;

        for(int i = 0; i < Projectile.oldPos.Length; i++) {
            float factor = (float)i / Projectile.oldPos.Length;
            Main.spriteBatch.Draw(
                colorMaskTexture,
                Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition + Vector2.UnitY * factor * 10f,
                source,
                Color.Black * alpha * (1f - factor) * 0.045f,
                Projectile.oldRot[i],
                source.Size() / 2f,
                Projectile.scale,
                SpriteEffects.None,
                0f
            );
        }

        Main.spriteBatch.Draw(
            texture,
            Projectile.Center - Main.screenPosition,
            source,
            Color.White * alpha,
            Projectile.rotation,
            source.Size() / 2f,
            Projectile.scale,
            SpriteEffects.None,
            0f
        );

        SpriteBatchData snapshot = Main.spriteBatch.Capture();
        Main.spriteBatch.End();

        Effect effect = EffectLoader.GetEffect("Pixel::JellyfishYe");
        effect.Parameters["time"].SetValue(Projectile.timeLeft * 0.05f + RotationOffset * 4f);
        effect.Parameters["uImageSize0"].SetValue(colorMaskTexture.Size());
        effect.Parameters["uSourceRect"].SetValue(new Vector4(source.X, source.Y, source.Width, source.Height));
        Main.spriteBatch.Begin(snapshot with { Effect = effect });

        Main.spriteBatch.Draw(
            colorMaskTexture,
            Projectile.Center - Main.screenPosition,
            source,
            color * alpha,
            Projectile.rotation,
            source.Size() / 2f,
            Projectile.scale,
            SpriteEffects.None,
            0f
        );

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.Additive);

        Main.spriteBatch.Draw(
            glowTexture,
            Projectile.Center - Main.screenPosition - Vector2.UnitY * 20f,
            null,
            color * alpha * 0.5f,
            Projectile.rotation,
            glowTexture.Size() / 2f,
            Projectile.scale,
            SpriteEffects.None,
            0f

        );

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(snapshot);

        return false;
    }
}