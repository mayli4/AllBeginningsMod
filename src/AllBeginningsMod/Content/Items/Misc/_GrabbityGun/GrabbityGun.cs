using AllBeginningsMod.Common.Camera;
using AllBeginningsMod.Common.Graphics;
using AllBeginningsMod.Content.Dusts;
using AllBeginningsMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace AllBeginningsMod.Content.Items.Misc;

internal sealed class GrabbityGunItem : ModItem {
    public sealed override string Texture => Textures.Items.Misc.GrabbityGun.KEY_GrabbityGunItem;

    private static float tooltipProgress;
    
    public override void SetDefaults() {
        Item.width = 40;
        Item.height = 40;
        Item.rare = ItemRarityID.Yellow;
        Item.value = Item.sellPrice(gold: 5);

        Item.useTurn = true;    
        Item.useTime = 5;
        Item.useAnimation = 5;
        Item.useStyle = ItemUseStyleID.Shoot; 
        Item.channel = true;
        Item.noMelee = true;
        Item.noUseGraphic = true;

        Item.shoot = ModContent.ProjectileType<GrabbityGunProjectile>();
        Item.shootSpeed = 10f;
    }

    public override bool AltFunctionUse(Player player) {
        return true;
    }

    public override bool CanUseItem(Player player) {
        return player.ownedProjectileCounts[ModContent.ProjectileType<GrabbityGunProjectile>()] <= 0;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        Projectile.NewProjectile(
            player.GetSource_ItemUse(Item),
            Main.MouseWorld,
            Vector2.Zero,
            type,
            (int)player.GetTotalDamage(DamageClass.Magic).ApplyTo(damage),
            knockback,
            player.whoAmI
        );
        
        Projectile.NewProjectile(
            source,
            player.Center,
            Vector2.Zero,
            ModContent.ProjectileType<GrabbityGunItemHeldProjectile>(),
            0,
            0,
            player.whoAmI
        );
        
        return false;
    }
    
    public override void UpdateInventory(Player Player) {
        if (!(Main.HoverItem.ModItem is GrabbityGunItem))
            tooltipProgress = 0;
    }
    
    public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
    {
        if (line.Mod == "Terraria" && line.Name == "ItemName")
        {
            Effect effect = Shaders.Text.WarpTooltip.Value;

            if (effect is null)
                return true;

            Texture2D tex = Textures.Sample.Glow.Value;

            effect.Parameters["speed"].SetValue(1);
            effect.Parameters["power"].SetValue(0.011f * tooltipProgress);
            effect.Parameters["uTime"].SetValue(Main.GameUpdateCount / 10f);

            int measure = (int)(line.Font.MeasureString(line.Text).X * 1.1f);
            int offset = (int)(Math.Sin(Main.GameUpdateCount / 25f) * 5);
            var target = new Rectangle(line.X + measure / 2, line.Y + 10, (int)(measure * 1.5f) + offset, 34 + offset);
            //Main.spriteBatch.Draw(tex, target, new Rectangle(4, 4, tex.Width - 4, tex.Height - 4), Color.Black * (0.675f * tooltipProgress + (float)Math.Sin(Main.GameUpdateCount / 25f) * -0.1f), 0, (tex.Size() - Vector2.One * 8) / 2, 0, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, effect, Main.UIScaleMatrix);

            Utils.DrawBorderString(Main.spriteBatch, line.Text, new Vector2(line.X, line.Y), Color.Orange, 1.1f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);

            if (tooltipProgress < 1)
                tooltipProgress += 0.05f;

            return false;
        }

        return base.PreDrawTooltipLine(line, ref yOffset);
    }
}

//lazy :\\
internal sealed class GrabbityGunItemHeldProjectile : ModProjectile {
    public override string Texture => Textures.Items.Misc.GrabbityGun.KEY_GrabbityGunHeld;
    
    public int GrabbityGunID => (int)Projectile.ai[0];
    public ref float ShakeTimer => ref Projectile.ai[1];

