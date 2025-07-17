using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AllBeginningsMod.Content.NPCs.Corruption;

public sealed class DevilOWarStingerProjectile : ModProjectile {
    public override string Texture => Assets.Assets.Textures.NPCs.Corruption.DevilOWar.KEY_DevilOWarTentacle;

    public Player TargetPlayer => Main.player[(int)Projectile.ai[0]];
    public NPC ParentNPC => Main.npc[(int)Projectile.ai[1]];

    public bool AttachedToPlayer { get; private set; }
    public bool IsRetracting;

    private int _stingerDuration;
    private int _healthDrained;
    private int _healthDrainTimer;
    private const int health_drain_interval = 30;
    private const int health_drain_amount = 10;
    private const float health_return_percentage = 0.75f;

    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.aiStyle = -1;
        Projectile.friendly = false;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 60 * 60;
        Projectile.netImportant = true;
    }

    public override void AI() {
        if (!ParentNPC.active || ParentNPC.life <= 0 || ParentNPC.type != ModContent.NPCType<DevilOWarNPC>()) {
            Projectile.Kill();
            return;
        }

        if (!TargetPlayer.active || TargetPlayer.dead) {
            IsRetracting = true;
            AttachedToPlayer = false;
        }

        if (IsRetracting) {
            AttachedToPlayer = false;
            Vector2 directionToNPC = Projectile.DirectionTo(ParentNPC.Center);
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, directionToNPC * 20f, 0.1f);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Projectile.Distance(ParentNPC.Center) < 20f) {
                DespawnIntoDangling();
            }
            return;
        }

        if (!AttachedToPlayer) {
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(TargetPlayer.Center) * 15f, 0.1f);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Projectile.Hitbox.Intersects(TargetPlayer.Hitbox)) {
                AttachedToPlayer = true;
                Projectile.velocity = Vector2.Zero;
                _stingerDuration = 0;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item3, Projectile.position);
            }
        }
        else {
            Projectile.Center = TargetPlayer.Center;
            Projectile.rotation = (ParentNPC.Center - Projectile.Center).ToRotation() + MathHelper.PiOver2;

            _stingerDuration++;

            _healthDrainTimer++;
            if (_healthDrainTimer >= health_drain_interval) {
                int actualDrain = Math.Min(health_drain_amount, Math.Max(0, TargetPlayer.statLife - 1));
                if (actualDrain > 0) {
                    TargetPlayer.statLife -= actualDrain;
                    _healthDrained += actualDrain;
                    //TargetPlayer.HealEffect(-actualDrain, true);
                    
                    CombatText.NewText(TargetPlayer.Hitbox, CombatText.DamagedHostile, actualDrain, true, false);
                    _healthDrainTimer = 0;
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item3, Projectile.position);
                }
            }

            if (_stingerDuration >= DevilOWarNPC.stinger_duration_max || ParentNPC.Center.Distance(TargetPlayer.Center) >= DevilOWarNPC.charging_radius + 16 * 2) {
                StartRetraction();
            }
        }
    }

    public void StartRetraction() {
        if (!IsRetracting) {
            IsRetracting = true;
            AttachedToPlayer = false;
        }
    }

    private void DespawnIntoDangling() {
        if (ParentNPC.active 
            && ParentNPC.ModNPC is DevilOWarNPC devilOWarNPC) {
            if (devilOWarNPC._stingerProjectileId == Projectile.whoAmI) {
                devilOWarNPC._stingerProjectileId = -1;
            }
        }

        Projectile.Kill();
    }


    public override void OnKill(int timeLeft) {
        if (_healthDrained > 0 && TargetPlayer.active) {
            int healthToReturn = (int)(_healthDrained * health_return_percentage);
            if (healthToReturn > 0) {
                int newItemIndex = Item.NewItem(
                    Projectile.GetSource_OnHit(TargetPlayer),
                    ParentNPC.Center,
                    1,
                    1,
                    ModContent.ItemType<DevilOWarHeartPickup>()
                );

                if (newItemIndex != -1) {
                    Main.item[newItemIndex].damage = healthToReturn;
                }
            }
        }

        if (ParentNPC.active 
            && ParentNPC.ModNPC is DevilOWarNPC devilOWarNPC 
            && devilOWarNPC._stingerProjectileId == Projectile.whoAmI) {
            devilOWarNPC._stingerProjectileId = -1;
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity) => false;

    public override bool ShouldUpdatePosition() => !AttachedToPlayer || IsRetracting;

    public override bool PreDraw(ref Color lightColor) => false;
}