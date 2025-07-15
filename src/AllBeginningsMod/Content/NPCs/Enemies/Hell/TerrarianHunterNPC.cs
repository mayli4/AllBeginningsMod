using AllBeginningsMod.Core.Loaders;
using AllBeginningsMod.Common.PrimitiveDrawing;
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

namespace AllBeginningsMod.Content.NPCs.Enemies.Hell; 

internal class TerrarianHunterNPC : ModNPC {
    public override string Texture => Assets.Assets.Textures.NPCs.Hell.KEY_TerrarianHunterNPC;
    
    public enum Attacks {
        HoverHorizontal,
        HoverVertical,
        BottomMine,
        Gun,
        BackMines
    }

    private ref float AttackTimer => ref NPC.ai[0];
    public Attacks Attack {
        get => (Attacks)NPC.ai[1];
        private set {

            NPC.ai[1] = (int)value;
            NPC.netUpdate = true;
        }
    }

    private ref float NextGunRotation => ref NPC.ai[2];
    private float gunRotation;
    private int consecutiveBottomMines;
    private int backMinesCooldown;
    private Player Target => Main.player[NPC.target];
    private static int GunAnimationTime => 20;
    private int GunOriginOffset => 10 - (int)(5f * smallMuzzleAlpha * bigMuzzleAlpha);
    private Vector2 GunElbowPosition => NPC.Center + new Vector2(10f * NPC.direction, 20f);
    private int GunFrame => (int)(AttackTimer % GunAnimationTime / (GunAnimationTime / 3f));
    private int LastGunFrame { get; set; } = -1;
    private Vector2 BigMuzzlePosition {
        get {
            Vector2 direction = gunRotation.ToRotationVector2();
            return GunElbowPosition
                + direction * (GunOriginOffset + 65)
                + direction.RotatedBy(MathHelper.PiOver2) * -4f * NPC.direction;
        }
    }

    private float bigMuzzleAlpha;

    private Vector2 SmallMuzzlePosition {
        get {
            Vector2 direction = gunRotation.ToRotationVector2();
            return GunElbowPosition
                + direction * (GunOriginOffset + 40)
                + direction.RotatedBy(MathHelper.PiOver2) * 12f * NPC.direction;
        }
    }

    private float smallMuzzleAlpha;
    private PrimitiveTrail[] engineTrails;
    private Vector2 TrailPositionOffset(int index) => index switch
    {
        0 => Attack switch
        {
            Attacks.HoverHorizontal => new Vector2(28 * NPC.direction, 76),
            Attacks.BottomMine => new Vector2(15 * NPC.direction, 70),
            Attacks.Gun => new Vector2(30 * NPC.direction, 64),
            _ => new Vector2(24 * NPC.direction, 66),
        },
        1 => Attack switch
        {
            Attacks.HoverHorizontal => new Vector2(-28 * NPC.direction, 65),
            Attacks.BottomMine => new Vector2(-28 * NPC.direction, 75),
            _ => new Vector2(-28 * NPC.direction, 75),
        },
        _ => new Vector2(-32 * NPC.direction, -35)
    };

    private bool initializedPositions = false;

    public override void SetDefaults() {
        NPC.width = 80;
        NPC.height = 140;
        NPC.knockBackResist = 0.2f;

        NPC.defense = 50;
        NPC.damage = 50;
        NPC.lifeMax = 1_000;

        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.friendly = false;

        NPC.alpha = 0;
        NPC.aiStyle = -1;

        NPC.HitSound = SoundID.NPCHit4;

        engineTrails = new PrimitiveTrail[3];
        for(int i = 0; i < engineTrails.Length; i++) {
            float width = i == 2 ? 25f : 50f;
            engineTrails[i] = new PrimitiveTrail(
                new Vector2[9],
                factor => (-MathF.Pow(factor - 1f, 4) + 1f) * width,
                factor => Color.Lerp(
                    Color.Lerp(Color.Orange, Color.MediumPurple, factor * 1.2f),
                    Color.Black,
                    (MathF.Sin(Main.GameUpdateCount * 0.25f + factor * 10f) + 1f) * 0.1f
                ) * 0.9f
            );
        }
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo) {
        return Main.hardMode && spawnInfo.Player.ZoneUnderworldHeight ? 0.05f : 0f;
    }