    public const float SHAKE_TIME = 15f;

    public override void SetStaticDefaults() {
        ProjectileID.Sets.DontAttachHideToAlpha[Type] = true;
    }

    public override void SetDefaults() {
        Projectile.width = 30;
        Projectile.height = 28;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.aiStyle = -1;
        Projectile.alpha = 0;
        Projectile.ownerHitCheck = true;
        Projectile.hide = true;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void AI() {
        var player = Main.player[Projectile.owner];
        
        Projectile logicProjectile = null;
        if (GrabbityGunID >= 0 && GrabbityGunID < Main.maxProjectiles) {
            logicProjectile = Main.projectile[GrabbityGunID];
        }

        if (!player.channel || player.dead || !player.active || player.HeldItem.type != ModContent.ItemType<GrabbityGunItem>()) {
            Projectile.Kill();
            return;
        }
        
        Projectile.velocity = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitY);

        player.heldProj = Projectile.whoAmI;
        player.ChangeDir(Math.Sign(Projectile.velocity.X));
        player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.velocity.ToRotation() * player.gravDir - MathHelper.PiOver2);
        player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Projectile.velocity.ToRotation() * player.gravDir - MathHelper.PiOver2 - MathHelper.PiOver4 * 0.5f * player.direction);

        player.SetDummyItemTime(2);
        Projectile.rotation = Projectile.velocity.ToRotation();
        Projectile.Center = player.MountedCenter;
        Projectile.timeLeft = 2;

        Projectile.spriteDirection = player.direction;
        
        if (ShakeTimer > 0) {
            ShakeTimer--;
        }
    }
    
    public void TriggerShake() {
        ShakeTimer = SHAKE_TIME;
        Projectile.netUpdate = true;
    }

    public override bool? CanCutTiles() => false;

    public override bool PreDraw(ref Color lightColor) {
        var player = Main.player[Projectile.owner];
        
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        Vector2 origin = new Vector2(10, texture.Height / 2f);
        SpriteEffects effect = SpriteEffects.None;
        if (player.direction * player.gravDir < 0) {
            effect = SpriteEffects.FlipVertically;
        }
        
        var shakeOffset = Vector2.Zero;
        if (ShakeTimer > 0) {
            float shakeIntensity = Math.Clamp(ShakeTimer / SHAKE_TIME, 0f, 1f);
            shakeOffset = Main.rand.NextVector2Circular(10f, 10f) * shakeIntensity;
        }
        var position = player.MountedCenter + Projectile.velocity.SafeNormalize(Vector2.Zero) * 10f + Vector2.UnitY * player.gfxOffY;
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.Zero);
        
        Main.EntitySpriteDraw(texture, position - Main.screenPosition + shakeOffset, null, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, effect, 0);

        return false;
    }
}

internal sealed class GrabbityGunProjectile : ModProjectile {
    public sealed override string Texture => Helper.PlaceholderTextureKey;

    private const float grab_range = 16 * 25;
    
    public SlotId loopSlotID;

