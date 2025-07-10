using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using AllBeginningsMod.Common.PrimitiveDrawing;
using AllBeginningsMod.Common.Rendering;
using AllBeginningsMod.Utilities;
using ReLogic.Content;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;

namespace AllBeginningsMod.Content.NPCs.Corruption;

//todo: fix trails, add sfx, add bestiary entry, balancing

public sealed class DevilOWarNPC : ModNPC {
    public enum State {
        Idle,
        Charging,
        AttackCooldown,
    }
    
    public override string Texture => Assets.Assets.Textures.NPCs.Corruption.DevilOWar.KEY_DevilOWarHead;

    public State CurrentState {
        get => (State)NPC.ai[0];
        set => NPC.ai[0] = (float)value;
    }

    private int _attackCooldownTimer;
    public int _stingerProjectileId = -1;

    public Player Target => Main.player[NPC.target];

    private const int follow_range = 16 * 30;
    public const int charging_radius = 26 * 10;
    private const int attack_cooldown_time = 60 * 1; // 1 second
    public const int stinger_duration_max = 60 * 5; // 5 seconds

    private PrimitiveTrail _activeStingerTrail;
    private const int tentacle_segment_count = 8;
    
    private PrimitiveTrail[] _tentacleTrails;
    
    public Vector2 DrawScale = Vector2.One;
    public float Pulsation;

    public override void SetDefaults() {
        NPC.width = 36;
        NPC.height = 36;
        //NPC.damage = 25;
        //NPC.defense = 35;
        NPC.lifeMax = 120;
        NPC.value = 67f;
        NPC.noTileCollide = false;
        NPC.aiStyle = -1;
        NPC.noGravity = true;
        NPC.knockBackResist = 0.05f;
        NPC.friendly = false;

        NPC.HitSound = SoundID.NPCHit23;
    }

    public override void OnSpawn(IEntitySource source) {
        _tentacleTrails = new PrimitiveTrail[4];
        for(int i = 0; i < _tentacleTrails.Length; i++) {
            _tentacleTrails[i] = new(new Vector2[8], _ => 10);
        }
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        base.SetBestiary(database, bestiaryEntry);
    }

