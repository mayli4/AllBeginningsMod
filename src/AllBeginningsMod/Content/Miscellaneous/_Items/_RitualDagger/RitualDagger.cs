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
        Item.damage = 25;
        Item.crit = 0;
        Item.DamageType = DamageClass.Magic;
        Item.knockBack = 0f;

        Item.width = 30;
        Item.height = 30;
        
        Item.useTurn = true;
        Item.useTime = 5;
        Item.useAnimation = 5;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.channel = true;
        Item.noMelee = true;
        Item.noUseGraphic = true;

        Item.shoot = ModContent.ProjectileType<RitualDaggerHeld>();
        Item.shootSpeed = 10f;
        
        Item.autoReuse = false;
    }

    public override bool CanUseItem(Player player) {
        return player.ownedProjectileCounts[Item.shoot] == 0;
    }
}

internal sealed class RitualDaggerHeld : ModProjectile {
    public override string Texture => Textures.Items.Misc.RitualDagger.KEY_RitualDaggerItem;

    public Player Owner => Main.player[Projectile.owner];

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

    public override void OnSpawn(IEntitySource source) {
        base.OnSpawn(source);
    }

    public override void AI() {
        base.AI();
    }

    public override bool PreDraw(ref Color lightColor) {
        return base.PreDraw(ref lightColor);
    }
}