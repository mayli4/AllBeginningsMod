using AllBeginningsMod.Utilities;
using System;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;

namespace AllBeginningsMod.Content.Miscellaneous;

internal sealed class RitualDagger : ModItem {
    public override string Texture => Assets.Textures.Items.Misc.RitualDagger.RitualDaggerItem.KEY;

    public override void SetDefaults() {    
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useAnimation = 45;
        Item.useTime = 45;
        Item.knockBack = 5.5f;
        Item.width = 32;
        Item.mana = 4;
        Item.height = 32;
        Item.damage = 11;
        Item.noUseGraphic = true;
        Item.shoot = ModContent.ProjectileType<RitualDaggerHeld>();
        Item.shootSpeed = 12f;
        Item.UseSound = SoundID.Item1;
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(gold: 3);
        Item.DamageType = DamageClass.Magic;
        Item.channel = true;
        Item.noMelee = true;
        Item.autoReuse = true;
    }

    public override bool CanUseItem(Player player) {
        return player.ownedProjectileCounts[Item.shoot] == 0;
    }
}

internal sealed class RitualDaggerHeld : ModProjectile {
    public override string Texture => Assets.Textures.Items.Misc.RitualDagger.RitualDaggerHeld.KEY;

    public Player Owner => Main.player[Projectile.owner];

    public ref float DeployedFrames => ref Projectile.ai[0];
    public ref float InitialOffset => ref Projectile.ai[1]; 
    
    public Vector2 DaggerStartingPosition => Owner.Center - Vector2.UnitY * -18f * Owner.gravDir + Vector2.UnitX * 6 * Owner.direction;
    
    public int DelayBeforeMovement = 15;
    public int StabMovementDuration = 25;
    
    public ref float TendrilSpawnCooldown => ref Projectile.localAI[0];
    
    public float TendrilMinSpawnDistance = 30f;
    public float TendrilMaxSpawnDistance = 60f;
    public int TendrilSpawnRate = 18;
    public float TendrilDetectionRange = 250f;
    public float TendrilConeAngleEnemyTarget = MathHelper.PiOver4 * 0.75f;
    public float TendrilConeAngleMouse = MathHelper.PiOver2;
    
    public const int LIFE_DRAIN_AMOUNT = 1;
    public const int LIFE_DRAIN_INTERVAL = 12;

    public ref float LifeDrainCooldown => ref Projectile.localAI[1];
    public override void SetDefaults() {
        Projectile.netImportant = true;
        Projectile.width = 26;
        Projectile.height = 26;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }
    
    public override void OnSpawn(IEntitySource source) {
        InitialOffset = 46f;
        TendrilSpawnCooldown = TendrilSpawnRate;
    }

    public override bool ShouldUpdatePosition() => false;
    public override bool? CanDamage() => false;
    public override bool? CanCutTiles() => false;
    