    private void ManageTrails() {
        for(int i = 0; i < engineTrails.Length; i++) {
            engineTrails[i].Positions[0] = NPC.Center + TrailPositionOffset(i);
            for(int j = 1; j < engineTrails[i].Positions.Length; j++) {
                Vector2 direction;
                if(i == 2) {
                    direction = (NPC.direction == -1 ? MathHelper.PiOver4 : MathHelper.Pi * 0.75f).ToRotationVector2();
                }
                else {
                    direction = Attack switch
                    {
                        Attacks.HoverHorizontal => (NPC.direction == -1 ? MathHelper.PiOver4 : MathHelper.Pi * 0.75f).ToRotationVector2(),
                        Attacks.BottomMine => (NPC.direction == 1 ? MathHelper.PiOver4 : MathHelper.Pi * 0.75f).ToRotationVector2(),
                        _ => Vector2.UnitY,
                    };
                }

                engineTrails[i].Positions[j] = Vector2.Lerp(
                    engineTrails[i].Positions[j],
                    engineTrails[i].Positions[0] + direction * 6f * j,
                    0.15f + (1f - (float)j / engineTrails[i].Positions.Length) * 0.5f
                );
            }
        }
    }

    public override void AI() {
        if(NPC.target == -1 || !Target.active || Target.dead) {
            NPC.TargetClosest();
        }

        if(!initializedPositions) {
            for(int i = 0; i < engineTrails.Length; i++) {
                for(int j = 0; j < engineTrails[i].Positions.Length; j++) {
                    engineTrails[i].Positions[j] = NPC.Center;
                }
            }

            initializedPositions = true;
        }

        ManageTrails();
        switch(Attack) {
            case Attacks.HoverHorizontal:
                FaceTarget();
                float distanceX = Target.Center.X - NPC.Center.X;
                if(MathF.Abs(distanceX) > 350f) {
                    NPC.velocity.X += MathF.Sign(distanceX) * 1.25f;
                }
                else {
                    if(backMinesCooldown <= 0) {
                        Attack = Main.rand.NextFromList(Attacks.Gun, Attacks.BackMines);
                    }
                    else {
                        Attack = Attacks.Gun;
                    }

                    AttackTimer = 0f;
                }

                break;
            case Attacks.HoverVertical:
                FaceTarget();
                float distanceY = Target.Center.Y - NPC.Center.Y;
                if(MathF.Abs(distanceY) > MathF.Abs(NPC.velocity.Y)) {
                    NPC.velocity.Y += MathF.Sign(distanceY) * 1.25f;
                }
                else {
                    Attack = Attacks.HoverHorizontal;
                    AttackTimer = 0f;
                }
                break;
            case Attacks.BottomMine:
                if(AttackTimer == 0) {
                    consecutiveBottomMines++;
                    NPC.velocity.X -= MathF.Sign(Target.Center.X - NPC.Center.X) * 16f;
                    Projectile.NewProjectile(
                        NPC.GetSource_FromAI(),
                        NPC.Center + new Vector2(5f * NPC.direction, 70f),
                        (MathHelper.Pi * (NPC.direction == 1 ? 0.25f : 0.75f)).ToRotationVector2() * 11f,
                        ModContent.ProjectileType<TerrarianHunterMineProjectile>(),
                        NPC.damage,
                        6f,
                        -1,
                        1f
                    );
                }

                NPC.velocity.X -= MathF.Sign(Target.Center.X - NPC.Center.X) * 0.25f;

                FaceTarget();
                AttackTimer++;
                if(AttackTimer > 30) {
                    if(consecutiveBottomMines < 2) {
                        Attack = Main.rand.NextFromList(Attacks.BottomMine, Attacks.HoverVertical);
                    }
                    else {
                        Attack = Attacks.HoverVertical;
                    }

                    if(Attack != Attacks.BottomMine) {
                        consecutiveBottomMines = 0;
                    }

                    AttackTimer = 0f;
                }
                break;
            case Attacks.Gun:
                void GenerateNextGunRotation() {
                    NextGunRotation = GunElbowPosition.DirectionTo(Target.Center).ToRotation() + MathHelper.PiOver4 * 0.5f * Main.rand.NextFloatDirection();
                    NPC.netUpdate = true;
                }
                if(AttackTimer == 0) {
                    gunRotation = GunElbowPosition.DirectionTo(Target.Center).ToRotation();
                    GenerateNextGunRotation();
                }
                else {
                    gunRotation = Utils.AngleLerp(
                        gunRotation,
                        NextGunRotation,
                        0.15f
                    );
                }

                Vector2 shootDirection = gunRotation.ToRotationVector2();
                if(GunFrame == 0 && LastGunFrame != 0) {
                    Projectile.NewProjectile(
                        NPC.GetSource_FromAI(),
                        SmallMuzzlePosition,
                        shootDirection * 10f,
                        ProjectileID.DeathLaser,
                        NPC.damage / 5,
                        2f
                    );
                    smallMuzzleAlpha = 1f;
                    GenerateNextGunRotation();
                }

                if(GunFrame == 2 && LastGunFrame != 2) {
                    Projectile.NewProjectile(
                        NPC.GetSource_FromAI(),
                        BigMuzzlePosition,
                        shootDirection * 10f,
                        ProjectileID.DeathLaser,
                        NPC.damage / 5,
                        2f
                    );
                    bigMuzzleAlpha = 1f;
                    GenerateNextGunRotation();
                }

                FaceTarget();
                NPC.velocity.X += 0.05f * NPC.direction;

                bigMuzzleAlpha *= 0.925f;
                smallMuzzleAlpha *= 0.925f;
                LastGunFrame = GunFrame;
                AttackTimer++;
                if(AttackTimer > 150) {
                    Attack = Main.rand.NextFromList(Attacks.BottomMine);
                    AttackTimer = 0f;
                }
                break;
            case Attacks.BackMines:
                if(AttackTimer == 0) {
                    backMinesCooldown = 460;
                }

                if(AttackTimer % 15 == 0) {
                    Projectile.NewProjectile(
                        NPC.GetSource_FromAI(),
                        NPC.Center + new Vector2(-25f * NPC.direction, -35f),
                        (MathHelper.Pi * (NPC.direction == -1 ? 0.25f : 0.75f) + 0.25f * MathHelper.PiOver4 * Main.rand.NextFloatDirection()).ToRotationVector2() * 11f,
                        ModContent.ProjectileType<TerrarianHunterMineProjectile>(),
                        NPC.damage / 4,
                        6f
                    );
                }

                NPC.velocity.X += -0.035f * NPC.direction;

                AttackTimer++;
                if(AttackTimer > 90) {
                    Attack = Main.rand.NextFromList(Attacks.BottomMine);
                    AttackTimer = 0f;
                }
                break;
        }

        if(!Main.dedServ) {
            for(int i = 0; i < engineTrails.Length; i++) {
                Lighting.AddLight(NPC.Center + TrailPositionOffset(i), Color.Orange.ToVector3());
            }
        }

        NPC.velocity *= 0.9f;

        float maxRotation = 0.2f;
        NPC.rotation = Math.Clamp(NPC.velocity.X * 0.05f, -maxRotation, maxRotation);
        if(backMinesCooldown > 0) {
            backMinesCooldown--;
        }
    }