    public override void AI() {
        NPC.TargetClosest();
        if (Target != null && Target.active && !Target.dead) {
            if (Target.Center.X < NPC.Center.X) {
                NPC.direction = -1;
            }
            else {
                NPC.direction = 1;
            }
        }

        NPC.rotation = NPC.velocity.X * 0.1f;

        if (_stingerProjectileId != -1) {
            Projectile stingerProj = Main.projectile[_stingerProjectileId];
            if (!stingerProj.active || stingerProj.type != ModContent.ProjectileType<DevilOWarStingerProjectile>()) {
                _stingerProjectileId = -1;
                _activeStingerTrail = null;
            }
            else if (_activeStingerTrail == null) {
                _activeStingerTrail = new PrimitiveTrail(new Vector2[tentacle_segment_count], _ => 10, initPosition: stingerProj.Center);
            }
            
            if (stingerProj.active && stingerProj.ModProjectile is DevilOWarStingerProjectile stinger && stinger.AttachedToPlayer) {
                Pulsation = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.5f + 0.5f;

                float minScale = 0.95f;
                float maxScale = 1.05f;

                float scaleX = MathHelper.Lerp(minScale, maxScale, Pulsation);
                float scaleY = MathHelper.Lerp(maxScale, minScale, Pulsation);

                DrawScale = new Vector2(scaleX, scaleY);
            } else {
                Pulsation = 0f;
                DrawScale = Vector2.One;
            }
        }
        else {
            Pulsation = 0f;
            _activeStingerTrail = null;
            DrawScale = Vector2.One;
        }

        if (_activeStingerTrail != null && _stingerProjectileId != -1) {
            var stingerProj = Main.projectile[_stingerProjectileId];
            if (stingerProj.active && stingerProj.ModProjectile is DevilOWarStingerProjectile stinger && !stinger.IsRetracting) {
                var activeStingerStart = NPC.Center;
                var activeStingerPoints = new Vector2[tentacle_segment_count];
                activeStingerPoints[0] = activeStingerStart;
                GenerateWavyTentaclePoints(activeStingerPoints, activeStingerStart, stingerProj.Center, tentacle_segment_count, 0.5f, 0.1f, 15f);
                
                _activeStingerTrail.Positions = activeStingerPoints.Select(p => p - Main.screenPosition).ToArray();
            }
            else if (stingerProj.active && stingerProj.ModProjectile is DevilOWarStingerProjectile retractingStinger && retractingStinger.IsRetracting) {
                var retractingStingerStart = NPC.Center;
                var retractingStingerPoints = new Vector2[tentacle_segment_count];
                retractingStingerPoints[0] = retractingStingerStart;
                GenerateWavyTentaclePoints(retractingStingerPoints, retractingStingerStart, retractingStinger.Projectile.Center, tentacle_segment_count, 0.5f, 0.1f, 15f);
                
                _activeStingerTrail.Positions = retractingStingerPoints.Select(p => p - Main.screenPosition).ToArray();
            }
            else {
                _activeStingerTrail = null;
                _stingerProjectileId = -1;
            }
        }


        switch (CurrentState) {
            case State.Idle:
                if (NPC.Center.Distance(Target!.Center) < follow_range) {
                    NPC.velocity += 0.05f * NPC.Center.DirectionTo(Target.Center);
                    if (NPC.velocity.Length() > 2f) {
                        NPC.velocity = Vector2.Normalize(NPC.velocity) * 2f;
                    }
                }
                else {
                    NPC.velocity *= 0.98f;
                }

                if (_stingerProjectileId == -1 && NPC.Center.Distance(Target.Center) < charging_radius) {
                    FireStinger();
                    CurrentState = State.Charging;
                }
                break;

            case State.Charging:
                if (_stingerProjectileId != -1 
                    && Main.projectile[_stingerProjectileId].active 
                    && Main.projectile[_stingerProjectileId].ModProjectile is DevilOWarStingerProjectile stinger) {
                    if (!stinger.IsRetracting) {
                        NPC.velocity += 0.02f * NPC.Center.DirectionTo(Target!.Center);
                        if (NPC.velocity.Length() > 1.5f) {
                            NPC.velocity = Vector2.Normalize(NPC.velocity) * 1.5f;
                        }

                        if (NPC.Center.Distance(Target.Center) >= charging_radius + 16 * 2) {
                            RetractStinger();
                        }
                    }
                    else {
                        CurrentState = State.AttackCooldown;
                        _attackCooldownTimer = attack_cooldown_time;
                    }
                }
                else
                {
                    CurrentState = State.AttackCooldown;
                    _attackCooldownTimer = attack_cooldown_time;
                }
                break;

            case State.AttackCooldown:
                NPC.velocity *= 0.95f;
                _attackCooldownTimer--;
                if (_attackCooldownTimer <= 0)
                {
                    CurrentState = State.Idle;
                }
                break;
        }
    }

