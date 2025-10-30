using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace AllBeginningsMod.Content.Miscellaneous;

internal sealed class RitualDagger : ModItem {
    public override string Texture => Textures.Sample.KEY_Blobs;
    
    public override void SetDefaults() {
        Item.damage = 25;
        Item.crit = 0;
        Item.DamageType = DamageClass.Magic;
        Item.knockBack = 0f;

        Item.width = 30;
        Item.height = 30;

        Item.useTime = 60;
        Item.useAnimation = 60;
        Item.useStyle = ItemUseStyleID.Swing;

        Item.noMelee = true;
        Item.noUseGraphic = true;

        Item.shoot = ModContent.ProjectileType<RitualDaggerHeld>();
        Item.shootSpeed = 1f;
        
        Item.channel = false;
        Item.autoReuse = false;
    }

    public override bool CanUseItem(Player player) {
        return player.ownedProjectileCounts[Item.shoot] == 0;
    }
}

internal sealed class RitualDaggerHeld : ModProjectile {
    public override string Texture => Textures.Sample.KEY_Blobs;

    public override void SetDefaults() {
        
    }

    public override void OnSpawn(IEntitySource source) {
        base.OnSpawn(source);
    }

    public override void AI() {
        
    }
}