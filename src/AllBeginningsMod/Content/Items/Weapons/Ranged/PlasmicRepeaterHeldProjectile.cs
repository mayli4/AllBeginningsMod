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

namespace AllBeginningsMod.Content.Items.Weapons.Ranged;

internal sealed class PlasmicRepeaterHeldProjectile : ModProjectile {
    public override string Texture => Assets.Assets.Textures.Items.Weapons.Ranged.KEY_PlasmicRepeaterHeldProjectile;
    
    private Player Player => Main.player[Projectile.owner];
    private bool shouldDie;
    private float Progress => 1f - (float)Player.itemAnimation / Player.itemAnimationMax;
    private float recoil;
    private int frame;
    private int lastFrame = -1;
    private Vector2 muzzlePositionBottom;
    private Vector2 muzzlePositionTop;
    private static float muzzleFlashAlphaBottom;
    private static float muzzleFlashAlphaTop;
    private static float lastRotation;
    private PlasmicRepeaterItem repeaterItem;

    public override void SetDefaults() {
        Projectile.width = 0;
        Projectile.height = 0;
        Projectile.aiStyle = -1;
        Projectile.penetrate = -1;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.friendly = false;
        Projectile.hostile = false;
    }

    public override void OnSpawn(IEntitySource source) {
        Projectile.rotation = lastRotation;
        if(Player.HeldItem.ModItem is PlasmicRepeaterItem repeaterItem) {
            this.repeaterItem = repeaterItem;
        }
        else {
            Projectile.Kill();
        }
    }

    public override bool PreAI() {
        if(!shouldDie) {
            Projectile.timeLeft = Projectile.extraUpdates + 2;
        }

        Player.heldProj = Projectile.whoAmI;
        if(Player.HeldItem.type != ModContent.ItemType<PlasmicRepeaterItem>() || Player.ItemAnimationEndingOrEnded) {
            lastRotation = Projectile.rotation;
            shouldDie = true;
        }

        return true;
    }

    public override void AI() {
        if(Main.myPlayer == Player.whoAmI) {
            if(repeaterItem is null) {
                return;
            }

            Projectile.Center = repeaterItem.GunCenter + Player.velocity;
            Projectile.rotation = repeaterItem.GunRotation;
            Projectile.netUpdate = true;
        }

        Vector2 directionToMouse = Projectile.rotation.ToRotationVector2();
        Vector2 rotatedDirection = directionToMouse.RotatedBy(MathHelper.PiOver2 * Player.direction);
        Vector2 defaultShootPositionBottom = Projectile.Center + rotatedDirection * 2f;
        Vector2 defaultShootPositionTop = Projectile.Center - rotatedDirection * 8f;
        muzzlePositionBottom = defaultShootPositionBottom + directionToMouse * 48;
        muzzlePositionTop = defaultShootPositionTop + directionToMouse * 63;

        if(Main.myPlayer == Player.whoAmI) {
            if(frame == 0 && lastFrame != 0) {
                Vector2 shootPosition = defaultShootPositionBottom;
                if(Collision.CanHit(defaultShootPositionBottom, 0, 0, muzzlePositionBottom, 0, 0)) {
                    shootPosition = muzzlePositionBottom;
                }

                Projectile projectile = Projectile.NewProjectileDirect(
                    Projectile.GetSource_FromAI(),
                    shootPosition,
                    directionToMouse * 7f,
                    ProjectileID.DeathLaser,
                    Projectile.damage,
                    Projectile.knockBack,
                    Player.whoAmI
                );

                StrikeCloseRange(muzzlePositionBottom, directionToMouse * 80f, 5f);

                projectile.friendly = true;
                projectile.hostile = false;
                projectile.usesLocalNPCImmunity = true;
                projectile.localNPCHitCooldown = projectile.timeLeft;
                projectile.extraUpdates = 5;

                recoil = 1f;
                muzzleFlashAlphaBottom = 1f;
            }

            if(frame == 2 && lastFrame != 2) {
                Vector2 shootPosition = defaultShootPositionTop;
                if(Collision.CanHit(defaultShootPositionTop, 0, 0, muzzlePositionTop, 0, 0)) {
                    shootPosition = muzzlePositionTop;
                }

                Projectile projectile = Projectile.NewProjectileDirect(
                    Projectile.GetSource_FromAI(),
                    shootPosition,
                    directionToMouse * 7f,
                    ProjectileID.DeathLaser,
                    Projectile.damage,
                    Projectile.knockBack,
                    Player.whoAmI
                );

                StrikeCloseRange(muzzlePositionTop, directionToMouse * 80f, 5f);

                projectile.friendly = true;
                projectile.hostile = false;
                projectile.localNPCHitCooldown = projectile.timeLeft;
                projectile.extraUpdates = 5;

                recoil = 1f;
                muzzleFlashAlphaTop = 1f;
            }
        }

        Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);