    private void FireStinger() {
        if (_stingerProjectileId == -1) {
            var proj = Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                NPC.Center,
                NPC.Center.DirectionTo(Target.Center) * 10f,
                ModContent.ProjectileType<DevilOWarStingerProjectile>(),
                NPC.damage,
                0,
                Main.myPlayer,
                Target.whoAmI,
                NPC.whoAmI
            );

            if (proj != -1 && Main.projectile[proj].active) {
                _stingerProjectileId = proj;
            }
            else {
                _stingerProjectileId = -1;
                _activeStingerTrail = null;
                CurrentState = State.AttackCooldown;
                _attackCooldownTimer = attack_cooldown_time;
            }
        }
    }


    public void RetractStinger() {
        if (_stingerProjectileId != -1 && Main.projectile[_stingerProjectileId].active && Main.projectile[_stingerProjectileId].ModProjectile is DevilOWarStingerProjectile stinger) {
            if (!stinger.IsRetracting) {
                stinger.StartRetraction();
            }
        }
    }

    public override void OnKill() {
        if (_stingerProjectileId != -1 && Main.projectile[_stingerProjectileId].active && Main.projectile[_stingerProjectileId].ModProjectile is DevilOWarStingerProjectile stinger) {
            stinger.StartRetraction();
            _stingerProjectileId = -1;
            _activeStingerTrail = null;
        }   
    }
    
    private void DrawTrails(Vector2 bodyDrawPosition, Color drawColor) {
        float Equation(float x) {
            return 0.2f * MathF.Sin(x) + 0.8f * MathF.Cos(x + MathHelper.PiOver4);
        }
        
        var initialPositions =  new[] {
            new Vector2(-1, 1),
            new Vector2(-2, 1),
            new Vector2(2, 1),
            new Vector2(1, 1)
        };

        for(int i = 0; i < _tentacleTrails.Length; i++) {
            var positions = new Vector2[_tentacleTrails[i].MaxTrailPositions];
            positions[0] = initialPositions[i];
            var moveDirection = positions[0].SafeNormalize(Vector2.Zero);
            for(int j = 1; j < _tentacleTrails[i].MaxTrailPositions; j++) {
                float factor = j / (_tentacleTrails[i].MaxTrailPositions - 1f);
                positions[j] = positions[0]
                               + moveDirection * MathHelper.Lerp(110, 130, MathF.Sin(Main.GameUpdateCount * (0.02f + i * 0.003f) + i * 0.6f)) * factor
                               + moveDirection.RotatedBy(MathHelper.PiOver2) * Equation(Main.GameUpdateCount * (0.04f + i * 0.005f) + factor * 4f + factor + i * 0.4f) * 20f;
            }

            _tentacleTrails[i].Positions = positions.Select(position => position + bodyDrawPosition).ToArray();
        }

        Texture2D intestineTexture = Assets.Assets.Textures.NPCs.Corruption.DevilOWar.DevilOWarTentacle.Value;
        Matrix transformationMatrix = Main.GameViewMatrix.TransformationMatrix
                                      * Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);
        for(int i = 0; i < _tentacleTrails.Length; i++) {
            _tentacleTrails[i].Draw(intestineTexture, drawColor, transformationMatrix);
        }
    }
    
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        var headTexture = Assets.Assets.Textures.NPCs.Corruption.DevilOWar.DevilOWarHead.Value;
        var insidesTexture = Assets.Assets.Textures.NPCs.Corruption.DevilOWar.DevilOWarInsides.Value;
        
        if(!NPC.IsABestiaryIconDummy) {
            Vector2 bodyDrawPosition = NPC.Center + new Vector2(0, 40) - Main.screenPosition;
            
            SpriteBatchData capture = spriteBatch.Capture();
            spriteBatch.End();
            spriteBatch.Begin(capture);

            DrawTrails(bodyDrawPosition, drawColor);  
        }
        
        var glowColor = Color.Lerp(drawColor, new Color(114, 109, 27, 200), Pulsation);
        
        var flipped = NPC.direction != -1;
        var effects = flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        
        Vector2 origin = new Vector2(headTexture.Width, headTexture.Height) / 2;
        origin.X = flipped ? headTexture.Width - origin.X : origin.X;
        
        Main.spriteBatch.Draw(insidesTexture, NPC.Center + new Vector2(0, 19) - Main.screenPosition, null, drawColor, NPC.rotation, insidesTexture.Size() / 2, 1f, effects, 0f);
        Main.spriteBatch.Draw(headTexture, NPC.Center - new Vector2(0, 4) - Main.screenPosition, null, glowColor * 0.8f, NPC.rotation, origin, DrawScale, effects, 0f);

        var transformationMatrix = Main.GameViewMatrix.TransformationMatrix * Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

        if (_activeStingerTrail != null) {
            var stingerColor = Color.Lerp(drawColor, Color.Yellow, 0.5f + MathF.Sin(Main.GameUpdateCount * 0.1f) * 0.2f);
            _activeStingerTrail.Draw(Assets.Assets.Textures.NPCs.Corruption.DevilOWar.DevilOWarTentacle.Value, stingerColor, transformationMatrix);
        }

        return false;
    }

    private void GenerateWavyTentaclePoints(Vector2[] pointsArray, Vector2 start, Vector2 end, int segments, float waveFrequency, float waveSpeed, float waveAmplitude, float phaseOffset = 0f) {
        pointsArray[0] = start;

        var direction = Vector2.Zero;
        if (Vector2.DistanceSquared(start, end) > 0.001f) {
            direction = Vector2.Normalize(end - start);
        }
        else {
            direction = Vector2.UnitY;
        }
        
        Vector2 perpendicular = new Vector2(-direction.Y, direction.X);

        float instancePhaseOffset = Main.GameUpdateCount * waveSpeed + phaseOffset;

        for (int i = 1; i < segments; i++) {
            float t = (float)i / (segments - 1);
            var basePoint = Vector2.Lerp(start, end, t);

            float waveDisplacement = (float)Math.Sin(t * MathHelper.TwoPi * waveFrequency + instancePhaseOffset) * waveAmplitude * (1f - t);

            pointsArray[i] = basePoint + perpendicular * waveDisplacement;
        }

        pointsArray[segments - 1] = end;
    }
}