    public override void SetDefaults() {
        Projectile.width = 1;
        Projectile.height = 1;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.aiStyle = -1;
        Projectile.timeLeft = 3600;
        Projectile.ownerHitCheck = true;
        Projectile.extraUpdates = 1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void AI() {
        Player player = Main.player[Projectile.owner];

        if (player.dead || !player.active || !player.channel || player.noItems || player.CCed) {
            Projectile.Kill();
            return;
        }

        player.heldProj = Projectile.whoAmI;

        Vector2 gunOffset = new Vector2(player.direction * 14f, -4f); 
        Projectile.Center = player.Center + gunOffset; 
        
        Projectile.rotation = (Main.MouseWorld - Projectile.Center).ToRotation(); 
        
        Projectile.spriteDirection = player.direction;
        if (Main.MouseWorld.X < Projectile.Center.X) {
            Projectile.spriteDirection = -1;
        }
        
        if (Projectile.ai[2] > 0) {
            Projectile.ai[2]--;
        }
        
        bool loopPlaying = SoundEngine.TryGetActiveSound(loopSlotID, out var loopSound) && loopSound.IsPlaying;

        NPC grabbedNPC = null;
        if (Projectile.ai[0] == 1) {
            grabbedNPC = Main.npc[(int)Projectile.ai[1]];

            if (!grabbedNPC.active || grabbedNPC.lifeMax <= 0) {
                ReleaseGrab(); 
                return;
            }

            float currentDistanceFromGun = Vector2.Distance(grabbedNPC.Center, Projectile.Center);
            if (currentDistanceFromGun > grab_range) {
                Vector2 directionToNPC = (grabbedNPC.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Vector2 clampedPosition = Projectile.Center + directionToNPC * grab_range;

                grabbedNPC.Center = clampedPosition;
                grabbedNPC.velocity = Vector2.Zero;
                grabbedNPC.netUpdate = true;
            }
            
            if (!loopPlaying) {
                loopSlotID = SoundEngine.PlaySound(Sounds.Item.GrabbityGun.GrabbityGunHum);
                loopPlaying = SoundEngine.TryGetActiveSound(loopSlotID, out loopSound);
                if (loopPlaying)
                {
                    loopSound.Volume = 0f;
                    loopSound.Update();
                }
            }
            else {
                loopSound.Volume += 1 / (60f * 0.4f) * 0.3f;
                if (loopSound.Volume > 0.3f)
                    loopSound.Volume = 0.3f;
            
                loopSound.Update();
            }
        }

        if (Projectile.ai[0] == 0) {
            if (loopPlaying)
            {
                loopSound.Volume -= 0.01f;
                if (loopSound.Volume <= 0f)
                    loopSound.Stop();

                loopSound.Update();
            }
            
            if (Projectile.ai[2] > 0) {
                return;
            }
            
            if (player.controlUseItem) {
                if (Vector2.Distance(player.MountedCenter, Main.MouseWorld) > grab_range) {
                    return;
                }
                
                foreach (NPC npc in Main.npc) {
                    if (npc.active && npc.lifeMax > 0) {
                        if (npc.Hitbox.Contains(Main.MouseWorld.ToPoint()) && npc.Distance(Main.MouseWorld) < grab_range) {
                            Projectile.ai[0] = 1;
                            Projectile.ai[1] = npc.whoAmI;
                            Projectile.netUpdate = true;

                            npc.GetGlobalNPC<GrabbityGunGlobalNPC>().IsGrabbed = true;
                            
                            SoundEngine.PlaySound(Sounds.Item.GrabbityGun.GrabbityGunGrab with { Pitch = 0.0f, PitchVariance = 0.5f }, Projectile.Center);
                            npc.netUpdate = true;
                            break;
                        }
                    }
                }
            }
        }
        else {
            if (grabbedNPC != null) {
                if (!player.controlUseItem) {
                    ReleaseGrab(true); 
                    return;
                }
                
                Vector2 dragTarget = Main.MouseWorld;
                Vector2 directionToMouse = (dragTarget - grabbedNPC.Center);

                grabbedNPC.velocity += directionToMouse * 0.1f;

                if (grabbedNPC.velocity.Length() > 20) {
                    grabbedNPC.velocity = Vector2.Normalize(grabbedNPC.velocity) * 20;
                }

                grabbedNPC.velocity *= 0.92f;
                grabbedNPC.netUpdate = true;
                
                if (Main.mouseRight) {
                    Vector2 pushDirection = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX * player.direction);
                    grabbedNPC.velocity = pushDirection * 20;

                    ReleaseGrab(false);
                }
            }
        }
    }

    private void ReleaseGrab(bool withPushback = false) {
        Player player = Main.player[Projectile.owner];
        Projectile heldGunVisualProjectile = null;
        if (Projectile.owner >= 0 && Projectile.owner < Main.maxProjectiles) {
            foreach (Projectile proj in Main.projectile) {
                if (proj.active && proj.owner == Projectile.owner && proj.type == ModContent.ProjectileType<GrabbityGunItemHeldProjectile>()) {
                    heldGunVisualProjectile = proj;
                    break;
                }
            }
        }

        if (Projectile.ai[0] == 1) {
            NPC grabbedNPC = Main.npc[(int)Projectile.ai[1]];
            if (grabbedNPC.active && grabbedNPC.lifeMax > 0) { 
                Vector2 flingDirection = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX * player.direction); 
                    
                for (int i = 0; i < 10; i++) {
                    Vector2 dustPosition = grabbedNPC.Center + Main.rand.NextVector2Circular(grabbedNPC.width / 2f, grabbedNPC.height / 2f);
                
                    var dustVelocity = -flingDirection.RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.NextFloat(-30 * 0.5f, -30);
                
                    Dust.NewDustPerfect(
                        dustPosition,
                        ModContent.DustType<ImpactLineDust>(),
                        dustVelocity,
                        0,
                        new Color(255, 120, 0, 5),
                        1f
                    );
                }
                
                if (withPushback) {
                    grabbedNPC.velocity = flingDirection * 15;
                }
                
                int impactDamage = 111; // Or derive it from the player's damage
                float knockback = Projectile.knockBack; // You might want to use this too

                GrabbityGunGlobalNPC globalNPC = grabbedNPC.GetGlobalNPC<GrabbityGunGlobalNPC>();
                globalNPC.IsGrabbed = false;

                // Mark the NPC as launched and store relevant information
                globalNPC.IsLaunched = true;
                globalNPC.LauncherProjectileIdentity = Projectile.whoAmI;
                globalNPC.LaunchDamage = impactDamage; // Store the damage

                grabbedNPC.netUpdate = true;
                
                grabbedNPC.GetGlobalNPC<GrabbityGunGlobalNPC>().IsGrabbed = false; 
                grabbedNPC.netUpdate = true;
            }
        }
        Projectile.ai[0] = 0;
        Projectile.ai[1] = -1;
        Projectile.netUpdate = true;
        SoundEngine.PlaySound(Sounds.Item.GrabbityGun.GrabbityGunLaunch with { Pitch = 0.0f, PitchVariance = 0.5f }, Projectile.Center);
        
        GrabbityGunItemHeldProjectile heldGun = heldGunVisualProjectile?.ModProjectile as GrabbityGunItemHeldProjectile;
        heldGun?.TriggerShake();
        Main.instance.CameraModifiers.Add(new ExplosionShakeCameraModifier(10, 0.5f));
        
        Projectile.ai[2] = 60;
    }
    
