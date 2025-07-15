using AllBeginningsMod.Common.PrimitiveDrawing;
using AllBeginningsMod.Content.CameraModifiers;
using AllBeginningsMod.Content.NPCs.Enemies.Nighttime;
using AllBeginningsMod.Content.Projectiles;
using AllBeginningsMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace AllBeginningsMod.Content.Items.Weapons.Magic; 

internal class MistaBombProjectile : ModProjectile {
    public override string Texture => Assets.Assets.Textures.Items.Weapons.Magic.KEY_MistaBombProjectile;
    
    private int maxTimeLeft = 4 * 60;
    private float Progress => 1f - (float)Projectile.timeLeft / maxTimeLeft;
    private PrimitiveTrail trail;
    private PrimitiveTrail sparkleTrail;
    private PositionCache positionCache;
    private bool trailInit;
    private readonly float explodingTime = 0.85f;

    public override void SetStaticDefaults() {
        Main.projFrames[Type] = 6;
        ProjectileID.Sets.TrailCacheLength[Type] = 20;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    public override void SetDefaults() {
        Projectile.width = 23;
        Projectile.height = 23;
        Projectile.aiStyle = -1;
        Projectile.penetrate = -1;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = true;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.timeLeft = 60 * 4;

        positionCache = new(30);
        sparkleTrail = new PrimitiveTrail(
            positionCache.Positions,
            progress => 3f * (1f - progress + MathF.Sin(-Main.GameUpdateCount * 0.5f + progress * 13f) * 0.3f),
            progress => Color.Lerp(Color.Orange, Color.DarkRed * 0.2f, progress)
        );
    }

    public override void OnSpawn(IEntitySource source) {
        Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
    }

    public override void AI() {
        if(Projectile.owner == Main.myPlayer) {
            Projectile.velocity += Projectile.Center.DirectionTo(Main.MouseWorld) * 0.2f;
            Projectile.netUpdate = true;
        }

        Projectile.velocity *= 0.95f - Progress * 0.15f;
        Projectile.rotation -= 0.275f * (1f - Progress);

        Vector2 linePosition = Projectile.Center + new Vector2(10, -20).RotatedBy(Projectile.rotation);
        if(!trailInit) {
            positionCache.SetAll(linePosition);
            Projectile.oldPos = Projectile.oldPos.Select(_ => Projectile.position).ToArray();
            trailInit = true;
        }

        if(!Main.dedServ) {
            Dust.NewDustPerfect(
                linePosition,
                DustID.TreasureSparkle,
                Main.rand.NextVector2Unit(Projectile.rotation - MathHelper.PiOver2, MathHelper.Pi) * Main.rand.NextFloat(10f)
            );
        }

        positionCache.Add(linePosition);
        sparkleTrail.Positions = positionCache.Positions;
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        return false;
    }

    public override void OnKill(int timeLeft) {
        if(!Main.dedServ) {
            ExplosionVFXProjectile.Spawn(
                Projectile.GetSource_Death(),
                Projectile.Center,
                Color.Yellow,
                Color.OrangeRed,
                progress => Color.Lerp(Color.OrangeRed, Color.Black, progress),
                350,
                150
            );

            Lighting.AddLight(Projectile.Center, new Vector3(1.86f, 1.22f, 0.69f) * 3.5f);
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Projectile.Center);

            Main.instance.CameraModifiers.Add(
                new ExplosionShakeCameraModifier(80f, 0.85f, Projectile.Center, 5000, FullName)
            );
        }

        if(Main.myPlayer == Projectile.owner) {
            MathUtilities.ForEachNPCInRange(
                Projectile.Center,
                225,
                npc =>
                {
                    if(
                        npc is null
                        || npc.friendly
                        || npc.dontTakeDamage
                        || npc.immune[Projectile.owner] > 0
                    ) {
                        return;
                    }

                    Main.player[Projectile.owner].ApplyDamageToNPC(
                        npc,
                        Projectile.damage,
                        6f,
                        MathF.Sign(npc.Center.X - Projectile.Center.X),
                        Main.rand.NextFloat() < Projectile.CritChance * 0.01f,
                        DamageClass.Magic
                    );
                }
            );

            Projectile projectile = Projectile.NewProjectileDirect(
                Projectile.GetSource_Death(),
                Projectile.Center,
                Vector2.Zero,
                ModContent.ProjectileType<DevilVampireAuraProjectile>(),
                10,
                0f,
                Projectile.owner
            );

            projectile.hostile = !(projectile.friendly = true);
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        float squashTime = explodingTime - 0.2f;
        float stretch = MathF.Sin(
            MathUtilities.Lerp3(0f, 0f, MathF.Pow((Progress - squashTime) / (1f - squashTime), 3f) * 30f, Progress, squashTime)
        );
        Vector2 scale = (Vector2.One + new Vector2(-stretch, stretch) * 0.25f) * MathUtilities.Lerp3(1f, 1f, 1.5f, Progress, squashTime);

        Texture2D texture = TextureAssets.Projectile[Type].Value;
        if(Progress > explodingTime) {
            Projectile.frame = (int)(5f + stretch);
        }
        else {
            Projectile.frame = (int)MathUtilities.Lerp3(0f, 3f, 6f, Progress, explodingTime);
        }
        Rectangle source = Projectile.SourceRectangle();

        for(int i = 0; i < Projectile.oldPos.Length; i++) {
            float factor = (float)i / Projectile.oldPos.Length;
            Main.spriteBatch.Draw(
                texture,
                Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition,
                source,
                lightColor * 0.1f * (1f - factor),
                Projectile.oldRot[i],
                new Vector2(17, 35),
                scale,
                SpriteEffects.None,
                0f
            );
        }

        sparkleTrail.Draw(TextureAssets.MagicPixel.Value, Color.White, MathUtilities.WorldTransformationMatrix);

        Main.spriteBatch.Draw(
            texture,
            Projectile.Center - Main.screenPosition,
            source,
            lightColor,
            Projectile.rotation,
            new Vector2(17, 35),
            scale,
            SpriteEffects.None,
            0f
        );

        return false;
    }
}