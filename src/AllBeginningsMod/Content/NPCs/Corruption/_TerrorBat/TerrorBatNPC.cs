using AllBeginningsMod.Common.Bestiary;
using AllBeginningsMod.Content.Biomes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Drawing;
using Terraria.Graphics.Renderers;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace AllBeginningsMod.Content.NPCs.Corruption;

//todo: natural spawning

public class TerrorBatNPC : ModNPC {
    public override string Texture => Assets.Assets.Textures.NPCs.Corruption.TerrorBat.KEY_TerrorBatNPC;

    public enum State {
        IdleOnCeiling,
        WakingUp,
        Awake,
        DashTelegraph,
        Dashing,
        Spitting,
    }
    
    public State CurrentState {
        get => (State)NPC.ai[0];
        set => NPC.ai[0] = (float)value;
    }

    public float StateTimer {
        get => NPC.ai[1];
        set => NPC.ai[1] = value;
    }

    public float DashCooldown {
        get => NPC.ai[2];
        set => NPC.ai[2] = value;
    }
    public float SpitCooldown {
        get => NPC.ai[3];
        set => NPC.ai[3] = value;
    }

    private const float wake_up_detection_range = 300f;
    private const int waking_up_time = 2 * 60;
    private const float max_speed = 6f;
    private const float max_acceleration = 0.1f;

    private const float dash_speed = 20f;
    private const int dash_duration = 20;
    private const int dash_cooldown_time = 240;

    private const int spit_duration = 40;
    private const int spit_cooldown_time = 180;

    private const float rotation_factor = 0.08f;
    