    public override unsafe bool PreDraw(ref Color lightColor) {
        Projectile heldGunVisualProjectile = null;
        foreach (Projectile proj in Main.projectile) {
            if (proj.active && proj.owner == Projectile.owner && proj.type == ModContent.ProjectileType<GrabbityGunItemHeldProjectile>()) {
                heldGunVisualProjectile = proj;
                break;
            }
        }

        if (heldGunVisualProjectile == null) {
            return false;
        }

        if (Projectile.ai[0] == 1) {
            NPC grabbedNPC = Main.npc[(int)Projectile.ai[1]];
            if (grabbedNPC.active) {
                var heldGunTexture = ModContent.Request<Texture2D>(heldGunVisualProjectile.ModProjectile.Texture).Value;
                
                Vector2 heldGunVisualOrigin = new Vector2(10, heldGunTexture.Height / 2f);

                float muzzleOffsetXFromOriginPivot = (heldGunTexture.Width - 5f) - heldGunVisualOrigin.X;
                float muzzleOffsetYFromOriginPivot = 0f;

                Vector2 muzzleLocalOffsetFromPivot = new Vector2(muzzleOffsetXFromOriginPivot, muzzleOffsetYFromOriginPivot);
                
                Vector2 rotatedMuzzleOffset = muzzleLocalOffsetFromPivot.RotatedBy(heldGunVisualProjectile.rotation);

                Vector2 beamStart = heldGunVisualProjectile.Center + rotatedMuzzleOffset;
                Vector2 beamEnd = grabbedNPC.Center;

                int segments = 20;
                var trailPoints = new List<Vector2>(segments + 1);

                var midControlPoint = Vector2.Lerp(beamStart, beamEnd, 0.5f);
                
                float curveInfluence = 0.35f; 
                midControlPoint += (Main.MouseWorld - grabbedNPC.Center) * curveInfluence;

                ReadOnlySpan<Vector2> controlPoints = stackalloc Vector2[] { beamStart, midControlPoint, beamEnd };
                using (var curve = new BezierCurve(controlPoints)) {
                    trailPoints = curve.GetPoints(segments + 1);
                }

                var shader = Shaders.Trail.GrabbityGunBeam.Value;
                
                Graphics.BeginPipeline(0.5f)
                    .DrawTrail(
                        trailPoints.ToArray(), 
                        _ => 20f, 
                        _ => Color.OrangeRed,
                        shader,
                        ("transformMatrix", Graphics.WorldTransformMatrix),
                        ("time", Main.GameUpdateCount * -0.025f),
                        ("sampleTexture", Textures.Sample.GlowTrail.Value),
                        ("repeats", 0)
                        )
                    .Flush();
                
                Graphics.BeginPipeline(0.5f)    
                    .DrawTrail(
                        trailPoints.ToArray(), 
                        _ => 15f, 
                        _ => Color.OrangeRed,
                        shader,
                        ("transformMatrix", Graphics.WorldTransformMatrix),
                        ("time", Main.GameUpdateCount * -0.025f),
                        ("sampleTexture", Textures.Sample.EnergyTrail.Value),
                        ("repeats", 2)
                    )
                    .ApplyOutline(Color.Yellow)
                    .Flush();
                
                var snapshot = Main.spriteBatch.CaptureEndBegin(new() { BlendState = BlendState.Additive });

                Main.spriteBatch.Draw(
                    Textures.Sample.Glow1.Value,
                    beamEnd - Main.screenPosition,
                    null,
                    new Color(255, 106, 0) * 0.25f,
                    0f,
                    Textures.Sample.Glow1.Value.Size() / 2f,
                    0.6f,
                    SpriteEffects.None,
                    0f
                );
                
                Main.spriteBatch.Draw(
                    Textures.Sample.Glow1.Value,
                    beamEnd - Main.screenPosition,
                    null,
                    new Color(255, 106, 0) * 0.35f,
                    0f,
                    Textures.Sample.Glow1.Value.Size() / 2f,
                    0.3f,
                    SpriteEffects.None,
                    0f
                );
                
                Main.spriteBatch.Draw(
                    Textures.Sample.Glow1.Value,
                    beamStart - Main.screenPosition,
                    null,
                    new Color(255, 106, 0) * 0.35f,
                    0f,
                    Textures.Sample.Glow1.Value.Size() / 2f,
                    0.3f,
                    SpriteEffects.None,
                    0f
                );

                Main.spriteBatch.EndBegin(snapshot);
            }
        }

        return false;
    }
}   


