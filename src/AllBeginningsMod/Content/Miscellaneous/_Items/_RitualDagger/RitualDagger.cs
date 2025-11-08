using AllBeginningsMod.Utilities;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace AllBeginningsMod.Content.Miscellaneous;

internal sealed class RitualDagger : ModItem {
    public override string Texture => Textures.Items.Misc.RitualDagger.KEY_RitualDaggerItem;

    public override void SetDefaults() {    
        Item.useStyle = ItemUseStyleID.HiddenAnimation;
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
    public override string Texture => Textures.Items.Misc.RitualDagger.KEY_RitualDaggerHeld;

    public Player Owner => Main.player[Projectile.owner];

    public ref float DeployedFrames => ref Projectile.ai[0];
    public ref float InitialSpawnOffset => ref Projectile.ai[1];
    
    public Vector2 DaggerStartingPosition => Owner.Center - Vector2.UnitY * -18f * Owner.gravDir + Vector2.UnitX * 6 * Owner.direction;
    
    public float PutMaskOnTimer => Easings.SineInEasing(1 - Math.Clamp(DeployedFrames / 25f, 0f, 1f), 1);
    
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
        InitialSpawnOffset = 46f;
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

        InitialSpawnOffset = Math.Max(0f, InitialSpawnOffset - 2f); 
        Projectile.Center = DaggerStartingPosition + new Vector2(1, 0) * InitialSpawnOffset * Owner.direction;

        player.direction = (Main.MouseWorld.X - player.Center.X).NonZeroSign();
        player.heldProj = Projectile.whoAmI;
        player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, (MathHelper.Pi + MathHelper.PiOver4 * 11.9f + PutMaskOnTimer * MathHelper.PiOver4) * player.direction);
        
        player.SetDummyItemTime(2);
        if (player.mount.Active)
            player.mount.Dismount(player);
        DeployedFrames++;
    }

    public override bool PreDraw(ref Color lightColor) {
        var daggerTex = Textures.Items.Misc.RitualDagger.RitualDaggerHeld.Value;
        
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        Vector2 origin = new Vector2(10, texture.Height / 2f);
        SpriteEffects effect = SpriteEffects.None;
        
        if(Owner.direction == 1) {
            effect = SpriteEffects.FlipHorizontally;
        }
        
        var position = new Vector2((int)Projectile.position.X, (int)Projectile.position.Y - Owner.HeightOffsetVisual) + Vector2.UnitY * Owner.gfxOffY;

        Main.EntitySpriteDraw(texture, position - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, effect);
        
        return false;
    }
}