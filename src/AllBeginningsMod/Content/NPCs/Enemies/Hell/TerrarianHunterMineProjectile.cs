using AllBeginningsMod.Core.Loaders;
using AllBeginningsMod.Content.CameraModifiers;
using AllBeginningsMod.Content.Dusts;
using AllBeginningsMod.Content.Projectiles;
using AllBeginningsMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace AllBeginningsMod.Content.NPCs.Enemies.Hell; 

internal class TerrarianHunterMineProjectile : ModProjectile {
    public override string Texture => Assets.Assets.Textures.NPCs.Hell.KEY_TerrarianHunterMineProjectile;
    
    private int SizeType => (int)Projectile.ai[0];
    private float DetectionRadius => SizeType == 0 ? 45f : 60;
    private float detectionAlpha;
    public override void SetDefaults() {
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.tileCollide = false;
        Projectile.Size = SizeType == 0 ? new Vector2(15, 15) : new Vector2(25, 25);
        Projectile.timeLeft = 200;
        Projectile.penetrate = -1;
    }

    public override void AI() {
        if(Main.player.Any(player => player is not null && !player.dead && player.Hitbox.Intersects(Projectile.Center, DetectionRadius))) {
            Projectile.Kill();
            return;
        }

        if(MathUtilities.ClosestPlayer(Projectile.Center, out Player target, player => player.active && !player.dead) > -1) {
            Vector2 direction = Projectile.Center.DirectionTo(target.Center);
            Projectile.velocity = Projectile.velocity.RotatedBy(-Vector3.Cross(direction.ToVector3(), Projectile.velocity.ToVector3()).Z * 0.008f);
        }

        Projectile.velocity *= 0.995f;
        if(detectionAlpha < 1f) {
            detectionAlpha += 0.025f;
        }
    }

    public override void OnKill(int timeLeft) {
        if(Main.netMode != NetmodeID.MultiplayerClient) {
            MathUtilities.ForEachPlayerInRange(
                Projectile.Center,
                100,
                player => player.Hurt(
                    PlayerDeathReason.ByProjectile(player.whoAmI, Projectile.whoAmI),
                    Projectile.damage,
                    MathF.Sign(player.Center.X - Projectile.Center.X),
                    knockback: Projectile.knockBack
                )
            );
        }

        if(Main.dedServ) {
            return;
        }

        Vector2[] dustPositions = Projectile.Center.PositionsAround(
            SizeType == 0 ? 7 : 14,
            _ => Main.rand.NextFloat(10f, 15f),
            out Vector2[] dustDirections, Main.rand.NextFloat()
        );

        for(int i = 0; i < dustPositions.Length; i++) {
            Dust.NewDustPerfect(
                dustPositions[i],
                Main.rand.NextFromList(DustID.Smoke, DustID.TreasureSparkle, DustID.YellowTorch),
                dustDirections[i] * Main.rand.NextFloat(3f, 7f)
            );

            if(i % 3 == 0) {
                Dust.NewDustPerfect(
                    dustPositions[i],
                    ModContent.DustType<MineVampireExplosionDust>(),
                    dustDirections[i] * Main.rand.NextFloat(0.4f, 8f),
                    Main.rand.Next(30, 50),
                    Color.White,
                    Main.rand.NextFloat(0.6f, 1.0f)
                );
            }
        }

        ExplosionVFXProjectile.Spawn(
            Projectile.GetSource_Death(),
            Projectile.Center,
            Color.Yellow,
            Color.OrangeRed,
            progress => Color.Lerp(Color.OrangeRed, Color.Black, -MathF.Pow(progress - 1f, 2) + 1f),
            SizeType == 0 ? 100 : 200,
            90
        );

        Lighting.AddLight(Projectile.Center, new Vector3(1.86f, 1.22f, 0.69f) * 3.5f);
        SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Projectile.Center);
        Main.instance.CameraModifiers.Add(new ExplosionShakeCameraModifier(SizeType == 0 ? 4f : 11f, 0.87f, Projectile.Center, 5000, FullName));
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        behindNPCsAndTiles.Add(index);
    }

    private Effect effect;
    public override bool PreDraw(ref Color lightColor) {
        int blinkTime = 6;
        Color color = Color.OrangeRed * (1f - Projectile.alpha / 255f) * 0.2f * detectionAlpha * (Main.GameUpdateCount % blinkTime < blinkTime / 2 ? 1f : 0f);

        SpriteBatchSnapshot snapshot = Main.spriteBatch.Capture();
        effect ??= EffectLoader.GetEffect("Pixel::MineDetectionField");

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(snapshot with { CustomEffect = effect });

        Main.spriteBatch.Draw(
            TextureAssets.MagicPixel.Value,
            new Rectangle(
                (int)(Projectile.Center.X - DetectionRadius - Main.screenPosition.X),
                (int)(Projectile.Center.Y - DetectionRadius - Main.screenPosition.Y),
                (int)(DetectionRadius * 2f),
                (int)(DetectionRadius * 2f)
            ),
            color
        );

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(snapshot);

        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Rectangle source = SizeType == 0 ? new Rectangle(0, 28, 28, 28) : new Rectangle(0, 0, 28, 28);
        Main.spriteBatch.Draw(
            texture,
            Projectile.Center - Main.screenPosition,
            source,
            lightColor,
            Projectile.rotation,
            source.Size() / 2f,
            Projectile.scale,
            SpriteEffects.None,
            0f
        );
        return false;
    }
}