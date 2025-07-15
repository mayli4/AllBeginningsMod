using AllBeginningsMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace AllBeginningsMod.Content.NPCs.Enemies.Bosses.Nightgaunt; 

[AutoloadBossHead]
internal class NightgauntNPC : ModNPC {
    
    public enum Phases {
        Phase1,
        Phase2
    }

    public enum Attacks {
        None,
        Swing,
        Jump,
        Shield,
        Dip
    }

    public Phases Phase {
        get => (Phases)NPC.ai[0];
        private set {
            NPC.ai[0] = (int)value;
            NPC.netUpdate = true;
        }
    }

    private ref float AttackTimer => ref NPC.ai[1];
    public Attacks Attack {
        get => (Attacks)NPC.ai[2];
        private set {

            NPC.ai[2] = (int)value;
            NPC.netUpdate = true;
        }
    }

    private ref float ShieldJellyFishInterval => ref NPC.ai[3];

    private Player Target => Main.player[NPC.target];
    //private bool Grounded => Collision.SolidCollision(NPC.position + Vector2.UnitY * NPC.height, NPC.width, 4);
    private int frame;

    private int lastFrame = -1;

    private const int BigSwingAreaWidth = 400;
    private const int BigSwingAreaHeight = 150;
    private const int BigSwingAreaOffset = 50;
    private Rectangle BigSwingHitArea => new(
        (int)NPC.Center.X + (NPC.direction == -1 ? -BigSwingAreaWidth - BigSwingAreaOffset : BigSwingAreaOffset),
        (int)NPC.Center.Y - BigSwingAreaHeight / 2,
        BigSwingAreaWidth,
        BigSwingAreaHeight
    );

    private const int SmallSwingAreaWidth = 250;
    private const int SmallSwingAreaHeight = 175;
    private const int SmallSwingAreaOffset = 50;
    private Rectangle SmallSwingHitArea => new(
        (int)NPC.Center.X + (NPC.direction == -1 ? -SmallSwingAreaWidth - SmallSwingAreaOffset : SmallSwingAreaOffset),
        (int)NPC.Center.Y - SmallSwingAreaHeight / 2,
        SmallSwingAreaWidth,
        SmallSwingAreaHeight
    );

    private int shieldAttackCooldown;
    private static int SwingTime => 90;
    private static int ShieldTime => 750;
    private static int JumpTime => 40;
    private const float Gravity = 0.4f;
    private float jumpTrailAlpha;
    private int swingCount;

    public override string Texture => Assets.Assets.Textures.NPCs.Bosses.Nightgaunt.KEY_NightgauntNPC_Swing;
    private Texture2D jumpTexture;
    private Texture2D shieldTexture;
    private static float TeleportDistance => 3_500_000;

    public override void SetStaticDefaults() {
        NPCID.Sets.TrailCacheLength[Type] = 4;
        NPCID.Sets.TrailingMode[Type] = 0;
    }

    public override void SetDefaults() {
        NPC.width = 150;
        NPC.height = 175;
        NPC.knockBackResist = 0f;

        NPC.defense = 10;
        NPC.damage = 60;
        NPC.lifeMax = 10_000;

        NPC.boss = true;
        NPC.noGravity = true;
        NPC.noTileCollide = false;
        NPC.friendly = false;

        NPC.alpha = 0;
        NPC.aiStyle = -1;

        NPC.npcSlots = 40f;
        NPC.HitSound = SoundID.NPCHit2;

        /*if (!Main.dedServ)
            Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music");*/
    }

    private bool TargetValid(Player target) {
        return target is not null && !target.dead && target.active;
    }

    public override bool CheckActive() {
        return false;
    }

    private void SafeTargetClosest() {
        int target = -1;
        float distance = float.MaxValue;
        for(int i = 0; i < Main.maxPlayers; i++) {
            if(!TargetValid(Main.player[i])) {
                continue;
            }

            float currentDistance = Main.player[i].DistanceSQ(NPC.Center);
            if(currentDistance < distance) {
                target = i;
                distance = currentDistance;
            }
        }

        NPC.target = target;
    }