    public override bool? CanFallThroughPlatforms() {
        return true;
    }

    private void FaceTarget() {
        if(NPC.target == -1) {
            return;
        }

        NPC.direction = MathF.Sign(Target.Center.X - NPC.Center.X);
    }

    public override void HitEffect(NPC.HitInfo hit) {
        if(Main.netMode == NetmodeID.Server || NPC.life > 0) {
            return;
        }

        IEntitySource source = NPC.GetSource_Death();
        Gore gore1 = Gore.NewGoreDirect(
            source,
            NPC.Center - Vector2.UnitY * 8f,
            -Vector2.UnitY * Main.rand.NextFloat(2f, 3f),
            Mod.Find<ModGore>("TerrarianHunterGore0").Type
        );

        gore1.position -= new Vector2(gore1.Width, gore1.Height) * 0.5f;

        for(int i = 0; i < 4; i++) {
            Vector2 direction = Main.rand.NextVector2Unit();
            Gore gore2 = Gore.NewGoreDirect(
                source,
                NPC.Center - direction * Main.rand.NextFloat(20f, 40f),
                direction * Main.rand.NextFloat(1f, 2f),
                Mod.Find<ModGore>("TerrarianHunterGore1").Type
            );

            gore2.position -= new Vector2(gore2.Width, gore2.Height) * 0.5f;
        }

        Gore gore3 = Gore.NewGoreDirect(
            source,
            NPC.Center + Vector2.UnitY * 8f,
            Vector2.UnitY * Main.rand.NextFloat(1f, 4f),
            Mod.Find<ModGore>("TerrarianHunterGore2").Type
        );

        gore3.position -= new Vector2(gore1.Width, gore1.Height) * 0.5f;
    }