internal sealed class GrabbityGunGlobalNPC : GlobalNPC {
    public override bool InstancePerEntity => true;
    public bool IsGrabbed { get; set; }
    
    public bool IsLaunched { get; set; }
    public int LauncherProjectileIdentity { get; set; }
    public int LaunchDamage { get; set; }

    public override void ResetEffects(NPC npc) {
        if (IsGrabbed) {
            bool foundActiveGrabber = Main.projectile.Any(proj =>
                proj.active
                && proj.type == ModContent.ProjectileType<GrabbityGunProjectile>()
                && proj.ai[0] == 1f
                && (int)proj.ai[1] == npc.whoAmI
            );

            if (!foundActiveGrabber) {
                IsGrabbed = false;
            }
        }
        
        if (IsLaunched && npc.velocity.Length() < 7f) {
            IsLaunched = false;
            LauncherProjectileIdentity = -1;
            LaunchDamage = 0;
        }

        GrabbityGunOutline.AnyGrabbed = true;
    }

    public override bool PreAI(NPC npc) {
        if(IsLaunched) {
            if(npc.velocity.Length() > 16f) {
                Point tilePosition = (npc.Center + npc.velocity * 0.5f).ToTileCoordinates(); 

                bool collided = WorldGen.SolidTile(tilePosition.X, tilePosition.Y);

                if(!collided) {
                    for(int x = -1; x <= 1; x++) {
                        for(int y = -1; y <= 1; y++) {
                            Point checkTile = (npc.Center + new Vector2(x * npc.width, y * npc.height) + npc.velocity * 0.5f).ToTileCoordinates();
                            if(WorldGen.InWorld(checkTile.X, checkTile.Y) &&
                               Main.tile[checkTile.X, checkTile.Y].HasTile &&
                               Main.tileSolidTop[Main.tile[checkTile.X, checkTile.Y].type] == false &&
                               Main.tileSolid[Main.tile[checkTile.X, checkTile.Y].TileType]) {
                                collided = true;
                                break;
                            }
                        }

                        if(collided) break;
                    }
                }


                if(collided) {
                    SoundEngine.PlaySound(SoundID.Dig, npc.position);

                    for(int i = 0; i < 5; i++) {
                        Dust.NewDust(npc.position, npc.width, npc.height, DustID.Stone, -npc.velocity.X * 0.5f,
                            -npc.velocity.Y * 0.5f, 0, default, 1.2f);
                    }

                    int wallImpactDamage = (int)(LaunchDamage * 0.75f);
                    Player player = Main.player[Main.projectile[LauncherProjectileIdentity].owner];

                    Projectile sourceProjectile = Main.projectile[LauncherProjectileIdentity];

                    npc.StrikeNPC(wallImpactDamage, sourceProjectile.knockBack * 0.5f, npc.direction, true, true, false);

                    npc.velocity *= -0.5f;

                    IsLaunched = false;
                    LauncherProjectileIdentity = -1;
                    LaunchDamage = 0;
                    npc.netUpdate = true;
                }
            }
        }

        return true;
    }

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        // if (IsGrabbed) {
        //     var npcTexture = Terraria.GameContent.TextureAssets.Npc[npc.type].Value;
        //     var frame = npc.frame;
        //     var origin = frame.Size() / 2f;
        //     var effects = npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        //     
        //     var npcDrawingColor = npc.GetAlpha(drawColor);
        //     
        //     var pos = npc.townNPC 
        //         ? (npc.Center + Vector2.UnitY * npc.gfxOffY - new Vector2(0, 4)) - screenPos
        //         : npc.Center - screenPos;
        //
        //     var shader = Shaders.Fragment.Tint.Value;
        //     shader.Parameters["color"].SetValue(Color.Orange.ToVector4());
        //     
        //     var snapshot = spriteBatch.CaptureEndBegin(new SpriteBatchSnapshot() with { CustomEffect = shader });
        //
        //     spriteBatch.Draw(npcTexture, pos + new Vector2(2, 0), npc.frame, drawColor, npc.rotation, npc.frame.Size() / 2, npc.scale, npc.spriteDirection == -1 ? 0 : SpriteEffects.FlipHorizontally, 0);
        //     spriteBatch.Draw(npcTexture, pos - new Vector2(2, 0), npc.frame, drawColor, npc.rotation, npc.frame.Size() / 2, npc.scale, npc.spriteDirection == -1 ? 0 : SpriteEffects.FlipHorizontally, 0);
        //     
        //     spriteBatch.Draw(npcTexture, pos + new Vector2(0, 2), npc.frame, drawColor, npc.rotation, npc.frame.Size() / 2, npc.scale, npc.spriteDirection == -1 ? 0 : SpriteEffects.FlipHorizontally, 0);
        //     spriteBatch.Draw(npcTexture, pos - new Vector2(0, 2), npc.frame, drawColor, npc.rotation, npc.frame.Size() / 2, npc.scale, npc.spriteDirection == -1 ? 0 : SpriteEffects.FlipHorizontally, 0);
        //
        //     spriteBatch.EndBegin(snapshot);
        //
        //     //orugh... ughhhr... fuuckkk..
        //     // var pipeline = Graphics.BeginPipeline();
        //     //     pipeline
        //     //     .SetBlendState(BlendState.AlphaBlend)
        //     //     .DrawSprite(npcTexture, pos, drawColor, npc.frame, npc.rotation, npc.frame.Size() / 2, new Vector2(npc.scale), npc.spriteDirection == -1 ? 0 : SpriteEffects.FlipHorizontally)
        //     //     .Flush();
        // }