    public override void AI() {
        Player player = Main.player[Projectile.owner];
        if (!player.active || player.dead || player.noItems || player.CCed || !player.channel || (Main.myPlayer == Projectile.owner && Main.mapFullscreen)) {
            Projectile.Kill();
            return;
        }
        
        Projectile.timeLeft = 2;

        DeployedFrames++;

        float currentOffset;
        if (DeployedFrames < DelayBeforeMovement) {
            currentOffset = InitialOffset;
        } else {
            float stabProgress = Math.Min(1f, (DeployedFrames - DelayBeforeMovement) / StabMovementDuration);
            float easedProgress = Easings.CircInEasing(stabProgress);
            
            currentOffset = InitialOffset * (1f - easedProgress);
        }

        var currentPosition = DaggerStartingPosition + new Vector2(1, 0) * currentOffset * Owner.direction;

        if (currentOffset > 0f && DeployedFrames > 20) {
            const float maxShakeMagnitude = 4f; 
            float currentShakeMagnitude = (currentOffset / InitialOffset) * maxShakeMagnitude; 
            currentPosition += new Vector2(Main.rand.NextFloat(-currentShakeMagnitude, currentShakeMagnitude), Main.rand.NextFloat(-currentShakeMagnitude, currentShakeMagnitude));
        }
        Projectile.Center = currentPosition;
        
        if (currentOffset > 0f && DeployedFrames == (DelayBeforeMovement + StabMovementDuration)) {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
            for(int i = 0; i < 15; i++) {
                Dust.NewDustPerfect(Owner.Center, DustID.Blood, Main.rand.NextVector2Circular(2f, 2f));
            }
        }

        player.direction = (Main.MouseWorld.X - player.Center.X).NonZeroSign();
        player.heldProj = Projectile.whoAmI;
        
        player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, (MathHelper.Pi + MathHelper.PiOver4 * 11.9f + 0.1f * MathHelper.PiOver4) * player.direction);
        player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, (MathHelper.Pi + MathHelper.PiOver4 * 11.9f + 0.1f * MathHelper.PiOver4) * player.direction);
        
        player.SetDummyItemTime(2);
        if (player.mount.Active)
            player.mount.Dismount(player);
        
        if (DeployedFrames >= DelayBeforeMovement + StabMovementDuration) {
            LifeDrainCooldown--;
            if (LifeDrainCooldown <= 0) {
                LifeDrainCooldown = LIFE_DRAIN_INTERVAL;

                player.statLife -= LIFE_DRAIN_AMOUNT;
                player.lifeRegenTime = 0; 
                player.lifeRegen = 0;

                CombatText.NewText(player.Hitbox, CombatText.DamagedHostile, LIFE_DRAIN_AMOUNT);

                if (player.statLife <= 0) {
                    player.KillMe(PlayerDeathReason.ByProjectile(Projectile.owner, Projectile.whoAmI), 9999, 0);
                }
            }
            TendrilSpawnCooldown--;
            if (TendrilSpawnCooldown <= 0) {
                TendrilSpawnCooldown = TendrilSpawnRate;

                NPC target = null;
                float closestDistanceSq = TendrilDetectionRange * TendrilDetectionRange;

                foreach (NPC npc in Main.ActiveNPCs) {
                    if (npc.CanBeChasedBy(Projectile, false) && Projectile.DistanceSQ(npc.Center) < closestDistanceSq) {
                        target = npc;
                        closestDistanceSq = Projectile.DistanceSQ(npc.Center);
                    }
                }

                Vector2 baseDirection;
                float coneAngle;

                if (target != null) {
                    baseDirection = target.Center - Owner.Center;
                    coneAngle = TendrilConeAngleEnemyTarget;
                } else {
                    baseDirection = Main.MouseWorld - Owner.Center;
                    coneAngle = TendrilConeAngleMouse;
                }
                
                if (baseDirection.LengthSquared() == 0) { 
                    baseDirection = new Vector2(Owner.direction, 0); 
                }
                baseDirection.Normalize();

                float baseOutwardAngle = baseDirection.ToRotation();
                float randomAngleOffset = Main.rand.NextFloat(-coneAngle / 2f, coneAngle / 2f);
                float finalOutwardAngle = baseOutwardAngle + randomAngleOffset;
                
                Vector2 spawnDirection = finalOutwardAngle.ToRotationVector2(); 

                float randomDistance = Main.rand.NextFloat(TendrilMinSpawnDistance, TendrilMaxSpawnDistance);
                Vector2 arcPivotPoint = Owner.Center + spawnDirection * randomDistance;

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    arcPivotPoint,
                    Vector2.Zero,
                    ModContent.ProjectileType<RitualDaggerTendril>(),
                    Projectile.damage, 
                    Projectile.knockBack,
                    Projectile.owner
                );
            }
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D daggerTex = Assets.Textures.Items.Misc.RitualDagger.RitualDaggerHeld.Asset.Value;
        
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        Vector2 origin = new Vector2(9, texture.Height / 2f);

        SpriteEffects effect = SpriteEffects.None;
         
        if (Owner.direction == 1) {
            effect = SpriteEffects.FlipHorizontally;
            origin = new Vector2(4, texture.Height / 2f);
        }
        
        if (DeployedFrames > (DelayBeforeMovement + StabMovementDuration - 7)) {
            daggerTex = Assets.Textures.Items.Misc.RitualDagger.RitualDaggerHeld_Hilt.Asset.Value;
        }
        
        var position = new Vector2((int)Projectile.position.X, (int)Projectile.position.Y - Owner.HeightOffsetVisual) + Vector2.UnitY * Owner.gfxOffY;

        var fx = Assets.Shaders.Fragment.Tint.CreateAwesomePass();
        fx.Parameters.color = Color.Red.ToVector4();
        fx.Apply();
        
        Main.spriteBatch.End(out var ss);
        Main.spriteBatch.Begin(ss with { CustomEffect = fx.Shader });

        var offsets = new[]
        {
            new Vector2(0, 2),
            new Vector2(0, -2),
            new Vector2(Owner.direction == 1 ? 2f : -2f, 0),
        };

        foreach(var offset in offsets) {
            Main.spriteBatch.Draw(daggerTex, position - Main.screenPosition + offset, null, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, effect, 1f);   
        }
        
        Main.spriteBatch.EndBegin(ss);
        
        Main.spriteBatch.Draw(daggerTex, position - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, effect, 1f);
        
        return false;
    }
}

internal sealed class RitualDaggerTendril : ModProjectile {
    public override string Texture => Helper.PlaceholderTextureKey;

    public override bool PreDraw(ref Color lightColor) { 
        return true; 
    }
}