    public override void FindFrame(int frameHeight) {
        NPC.frame = Attack switch
        {
            Attacks.HoverHorizontal => new(110, 0, 110, 164),
            Attacks.BottomMine => new(0, 0, 110, 164),
            Attacks.Gun => new(330, 0, 110, 164),
            _ => new(220, 0, 110, 164),
        };
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        Matrix transformationMatrix = Matrix.CreateTranslation(-NPC.Center.X, -NPC.Center.Y, 0f)
            * Matrix.CreateRotationZ(NPC.rotation)
            * Matrix.CreateTranslation(-Main.screenPosition.X + NPC.Center.X, -Main.screenPosition.Y + NPC.Center.Y, 0f)
            * Main.GameViewMatrix.TransformationMatrix
            * Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

        Effect effect = EffectLoader.GetEffect("Trail::Fire");
        effect.Parameters["sampleTexture"].SetValue(Mod.Assets.Request<Texture2D>("Assets/Textures/Sample/Noise2", AssetRequestMode.ImmediateLoad).Value);
        effect.Parameters["transformationMatrix"].SetValue(transformationMatrix);
        effect.Parameters["amp"].SetValue(0.15f);
        effect.Parameters["smooth"].SetValue(0.45f);

        Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

        for(int i = 0; i < engineTrails.Length; i++) {
            effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.02f + i * 7.34893f);
            engineTrails[i].Draw(effect);
        }

        Texture2D texture = TextureAssets.Npc[Type].Value;
        spriteBatch.Draw(
            texture,
            NPC.Center - screenPos,
            NPC.frame,
            drawColor,
            NPC.rotation,
            NPC.frame.Size() / 2f,
            NPC.scale,
            NPC.direction == 1f ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
            0f
        );

        Texture2D glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow", AssetRequestMode.ImmediateLoad).Value;
        spriteBatch.Draw(
            glowTexture,
            NPC.Center - screenPos,
            NPC.frame,
            Color.White,
            NPC.rotation,
            NPC.frame.Size() / 2f,
            NPC.scale,
            NPC.direction == 1f ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
            0f
        );

        if(Attack == Attacks.Gun) {
            Texture2D gunTexture = ModContent.Request<Texture2D>(Texture + "_Gun", AssetRequestMode.ImmediateLoad).Value;
            int frame = GunFrame;
            Rectangle source = new(0, 34 * frame, 78, 34);
            Vector2 origin = new(NPC.direction == 1 ? -GunOriginOffset : source.Width + GunOriginOffset, source.Height / 2f);
            Vector2 position = GunElbowPosition.RotatedBy(NPC.rotation, NPC.Center) - screenPos;

            spriteBatch.Draw(
                gunTexture,
                position,
                source,
                drawColor,
                gunRotation - (NPC.direction == -1 ? MathHelper.Pi : 0f),
                origin,
                NPC.scale,
                NPC.direction == 1f ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0f
            );

            Texture2D gunGlowTexture = ModContent.Request<Texture2D>(Texture + "_Gun_Glow", AssetRequestMode.ImmediateLoad).Value;
            spriteBatch.Draw(
                gunGlowTexture,
                position,
                source,
                Color.White,
                gunRotation - (NPC.direction == -1 ? MathHelper.Pi : 0f),
                origin,
                NPC.scale,
                NPC.direction == 1f ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0f
            );

            SpriteBatchSnapshot snapshot = spriteBatch.Capture();
            spriteBatch.End();
            spriteBatch.Begin(snapshot with { BlendState = BlendState.Additive });

            Texture2D bloomGlowTexture = Mod.Assets.Request<Texture2D>("Assets/Textures/Misc/Glow2", AssetRequestMode.ImmediateLoad).Value;
            spriteBatch.Draw(
                bloomGlowTexture,
                SmallMuzzlePosition - screenPos,
                null,
                Color.OrangeRed * smallMuzzleAlpha,
                0f,
                bloomGlowTexture.Size() / 2f,
                0.5f,
                SpriteEffects.None,
                0f
            );

            spriteBatch.Draw(
                bloomGlowTexture,
                BigMuzzlePosition - screenPos,
                null,
                Color.OrangeRed * bigMuzzleAlpha,
                0f,
                bloomGlowTexture.Size() / 2f,
                0.5f,
                SpriteEffects.None,
                0f
            );

            spriteBatch.End();
            spriteBatch.Begin(snapshot);
        }

        return false;
    }
}