        if(IsGrabbed) {
            return false;
        }

        return true;
    }
}

public class GrabbityGunOutline : ILoadable {
    public static RenderTarget2D NPCTarget;

    public static bool AnyGrabbed;

    public void Load(Mod mod) {
        if (Main.dedServ)
            return;

        ResizeTarget();

        Main.OnPreDraw += On_PreDraw;
        On_Main.DrawNPCs += DrawOutline;
    }

    private void DrawOutline(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles) {
        orig(self, behindTiles);

        if (AnyGrabbed)
            DrawNPCTarget();
    }

    public static void ResizeTarget() {
        Main.QueueMainThreadAction(() => NPCTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight));
    }

    public void Unload() {
        Main.OnPreDraw -= On_PreDraw;
    }

    private void On_PreDraw(GameTime obj) {
        var graphicsDevice = Main.graphics.GraphicsDevice;
        var spriteBatch = Main.spriteBatch;

        if (Main.gameMenu || Main.dedServ || spriteBatch is null || NPCTarget is null || graphicsDevice is null)
            return;

        RenderTargetBinding[] bindings = graphicsDevice.GetRenderTargets();
        graphicsDevice.SetRenderTarget(NPCTarget);
        graphicsDevice.Clear(Color.Transparent);

        Main.spriteBatch.Begin(default, default, default, default, default, null, Main.GameViewMatrix.ZoomMatrix);

        for (int i = 0; i < Main.npc.Length; i++) {
            NPC NPC = Main.npc[i];

            if (NPC.active && NPC.GetGlobalNPC<GrabbityGunGlobalNPC>().IsGrabbed) {
                if (NPC.ModNPC != null) {
                    if (NPC.ModNPC != null && NPC.ModNPC is ModNPC ModNPC) {
                        if (ModNPC.PreDraw(spriteBatch, Main.screenPosition, NPC.GetAlpha(Color.White)))
                            Main.instance.DrawNPC(i, false);

                        ModNPC.PostDraw(spriteBatch, Main.screenPosition, NPC.GetAlpha(Color.White));
                    }
                }
                else {
                    Main.instance.DrawNPC(i, false);
                }
            }
        }

        spriteBatch.End();
        graphicsDevice.SetRenderTargets(bindings);
    }

    private static void DrawNPCTarget() {
        var graphicsDevice = Main.graphics.GraphicsDevice;
        var spriteBatch = Main.spriteBatch;

        if (Main.dedServ || spriteBatch == null || NPCTarget == null || graphicsDevice == null)
            return;

        var shader = Shaders.Fragment.Outline.Value;
            
        shader.Parameters["uColor"].SetValue(Color.Orange.ToVector4());
        shader.Parameters["uSize"].SetValue(Main.ScreenSize.ToVector2());
        shader.Parameters["uThreshold"].SetValue(0f);
            
        var snapshot = spriteBatch.CaptureEndBegin(new SpriteBatchSnapshot() with { CustomEffect = shader });
            
        spriteBatch.Draw(NPCTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

        spriteBatch.EndBegin(snapshot);
    }
}