    private int _sleepDustSpawnTimer;
    private int _currentSleepDustIndex;

    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 10;
    }

    public override void SetDefaults() {
        NPC.width = 40;
        NPC.height = 30;
        NPC.lifeMax = 90;
        NPC.damage = 30;
        NPC.defense = 10;
        NPC.value = 100 * 30;
        NPC.noTileCollide = false;
        NPC.aiStyle = -1;
        NPC.noGravity = true;
        NPC.knockBackResist = 0.05f;
        NPC.friendly = false;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath2;
        
        SpawnModBiomes = [ModContent.GetInstance<UnderworldCorruptionBiome>().Type];
        
        NPC.buffImmune[BuffID.CursedInferno] = true;
        NPC.buffImmune[BuffID.OnFire] = true;
        NPC.lavaImmune = true;
    }
    
    public override float SpawnChance(NPCSpawnInfo spawnInfo) {
        return spawnInfo.Player.InModBiome<UnderworldCorruptionBiome>() ? 0.1f : 0;
    }

    public override void OnSpawn(IEntitySource source) {
        int startX = (int)(NPC.Center.X / 16f);
        int startY = (int)(NPC.Center.Y / 16f);
        int searchRange = 30;

        for (int j = 0; j < searchRange; j++) {
            int checkY = startY - j;

            if (checkY < 10) {
                break;
            }

            var ceilingTile = Main.tile[startX, checkY];
            if (ceilingTile.HasTile 
                && Main.tileSolid[ceilingTile.TileType] 
                && !TileID.Sets.Platforms[ceilingTile.TileType]) {
                var newPosition = new Vector2(startX * 16f, (checkY + 2) * 16f);

                NPC.position = newPosition;
                CurrentState = State.IdleOnCeiling;
                NPC.velocity = Vector2.Zero;
                NPC.rotation = 0f;

                return;
            }
        }
        
        //if the loop completes and doesnt find a valid ceiling, just despawn so its not floating awkwardly
        NPC.active = false;
    }
    
    public override void Load() {
        for (int j = 0; j <= 3; j++)
            GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, "AllBeginningsMod/Assets/Textures/Gores/TerrorBatGore" + j);
    }
    
    public override void HitEffect(NPC.HitInfo hit) {
        if(Main.netMode == NetmodeID.Server || NPC.life > 0) {
            return;
        }
        
        for (int i = 0; i <= 3; i++) {
            Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.Center, Main.rand.NextVector2Circular(2, 2), Mod.Find<ModGore>("TerrorBatGore" + i).Type);
        }
    }

    public override void AI() {
        NPC.TargetClosest();
        Player targetPlayer = Main.player[NPC.target];

        if (!targetPlayer.active || targetPlayer.dead) {
            CurrentState = State.Awake; //todo make bat search for a place to perch after player is dead
            NPC.netUpdate = true;
            return;
        }
        
        if (DashCooldown > 0) {
            DashCooldown--;
        }
        if (SpitCooldown > 0) {
            SpitCooldown--;
        }

        switch (CurrentState) {
            case State.IdleOnCeiling:
                NPC.velocity = Vector2.Zero;
                NPC.rotation = 0f;
                NPC.noTileCollide = false;

                _sleepDustSpawnTimer--;
                if (_sleepDustSpawnTimer <= 0) {
                    Dust.NewDustPerfect(
                        NPC.Center + new Vector2(0f, -NPC.height / 2f - 4f - (_currentSleepDustIndex * 6f)),
                        ModContent.DustType<Sleep>(),
                        new Vector2(Main.rand.NextFloat(0, 0), Main.rand.NextFloat(0, -1))
                    ).fadeIn = 60;
                    _currentSleepDustIndex++;

                    if (_currentSleepDustIndex < 3) {
                        _sleepDustSpawnTimer = Main.rand.Next(8, 12);
                    } else {
                        _sleepDustSpawnTimer = Main.rand.Next(90, 150);
                        _currentSleepDustIndex = 0;
                    }
                }

                if (Vector2.Distance(NPC.Center, targetPlayer.Center) < wake_up_detection_range) {
                    CurrentState = State.WakingUp;
                    StateTimer = waking_up_time;
                    NPC.netUpdate = true;
                }
                break;

            case State.WakingUp:
                WakeUp();
                break;

            case State.Awake:
                AwakeMovement(targetPlayer);
                break;

            case State.DashTelegraph:
                DashTelegraph(targetPlayer);
                break;

            case State.Dashing:
                Dash(targetPlayer);
                break;

            case State.Spitting:
                Spitting(targetPlayer);
                break;
        }
    }

    public void WakeUp() {
        NPC.velocity = Vector2.Zero;
        NPC.noTileCollide = false;

        if(StateTimer == waking_up_time) {
            SoundEngine.PlaySound(SoundID.DD2_WyvernScream, NPC.Center);
        }

        if (StateTimer == waking_up_time - 60) {
            //WAKE UP other bats
            for (int i = 0; i < Main.npc.Length; i++) {
                var otherNpc = Main.npc[i];
                if (otherNpc.active && otherNpc.type == Type && otherNpc.whoAmI != NPC.whoAmI) {
                    var otherBat = otherNpc.ModNPC as TerrorBatNPC;
                    if (otherBat != null && otherBat.CurrentState == State.IdleOnCeiling) {
                        otherBat.CurrentState = State.Awake;
                        otherNpc.netUpdate = true;
                    }
                }
            }
        }

        StateTimer--;
        if (StateTimer <= 0) {
            CurrentState = State.Awake;
            NPC.noTileCollide = true;
            NPC.netUpdate = true;
        }
    }

    public void AwakeMovement(Player targetPlayer) {
        NPC.noTileCollide = false;
        NPC.direction = NPC.spriteDirection = (targetPlayer.Center.X < NPC.Center.X) ? -1 : 1;

        Vector2 directionToPlayer = targetPlayer.Center - NPC.Center;
        directionToPlayer.Normalize();
        directionToPlayer *= max_acceleration;

        NPC.velocity += directionToPlayer;
        NPC.velocity = Vector2.Clamp(NPC.velocity, -Vector2.One * max_speed, Vector2.One * max_speed);

        NPC.velocity.X += Main.rand.NextFloat(-0.05f, 0.05f);
        NPC.velocity.Y += Main.rand.NextFloat(-0.05f, 0.05f);

        NPC.rotation = NPC.velocity.X * rotation_factor;
                
        if (DashCooldown <= 0 && Vector2.Distance(NPC.Center, targetPlayer.Center) > 100f && Main.rand.NextBool(100)) {
            CurrentState = State.DashTelegraph;
            StateTimer = 25;
            NPC.netUpdate = true;
        }
        else if (SpitCooldown <= 0 && Vector2.Distance(NPC.Center, targetPlayer.Center) < 400f && Main.rand.NextBool(180)) {
            CurrentState = State.Spitting;
            StateTimer = spit_duration;
            NPC.netUpdate = true;
            SpitCooldown = spit_cooldown_time;
        }
    }

    public void DashTelegraph(Player targetPlayer) {
        NPC.direction = NPC.spriteDirection = (targetPlayer.Center.X < NPC.Center.X) ? -1 : 1;

        Vector2 directionAwayFromPlayer = Vector2.Normalize(NPC.Center - targetPlayer.Center);
        NPC.velocity = directionAwayFromPlayer * 4;

        NPC.rotation = NPC.velocity.X * rotation_factor;

        StateTimer--;
        if (StateTimer <= 0) {
            CurrentState = State.Dashing;
            StateTimer = dash_duration;
            NPC.netUpdate = true;
            DashCooldown = dash_cooldown_time;
        }
    }

    public void Dash(Player targetPlayer) {
        NPC.noTileCollide = true;

        if (StateTimer == dash_duration) {
            Vector2 dashDirection = Vector2.Normalize(targetPlayer.Center - NPC.Center);
            NPC.velocity = dashDirection * dash_speed;
            SoundEngine.PlaySound(SoundID.DD2_WyvernScream, NPC.Center);
        }

        NPC.rotation = NPC.velocity.X * rotation_factor;

        StateTimer--;
        if (StateTimer <= 0)
        {
            CurrentState = State.Awake;
            NPC.velocity *= 0.5f;
            NPC.netUpdate = true;
        }
    }

    public void Spitting(Player targetPlayer) {
        NPC.velocity *= 0.9f;
        NPC.noTileCollide = true;
        NPC.direction = NPC.spriteDirection = (targetPlayer.Center.X < NPC.Center.X) ? -1 : 1;

        if (StateTimer == (spit_duration / 2)) {
            var projectileVelocity = Vector2.Normalize(targetPlayer.Center - NPC.Center) * 8f;
            Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                NPC.Center,
                projectileVelocity,
                ModContent.ProjectileType<TerrorBatSpit>(),
                NPC.damage / 2,
                0.5f,
                Main.myPlayer
            );
            SoundEngine.PlaySound(SoundID.NPCDeath1, NPC.Center);
        }

        NPC.rotation = NPC.velocity.X * rotation_factor;

        StateTimer--;
        if (StateTimer <= 0) {
            CurrentState = State.Awake;
            NPC.netUpdate = true;
        }        
    }

    public override void FindFrame(int frameHeight) {
        NPC.spriteDirection = NPC.direction;

        NPC.frameCounter++;

        switch (CurrentState) {
            case State.IdleOnCeiling:
                NPC.frame.Y = 0 * frameHeight;
                break;

            case State.WakingUp:
                NPC.frame.Y = 1 * frameHeight;
                break;

            case State.Awake:
            case State.DashTelegraph:
            case State.Dashing:
                NPC.frame.Y = (int)(NPC.frameCounter / 5 % 4 + 2) * frameHeight;
                break;

            case State.Spitting:
                if (NPC.frameCounter < 10) {
                    NPC.frame.Y = 6 * frameHeight;
                }
                else if (NPC.frameCounter < 20) {
                    NPC.frame.Y = 7 * frameHeight;
                }
                else if (NPC.frameCounter < 30) {
                    NPC.frame.Y = 8 * frameHeight;
                }
                else if (NPC.frameCounter < 40) {
                    NPC.frame.Y = 9 * frameHeight;
                }
                else {
                    NPC.frameCounter = 0;
                }
                break;
        }
    }
    
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) => bestiaryEntry.AddInfo(this, "");

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if (CurrentState == State.IdleOnCeiling || CurrentState == State.WakingUp) {
            var effects = NPC.spriteDirection == 1 ? SpriteEffects.FlipVertically : (SpriteEffects.FlipVertically | SpriteEffects.FlipHorizontally);
            var texture = ModContent.Request<Texture2D>(Texture).Value;
            var frame = NPC.frame;
            var origin = new Vector2(frame.Width / 2f, frame.Height);
            var drawPos = new Vector2(NPC.Center.X, NPC.position.Y - 20) - screenPos;
            
            float sway = CurrentState == State.IdleOnCeiling ? (float)Math.Sin(Main.timeForVisualEffects / 30f + NPC.whoAmI) * 0.1f 
                : CurrentState == State.WakingUp ? (float)Math.Sin(Main.timeForVisualEffects / 10f + NPC.whoAmI) * 0.1f : 0f;

            spriteBatch.Draw(
                texture,
                drawPos,
                frame,
                drawColor,
                NPC.rotation + MathHelper.Pi + sway,
                origin,
                NPC.scale,
                effects,
                0f
            );

            return false;
        }

        if(NPC.IsABestiaryIconDummy) {
            NPC.frame.Y = 2 * NPC.frame.Height;
            return true;
        }
        
        return true;
    }
}