    public override void AI() {
        frame = Attack switch
        {
            Attacks.None => 0,
            Attacks.Swing => (int)(18 * AttackTimer / (SwingTime + 1)),
            Attacks.Jump or Attacks.Dip => AttackTimer > JumpTime ? 5 : (int)(6 * AttackTimer / (JumpTime + 1)),
            _ => 0,
        };

        switch(Phase) {
            case Phases.Phase1:
                switch(Attack) {
                    case Attacks.None:
                        AttackTimer++;
                        if(AttackTimer > 10) {
                            if(NPC.target == -1 || !TargetValid(Target) || NPC.DistanceSQ(Target.Center) > TeleportDistance) {
                                SafeTargetClosest();
                            }

                            if(NPC.target == -1) {
                                Attack = Attacks.Dip;
                            }
                            else if(!BigSwingHitArea.Intersects(Target.Hitbox) || swingCount >= 2) {
                                Attack = Attacks.Jump;
                                swingCount = 0;
                            }
                            else {
                                Attack = Main.rand.NextFloat() < 0.2f && shieldAttackCooldown <= 0 ? Attacks.Shield : Attacks.Swing;
                            }

                            AttackTimer = 0;
                        }

                        FaceTarget();
                        break;
                    case Attacks.Swing:
                        AttackTimer++;
                        if(AttackTimer > SwingTime) {
                            swingCount++;
                            Attack = Attacks.None;
                            AttackTimer = 0;
                        }

                        if(Target.active && !Target.dead) {
                            if(frame == 8 && lastFrame == 7 && BigSwingHitArea.Intersects(Target.Hitbox)) {
                                //SoundEngine.PlaySound(SoundID.DD2_BallistaTowerShot, NPC.Center);
                                Target.Hurt(
                                    PlayerDeathReason.ByNPC(NPC.whoAmI),
                                    NPC.damage,
                                    MathF.Sign(Target.Center.X - NPC.Center.X),
                                    knockback: 9f
                                );

                                // spawn dust on hit?
                                /*Vector2 directionToPlayer = NPC.Center.DirectionTo(Target.Center);
                                Vector2 position = Target.Center - directionToPlayer * 20f;
                                for (int i = 0; i < 80; i++) {
                                    Dust.NewDustPerfect(position, DustID.Blood, directionToPlayer.RotatedByRandom(MathHelper.PiOver4 / 2f) * Main.rand.NextFloat(15f, 40f));
                                }*/
                            }

                            if(frame == 14 && lastFrame == 7 && SmallSwingHitArea.Intersects(Target.Hitbox)) {
                                //SoundEngine.PlaySound(SoundID.DD2_BallistaTowerShot, NPC.Center);
                                Target.Hurt(
                                    PlayerDeathReason.ByNPC(NPC.whoAmI),
                                    NPC.damage / 2,
                                    MathF.Sign(Target.Center.X - NPC.Center.X),
                                    knockback: 7f
                                );
                            }
                        }

                        break;
                    case Attacks.Jump:
                        AttackTimer++;
                        if(frame == 4 && lastFrame == 3) {
                            NPC.noTileCollide = true;

                            float distance = NPC.Center.DistanceSQ(Target.Center);
                            if(distance > TeleportDistance) {
                                NPC.Center = Target.Center - Vector2.UnitY * 2000f;
                            }
                            else {
                                NPC.velocity = MathUtilities.InitialVelocityRequiredToHitPosition(
                                    NPC.Bottom,
                                    Target.Bottom,
                                    Gravity,
                                    Main.rand.NextFloat(15f, 19f)
                                );
                                // Main.NewText(NPC.velocity);
                            }
                        }

                        if(frame < 4) {
                            FaceTarget();
                        }

                        if(NPC.velocity.Y >= 0 && frame == 5) {
                            NPC.noTileCollide = false;
                            if(
                                Target.Center.X > NPC.Left.X && Target.Center.X < NPC.Right.X
                                && (Target.Center.Y < NPC.Top.Y / 2f || Target.Center.Y > NPC.Bottom.Y)
                                && Target.Center.Y > NPC.Center.Y
                            ) {
                                NPC.noTileCollide = true;
                            }

                            if(NPC.velocity.Y == 0f) {
                                NPC.noTileCollide = false;
                                Attack = Attacks.None;
                                MathUtilities.ForEachPlayerInRange(
                                    NPC.Center,
                                    120,
                                    player =>
                                    {
                                        player.Hurt(
                                            PlayerDeathReason.ByNPC(NPC.whoAmI),
                                            NPC.damage / 2,
                                            MathF.Sign(Target.Center.X - NPC.Center.X),
                                            knockback: 8.5f
                                        );
                                    }
                                );
                                JumpSmashEffects();
                                AttackTimer = 0;
                            }
                        }

                        break;
                    case Attacks.Shield:
                        if(AttackTimer == 0) {
                            NPC.dontTakeDamage = true;
                            Projectile.NewProjectile(
                                NPC.GetSource_FromAI(),
                                NPC.Center,
                                Vector2.Zero,
                                ModContent.ProjectileType<NightgauntForceField>(),
                                NPC.damage / 2,
                                30f,
                                -1,
                                NPC.whoAmI
                            );
                        }

                        if(ShieldJellyFishInterval-- <= 0 && (ShieldTime - AttackTimer) > 90) {
                            float spread = 140f;
                            int count = 5;
                            for(int i = -count; i < count; i++) {
                                Projectile.NewProjectile(
                                    NPC.GetSource_FromAI(),
                                    new Vector2(Target.Center.X + i * Main.rand.NextFloat(spread, spread * 2f), Target.Center.Y + 750f + Main.rand.Next(-60, 60)),
                                    Vector2.Zero,
                                    ModContent.ProjectileType<NightgauntReverseGravityProjectile>(),
                                    NPC.damage / 3,
                                    2f
                                );
                            }

                            ShieldJellyFishInterval = Main.rand.Next(20, 40);
                        }

                        if(AttackTimer >= ShieldTime || Target.dead) {
                            shieldAttackCooldown = 900;
                            NPC.dontTakeDamage = false;
                            Attack = Attacks.None;
                            AttackTimer = 0;
                        }

                        AttackTimer++;
                        break;
                    case Attacks.Dip:
                        AttackTimer++;
                        if(frame == 4 && lastFrame == 3) {
                            NPC.noTileCollide = true;
                            NPC.velocity = new Vector2(35f * NPC.direction, -45f);
                        }

                        if(AttackTimer > 200) {
                            NPC.active = false;
                        }

                        break;
                }
                break;
            case Phases.Phase2:
                break;
        }

        if((Attack != Attacks.Jump || frame < 4) && NPC.velocity.Y == 0f) {
            NPC.velocity.X *= 0.885f;
        }

        if(Attack == Attacks.Jump) {
            if(jumpTrailAlpha < 1f) {
                jumpTrailAlpha += 0.05f;
            }
        }
        else {
            jumpTrailAlpha *= 0.96f;
        }

        if(shieldAttackCooldown > 0) {
            shieldAttackCooldown--;
        }

        lastFrame = frame;
        NPC.velocity.Y += Gravity;
    }

