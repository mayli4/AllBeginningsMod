using AllBeginningsMod.Core.Loaders;
using AllBeginningsMod.Common.PrimitiveDrawing;
using AllBeginningsMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AllBeginningsMod.Content.NPCs.Enemies.Bosses.Bastroboy; 

internal class PhantomProjectile : ModProjectile {
    public override string Texture => Assets.Assets.Textures.NPCs.Bosses.Bastroboy.KEY_PhantomProjectile;
    
    private int Style => (int)Projectile.ai[0];

    private PrimitiveTrail trail;
    private Texture2D trailTexture;
    private Effect effect;
    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Type] = 20;
        ProjectileID.Sets.TrailingMode[Type] = 2;
        Main.projFrames[Type] = 6;
    }

    public override void SetDefaults() {
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.tileCollide = false;
        Projectile.Size = new Vector2(25, 25);
        Projectile.alpha = 255;
        Projectile.timeLeft = 200;
    }

    public override void AI() {
        Projectile.alpha = 255 - (int)((-MathF.Pow(2f * (Projectile.timeLeft / 200f) - 1, 4) + 1) * 255f);

        Projectile.rotation = Projectile.velocity.ToRotation();

        trail ??= new PrimitiveTrail(
            Projectile.oldPos,
            factor => 20f + 4f * MathF.Sin(Main.GameUpdateCount * 0.5f),
            factor => GetAlpha(Lighting.GetColor(Projectile.Center.ToTileCoordinates())).Value,
            initPosition: Projectile.Center
        );

        MathUtilities.ClosestPlayer(Projectile.Center, out Player player);
        Vector2 direction = Projectile.Center.DirectionTo(player.Center);

        Projectile.velocity += direction.RotatedBy(MathF.Sin(Main.GameUpdateCount * 0.05f) * MathHelper.PiOver2) * 0.2f;

        int frameTime = 10;
        Projectile.frame = (Projectile.frameCounter / frameTime) + (Style == 1 ? 3 : 0);
        if(++Projectile.frameCounter >= 3 * frameTime) {
            Projectile.frameCounter = 0;
        }

        // Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.HallowedTorch);
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        Projectile.velocity = target.Center.DirectionTo(Projectile.Center) * Projectile.velocity.Length();
    }

    public override Color? GetAlpha(Color lightColor) {
        return lightColor;
    }

    public override bool PreDraw(ref Color lightColor) {
        Color color = lightColor * ((255 - Projectile.alpha) / 255f);
        effect ??= EffectLoader.GetEffect("Trail::Default");
        trailTexture ??= ModContent.Request<Texture2D>(Texture + "_Trail", AssetRequestMode.ImmediateLoad).Value;

        effect.Parameters["sampleTexture"].SetValue(trailTexture);
        effect.Parameters["transformationMatrix"].SetValue(
            Matrix.CreateTranslation(
                -Main.screenPosition.X + Projectile.Size.X / 2f,
                -Main.screenPosition.Y + Projectile.Size.Y / 2f,
                0f
            )
            * Main.GameViewMatrix.TransformationMatrix
            * Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1)
        );
        effect.Parameters["color"].SetValue(color.ToVector4());

        trail.Draw(effect);

        Texture2D texture = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
        Rectangle source = Projectile.SourceRectangle();
        Main.spriteBatch.Draw(
            texture,
            Projectile.Center - Main.screenPosition,
            source,
            color,
            Projectile.rotation + MathHelper.PiOver2,
            source.Size() / 2f,
            Projectile.scale + 0.1f * MathF.Sin(Main.GameUpdateCount * 0.5f),
            SpriteEffects.None,
            0f
        );
        return false;
    }
}