        lastFrame = frame;
        frame = (int)(Progress * 3f);

        recoil *= 0.9f;
        muzzleFlashAlphaBottom *= 0.97f;
        muzzleFlashAlphaTop *= 0.97f;
    }

    public void StrikeCloseRange(Vector2 position, Vector2 direction, float width) {
        for(int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            float collisionPoint = 0;
            if(
                npc == null
                || npc.friendly
                || npc.dontTakeDamage
                || !npc.active
                || !Collision.CheckAABBvLineCollision(npc.position, npc.Size, position, position + direction, width, ref collisionPoint)
                || npc.immune[Player.whoAmI] > 0
                ) {
                continue;
            }

            Player.ApplyDamageToNPC(npc, Projectile.damage, Projectile.knockBack, MathF.Sign(npc.Center.X - Player.Center.X), Main.rand.NextBool(10), DamageClass.Ranged);
        }
    }

    public override bool ShouldUpdatePosition() {
        return false;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Vector2 position = Projectile.Center - Main.screenPosition;
        Rectangle source = new(0, 42 * (frame % 3), 76, 42);
        float recoilOffset = recoil * 10f;
        Vector2 origin = new(75f - recoilOffset, 26f);
        float rotation = Projectile.rotation + (Player.direction == -1 ? MathHelper.Pi : 0);

        Main.spriteBatch.End(out SpriteBatchSnapshot snapshot);
        Main.spriteBatch.Begin(snapshot with { BlendState = BlendState.Additive });

        Texture2D glowTexture = Mod.Assets.Request<Texture2D>("Assets/Textures/Misc/Glow2", AssetRequestMode.ImmediateLoad).Value;
        Color color = Color.Red * 1f;
        Vector2 glowOrigin = glowTexture.Size() / 2f - Vector2.UnitX * recoilOffset * Player.direction;

        Main.spriteBatch.Draw(
            glowTexture,
            muzzlePositionBottom - Main.screenPosition,
            null,
            color * muzzleFlashAlphaBottom,
            rotation,
            glowOrigin,
            new Vector2(0.7f + muzzleFlashAlphaBottom * 2.5f, 0.2f),
            SpriteEffects.None,
            0f
        );

        Main.spriteBatch.Draw(
            glowTexture,
            muzzlePositionTop - Main.screenPosition,
            null,
            color * muzzleFlashAlphaTop,
            rotation,
            glowOrigin,
            new Vector2(0.7f + muzzleFlashAlphaBottom * 2.5f, 0.3f),
            SpriteEffects.None,
            0f
        );

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(snapshot);

        Main.spriteBatch.Draw(
            texture,
            position,
            source,
            lightColor,
            rotation,
            Player.direction == 1 ? new Vector2(texture.Width - origin.X, origin.Y) : origin,
            Projectile.scale,
            Player.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
            0
        );

        Texture2D glowMaskTexture = ModContent.Request<Texture2D>(Texture + "_Glow", AssetRequestMode.ImmediateLoad).Value;
        Main.spriteBatch.Draw(
            glowMaskTexture,
            position,
            source,
            Color.White,
            rotation,
            Player.direction == 1 ? new Vector2(texture.Width - origin.X, origin.Y) : origin,
            Projectile.scale,
            Player.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
            0
        );

        return false;
    }
}