    public override bool? CanFallThroughPlatforms() {
        if(NPC.target == -1) {
            return null;
        }

        return Target is not null && Target.Top.Y > NPC.Bottom.Y && (Attack != Attacks.Jump || frame >= 4) && Attack != Attacks.Shield;
    }

    private void FaceTarget() {
        if(NPC.target == -1) {
            return;
        }

        NPC.direction = MathF.Sign(Target.Center.X - NPC.Center.X);
    }

    private void JumpSmashEffects() {
        if(Main.dedServ) {
            return;
        }

        Main.instance.CameraModifiers.Add(
            new PunchCameraModifier(NPC.Center, Vector2.UnitY, 28f, 4f, 18, 5000f, FullName)
        );
        SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact, NPC.Center);
        int dustCount = 25;
        for(int i = 0; i < dustCount; i++) {
            Vector2 position = NPC.BottomLeft + Vector2.UnitX * Main.rand.NextFloat(NPC.width);
            Dust.NewDustPerfect(position, DustID.Dirt, new Vector2(MathF.Sign(position.X - NPC.Center.X) * 2f, -6f) * Main.rand.NextFloat());
        }
    }

    public override bool CanHitPlayer(Player target, ref int cooldownSlot) {
        return false;
    }

    public override void FindFrame(int frameHeight) {
        switch(Attack) {
            case Attacks.None:
                NPC.frame = new(0, 0, 732, 288);
                break;
            case Attacks.Swing:
                NPC.frame = new Rectangle(0, 0, 732, 288);
                NPC.frame.X = NPC.frame.Width * (frame / 6);
                NPC.frame.Y = NPC.frame.Height * (frame % 6);
                break;
            case Attacks.Dip:
            case Attacks.Jump:
                NPC.frame = new(0, 0, 274, 284);
                NPC.frame.Y = NPC.frame.Height * frame;
                break;
            case Attacks.Shield:
                NPC.frame = new(0, 240, 336, 240);
                break;
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        Texture2D swingTexture = TextureAssets.Npc[Type].Value;
        jumpTexture ??= ModContent.Request<Texture2D>(Texture.Replace("Swing", "Jump"), ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
        shieldTexture ??= ModContent.Request<Texture2D>(Texture.Replace("Swing", "Shield"), ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

        Texture2D texture = Attack switch
        {
            Attacks.None or Attacks.Swing => swingTexture,
            Attacks.Jump or Attacks.Dip => jumpTexture,
            _ => shieldTexture,
        };

        Vector2 offset = Attack switch
        {
            Attacks.None or Attacks.Swing => new(70, 55),
            Attacks.Jump or Attacks.Dip => new Vector2(19, 53) + frame switch
            {
                4 => new Vector2(-20, -20),
                5 => new Vector2(-50, -80),
                _ => Vector2.Zero
            },
            _ => new(0, 31),
        };
        offset.X *= -NPC.direction;

        SpriteBatchSnapshot snapshot = Main.spriteBatch.Capture();
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.AnisotropicClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
        for(int i = 0; i < NPC.oldPos.Length; i++) {
            float factor = (float)i / NPC.oldPos.Length;
            Main.spriteBatch.Draw(
                texture,
                NPC.oldPos[i] + NPC.Size / 2f - screenPos,
                NPC.frame,
                Color.DarkSlateGray * (1f - factor) * jumpTrailAlpha * 0.6f,
                NPC.rotation,
                NPC.frame.Size() / 2f + offset,
                NPC.scale,
                NPC.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0f
            );
        }


        Main.spriteBatch.End();
        Main.spriteBatch.Begin(snapshot);

        Main.spriteBatch.Draw(
            texture,
            NPC.Center - screenPos,
            NPC.frame,
            drawColor,
            NPC.rotation,
            NPC.frame.Size() / 2f + offset,
            NPC.scale,
            NPC.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
            0f
        );

        return false;
    }
}